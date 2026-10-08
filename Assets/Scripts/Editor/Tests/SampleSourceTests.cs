using System;
using System.Linq;
using BTV.Data;
using BTV.Services.CalculationService;
using NUnit.Framework;
using Tools.CSharp.EEG;

/// <summary>
/// Edit-mode tests for the sample-source seam (windowed-loading design, phase P1). Channels used
/// to hold their whole recording in a public float array; they now read ranges from an
/// ISampleSource, and the whole-channel statistics and the overview strip come from one pass
/// that keeps 8192 samples per channel. These pin the source contract and that every consumer
/// gets what it got from the array.
/// </summary>
public class SampleSourceTests
{
    private static float[] Noise(int length, int seed, float offset = 0)
    {
        Random random = new Random(seed);
        // An EEG-like signal: a slow wave plus noise, around an offset.
        return Enumerable.Range(0, length)
            .Select(i => offset + 50f * (float)Math.Sin(i * 0.01) + (float)(random.NextDouble() * 40 - 20))
            .ToArray();
    }

    private static BtvChannel Channel(float[] data)
    {
        ISampleSource source = new InMemorySampleSource(new[] { data }, new Frequency(512));
        return new BtvChannel("A1", 0, source, 0, ChannelStats.Compute(source, new[] { 0 })[0], new BlockCache(source));
    }

    // --- InMemorySampleSource ----------------------------------------------------------------

    [Test]
    public void ReadRange_FillsEachDestinationWithItsRequestedChannel()
    {
        ISampleSource source = new InMemorySampleSource(new[]
        {
            new float[] { 0, 1, 2, 3 },
            new float[] { 10, 11, 12, 13 },
            new float[] { 20, 21, 22, 23 },
        }, new Frequency(512));
        float[][] dst = { new float[2], new float[2] };

        int read = source.ReadRange(1, 2, new[] { 2, 0 }, dst);

        Assert.AreEqual(2, read);
        CollectionAssert.AreEqual(new float[] { 21, 22 }, dst[0]);
        CollectionAssert.AreEqual(new float[] { 1, 2 }, dst[1]);
    }

    [Test]
    public void ReadRange_ClampsAtTheEndAndReturnsTheCountRead()
    {
        ISampleSource source = new InMemorySampleSource(new[] { new float[] { 0, 1, 2, 3 } }, new Frequency(512));
        float[][] dst = { new float[] { -1, -1, -1 } };

        Assert.AreEqual(1, source.ReadRange(3, 3, new[] { 0 }, dst));
        Assert.AreEqual(3f, dst[0][0]);
        Assert.AreEqual(0, source.ReadRange(4, 3, new[] { 0 }, dst));
        Assert.AreEqual(0, source.ReadRange(100, 3, new[] { 0 }, dst));
        Assert.Throws<ArgumentOutOfRangeException>(() => source.ReadRange(-1, 3, new[] { 0 }, dst));
    }

    [Test]
    public void ReadRange_ShorterChannelReadsZerosPastItsOwnEnd()
    {
        // Only a processed-audio CSV can have ragged rows; GetSample returned 0 past a
        // channel's own end, and the source keeps that.
        ISampleSource source = new InMemorySampleSource(new[]
        {
            new float[] { 1, 2, 3, 4 },
            new float[] { 5, 6 },
        }, new Frequency(64));
        float[][] dst = { new float[] { -1, -1, -1 } };

        Assert.AreEqual(4, source.SampleCount);
        Assert.AreEqual(3, source.ReadRange(1, 3, new[] { 1 }, dst));
        CollectionAssert.AreEqual(new float[] { 6, 0, 0 }, dst[0]);
    }

    // --- ChannelStats ----------------------------------------------------------------------

    [Test]
    public void Stats_MinAndMaxAreExactAcrossBlocks()
    {
        float[] data = Noise(3 * ChannelStats.BlockSize + 17, 1);
        data[3 * ChannelStats.BlockSize + 5] = 1e6f;   // in the last, partial block
        data[ChannelStats.BlockSize] = -1e6f;          // first sample of the second block

        ChannelStats stats = Channel(data).Stats;

        Assert.AreEqual(data.Min(), stats.Min);
        Assert.AreEqual(data.Max(), stats.Max);
    }

