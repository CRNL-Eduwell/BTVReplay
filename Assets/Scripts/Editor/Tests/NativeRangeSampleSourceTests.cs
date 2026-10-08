using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BTV.Data;
using BTV.Services;
using BTV.Services.EegFileService;
using NUnit.Framework;
using Tools.CSharp.EEG;
using UnityEngine;
using UnityEngine.TestTools;
using EegFile = Tools.CSharp.EEG.File;

/// <summary>
/// Edit-mode tests for the windowed EEG source (design phase P3). An EEG file used to be loaded
/// whole into managed arrays (IEegDataContainer.ValuesByChannel); it is now opened header-only
/// and read by range from disk through EEGFormat's EEGF_ReadRange. These run the real native
/// reader on synthesized EDFs and pin that the source reads exactly what the whole-file path
/// reads, keeps the metadata, owns and closes its file, and reports a file gone from disk.
/// </summary>
public class NativeRangeSampleSourceTests
{
    private string m_TempDir;

    [SetUp]
    public void SetUp()
    {
        m_TempDir = Path.Combine(Path.GetTempPath(), "btv-nativesource-tests-" + Path.GetRandomFileName());
        Directory.CreateDirectory(m_TempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(m_TempDir))
            Directory.Delete(m_TempDir, true);
    }

    [Test]
    public void Container_KeepsTheMetadataAndLeavesTheSamplesOnDisk()
    {
        string[] labels = { "A1", "A2", "B1" };
        string path = WriteEdf(labels, 2, 50);

        IEegDataContainer container = new IEegDataContainer(new EdfFileInfo(path));
        try
        {
            CollectionAssert.AreEqual(labels, container.Labels);
            CollectionAssert.AreEqual(labels.Select(_ => "uV"), labels.Select(l => container.UnitByChannel[l]));
            Assert.AreEqual(50, container.Frequency.RawValue, 1e-3);
            Assert.IsEmpty(container.ValuesByChannel, "no samples copied into managed memory");
            Assert.IsInstanceOf<NativeRangeSampleSource>(container.Source);
            Assert.AreEqual(100, container.Source.SampleCount);
            Assert.AreEqual(3, container.Source.ChannelCount);
        }
        finally
        {
            container.Source.Dispose();
        }
    }

    [Test]
    public void Source_ReadsExactlyWhatTheWholeFilePathReads()
    {
        string path = WriteEdf(new[] { "A1", "A2", "B1", "B2" }, 7, 40);
        List<float[]> reference;
        using (EegFile whole = new EegFile(EegFile.FileType.EDF, true, path))
            reference = whole.Electrodes.Select(e => e.Data).ToList();

        using (NativeRangeSampleSource source = new NativeRangeSampleSource(EegFile.FileType.EDF, path))
        {
            int n = (int)source.SampleCount;
            // All channels at once, one channel, a scrambled subset; ranges across records and
            // clamped at the end.
            foreach ((long first, int count) in new[] { (0L, n), (0L, 1), (39L, 2), (77L, 150), ((long)n - 5, 40) })
            {
                foreach (int[] channels in new[] { new[] { 0, 1, 2, 3 }, new[] { 2 }, new[] { 3, 0, 2 } })
                {
                    float[][] dst = channels.Select(_ => new float[count]).ToArray();
                    int read = source.ReadRange(first, count, channels, dst);
                    Assert.AreEqual(Math.Min(count, n - first), read);
                    for (int k = 0; k < channels.Length; k++)
                        CollectionAssert.AreEqual(reference[channels[k]].Skip((int)first).Take(read), dst[k].Take(read),
                            "channel " + channels[k] + " from " + first);
                }
            }
            Assert.AreEqual(0, source.ReadRange(n, 10, new[] { 0 }, new[] { new float[10] }), "past the end");
        }
    }

    [Test]
    public void Program_ReadsTheFileAndOwnsTheSource()
    {
        string[] labels = { "A1", "A2" };
        string path = WriteEdf(labels, 3, 64);
        IEegDataContainer container = new IEegDataContainer(new EdfFileInfo(path));
        BtvProgram program = new BtvProgram(container.Source, container.Labels, container, "rec");
        try
        {
            Assert.AreSame(container.Source, program.OwnedSource);
            Assert.AreEqual(192, program.NumberOfSample);
            for (int c = 0; c < labels.Length; c++)
            {
                BtvChannel channel = program.Channels[c];
                float[] samples = new float[channel.NumberOfSample];
                channel.ReadWindow(0, samples.Length, samples);
                CollectionAssert.AreEqual(Enumerable.Range(0, 192).Select(i => (float)SampleValue(c, i)), samples, labels[c]);
                Assert.AreEqual(samples.Min(), channel.Stats.Min);
                Assert.AreEqual(samples.Max(), channel.Stats.Max);
            }
            Assert.IsNull(new BtvProgram(program, program.Channels).OwnedSource, "a montage shares the source without owning it");
        }
        finally
        {
            program.OwnedSource.Dispose();
        }
    }

