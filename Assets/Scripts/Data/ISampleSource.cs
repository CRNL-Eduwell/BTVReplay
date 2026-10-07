using System;
using Tools.CSharp.EEG;

namespace BTV.Data
{
    /// <summary>
    /// The samples of a multi-channel recording, read by range instead of held as one array per
    /// channel. Channels and every consumer go through it, so where the samples live (managed
    /// arrays today, the native file later) is the source's business only.
    /// </summary>
    public interface ISampleSource : IDisposable
    {
        int ChannelCount { get; }
        long SampleCount { get; }
        Frequency Frequency { get; }

        /// <summary>
        /// Worker-safe, blocking. Fills <c>dst[k][0..count)</c> with channel <c>channels[k]</c>
        /// from sample <paramref name="first"/>, clamped at the end of the recording, and returns
        /// the number of samples read: 0 when <paramref name="first"/> is at or past the end.
        /// <paramref name="first"/> must not be negative.
        /// </summary>
        int ReadRange(long first, int count, int[] channels, float[][] dst);
    }
}
