using System;
using System.Collections.Generic;
using System.Linq;
using BTV.Data;
using BTV.Services.EegFileService;
using NUnit.Framework;

/// <summary>
/// Edit-mode tests for montage generation. A montage used to deep-copy every loaded file and
/// overwrite the copy, so each montage cost the full size of the recordings (3.9 GiB for a 6 h,
/// 183-channel TRC) even for one changed channel, and the evaluated channels kept the base
/// channel's median/min/max. These pin the sharing of unchanged data and the statistics of
/// evaluated channels.
/// </summary>
public class MontageGenerationTests
{
    private sealed class SyncProgress : IProgress<(float progress, string message)>
    {
        public void Report((float progress, string message) value) { }
    }

    private static BtvProgram Program(string description, params (string label, float[] data)[] channels)
    {
        DataContainer container = new DataContainer("/fixtures/" + description + ".TRC");
        foreach (var channel in channels)
            container.ValuesByChannel.Add(channel.label, channel.data);
        container.Frequency = new Tools.CSharp.EEG.Frequency(512);
        return new BtvProgram(container, description);
    }

    private static (BtvProgram[] files, string errors) Generate(BtvProgram[] baseFiles, string fileName, params (string from, string to)[] description)
    {
        List<ChannelCorrespondance> montage = description.Select(d => new ChannelCorrespondance(d.from, d.to)).ToList();
        return EegFileService.GenerateMontage(baseFiles, montage, fileName, new SyncProgress());
    }

    private static float[] Samples(BtvChannel channel)
    {
        float[] samples = new float[channel.NumberOfSample];
        channel.ReadWindow(0, samples.Length, samples);
        return samples;
    }

    private static BtvProgram[] Slots(params BtvProgram[] files)
    {
        BtvProgram[] slots = new BtvProgram[EegSlots.Count];
        Array.Copy(files, slots, files.Length);
        return slots;
    }

    [Test]
    public void UnmappedChannels_AreSharedWithTheBaseFile()
    {
        BtvProgram baseFile = Program("rec", ("A1", new float[] { 1, 2, 3 }), ("A2", new float[] { 4, 5, 6 }));

        var (files, errors) = Generate(Slots(baseFile), "", ("A1", "A1 - A2"));

        Assert.AreEqual("", errors);
        Assert.AreSame(baseFile.Channels[1], files[0].Channels[1], "A2 is unmapped: no copy");
        Assert.AreNotSame(baseFile.Channels[0].Source, files[0].Channels[0].Source, "A1 is evaluated into its own source");
    }

    [Test]
    public void EvaluatedChannel_HasMontageValuesAndLeavesTheBaseUntouched()
    {
        BtvProgram baseFile = Program("rec", ("A1", new float[] { 10, 20, 30, 40 }), ("A2", new float[] { 1, 2, 3, 4 }));

        var (files, _) = Generate(Slots(baseFile), "", ("A1", "A1 - A2"));

        CollectionAssert.AreEqual(new float[] { 9, 18, 27, 36 }, Samples(files[0].Channels[0]));
        CollectionAssert.AreEqual(new float[] { 10, 20, 30, 40 }, Samples(baseFile.Channels[0]));
        Assert.AreEqual("A1", files[0].Channels[0].Label);
        Assert.AreEqual(0, files[0].Channels[0].ID);
    }

    [Test]
    public void EvaluatedChannel_StatisticsComeFromTheMontageValues()
    {
        // The base channel's statistics (max |x| = 1000) used to be kept for the bipolar result
        // (max |x| = 4), so its trace was scaled as if it were 250 times larger.
        BtvProgram baseFile = Program("rec", ("A1", new float[] { 1000, 1001, 1002, 1004 }), ("A2", new float[] { 1000, 1000, 1000, 1000 }));

        var (files, _) = Generate(Slots(baseFile), "", ("A1", "A1 - A2"));

        Assert.AreEqual(4f, files[0].Channels[0].MaxValue);
        Assert.AreEqual(1004f, baseFile.Channels[0].MaxValue);
    }