    [Test]
    public void Container_DuplicateLabels_StillRejected()
    {
        string path = WriteEdf(new[] { "A1", "A2", "A1" }, 1, 10);

        var e = Assert.Throws<InvalidDataException>(() => new IEegDataContainer(new EdfFileInfo(path)));
        StringAssert.Contains("A1", e.Message);
    }

    [Test]
    public void AFileGoneFromDisk_IsAnIOError()
    {
        // The file is opened for reading on the first read: a share that went away, a file
        // moved after the patient was loaded.
        string path = WriteEdf(new[] { "A1", "A2" }, 2, 50);
        using (NativeRangeSampleSource source = new NativeRangeSampleSource(EegFile.FileType.EDF, path))
        {
            System.IO.File.Delete(path);
            Assert.Throws<IOException>(() => source.ReadRange(0, 10, new[] { 0 }, new[] { new float[10] }));
        }
    }

    [Test]
    public void ADisposedSource_ThrowsObjectDisposed_AndTheCacheStaysQuiet()
    {
        string path = WriteEdf(new[] { "A1" }, 2, 50);
        NativeRangeSampleSource source = new NativeRangeSampleSource(EegFile.FileType.EDF, path);
        source.Dispose();

        Assert.Throws<ObjectDisposedException>(() => source.ReadRange(0, 10, new[] { 0 }, new[] { new float[10] }));

        // What the window cache's in-flight reads meet after a patient switch: no warning.
        SynchronizationContext previous = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        try
        {
            BlockCache cache = new BlockCache(source, () => 1, work => { work(); return Task.CompletedTask; });
            Assert.IsFalse(cache.TryReadWindow(0, 0, 10, new float[10]));
            Assert.AreEqual(0, cache.LoadedBlockCount);
            LogAssert.NoUnexpectedReceived();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
        }
    }

    private sealed class CountingSource : ISampleSource
    {
        public int Disposals;
        public int ChannelCount => 1;
        public long SampleCount => 4;
        public Frequency Frequency { get; } = new Frequency(512);
        public int ReadRange(long first, int count, int[] channels, float[][] dst)
        {
            int read = (int)Math.Max(0, Math.Min(count, SampleCount - first));
            Array.Clear(dst[0], 0, read);
            return read;
        }
        public void Dispose() { Disposals++; }
    }

    [Test]
    public void SessionDispose_ClosesEachOwnedFileOnce()
    {
        Session.ReplaceCurrent();
        CountingSource source = new CountingSource();
        DataContainer metadata = new DataContainer("/fixtures/rec.TRC") { Frequency = new Frequency(512) };
        BtvProgram program = new BtvProgram(source, new List<string> { "A1" }, metadata, "rec");
        EegFileService.Montages[0].SetEEGFile(program, 0);
        EegFileService.Montages[0].SetEEGFile(new BtvProgram(program, program.Channels), 1);   // a montage view

        Session.ReplaceCurrent();   // patient switch

        Assert.AreEqual(1, source.Disposals);
    }

    // Distinct per channel and sample, so a shifted or swapped channel fails the comparison.
    private static short SampleValue(int channel, int sample)
    {
        return (short)((channel + 1) * 1000 + sample);
    }

    /// <summary>
    /// Minimal plain EDF: full-range physical = digital scaling, so samples read back as is.
    /// </summary>
    private string WriteEdf(string[] labels, int records, int samplesPerRecord)
    {
        int signals = labels.Length;
        StringBuilder header = new StringBuilder();
        header.Append(Field("0", 8)).Append(Field("X X X X", 80)).Append(Field("Startdate X X X X", 80));
        header.Append(Field("06.10.26", 8)).Append(Field("12.00.00", 8));
        header.Append(Field((256 + signals * 256).ToString(), 8)).Append(Field("", 44));
        header.Append(Field(records.ToString(), 8)).Append(Field("1", 8)).Append(Field(signals.ToString(), 4));
        foreach (string label in labels) header.Append(Field(label, 16));
        for (int i = 0; i < signals; i++) header.Append(Field("", 80));
        for (int i = 0; i < signals; i++) header.Append(Field("uV", 8));
        for (int i = 0; i < signals; i++) header.Append(Field("-32768", 8));
        for (int i = 0; i < signals; i++) header.Append(Field("32767", 8));
        for (int i = 0; i < signals; i++) header.Append(Field("-32768", 8));
        for (int i = 0; i < signals; i++) header.Append(Field("32767", 8));
        for (int i = 0; i < signals; i++) header.Append(Field("", 80));
        for (int i = 0; i < signals; i++) header.Append(Field(samplesPerRecord.ToString(), 8));
        for (int i = 0; i < signals; i++) header.Append(Field("", 32));

        string path = Path.Combine(m_TempDir, "rec.edf");
        using (BinaryWriter writer = new BinaryWriter(System.IO.File.Create(path)))
        {
            writer.Write(Encoding.ASCII.GetBytes(header.ToString()));
            for (int record = 0; record < records; record++)
                for (int channel = 0; channel < signals; channel++)
                    for (int i = 0; i < samplesPerRecord; i++)
                        writer.Write(SampleValue(channel, record * samplesPerRecord + i));
        }
        return path;
    }

    private static string Field(string value, int width)
    {
        return value.PadRight(width).Substring(0, width);
    }
}
