using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTV.Data;

namespace SimpleExpressionEngine
{
    /// <summary>
    /// Resolves channel names to their sample at <see cref="Index"/> within the current block
    /// (<see cref="BeginBlock"/>). A channel's block is read from its source the first time the
    /// expression names it, so only the channels an expression uses are read.
    /// </summary>
    public class ChannelContext : IContext
    {
        /// <summary>Samples per block evaluated by montage generation.</summary>
        public const int BlockSize = 16384;

        public ChannelContext(List<BtvChannel> channels)
        {
            m_Channels = channels;
            Index = 0;
        }

        List<BtvChannel> m_Channels;
        /// <summary>Position within the current block.</summary>
        public int Index { get; set; }

        Dictionary<string, BtvChannel> m_ChannelCache = new Dictionary<string, BtvChannel>();
        // One buffer per channel read, reused from block to block; filled once per block.
        Dictionary<BtvChannel, float[]> m_Blocks = new Dictionary<BtvChannel, float[]>();
        HashSet<BtvChannel> m_LoadedThisBlock = new HashSet<BtvChannel>();
        long m_BlockFirst = 0;
        int m_BlockCount = 0;

        /// <summary>Makes samples [first, first + count) the current block.</summary>
        public void BeginBlock(long first, int count)
        {
            m_BlockFirst = first;
            m_BlockCount = count;
            m_LoadedThisBlock.Clear();
        }

        public double ResolveVariable(string name)
        {
            if (!m_ChannelCache.TryGetValue(name, out BtvChannel channel))
            {
                channel = m_Channels.FirstOrDefault(c => c.Label == name);
                if (channel == null)
                    throw new InvalidDataException($"Unknown channel: '{name}'");
                m_ChannelCache.Add(name, channel);
            }
            if (Index >= m_BlockCount)
                throw new InvalidDataException("Index out of range");

            if (!m_Blocks.TryGetValue(channel, out float[] block))
            {
                block = new float[BlockSize];
                m_Blocks.Add(channel, block);
            }
            if (m_LoadedThisBlock.Add(channel))
                channel.ReadWindow(m_BlockFirst, m_BlockCount, block);
            return block[Index];
        }

        public double CallFunction(string name, double[] arguments)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            m_ChannelCache.Clear();
            m_LoadedThisBlock.Clear();
        }
    }
}
