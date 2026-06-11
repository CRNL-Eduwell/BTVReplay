using Assets.Scripts.Data.Factory;
using BTV.Data;
using BTV.Services.DatabaseService;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Edit-mode tests for the patient-database persistence layer, covering the historical
/// data-loss bugs: the .txt migration chain overwriting its source, backups truncating the
/// main file, and the JSON converters zeroing stored values.
/// </summary>
public class DataSafetyTests
{
    private string m_TestDir;

    [SetUp]
    public void SetUp()
    {
        m_TestDir = Path.Combine(Path.GetTempPath(), "BTVReplayTests_" + Path.GetRandomFileName().Replace(".", ""));
        Directory.CreateDirectory(m_TestDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(m_TestDir)) Directory.Delete(m_TestDir, true);
    }

    #region Fixtures
    private string WriteV1Base(string name, int patientCount)
    {
        string path = Path.Combine(m_TestDir, name + ".txt");
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < patientCount; i++)
        {
            sb.Append("LH_MNI : /fake/lhemi.tri\n");
            sb.Append("RH_MNI : /fake/rhemi.tri\n");
            sb.Append("PTS_MNI : /fake/patient" + i + ".pts\n");
            sb.Append("MESH_MNI : Left/Right Mesh\n");
            sb.Append("EEG_MNI : Intracranial EEG\n");
            sb.Append("LH_PAT : \n");
            sb.Append("RH_PAT : \n");
            sb.Append("PTS_PAT : \n");
            sb.Append("ATLAS_PAT : \n");
            sb.Append("MESH_PAT : Left/Right Mesh\n");
            sb.Append("EEG_PAT : Intracranial EEG\n");
            sb.Append("SM0 : \n");
            sb.Append("SM250 : \n");
            sb.Append("SM500 : \n");
            sb.Append("SM1000 : \n");
            sb.Append("SM2500 : \n");
            sb.Append("SM5000 : \n");
            sb.Append("POS : \n");
            sb.Append("PROV : \n");
            sb.Append("VID : \n");
            sb.Append("[----------]\n");
        }
        File.WriteAllText(path, sb.ToString());
        return path;
    }

    private string WriteV3Base(string name, params string[] patientNames)
    {
        string path = Path.Combine(m_TestDir, name + ".dbtv2");
        System.Collections.Generic.List<Subject> subjects = new System.Collections.Generic.List<Subject>();
        foreach (string patientName in patientNames)
            subjects.Add(new Subject { PatientName = patientName });
        Assert.IsTrue(DBFile3.Save(path, subjects), "fixture save failed");
        return path;
    }
    #endregion

    #region Migration chain
    [Test]
    public void Migration_FromV1_CreatesNewFilesAndPreservesOriginal()
    {
        string txtPath = WriteV1Base("base", 2);
        string originalContent = File.ReadAllText(txtPath);

        ISubjectsContext context = SubjectsFactory.GetSubjectsContext(txtPath);

        Assert.AreEqual(originalContent, File.ReadAllText(txtPath), "the source .txt file must never be modified by the migration");
        Assert.IsTrue(File.Exists(Path.ChangeExtension(txtPath, ".dbtv")), "intermediate .dbtv file should be created");
        Assert.IsTrue(File.Exists(Path.ChangeExtension(txtPath, ".dbtv2")), ".dbtv2 file should be created");
        Assert.AreEqual(2, context.Subjects.Count);
        StringAssert.EndsWith(".dbtv2", context.FilePath);
    }

    [Test]
    public void Migration_FromV1_RunTwice_IsStable()
    {
        string txtPath = WriteV1Base("base", 2);
        string originalContent = File.ReadAllText(txtPath);

        SubjectsFactory.GetSubjectsContext(txtPath);
        ISubjectsContext secondRun = SubjectsFactory.GetSubjectsContext(txtPath);

        Assert.AreEqual(originalContent, File.ReadAllText(txtPath));
        Assert.AreEqual(2, secondRun.Subjects.Count, "a second migration of the same base must not lose subjects");
    }

    [Test]
    public void Migration_InvalidV1Content_ThrowsAndLeavesFileUntouched()
    {
        // Simulates the historical incident: JSON content sitting in a .txt file. The v1
        // parser used to silently yield 0 patients and the chain re-saved an empty base.
        string txtPath = Path.Combine(m_TestDir, "corrupt.txt");
        string garbage = "[{\"$type\": \"Subject, Assembly-CSharp\"}]";
        File.WriteAllText(txtPath, garbage);

        Assert.That(() => SubjectsFactory.GetSubjectsContext(txtPath), Throws.Exception);
        Assert.AreEqual(garbage, File.ReadAllText(txtPath), "an unparseable file must be left untouched");
        Assert.IsFalse(File.Exists(Path.ChangeExtension(txtPath, ".dbtv2")), "no converted file should be produced from an unparseable source");
    }
    #endregion

    #region Repository save / backup
    [Test]
    public void Save_KeepsPreviousVersionInBackup()
    {
        string path = WriteV3Base("repo", "PatientA");

        SubjectRepository repository = new SubjectRepository(path);
        repository.Add(new Subject { PatientName = "PatientB" });
        Assert.IsTrue(repository.Save());

        Assert.AreEqual(2, new DBFile3(path).Subjects.Count, "main file should contain the new state");
        string backupPath = Path.Combine(m_TestDir, "repoBU.dbtv2");
        Assert.IsTrue(File.Exists(backupPath), "a backup should be created");
        Assert.AreEqual(1, new DBFile3(backupPath).Subjects.Count, "backup should contain the previous state, not the new one");
    }

    [Test]
    public void Save_RefusesToOverwriteWhenLoadFailed()
    {
        // The load failure and the save refusal both log errors on purpose; the runner would
        // otherwise auto-fail the test for emitting them.
        LogAssert.ignoreFailingMessages = true;

        string path = Path.Combine(m_TestDir, "corrupt.dbtv2");
        string garbage = "{ this is not json";
        File.WriteAllText(path, garbage);

        SubjectRepository repository = new SubjectRepository(path);

        Assert.IsNull(repository.Subjects, "a failed load must not be mistaken for an empty database");
        Assert.IsFalse(repository.Save(), "saving an unloaded repository must be refused");
        Assert.AreEqual(garbage, File.ReadAllText(path), "the corrupt file must be left untouched for recovery");
    }
    #endregion

    #region Converters
    [Test]
    public void Vector3Converter_RoundTrips()
    {
        Vector3 original = new Vector3(1.5f, -2.25f, 3f);
        string json = JsonConvert.SerializeObject(original, new Vector3Converter());
        Vector3 result = JsonConvert.DeserializeObject<Vector3>(json, new Vector3Converter());

        Assert.AreEqual(original.x, result.x, 0.01f);
        Assert.AreEqual(original.y, result.y, 0.01f);
        Assert.AreEqual(original.z, result.z, 0.01f);
    }

    [Test]
    public void Vector2Converter_RoundTrips()
    {
        Vector2 original = new Vector2(800.5f, -600.25f);
        string json = JsonConvert.SerializeObject(original, new Vector2Converter());
        Vector2 result = JsonConvert.DeserializeObject<Vector2>(json, new Vector2Converter());

        Assert.AreEqual(original.x, result.x, 0.01f);
        Assert.AreEqual(original.y, result.y, 0.01f);
    }

    [Test]
    public void ColorConverter_RoundTrips()
    {
        Color original = new Color(1f, 0.557f, 0f, 1f);
        string json = JsonConvert.SerializeObject(original, new ColorConverter());
        Color result = JsonConvert.DeserializeObject<Color>(json, new ColorConverter());

        Assert.AreEqual(original.r, result.r, 0.01f);
        Assert.AreEqual(original.g, result.g, 0.01f);
        Assert.AreEqual(original.b, result.b, 0.01f);
        Assert.AreEqual(original.a, result.a, 0.01f);
    }

    [Test]
    public void ColorConverter_MalformedString_ReturnsWhiteInsteadOfThrowing()
    {
        Color result = JsonConvert.DeserializeObject<Color>("\"garbage\"", new ColorConverter());
        Assert.AreEqual(Color.white, result);
    }
    #endregion

    #region AtomicFile / BtvEvent
    [Test]
    public void AtomicFile_OverwritesContentAndLeavesNoTempFile()
    {
        string path = Path.Combine(m_TestDir, "file.json");
        BrainTV.Tools.AtomicFile.WriteAllText(path, "first");
        BrainTV.Tools.AtomicFile.WriteAllText(path, "second");

        Assert.AreEqual("second", File.ReadAllText(path));
        Assert.IsFalse(File.Exists(path + ".tmp"));
    }

    [Test]
    public void BtvEvent_CopyConstructor_CopiesCorrelationData()
    {
        BtvEvent original = new BtvEvent(1, 1000f, 500, "A1", "B2", "comment")
        {
            Correlation = new float[] { 0.5f, 0.75f },
            Correlation2D = new float[][] { new float[] { 0.1f, 0.2f } }
        };

        BtvEvent copy = new BtvEvent(original);

        Assert.AreNotSame(original.Correlation, copy.Correlation);
        Assert.AreEqual(0.75f, copy.Correlation[1]);
        Assert.AreNotSame(original.Correlation2D, copy.Correlation2D);
        Assert.AreEqual(0.2f, copy.Correlation2D[0][1]);
    }
    #endregion
}
