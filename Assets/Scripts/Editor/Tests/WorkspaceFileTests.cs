using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode tests for workspace persistence. The June JSON hardening (e39abe1) swapped
/// WorkspaceFile's own serializer settings for BtvJson's and silently dropped the Color
/// converter: pre-June workspaces (colour stored as "RGBA(...)") then loaded as defaults, and
/// saving truncated the file, then failed with Json.NET's "self referencing loop" error on
/// Color.linear, leaving it empty. Three of these tests fail against the pre-fix code.
/// </summary>
public class WorkspaceFileTests
{
    private string m_TempDir;

    [SetUp]
    public void SetUp()
    {
        m_TempDir = Path.Combine(Path.GetTempPath(), "btv-workspace-tests-" + Path.GetRandomFileName());
        Directory.CreateDirectory(m_TempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(m_TempDir))
            Directory.Delete(m_TempDir, true);
    }

    private static Workspace SampleWorkspace(Color trace1Color)
    {
        TraceParameters trace1 = new TraceParameters(2.5f, -1f, true, 10, trace1Color, 1.5f)
        {
            Parent = "Pannel",
            Id = 3,
            GridLayout = GridLayout.TwoBy3
        };
        TraceParameters trace2 = new TraceParameters(1f, 0f, false, 5, Color.blue, 1f);
        return new Workspace(new BrainParameters(true), trace1, trace2);
    }

    private static void AssertColor(Color expected, Color actual)
    {
        Assert.AreEqual(expected.r, actual.r, 0.001f, "r");
        Assert.AreEqual(expected.g, actual.g, 0.001f, "g");
        Assert.AreEqual(expected.b, actual.b, 0.001f, "b");
        Assert.AreEqual(expected.a, actual.a, 0.001f, "a");
    }

    [Test]
    public void Save_ThenLoad_RoundTripsTraceParameters()
    {
        string path = Path.Combine(m_TempDir, "layout.workspace");
        Color orange = new Color(1f, 0.557f, 0f, 1f);

        WorkspaceFile.Save(path, SampleWorkspace(orange));

        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0), "a save must never leave an empty workspace file");
        Workspace loaded = new WorkspaceFile(path).Workspace;
        AssertColor(orange, loaded.Trace1.Color);
        AssertColor(Color.blue, loaded.Trace2.Color);
        Assert.AreEqual(2.5f, loaded.Trace1.Gain);
        Assert.AreEqual(10, loaded.Trace1.Window);
        Assert.AreEqual("Pannel", loaded.Trace1.Parent);
        Assert.AreEqual(3, loaded.Trace1.Id);
        Assert.AreEqual(GridLayout.TwoBy3, loaded.Trace1.GridLayout);
        Assert.IsTrue(loaded.BrainParameters.IsMaxed);
    }

    [Test]
    public void Save_OverExistingFile_ReplacesItAndLeavesNoTempFile()
    {
        string path = Path.Combine(m_TempDir, "layout.workspace");
        WorkspaceFile.Save(path, SampleWorkspace(Color.red));

        WorkspaceFile.Save(path, SampleWorkspace(Color.green));

        AssertColor(Color.green, new WorkspaceFile(path).Workspace.Trace1.Color);
        Assert.IsFalse(File.Exists(path + ".tmp"));
    }

    [Test]
    public void Load_PreJuneFile_ReadsRgbaStringColour()
    {
        // Shape written by WorkspaceFile before e39abe1, when ColorConverter was registered.
        const string legacy = @"{
  ""BrainParameters"": { ""IsMaxed"": false },
  ""Trace1"": { ""Gain"": 3.0, ""Offset"": 0.5, ""ShowGrid"": true, ""Window"": 8,
              ""Color"": ""RGBA(1.000, 0.557, 0.000, 1.000)"", ""Width"": 2.0,
              ""Parent"": ""RightPannel"", ""Id"": 1, ""GridLayout"": 2 },
  ""Trace2"": { ""Gain"": 1.0, ""Offset"": 0.0, ""ShowGrid"": false, ""Window"": 10,
              ""Color"": ""RGBA(0.000, 0.000, 1.000, 1.000)"", ""Width"": 1.0,
              ""Parent"": ""Pannel"", ""Id"": 2, ""GridLayout"": 0 }
}";
        string path = Path.Combine(m_TempDir, "legacy.workspace");
        File.WriteAllText(path, legacy);

        Workspace loaded = new WorkspaceFile(path).Workspace;

        AssertColor(new Color(1f, 0.557f, 0f, 1f), loaded.Trace1.Color);
        AssertColor(Color.blue, loaded.Trace2.Color);
        Assert.AreEqual(3.0f, loaded.Trace1.Gain, "the rest of the layout must load too, not fall back to defaults");
        Assert.AreEqual(GridLayout.OneBy3, loaded.Trace1.GridLayout);
    }

    [Test]
    public void Load_ColourWrittenAsObject_IsAccepted()
    {
        // Defensive: the shape Json.NET produces for Color by reflection, in case any file was
        // written that way while the converter was missing.
        const string json = @"{ ""Trace1"": { ""Gain"": 1.0, ""Color"": { ""r"": 0.2, ""g"": 0.4, ""b"": 0.6, ""a"": 0.8 } } }";
        string path = Path.Combine(m_TempDir, "object.workspace");
        File.WriteAllText(path, json);

        AssertColor(new Color(0.2f, 0.4f, 0.6f, 0.8f), new WorkspaceFile(path).Workspace.Trace1.Color);
    }
}
