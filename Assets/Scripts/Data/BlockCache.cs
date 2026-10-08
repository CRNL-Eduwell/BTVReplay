using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BTV.Services;
using UnityEngine;

namespace BTV.Data
{
    /// <summary>
    /// The samples around the current time of one source, in fixed blocks of all channels, filled
    /// asynchronously. The traces and sites ask it for their window on every video tick; a window
    /// whose blocks are not all loaded yet is a miss, and the caller keeps its previous frame
    /// instead of reading synchronously (a read can be slow on a network share).
    ///
    /// The cache keeps what was asked for during the last frame with requests, plus one block
    /// before and one after, so it follows the longest trace period by itself. It reads through
    /// <see cref="ISampleSource.ReadRange"/> on a worker, at most <see cref="MaxInFlight"/>
    /// blocks at a time, and drops a block that arrives for a patient session that is no longer
    /// current. Main thread only, apart from the reads it starts.
    /// </summary>
    public sealed class BlockCache
    {
        /// <summary>Samples per block: one multiplexed read costs about the same for 1 channel or all.</summary>
        public const int BlockSize = 4096;
        public const int MaxInFlight = 2;

        private sealed class Block
        {
            public float[][] Samples;
            public int Count;
        }

        private readonly ISampleSource m_Source;
        private readonly int[] m_AllChannels;
        private readonly Func<int> m_FrameClock;
        private readonly Func<Action, Task> m_Runner;
        private readonly Dictionary<long, Block> m_Blocks = new Dictionary<long, Block>();
        private readonly HashSet<long> m_InFlight = new HashSet<long>();
        // Arrays of evicted blocks, handed to the next load instead of allocating.
        private readonly Stack<float[][]> m_Spare = new Stack<float[][]>();

        // Blocks asked for (margins included) during the frame in progress and the last one with requests.
        private int m_Frame = int.MinValue;
        private long m_NeedFirst, m_NeedLast = -1;
        private long m_KeepFirst, m_KeepLast = -1;

        /// <param name="frameClock">Current frame number; Time.frameCount by default.</param>
        /// <param name="runner">Runs a read off the main thread; Task.Run by default.</param>
        public BlockCache(ISampleSource source, Func<int> frameClock = null, Func<Action, Task> runner = null)
        {
            m_Source = source;
            m_AllChannels = new int[source.ChannelCount];
            for (int i = 0; i < m_AllChannels.Length; i++)
                m_AllChannels[i] = i;
            m_FrameClock = frameClock ?? (() => Time.frameCount);
            m_Runner = runner ?? (work => Task.Run(work));
        }

        public long BlockCount => (m_Source.SampleCount + BlockSize - 1) / BlockSize;
        public int LoadedBlockCount => m_Blocks.Count;
        public int InFlightCount => m_InFlight.Count;
        public bool IsLoaded(long block) => m_Blocks.ContainsKey(block);

        /// <summary>
        /// Copies samples [first, first + count) of <paramref name="channel"/> into
        /// dst[0..count), 0 outside the recording, when every block they fall in is loaded.
        /// Otherwise starts the missing loads and returns false, leaving dst untouched.
        /// </summary>
        public bool TryReadWindow(int channel, long first, int count, float[] dst)
        {
            BeginFrameIfNew();
            long start = Math.Max(first, 0);
            long end = Math.Min(first + count, m_Source.SampleCount);
            if (end <= start)
            {
                Array.Clear(dst, 0, count);
                return true;
            }

            long firstBlock = start / BlockSize;
            long lastBlock = (end - 1) / BlockSize;
            Need(firstBlock - 1, lastBlock + 1);
            // The window's own blocks first, then the margins.
            RequestLoads(firstBlock, lastBlock);
            RequestLoads(firstBlock - 1, lastBlock + 1);
            for (long b = firstBlock; b <= lastBlock; b++)
            {
                if (!m_Blocks.ContainsKey(b))
                    return false;
            }

            Array.Clear(dst, 0, (int)(start - first));
            for (long sample = start; sample < end;)
            {
                long b = sample / BlockSize;
                int offset = (int)(sample - b * BlockSize);
                Block block = m_Blocks[b];
                int length = (int)Math.Min(end - sample, BlockSize - offset);
                int available = Math.Max(0, Math.Min(length, block.Count - offset));
                Array.Copy(block.Samples[channel], offset, dst, sample - first, available);
                // A block the source returned short (a truncated file) reads as 0 past its end.
                if (available < length)
                    Array.Clear(dst, (int)(sample - first) + available, length - available);
                sample += length;
            }
            int written = (int)(end - first);
            if (written < count)
                Array.Clear(dst, written, count - written);
            return true;
        }

