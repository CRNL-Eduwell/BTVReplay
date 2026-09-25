using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode tests for the patient loader's scene wiring (review M-6). The loader used to reach
/// the loading-circle parent, the patient-name header and the performance toggle through
/// GameObject.Find on scene-object names (plus a child index), with two timed waits in between.
/// They are now serialized references, checked here against the scene YAML without opening it.
/// </summary>
public class LoaderSceneWiringTests
{
    private const string SubjectLoaderServiceGuid = "6e25f281202f49f4e8ddaf2bf5104ed7";
    private const string UiTextGuid = "5f7201a12d95ffc409449d95f23cf332";

    [Test]
    public void SubjectLoader_HasNoNameLookupsOrTimedWaits()
    {
        // Comments may name the old lookups; only code counts.
        string source = Regex.Replace(
            File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/Services/SubjectLoaderService.cs")),
            @"//[^\n]*", "");

        StringAssert.DoesNotContain("GameObject.Find(", source, "scene objects come from serialized references");
        StringAssert.DoesNotContain("GetChild(", source, "no lookups by child position");
        StringAssert.DoesNotContain("WaitForSeconds(", source, "load phases are sequenced by messages, not by timers");
    }

    [Test]
    public void SubjectLoader_SerializedReferences_AreAssignedInMainScene()
    {
        string scene = File.ReadAllText(Path.Combine(Application.dataPath, "_main.unity"));
        string loader = ComponentBlock(scene, SubjectLoaderServiceGuid);

        string circleParent = ReferencedDocumentHeader(scene, loader, "m_LoadingCircleParent");
        StringAssert.StartsWith("--- !u!224 ", circleParent, "the loading-circle parent is a RectTransform");

        string header = ReferencedDocumentHeader(scene, loader, "m_PatientNameHeader");
        StringAssert.StartsWith("--- !u!114 ", header, "the patient-name header is a component");
        string headerBlock = DocumentAt(scene, header);
        StringAssert.Contains("guid: " + UiTextGuid, headerBlock, "the patient-name header is a UI Text");
    }

    private static string ComponentBlock(string scene, string scriptGuid)
    {
        Match m = Regex.Match(scene, @"--- !u!114 &\d+\nMonoBehaviour:\n(?:(?!--- ).*\n)*?  m_Script: \{fileID: 11500000, guid: " + scriptGuid + @"[^\n]*\n(?:(?!--- ).*\n)*");
        Assert.IsTrue(m.Success, "component with script " + scriptGuid + " not found in _main.unity");
        return m.Value;
    }

    private static string ReferencedDocumentHeader(string scene, string block, string field)
    {
        Match fieldMatch = Regex.Match(block, "  " + field + @": \{fileID: (-?\d+)\}");
        Assert.IsTrue(fieldMatch.Success, field + " is not serialized on the component");
        string fileId = fieldMatch.Groups[1].Value;
        Assert.AreNotEqual("0", fileId, field + " is not assigned");

        Match doc = Regex.Match(scene, @"--- !u!\d+ &" + fileId + @"\n");
        Assert.IsTrue(doc.Success, field + " points at fileID " + fileId + ", which is not in the scene");
        return doc.Value;
    }

    private static string DocumentAt(string scene, string header)
    {
        int start = scene.IndexOf(header, System.StringComparison.Ordinal);
        int end = scene.IndexOf("\n--- ", start + header.Length, System.StringComparison.Ordinal);
        return end < 0 ? scene.Substring(start) : scene.Substring(start, end - start);
    }
}