    [Test]
    public void Stats_StoredSamplesAreEveryStridethSample()
    {
        float[] data = Noise(100000, 2);

        ChannelStats stats = Channel(data).Stats;

        Assert.AreEqual(13, stats.Stride, "100000 samples need a stride of 13 to keep at most 8192");
        Assert.AreEqual(7693, stats.StoredSamples.Length);
        for (int k = 0; k < stats.StoredSamples.Length; k++)
            Assert.AreEqual(data[k * stats.Stride], stats.StoredSamples[k], "stored sample " + k);
    }

    [Test]
    public void Stats_ShortChannelKeepsEverySampleAndTheExactMedian()
    {
        float[] data = Noise(ChannelStats.StoredSampleCount, 3, offset: 120);

        ChannelStats stats = Channel(data).Stats;

        Assert.AreEqual(1, stats.Stride);
        CollectionAssert.AreEqual(data, stats.StoredSamples);
        Assert.AreEqual(CalculationService.Median(data, data.Length), stats.Median);
    }

    [Test]
    public void Stats_LongChannelMedianIsCloseToTheWholeChannelMedian()
    {
        // The median only centres traces: the stored samples' median may move the centre by a
        // small fraction of the signal's range (the design accepts up to about 1%).
        float[] data = Noise(2000000, 4, offset: -300);

        ChannelStats stats = Channel(data).Stats;
        float wholeMedian = CalculationService.Median(data, data.Length);

        Assert.Less(Math.Abs(stats.Median - wholeMedian), 0.01f * (stats.Max - stats.Min),
            "stored-sample median " + stats.Median + " vs whole-channel median " + wholeMedian);
    }

    [Test]
    public void Stats_EmptyChannelHasZeroExtremes()
    {
        ChannelStats stats = Channel(new float[0]).Stats;

        Assert.AreEqual(0f, stats.Min);
        Assert.AreEqual(0f, stats.Max);
        Assert.AreEqual(0f, stats.Median);
        Assert.AreEqual(0, stats.StoredSamples.Length);
    }

    // --- BtvChannel reads ------------------------------------------------------------------

    [Test]
    public void ReadWindow_MatchesTheOldPerSampleRead()
    {
        // EegSignal used to call GetSample(i + start, centered) for every point: the sample minus
        // the median inside the recording, 0 past its end. One window read must give the same
        // values, including a window that starts before the recording and one that runs past it.
        float[] data = Noise(5000, 5, offset: 40);
        BtvChannel channel = Channel(data);
        float median = channel.Stats.Median;

        foreach (long start in new long[] { -300, 0, 1234, 4800 })
        {
            float[] window = new float[600];
            channel.ReadWindow(start, window.Length, window, true);
            for (int i = 0; i < window.Length; i++)
            {
                long index = start + i;
                float expected = index >= 0 && index < data.Length ? data[index] - median : 0;
                Assert.AreEqual(expected, window[i], "start " + start + ", point " + i);
            }
        }
    }

    [Test]
    public void GetSample_IsZeroOutsideTheRecording()
    {
        BtvChannel channel = Channel(new float[] { 5, 6, 7 });

        Assert.AreEqual(6f, channel.GetSample(1));
        Assert.AreEqual(0f, channel.GetSample(3));
        Assert.AreEqual(0f, channel.GetSample(-1));
    }

    [Test]
    public void MinMax_IsExactOverARangeSpanningBlocks()
    {
        float[] data = Noise(20000, 6);
        BtvChannel channel = Channel(data);

        (float min, float max) = channel.MinMax(1000, 15000);

        float[] range = data.Skip(1000).Take(14000).ToArray();
        Assert.AreEqual(range.Min(), min);
        Assert.AreEqual(range.Max(), max);
    }

    // --- Overview strip --------------------------------------------------------------------

    [Test]
    public void Overview_IsTodaysDecimationWhenTheFactorIsAMultipleOfTheStride()
    {
        // TracesDisplayer used to show GetNormalizedSample(i * factor, true) =
        // (x - median) / (max - min) for i < N / factor.
        float[] data = Noise(100000, 7, offset: 10);
        BtvChannel channel = Channel(data);
        ChannelStats stats = channel.Stats;
        int factor = 5 * stats.Stride;

        Assert.AreEqual(factor, OverviewSampling.Factor(factor, stats.Stride));
        float[] overview = new float[data.Length / factor];
        OverviewSampling.Centred(stats, factor, overview);

        for (int i = 0; i < overview.Length; i++)
            Assert.AreEqual((data[i * factor] - stats.Median) / (data.Max() - data.Min()), overview[i], "point " + i);
    }

