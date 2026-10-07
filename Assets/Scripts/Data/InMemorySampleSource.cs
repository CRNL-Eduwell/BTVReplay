using System;
using Tools.CSharp.EEG;

namespace BTV.Data
{
    /// <summary>
    /// Samples already in managed arrays (the loaded EEG channels, audio, evaluated montage
    /// channels). The arrays are wrapped, not copied, and never written to, so concurrent reads
    /// are safe. A channel shorter than the longest one (only a processed-audio CSV can have
    /// ragged rows) reads as zeros past its own end, as BtvChannel.GetSample used to return.
    /// </summary>
    public sealed class InMemorySampleSource : ISampleSource
    {
        private readonly float[][] m_Channels;

        public InMemorySampleSource(float[][] channels, Frequency frequency)
        {
            m_Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            Frequency = frequency ?? throw new ArgumentNullException(nameof(frequency));
            long longest = 0;
            foreach (float[] channel in channels)
            {
                if (channel == null)
                    throw new ArgumentException("A channel has no samples array.", nameof(channels));
                longest = Math.Max(longest, channel.Length);
            }
            SampleCount = longest;
        }

        public int ChannelCount => m_Channels.Length;
        public long SampleCount { get; }
        public Frequency Frequency { get; }

        public int ReadRange(long first, int count, int[] channels, float[][] dst)
        {
            if (first < 0)
                throw new ArgumentOutOfRangeException(nameof(first), first, "The first sample cannot be negative.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "The sample count cannot be negative.");
            if (first >= SampleCount)
                return 0;

            int read = (int)Math.Min(count, SampleCount - first);
            for (int k = 0; k < channels.Length; k++)
            {
                float[] channel = m_Channels[channels[k]];
                int available = (int)Math.Max(0, Math.Min(read, channel.Length - first));
                if (available > 0)
                    Array.Copy(channel, first, dst[k], 0, available);
                if (available < read)
                    Array.Clear(dst[k], available, read - available);
            }
            return read;
        }

        public void Dispose()
        {
        }
    }
}
