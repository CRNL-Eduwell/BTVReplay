using System;

namespace BTV.Data
{
    /// <summary>
    /// The overview strip's points, taken from a channel's stored samples (ChannelStats) instead
    /// of the whole channel. The strip used to show sample <c>i * factor</c> of the channel;
    /// it now shows the same samples, with the factor rounded up to a multiple of the stored
    /// samples' stride, so it is today's strip whenever the factor already was one, and
    /// otherwise a slightly coarser one.
    /// </summary>
    public static class OverviewSampling
    {
        /// <summary>The decimation factor, rounded up to a multiple of <paramref name="stride"/>.</summary>
        public static int Factor(int wantedFactor, int stride)
        {
            int factor = Math.Max(1, wantedFactor);
            return (factor + stride - 1) / stride * stride;
        }

        /// <summary>Centred and scaled by the channel's range: (x − median) / (max − min).</summary>
        public static void Centred(ChannelStats stats, int factor, float[] dst)
        {
            int step = factor / stats.Stride;
            for (int i = 0; i < dst.Length; i++)
                dst[i] = (stats.StoredSamples[i * step] - stats.Median) / (stats.Max - stats.Min);
        }

        /// <summary>
        /// Scaled to [−1, 1] by a baseline's extremes: ((x − min) / (max − min) − 0.5) × 2.
        /// </summary>
        public static void BaselineNormalized(ChannelStats stats, float min, float max, int factor, float[] dst)
        {
            int step = factor / stats.Stride;
            for (int i = 0; i < dst.Length; i++)
            {
                // Rounded to float before the shift, as the old normalized array stored it (Mono
                // keeps float intermediates in double precision otherwise).
                float normalized = (float)((stats.StoredSamples[i * step] - min) / (max - min));
                dst[i] = (normalized - 0.5f) * 2;
            }
        }
    }
}
