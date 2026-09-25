using System;

/// <summary>
/// Loop-mode clamping of a scrollbar seek, pulled out of CustomVideoPlayer (review M-7): in
/// loop mode a seek outside [min, max] snaps to the nearer edge, itself kept within the video.
/// </summary>
public static class LoopWindow
{
    /// <summary>Returns the time to seek to, and whether it was clamped to the loop window.</summary>
    public static (long time, bool clamped) Clamp(long requestedTime, long minTime, long maxTime, long totalTime)
    {
        if (requestedTime > maxTime) return (Math.Min(totalTime, maxTime), true);
        if (requestedTime < minTime) return (Math.Max(0, minTime), true);
        return (requestedTime, false);
    }
}