    [Test]
    public void RenamedChannel_SharesTheSourceSamplesUnderItsOwnLabel()
    {
        BtvProgram baseFile = Program("rec", ("A1", new float[] { 1, 2, 3 }), ("A2", new float[] { 7, 8, 9 }));

        var (files, _) = Generate(Slots(baseFile), "", ("A1", "A2"));

        BtvChannel renamed = files[0].Channels[0];
        Assert.AreSame(baseFile.Channels[1].Source, renamed.Source);
        Assert.AreEqual(baseFile.Channels[1].SourceChannel, renamed.SourceChannel);
        Assert.AreSame(baseFile.Channels[1].Stats, renamed.Stats);
        Assert.AreEqual("A1", renamed.Label);
        Assert.AreEqual(0, renamed.ID);
        Assert.AreEqual(baseFile.Channels[1].MaxValue, renamed.MaxValue);
    }

    [Test]
    public void ChannelMappedToItself_IsTheBaseChannel()
    {
        BtvProgram baseFile = Program("rec", ("A'1", new float[] { 1, 2, 3 }));

        var (files, _) = Generate(Slots(baseFile), "", ("A'1", "A'1"));

        Assert.AreSame(baseFile.Channels[0], files[0].Channels[0]);
    }

    [Test]
    public void OtherFiles_AreSharedWhenTheMontageTargetsOneFile()
    {
        BtvProgram target = Program("seizure", ("A1", new float[] { 1, 2 }), ("A2", new float[] { 1, 1 }));
        BtvProgram other = Program("baseline", ("A1", new float[] { 5, 6 }), ("A2", new float[] { 1, 1 }));

        var (files, _) = Generate(Slots(target, other), "seizure", ("A1", "A1 - A2"));

        CollectionAssert.AreEqual(new float[] { 0, 1 }, Samples(files[0].Channels[0]));
        Assert.AreNotSame(other, files[1]);
        Assert.AreSame(other.Channels[0], files[1].Channels[0]);
        Assert.AreSame(other.Channels[1], files[1].Channels[1]);
    }

    [Test]
    public void BadExpression_KeepsTheBaseChannelAndReportsIt()
    {
        BtvProgram baseFile = Program("rec", ("A1", new float[] { 1, 2 }), ("A2", new float[] { 3, 4 }));

        var (files, errors) = Generate(Slots(baseFile), "", ("A1", "A1 - Z9"));

        Assert.AreSame(baseFile.Channels[0], files[0].Channels[0]);
        StringAssert.Contains("Z9", errors);
    }

    [Test]
    public void LabelTheParserRejects_NoLongerAbortsTheMontage()
    {
        // The fallback used to re-parse the channel's own label; "1A" starts with a digit and
        // threw out of GenerateMontage.
        BtvProgram baseFile = Program("rec", ("1A", new float[] { 1, 2 }), ("A2", new float[] { 3, 4 }));

        var (files, errors) = Generate(Slots(baseFile), "", ("1A", "1A - A2"), ("A2", "A2 * 2"));

        Assert.AreSame(baseFile.Channels[0], files[0].Channels[0]);
        CollectionAssert.AreEqual(new float[] { 6, 8 }, Samples(files[0].Channels[1]));
        StringAssert.Contains("1A", errors);
    }