        private void BeginFrameIfNew()
        {
            int frame = m_FrameClock();
            if (frame == m_Frame)
                return;
            // A new frame: the last frame's requests are what to keep. Frames without any
            // request (paused) never get here, so nothing is evicted while nobody draws.
            if (m_NeedLast >= m_NeedFirst)
            {
                m_KeepFirst = m_NeedFirst;
                m_KeepLast = m_NeedLast;
            }
            m_Frame = frame;
            m_NeedFirst = 0;
            m_NeedLast = -1;
            Evict();
        }

        private void Need(long firstBlock, long lastBlock)
        {
            firstBlock = Math.Max(firstBlock, 0);
            lastBlock = Math.Min(lastBlock, BlockCount - 1);
            if (m_NeedLast < m_NeedFirst)
            {
                m_NeedFirst = firstBlock;
                m_NeedLast = lastBlock;
            }
            else
            {
                m_NeedFirst = Math.Min(m_NeedFirst, firstBlock);
                m_NeedLast = Math.Max(m_NeedLast, lastBlock);
            }
        }

        private void Evict()
        {
            List<long> evicted = null;
            foreach (long b in m_Blocks.Keys)
            {
                if (b < m_KeepFirst || b > m_KeepLast)
                    (evicted ?? (evicted = new List<long>())).Add(b);
            }
            if (evicted == null)
                return;
            foreach (long b in evicted)
            {
                if (m_Spare.Count < MaxInFlight)
                    m_Spare.Push(m_Blocks[b].Samples);
                m_Blocks.Remove(b);
            }
        }

        private void RequestLoads(long firstBlock, long lastBlock)
        {
            firstBlock = Math.Max(firstBlock, 0);
            lastBlock = Math.Min(lastBlock, BlockCount - 1);
            for (long b = firstBlock; b <= lastBlock && m_InFlight.Count < MaxInFlight; b++)
            {
                if (!m_Blocks.ContainsKey(b) && !m_InFlight.Contains(b))
                    Load(b);
            }
        }

        private async void Load(long block)
        {
            m_InFlight.Add(block);
            Session session = Session.Current;
            long first = block * BlockSize;
            int count = (int)Math.Min(BlockSize, m_Source.SampleCount - first);
            float[][] samples = m_Spare.Count > 0 ? m_Spare.Pop() : NewBlockArrays();
            int read = 0;
            try
            {
                await m_Runner(() => read = m_Source.ReadRange(first, count, m_AllChannels, samples));
                // Back on the main thread. A block read for a previous patient is dropped.
                if (Session.IsCurrent(session))
                    m_Blocks[block] = new Block { Samples = samples, Count = read };
            }
            catch (ObjectDisposedException)
            {
                // The file was closed under the read: its patient session ended. Nothing to report.
            }
            catch (Exception e)
            {
                // The frame stays frozen and the block is asked for again on the next tick.
                Debug.LogWarning("Could not read samples " + first + " to " + (first + count) + ": " + e.Message);
            }
            finally
            {
                m_InFlight.Remove(block);
            }
        }

        private float[][] NewBlockArrays()
        {
            float[][] arrays = new float[m_AllChannels.Length][];
            for (int c = 0; c < arrays.Length; c++)
                arrays[c] = new float[BlockSize];
            return arrays;
        }
    }
}
