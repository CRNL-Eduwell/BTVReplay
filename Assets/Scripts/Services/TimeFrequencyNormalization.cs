using System;

/// <summary>
/// Z-score normalization of an event's time-frequency map against a baseline event's map, one
/// frequency bin at a time: z = (value - baseline mean) / baseline standard deviation.
///
/// The old implementation divided by the native standard deviation unguarded. A baseline that is
/// flat for the whole window (a zero-filled or disconnected contact) has a deviation of exactly
/// zero, so every value became +/-infinity and rendered as saturated red, which reads as
/// activity; a one-window baseline gave 0/0 = NaN for every bin. Flat bins are now marked NaN
/// ("no data", drawn neutral) and a too-short baseline is refused with a message.
///
/// Pure managed code (runs on the TF worker, and in edit-mode tests without the native plugin).
/// The deviation is the sample one (n - 1), the same definition the native Framework used.
/// </summary>
public static class TimeFrequencyNormalization
{
    /// <summary>A sample standard deviation needs at least two analysis windows.</summary>
    public const int MinimumBaselineFrames = 2;

    // A bin counts as flat when its deviation is this small relative to its mean (or to 1, for
    // near-zero magnitudes). Only an exactly constant baseline reaches zero; the tolerance
    // absorbs rounding.
    private const double FlatTolerance = 1e-6;

    /// <summary>
    /// Normalizes <paramref name="evt"/> in place and returns how many frequency bins had a flat
    /// baseline (their values are set to NaN). Throws when the baseline is too short.
    /// </summary>
    public static int ApplyZscore(TimeFrequencyDataStructure baseline, TimeFrequencyDataStructure evt)
    {
        if (baseline.TimeFrameCount < MinimumBaselineFrames)
            throw new InvalidOperationException(string.Format(
                "The baseline event is too short for a z-score: it covers {0} analysis window(s), and at least {1} are needed. Choose a longer baseline event, or a shorter TF window.",
                baseline.TimeFrameCount, MinimumBaselineFrames));

        int flatBins = 0;
        for (int i = 0; i < baseline.FrequencyBinCount; i++)
        {
            float[] baselineData = baseline.GetFrequencyBinData(i);
            double mean = 0;
            for (int j = 0; j < baselineData.Length; j++) mean += baselineData[j];
            mean /= baselineData.Length;
            double sumSquares = 0;
            for (int j = 0; j < baselineData.Length; j++) sumSquares += (baselineData[j] - mean) * (baselineData[j] - mean);
            double stdDev = Math.Sqrt(sumSquares / (baselineData.Length - 1));

            float[] eventData = evt.GetFrequencyBinData(i);
            if (!(stdDev > FlatTolerance * Math.Max(1.0, Math.Abs(mean))))
            {
                flatBins++;
                for (int j = 0; j < eventData.Length; j++) eventData[j] = float.NaN;
            }
            else
            {
                for (int j = 0; j < eventData.Length; j++) eventData[j] = (float)((eventData[j] - mean) / stdDev);
            }
            evt.SetFrequencyBinData(i, eventData);
        }
        return flatBins;
    }
}
