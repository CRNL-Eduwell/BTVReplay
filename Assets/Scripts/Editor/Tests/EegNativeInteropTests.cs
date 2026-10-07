using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;
using Tools.CSharp.EEG;
using Tools.DLL;
using UnityEngine;
using EegFile = Tools.CSharp.EEG.File;

/// <summary>
/// Edit-mode tests for the EEGFormat P/Invoke signatures of notes and triggers. The wrapper declares
/// <c>long GetNoteSample(INote*)</c> and <c>long GetTriggerSample(ITrigger*)</c>; C long is 32 bits
/// on Windows and 64 bits on macOS/Linux, but both used to be marshalled as a 64-bit C# long, which
/// on Windows x64 reads the upper half of RAX that the native side never set. Note descriptions used
/// PtrToStringAnsi although the wrapper hands out UTF-8. These tests pin the width rule, the twin
/// externs, the UTF-8 decoding, and run the real reader on a synthesized EDF+ file.
/// </summary>
public class EegNativeInteropTests
{
    private string m_TempDir;

    [SetUp]
    public void SetUp()
    {
        m_TempDir = Path.Combine(Path.GetTempPath(), "btv-eeginterop-tests-" + Path.GetRandomFileName());
        Directory.CreateDirectory(m_TempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(m_TempDir))
            Directory.Delete(m_TempDir, true);
    }

    [TestCase(true, 8, 4, TestName = "CLong_Windows64_Is32Bit")]
    [TestCase(true, 4, 4, TestName = "CLong_Windows32_Is32Bit")]
    [TestCase(false, 8, 8, TestName = "CLong_Lp64_Is64Bit")]
    [TestCase(false, 4, 4, TestName = "CLong_Ilp32_Is32Bit")]
    public void CLongSize_FollowsThePlatformDataModel(bool isWindows, int pointerSize, int expected)
    {
        Assert.AreEqual(expected, NativeCLong.SizeInBytes(isWindows, pointerSize));
    }

    [Test]
    public void CLong_Is32Bit_MatchesTheRunningProcess()
    {
        bool expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || IntPtr.Size == 4;
        Assert.AreEqual(expected, NativeCLong.Is32Bit);
    }

    // A later "cleanup" collapsing the twins back into one long extern would bring the mismatch back.
    [TestCase(typeof(Note), "GetNoteSample")]
    [TestCase(typeof(Trigger), "GetTriggerSample")]
    public void CLongEntryPoints_HaveOneIntAndOneLongExtern(Type type, string entryPoint)
    {
        List<Type> returnTypes = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<DllImportAttribute>()?.EntryPoint == entryPoint)
            .Select(m => m.ReturnType)
            .OrderBy(t => t.Name)
            .ToList();

        CollectionAssert.AreEqual(new[] { typeof(int), typeof(long) }, returnTypes);
    }

    [Test]
    public void NoteDescription_DecodesTheWrapperUtf8()
    {
        string source = System.IO.File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/Data/Files/EEG/Note.cs"));

        StringAssert.Contains("Marshal.PtrToStringUTF8(GetNoteDescription(", source);
        StringAssert.DoesNotContain("Marshal.PtrToStringAnsi(", source);
    }

    [Test]
    public void Utf8Decoding_KeepsAccentedText()
    {
        // What the switch buys on Windows: the wrapper's UTF-8 bytes for "é" read back as "é", not "Ã©".
        byte[] bytes = Encoding.UTF8.GetBytes("Crise é\0");
        IntPtr buffer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, buffer, bytes.Length);
            Assert.AreEqual("Crise é", Marshal.PtrToStringUTF8(buffer));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [Test]
    public void EdfPlusAnnotations_ReadBackWithTheirSamples()
    {
        // ASCII only: the shipped EEGFormat (dbe3394) drops annotation bytes >= 0x80 before the wrapper.
        string path = WriteEdfPlus(50, new[]
        {
            "+0\u0014\u0014\0+0.4\u00147\u0014\0",
            "+1\u0014\u0014\0+1.5\u0014Event\u0014\0",
        });

        using (EegFile file = new EegFile(EegFile.FileType.EDF, true, path))
        {
            List<Trigger> triggers = file.Triggers;
            Assert.AreEqual(1, triggers.Count);
            Assert.AreEqual(7, triggers[0].Code);
            Assert.AreEqual(20L, triggers[0].Sample);

            List<Note> notes = file.Notes;
            Assert.AreEqual(1, notes.Count);
            Assert.AreEqual("Event", notes[0].Description);
            Assert.AreEqual(75L, notes[0].Sample);
        }
    }

    /// <summary>
    /// Minimal EDF+C: two 1-second channels then the annotations signal, stored last (the layout the
    /// shipped reader handles), one record per entry of <paramref name="tals"/>.
    /// </summary>
    private string WriteEdfPlus(int samplingFrequency, string[] tals)
    {
        string[] labels = { "A1", "A2", "EDF Annotations" };
        const int annotationSamples = 30;
        int[] samplesPerRecord = { samplingFrequency, samplingFrequency, annotationSamples };
        int signals = labels.Length;

        StringBuilder header = new StringBuilder();
        header.Append(Field("0", 8)).Append(Field("X X X X", 80)).Append(Field("Startdate X X X X", 80));
        header.Append(Field("06.10.26", 8)).Append(Field("12.00.00", 8));
        header.Append(Field((256 + signals * 256).ToString(), 8)).Append(Field("EDF+C", 44));
        header.Append(Field(tals.Length.ToString(), 8)).Append(Field("1", 8)).Append(Field(signals.ToString(), 4));
        foreach (string label in labels) header.Append(Field(label, 16));
        for (int i = 0; i < signals; i++) header.Append(Field("", 80));
        for (int i = 0; i < signals; i++) header.Append(Field(i < 2 ? "uV" : "", 8));
        for (int i = 0; i < signals; i++) header.Append(Field("-32768", 8));
        for (int i = 0; i < signals; i++) header.Append(Field("32767", 8));
        for (int i = 0; i < signals; i++) header.Append(Field("-32768", 8));
        for (int i = 0; i < signals; i++) header.Append(Field("32767", 8));
        for (int i = 0; i < signals; i++) header.Append(Field("", 80));
        for (int i = 0; i < signals; i++) header.Append(Field(samplesPerRecord[i].ToString(), 8));
        for (int i = 0; i < signals; i++) header.Append(Field("", 32));

        string path = Path.Combine(m_TempDir, "annotated.edf");
        using (BinaryWriter writer = new BinaryWriter(System.IO.File.Create(path)))
        {
            writer.Write(Encoding.ASCII.GetBytes(header.ToString()));
            foreach (string tal in tals)
            {
                for (int channel = 0; channel < 2; channel++)
                    for (int i = 0; i < samplingFrequency; i++)
                        writer.Write((short)0);
                byte[] annotations = new byte[annotationSamples * 2];
                Encoding.ASCII.GetBytes(tal).CopyTo(annotations, 0);
                writer.Write(annotations);
            }
        }
        return path;
    }

    private static string Field(string value, int width)
    {
        return value.PadRight(width).Substring(0, width);
    }
}
