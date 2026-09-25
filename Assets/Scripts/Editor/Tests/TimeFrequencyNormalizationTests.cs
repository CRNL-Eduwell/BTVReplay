using System;
using NUnit.Framework;

/// <summary>
/// Edit-mode tests for the TF z-score (review B-6). The baseline's standard deviation used to be
/// divided by unguarded: a flat baseline (disconnected or zero-filled contact) turned the map into
/// infinities that rendered as saturated red, and a one-window baseline gave NaN everywhere.
/// </summary>
public class TimeFrequencyNormalizationTests
{
    // bins x frames, filled row by row (one row per frequency bin).
    private static TimeFrequencyDataStructure Map(params float[][] bins)
    {
        var map = new TimeFrequencyDataStructure(1000f, bins.Length, bins[0].Length);
        for (int i = 0; i < bins.Length; i++) map.SetFrequencyBinData(i, bins[i]);
        return map;
    }

    [Test]
    public void ApplyZscore_UsesTheBaselineMeanAndSampleDeviation()
    {
        // Baseline 2, 4, 6: mean 4, sample deviation sqrt(((4 + 0 + 4) / 2)) = 2.
        var baseline = Map(new[] { 2f, 4f, 6f });
        var evt = Map(new[] { 4f, 8f, 0f, 10f });

        int flat = TimeFrequencyNormalization.ApplyZscore(baseline, evt);

        Assert.AreEqual(0, flat);
        CollectionAssert.AreEqual(new[] { 0f, 2f, -2f, 3f }, evt.GetFrequencyBinData(0));
    }

    [Test]
    public void ApplyZscore_FlatBaseline_MarksTheBinAsNoDataInsteadOfInfinity()
    {
        var baseline = Map(new[] { 0f, 0f, 0f, 0f }, new[] { 1f, 3f, 5f, 7f });
        var evt = Map(new[] { 5f, 9f }, new[] { 4f, 6f });

        int flat = TimeFrequencyNormalization.ApplyZscore(baseline, evt);

        Assert.AreEqual(1, flat);
        foreach (float v in evt.GetFrequencyBinData(0))
            Assert.IsTrue(float.IsNaN(v), "a flat-baseline bin has no defined z-score");
        foreach (float v in evt.GetFrequencyBinData(1))
            Assert.IsFalse(float.IsNaN(v) || float.IsInfinity(v), "other bins are unaffected");
    }

    [Test]
    public void ApplyZscore_ConstantNonZeroBaseline_IsFlatToo()
    {
        var baseline = Map(new[] { 1234.5f, 1234.5f, 1234.5f });
        var evt = Map(new[] { 1300f });

        Assert.AreEqual(1, TimeFrequencyNormalization.ApplyZscore(baseline, evt));
        Assert.IsTrue(float.IsNaN(evt.GetFrequencyBinData(0)[0]));
    }

    [Test]
    public void ApplyZscore_OneWindowBaseline_IsRefusedWithAMessage()
    {
        var baseline = Map(new[] { 3f }, new[] { 5f });
        var evt = Map(new[] { 1f, 2f }, new[] { 3f, 4f });

        var e = Assert.Throws<InvalidOperationException>(() => TimeFrequencyNormalization.ApplyZscore(baseline, evt));
        StringAssert.Contains("too short", e.Message);
    }

    [Test]
    public void TopValue_IgnoresNoDataBins()
    {
        var baseline = Map(new[] { 0f, 0f, 0f }, new[] { 2f, 4f, 6f });
        var evt = Map(new[] { 1f, 1f, 1f, 1f, 1f }, new[] { 4f, 6f, 8f, 10f, 12f });

        TimeFrequencyNormalization.ApplyZscore(baseline, evt);

        // Bin 1 becomes 0, 1, 2, 3, 4: the 90th percentile of those five values is 4.
        Assert.AreEqual(4f, evt.TopValue);
    }

    [Test]
    public void TopValue_AllBinsFlat_IsZeroNotNaN()
    {
        var baseline = Map(new[] { 0f, 0f });
        var evt = Map(new[] { 3f, 4f });

        TimeFrequencyNormalization.ApplyZscore(baseline, evt);

        Assert.AreEqual(0f, evt.TopValue);
    }
}
