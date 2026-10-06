using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BTV.Data;
using NUnit.Framework;
using Tools.CSharp.EEG;
using EegFile = Tools.CSharp.EEG.File;

/// <summary>
/// Edit-mode tests for the channel-by-channel EEG load. IEegDataContainer used to copy every
/// channel to managed arrays (File.Electrodes) before freeing the native ones, so both copies
/// coexisted: twice the float size of the file (7.8 GiB for a 2 GB, 183-channel TRC). It now
/// copies one channel, frees it natively, then the next. These tests run the real native reader on a
/// synthesized EDF and pin what that reordering could break: labels/units read before their
/// native electrode is freed, channel order, and per-channel samples, against the old
/// all-at-once copy.
/// </summary>
public class EegChannelHandoffTests
{
    private string m_TempDir;

    [SetUp]
    public void SetUp()
    {
        m_TempDir = Path.Combine(Path.GetTempPath(), "btv-eeghandoff-tests-" + Path.GetRandomFileName());
        Directory.CreateDirectory(m_TempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(m_TempDir))
            Directory.Delete(m_TempDir, true);
    }

    [Test]
    public void Container_MovesEveryChannelInOrderWithItsSamples()
    {
        string[] labels = { "A1", "A2", "B1" };
        string path = WriteEdf(labels, 2, 50);

        IEegDataContainer container = new IEegDataContainer(new EdfFileInfo(path));

        CollectionAssert.AreEqual(labels, container.ValuesByChannel.Keys);
        for (int channel = 0; channel < labels.Length; channel++)
        {
            float[] data = container.ValuesByChannel[labels[channel]];
            Assert.AreEqual(100, data.Length, labels[channel]);
            for (int sample = 0; sample < data.Length; sample++)
                // The native digital-to-physical scaling rounds to ~0.02; neighbouring samples
                // differ by 1 and channels by 1000, so a shift or swap still fails.
                Assert.AreEqual(SampleValue(channel, sample), data[sample], 0.5, labels[channel] + "[" + sample + "]");
        }
        Assert.AreEqual(50, container.Frequency.RawValue, 1e-3);
    }

    [Test]
    public void Container_MatchesTheAllAtOnceCopyExactly()
    {
        string path = WriteEdf(new[] { "A1", "A2", "B1", "B2" }, 3, 40);

        IEegDataContainer container = new IEegDataContainer(new EdfFileInfo(path));

        using (EegFile reference = new EegFile(EegFile.FileType.EDF, true, path))
        {
            List<Electrode> electrodes = reference.Electrodes;
            CollectionAssert.AreEqual(electrodes.Select(e => e.Label).ToList(), container.ValuesByChannel.Keys);
            foreach (Electrode electrode in electrodes)
            {
                Assert.AreEqual(electrode.Unit, container.UnitByChannel[electrode.Label], electrode.Label);
                CollectionAssert.AreEqual(electrode.Data, container.ValuesByChannel[electrode.Label], electrode.Label);
            }
        }
    }

    [Test]
    public void Container_DuplicateLabels_StillRejectedBeforeAnyCopy()
    {
        string path = WriteEdf(new[] { "A1", "A2", "A1" }, 1, 10);

        var e = Assert.Throws<InvalidDataException>(() => new IEegDataContainer(new EdfFileInfo(path)));
        StringAssert.Contains("A1", e.Message);
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
