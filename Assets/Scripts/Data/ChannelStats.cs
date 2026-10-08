using System;
using BTV.Services.CalculationService;

namespace BTV.Data
{
    /// <summary>
    /// What the display needs to know about a whole channel without holding it: exact min and
    /// max, and up to <see cref="StoredSampleCount"/> evenly spaced samples (sample
    /// <c>k * Stride</c>) that give the overview strip and the median. The median used to come
    /// from the whole channel; it only centres traces on screen, so the median of the stored
    /// samples is enough, and it is the exact one for channels of up to 8192 samples.
    /// </summary>
    public sealed class ChannelStats
    {
        public const int StoredSampleCount = 8192;
        /// <summary>Samples read per channel and per block by <see cref="Compute"/>.</summary>
        public const int BlockSize = 4096;

        public float Min { get; }
        public float Max { get; }
        public float Median { get; }
        /// <summary>Distance, in samples, between two stored samples.</summary>
        public int Stride { get; }
        /// <summary>Sample <c>k * Stride</c> of the channel, for every such sample.</summary>
        public float[] StoredSamples { get; }

        private ChannelStats(float min, float max, int stride, float[] storedSamples)
        {
            Min = min;
            Max = max;
            Stride = stride;
            StoredSamples = storedSamples;
            Median = storedSamples.Length > 0 ? CalculationService.Median(storedSamples, storedSamples.Length) : 0;
        }

        /// <summary>
        /// Statistics of a channel known only through its stored samples (a montage expression
        /// evaluated on its base channels' stored samples): the median as usual, and min/max
        /// estimated from those samples, which can miss a short spike between two of them.
        /// </summary>
        public static ChannelStats FromStoredSamples(float[] storedSamples, int stride)
        {
            float min = 0, max = 0;
            if (storedSamples.Length > 0)
            {
                min = float.PositiveInfinity;
                max = float.NegativeInfinity;
                foreach (float value in storedSamples)
                {
                    if (value < min) min = value;
                    if (value > max) max = value;
                }
            }
            return new ChannelStats(min, max, stride, storedSamples);
        }

        /// <summary>
        /// Distance between stored samples for a channel of <paramref name="sampleCount"/>
        /// samples: 1 up to 8192 samples, then the smallest that keeps at most 8192.
        /// </summary>
        public static int StrideFor(long sampleCount)
        {
            return (int)Math.Max(1, (sampleCount + StoredSampleCount - 1) / StoredSampleCount);
        }

        /// <summary>
        /// One pass over <paramref name="channels"/> of <paramref name="source"/>, block by block:
        /// runs where the channels are built (the load and montage workers).
        /// </summary>
        public static ChannelStats[] Compute(ISampleSource source, int[] channels)
        {
            long sampleCount = source.SampleCount;
            int stride = StrideFor(sampleCount);
            int storedCount = (int)((sampleCount + stride - 1) / stride);

            float[] min = new float[channels.Length];
            float[] max = new float[channels.Length];
            float[][] stored = new float[channels.Length][];
            float[][] block = new float[channels.Length][];
            for (int c = 0; c < channels.Length; c++)
            {
                min[c] = float.PositiveInfinity;
                max[c] = float.NegativeInfinity;
                stored[c] = new float[storedCount];
                block[c] = new float[(int)Math.Min(BlockSize, sampleCount)];
            }

            for (long first = 0; first < sampleCount; first += BlockSize)
            {
                int read = source.ReadRange(first, (int)Math.Min(BlockSize, sampleCount - first), channels, block);
                long firstStored = (first + stride - 1) / stride;
                for (int c = 0; c < channels.Length; c++)
                {
                    float[] values = block[c];
                    for (int j = 0; j < read; j++)
                    {
                        float value = values[j];
                        if (value < min[c]) min[c] = value;
                        if (value > max[c]) max[c] = value;
                    }
                    for (long k = firstStored; k * stride < first + read; k++)
                        stored[c][k] = values[k * stride - first];
                }
            }

            ChannelStats[] stats = new ChannelStats[channels.Length];
            for (int c = 0; c < channels.Length; c++)
            {
                // An empty channel has no extremes: 0 rather than the infinities.
                bool empty = sampleCount == 0;
                stats[c] = new ChannelStats(empty ? 0 : min[c], empty ? 0 : max[c], stride, stored[c]);
            }
            return stats;
        }
    }
}