    [Test]
    public void Overview_RoundsAnyOtherFactorUpToTheStoredSamples()
    {
        float[] data = Noise(100000, 8);
        ChannelStats stats = Channel(data).Stats;

        int factor = OverviewSampling.Factor(5 * stats.Stride + 1, stats.Stride);
        Assert.AreEqual(6 * stats.Stride, factor);
        float[] overview = new float[data.Length / factor];
        OverviewSampling.Centred(stats, factor, overview);

        for (int i = 0; i < overview.Length; i++)
            Assert.AreEqual((data[i * factor] - stats.Median) / (stats.Max - stats.Min), overview[i], "point " + i);
        Assert.AreEqual(1, OverviewSampling.Factor(0, 1), "a factor is at least 1");
    }

    [Test]
    public void Overview_ShortChannelIsExactlyTodaysStrip()
    {
        // Up to 8192 samples the median is the whole-channel one too, so the strip is identical.
        float[] data = Noise(8000, 9, offset: 25);
        ChannelStats stats = Channel(data).Stats;
        float oldMedian = CalculationService.Median(data, data.Length);
        int factor = OverviewSampling.Factor(3, stats.Stride);

        float[] overview = new float[data.Length / factor];
        OverviewSampling.Centred(stats, factor, overview);

        for (int i = 0; i < overview.Length; i++)
            Assert.AreEqual((data[i * factor] - oldMedian) / (data.Max() - data.Min()), overview[i], "point " + i);
    }

    [Test]
    public void Overview_BaselineNormalizationMatchesTheOldWholeChannelOne()
    {
        // The old path: min/max over [begin, end), normalize the whole channel into a new array,
        // then take every factor-th value as (v - 0.5) * 2.
        float[] data = Noise(100000, 10);
        BtvChannel channel = Channel(data);
        int begin = 20000, end = 31000;
        int factor = 4 * channel.Stats.Stride;

        float oldMin = float.PositiveInfinity, oldMax = float.NegativeInfinity;
        for (int i = begin; i < end; i++) { oldMin = Math.Min(oldMin, data[i]); oldMax = Math.Max(oldMax, data[i]); }
        float[] oldNormalized = data.Select(x => (x - oldMin) / (oldMax - oldMin)).ToArray();

        (float min, float max) = channel.MinMax(begin, end);
        float[] overview = new float[data.Length / factor];
        OverviewSampling.BaselineNormalized(channel.Stats, min, max, factor, overview);

        for (int i = 0; i < overview.Length; i++)
            Assert.AreEqual((oldNormalized[i * factor] - 0.5f) * 2, overview[i], "point " + i);
    }

    // --- Range consumers -------------------------------------------------------------------

    [Test]
    public void Pearson_OnEventSlicesEqualsTheWholeChannelsWithAnOffset()
    {
        // Correlation used to pass whole channels and {begin, duration} to the native Pearson,
        // which copies [begin, begin + duration) of both; it now gets the slices and {0, duration}.
        float[] baseline = Noise(50000, 11);
        float[] other = Noise(50000, 12);
        int begin = 12345, duration = 5120;

        float whole = CalculationService.PearsonCorrelationCoefficients(baseline, other, new[] { begin, duration });

        float[] baselineSlice = new float[duration];
        float[] otherSlice = new float[duration];
        Channel(baseline).ReadWindow(begin, duration, baselineSlice);
        Channel(other).ReadWindow(begin, duration, otherSlice);
        float sliced = CalculationService.PearsonCorrelationCoefficients(baselineSlice, otherSlice, new[] { 0, duration });

        Assert.AreEqual(whole, sliced);
    }

    [Test]
    public void Program_ChannelsShareOneSourceInTheContainersOrder()
    {
        DataContainer container = new DataContainer("/fixtures/rec.TRC");
        container.ValuesByChannel.Add("B2", new float[] { 1, 2, 3 });
        container.ValuesByChannel.Add("A1", new float[] { 4, 5, 6 });
        container.Frequency = new Frequency(512);

        BtvProgram program = new BtvProgram(container, "rec");

        Assert.AreEqual(new[] { "B2", "A1" }, program.Channels.Select(c => c.Label).ToArray());
        Assert.AreSame(program.Channels[0].Source, program.Channels[1].Source);
        Assert.AreEqual(1, program.Channels[1].SourceChannel);
        Assert.AreEqual(5f, program.Channels[1].GetSample(1));
        Assert.AreEqual(3, program.NumberOfSample);
        Assert.AreEqual(6f, program.Channels[1].MaxValue);
    }
}
