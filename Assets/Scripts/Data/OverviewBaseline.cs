using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BTV.Data
{
    /// <summary>
    /// The extremes of a channel over a baseline, for the normalised overview strip, read off the
    /// main thread and kept per (channel, baseline). The strip used to read them synchronously on
    /// every normalise, electrode or file switch: <see cref="BtvChannel.MinMax"/> reads the whole
    /// range from disk, and on a multiplexed file every channel's bytes with it, which froze the
    /// app for seconds on a long baseline (review L-5). Main thread only, apart from the reads.
    /// </summary>
    public sealed class OverviewBaseline
    {
        private readonly Func<Func<(float min, float max)>, Task<(float min, float max)>> m_Runner;
        private readonly Dictionary<(BtvChannel channel, long begin, long end), Task<(float min, float max)>> m_Reads =
            new Dictionary<(BtvChannel channel, long begin, long end), Task<(float min, float max)>>();

        /// <param name="runner">Runs a read off the main thread; Task.Run by default.</param>
        public OverviewBaseline(Func<Func<(float min, float max)>, Task<(float min, float max)>> runner = null)
        {
            m_Runner = runner ?? (work => Task.Run(work));
        }

        /// <summary>
        /// The extremes of samples [begin, end) of <paramref name="channel"/>: a completed task when
        /// they are known, otherwise the read in flight, started on the first request for that
        /// baseline. Never reads on the calling thread. A failed read is forgotten, so the next
        /// request tries again.
        /// </summary>
        public Task<(float min, float max)> Get(BtvChannel channel, long begin, long end)
        {
            var key = (channel, begin, end);
            if (m_Reads.TryGetValue(key, out Task<(float min, float max)> read) && !read.IsFaulted && !read.IsCanceled)
                return read;

            read = m_Runner(() => channel.MinMax(begin, end));
            m_Reads[key] = read;
            return read;
        }
    }
}
