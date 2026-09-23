using System.Collections.Generic;
using System.IO;
using BTV.Data;
using NUnit.Framework;
using EegFile = Tools.CSharp.EEG.File;

/// <summary>
/// Edit-mode tests for EEG files the native reader cannot open (review B-7). The EEGFormat
/// Create* functions return null on failure; File used to wrap that null and hand it to every
/// later native call, which dereferenced it and took the whole app down. The native tests call
/// the real platform plugin: before the fix the constructor silently succeeded with a null
/// handle (these tests fail), and the first getter on it would have crashed the process.
/// </summary>
public class EegFileLoadFailureTests
{
    private string m_TempDir;

    [SetUp]
    public void SetUp()
    {
        m_TempDir = Path.Combine(Path.GetTempPath(), "btv-eegfile-tests-" + Path.GetRandomFileName());
        Directory.CreateDirectory(m_TempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(m_TempDir))
            Directory.Delete(m_TempDir, true);
    }

    [Test]
    public void NativeOpen_MissingFile_ThrowsFileLoadExceptionNamingIt()
    {
        string path = Path.Combine(m_TempDir, "missing.TRC");

        FileLoadException e = Assert.Throws<FileLoadException>(() => new EegFile(EegFile.FileType.Micromed, true, path));
        Assert.AreEqual(path, e.FileName);
    }

    [Test]
    public void NativeOpen_GarbageFile_ThrowsFileLoadException()
    {
        string path = Path.Combine(m_TempDir, "garbage.TRC");
        File.WriteAllBytes(path, new byte[] { 0x42, 0x54, 0x56, 0x00, 0xFF, 0x13, 0x37 });

        Assert.Throws<FileLoadException>(() => new EegFile(EegFile.FileType.Micromed, true, path));
    }

    [Test]
    public void Validate_NoSamples_Throws()
    {
        var e = Assert.Throws<InvalidDataException>(() => IEegDataContainer.Validate(new List<string> { "A1", "A2" }, 0, "rec.TRC"));
        StringAssert.Contains("no samples", e.Message);
    }

    [Test]
    public void Validate_NoChannels_Throws()
    {
        var e = Assert.Throws<InvalidDataException>(() => IEegDataContainer.Validate(new List<string>(), 2048, "rec.TRC"));
        StringAssert.Contains("no channels", e.Message);
    }

    [Test]
    public void Validate_DuplicateLabels_ThrowsListingThem()
    {
        var e = Assert.Throws<InvalidDataException>(() =>
            IEegDataContainer.Validate(new List<string> { "A1", "B2", "A1", "C3", "B2" }, 2048, "rec.TRC"));
        StringAssert.Contains("A1", e.Message);
        StringAssert.Contains("B2", e.Message);
        StringAssert.DoesNotContain("C3", e.Message);
    }

    [Test]
    public void Validate_WellFormedFile_Passes()
    {
        Assert.DoesNotThrow(() => IEegDataContainer.Validate(new List<string> { "A1", "A2" }, 2048, "rec.TRC"));
    }
}
