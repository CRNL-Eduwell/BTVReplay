using System;
using Tools.CSharp.EEG;

namespace BTV.Data
{
    /// <summary>
    /// One channel of a recording: a channel of an <see cref="ISampleSource"/> plus its
    /// statistics. It used to own the whole channel as a float array (its public Data); reads
    /// now go through the source, which is what lets the samples stay out of memory later.
    /// </summary>
    public class BtvChannel
    {
        public int NumberOfSample
        {
            get
            {
                return (int)Math.Min(Source.SampleCount, int.MaxValue);
            }
        }
        public string Label { get; private set; } = "";
        public int ID { get; private set; } = -1;
        public Frequency Frequency { get; private set; } = new Frequency();
        public float MaxValue { get; private set; }
        public ISampleSource Source { get; }
        /// <summary>Index of this channel in <see cref="Source"/>.</summary>
        public int SourceChannel { get; }
        public ChannelStats Stats { get; }
        //==
        // { SourceChannel }, the channel list ReadRange takes: never written, so shared safely.
        private readonly int[] m_SourceChannels;
        // ReadRange takes one destination per channel: a per-thread holder for the single one
        // (traces read on the main thread while STFT and correlation read on workers).
        [ThreadStatic] private static float[][] t_Destination;
        [ThreadStatic] private static float[] t_Sample;

        public BtvChannel(string Name, int Position, ISampleSource source, int sourceChannel, ChannelStats stats)
        {
            Label = Name;
            ID = Position;
            Source = source;
            SourceChannel = sourceChannel;
            Stats = stats;
            Frequency = source.Frequency;
            MaxValue = Math.Max(Math.Abs(stats.Min), Math.Abs(stats.Max));
            m_SourceChannels = new[] { sourceChannel };
        }
        /// <summary>
        /// Another channel's samples under a new label and position (a montage renaming a
        /// channel): shares its source channel and statistics instead of copying them.
        /// </summary>
        public BtvChannel(BtvChannel source, string Name, int Position)
            : this(Name, Position, source.Source, source.SourceChannel, source.Stats)
        {
        }

        /// <summary>
        /// Sample <paramref name="index"/>, minus the median when centered; 0 outside the
        /// recording. Reads one sample: use <see cref="ReadWindow"/> for a run of samples.
        /// </summary>
        public float GetSample(int index, bool centered = false)
        {
            if (index < 0 || index >= Source.SampleCount)
            {
                return 0;
            }

            if (t_Sample == null) t_Sample = new float[1];
            Read(index, 1, t_Sample);
            return centered ? t_Sample[0] - Stats.Median : t_Sample[0];
        }

        /// <summary>
        /// Samples [first, first + count) into dst[0..count), minus the median when centered.
        /// Positions before the start or past the end of the recording are 0, as GetSample
        /// returns there.
        /// </summary>
        public void ReadWindow(long first, int count, float[] dst, bool centered = false)
        {
            long start = Math.Max(first, 0);
            long end = Math.Min(first + count, Source.SampleCount);
            int offset = (int)(start - first);
            int read = end > start ? Read(start, (int)(end - start), dst) : 0;
            if (offset > 0 && read > 0)
                Array.Copy(dst, 0, dst, offset, read);
            if (centered)
            {
                for (int i = offset; i < offset + read; i++)
                    dst[i] -= Stats.Median;
            }
            Array.Clear(dst, 0, Math.Min(offset, count));
            if (offset + read < count)
                Array.Clear(dst, offset + read, count - offset - read);
        }

        /// <summary>
        /// Min and max over samples [begin, end) of the channel, read block by block.
        /// (+inf, -inf) for an empty range.
        /// </summary>
        public (float min, float max) MinMax(long begin, long end)
        {
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            begin = Math.Max(begin, 0);
            end = Math.Min(end, Source.SampleCount);
            float[] block = new float[(int)Math.Max(0, Math.Min(ChannelStats.BlockSize, end - begin))];
            for (long first = begin; first < end; first += block.Length)
            {
                int read = Read(first, (int)Math.Min(block.Length, end - first), block);
                for (int i = 0; i < read; i++)
                {
                    if (block[i] < min) min = block[i];
                    if (block[i] > max) max = block[i];
                }
            }
            return (min, max);
        }

        private int Read(long first, int count, float[] dst)
        {
            if (t_Destination == null) t_Destination = new float[1][];
            t_Destination[0] = dst;
            try
            {
                return Source.ReadRange(first, count, m_SourceChannels, t_Destination);
            }
            finally
            {
                t_Destination[0] = null;
            }
        }
    }
}