    [Test]
    public void EvaluatedChannel_ReadInBlocksEqualsTheWholeEvaluation()
    {
        // A montage channel is evaluated on the range being read. Read block by block, as the
        // window cache does (the last block partial), it must equal the expression evaluated
        // over the whole recording, which is what montages used to store.
        int length = 3 * BlockCache.BlockSize + 123;
        System.Random random = new System.Random(7);
        float[] a1 = Enumerable.Range(0, length).Select(_ => (float)random.NextDouble() * 200 - 100).ToArray();
        float[] a2 = Enumerable.Range(0, length).Select(_ => (float)random.NextDouble() * 200 - 100).ToArray();
        BtvProgram baseFile = Program("rec", ("A1", a1), ("A2", a2));

        var (files, errors) = Generate(Slots(baseFile), "", ("A1", "A1 - A2 * 2"));

        Assert.AreEqual("", errors);
        BtvChannel montage = files[0].Channels[0];
        Assert.IsInstanceOf<DerivedSampleSource>(montage.Source, "evaluated lazily, not stored");
        float[] expected = Enumerable.Range(0, length).Select(i => (float)(a1[i] - a2[i] * 2.0)).ToArray();
        CollectionAssert.AreEqual(expected, Samples(montage), "one read of the whole recording");
        float[] blockwise = new float[length];
        float[] block = new float[BlockCache.BlockSize];
        for (int first = 0; first < length; first += block.Length)
        {
            int count = Math.Min(block.Length, length - first);
            montage.ReadWindow(first, count, block);
            Array.Copy(block, 0, blockwise, first, count);
        }
        CollectionAssert.AreEqual(expected, blockwise, "block by block");
    }

    [Test]
    public void EvaluatedChannel_StatisticsComeFromTheStoredSamples()
    {
        // No pass over the recording when a montage is built: the expression is evaluated on the
        // base channels' 8192 stored samples. The median uses them as for any channel; min and max
        // are estimated from them (they can miss a short spike between two stored samples).
        int length = 100000;
        System.Random random = new System.Random(11);
        float[] a1 = Enumerable.Range(0, length).Select(_ => (float)random.NextDouble() * 200 - 100).ToArray();
        float[] a2 = Enumerable.Range(0, length).Select(_ => (float)random.NextDouble() * 200 - 100).ToArray();
        BtvProgram baseFile = Program("rec", ("A1", a1), ("A2", a2));

        var (files, _) = Generate(Slots(baseFile), "", ("A1", "A1 - A2"));

        ChannelStats stats = files[0].Channels[0].Stats;
        int stride = ChannelStats.StrideFor(length);
        float[] stored = Enumerable.Range(0, (length + stride - 1) / stride).Select(k => (float)(a1[k * stride] - (double)a2[k * stride])).ToArray();
        Assert.AreEqual(stride, stats.Stride);
        CollectionAssert.AreEqual(stored, stats.StoredSamples);
        Assert.AreEqual(stored.Min(), stats.Min);
        Assert.AreEqual(stored.Max(), stats.Max);
        Assert.AreEqual(BTV.Services.CalculationService.CalculationService.Median(stored, stored.Length), stats.Median);
    }

    [Test]
    public void MontageExpressions_ShareOneSourcePerFile()
    {
        BtvProgram baseFile = Program("rec", ("A1", new float[] { 1, 2 }), ("A2", new float[] { 3, 4 }), ("A3", new float[] { 5, 7 }));

        var (files, _) = Generate(Slots(baseFile), "", ("A1", "A1 - A2"), ("A3", "A3 - A2"));

        BtvChannel a1 = files[0].Channels[0], a3 = files[0].Channels[2];
        Assert.AreSame(a1.Source, a3.Source, "one multiplexed source per montage file");
        Assert.AreSame(a1.Cache, a3.Cache);
        Assert.AreEqual(new[] { 0, 1 }, new[] { a1.SourceChannel, a3.SourceChannel });
        CollectionAssert.AreEqual(new float[] { 2, 3 }, Samples(a3));
        Assert.AreSame(baseFile.Channels[1], files[0].Channels[1], "A2 is unmapped: still the base channel");
    }

    [Test]
    public void Montage_CopiesTheEventsSoEachMontageEditsItsOwn()
    {
        BtvProgram baseFile = Program("rec", ("A1", new float[] { 1, 2 }));

        var (files, _) = Generate(Slots(baseFile), "", ("A1", "A1 * 2"));

        Assert.AreNotSame(baseFile.Events, files[0].Events);
        Assert.AreNotSame(baseFile.Channels, files[0].Channels);
    }
}
