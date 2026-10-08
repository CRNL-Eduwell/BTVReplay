using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode checks that THIRD-PARTY-NOTICES.md keeps up with the repository (review L-2). The
/// vendored code used to ship without its licences: nothing named HiBoP's list and grid tools,
/// StandaloneFileBrowser, the Windows dialog DLLs or Json.NET, and the adapted files said nothing
/// about where they came from.
/// </summary>
public class ThirdPartyNoticesTests
{
    private static string RepositoryRoot => Directory.GetParent(Application.dataPath).FullName;

    private static string Notices => File.ReadAllText(Path.Combine(RepositoryRoot, "THIRD-PARTY-NOTICES.md"));

    [Test]
    public void EveryPathTheNoticesName_Exists()
    {
        List<string> paths = Regex.Matches(Notices, @"`((?:Assets|\.claude)/[^`*]+)`")
            .Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();
        Assert.That(paths, Is.Not.Empty);

        foreach (string path in paths)
        {
            string full = Path.Combine(RepositoryRoot, path);
            Assert.IsTrue(File.Exists(full) || Directory.Exists(full), "THIRD-PARTY-NOTICES.md names a path that does not exist: " + path);
        }
    }

    [Test]
    public void EveryAdaptedSourceFile_CarriesACreditLine()
    {
        List<string> files = Regex.Matches(Notices, @"^  - `(Assets/[^`]+\.cs)`$", RegexOptions.Multiline)
            .Cast<Match>().Select(m => m.Groups[1].Value).ToList();
        Assert.That(files, Is.Not.Empty);

        foreach (string file in files)
        {
            string head = string.Join("\n", File.ReadLines(Path.Combine(RepositoryRoot, file)).Take(3));
            StringAssert.Contains("THIRD-PARTY-NOTICES.md", head, file + " is listed as adapted but has no credit line at the top");
        }
    }

    [Test]
    public void EveryVendoredToolFolder_IsNamed()
    {
        string notices = Notices;
        foreach (string directory in Directory.GetDirectories(Path.Combine(Application.dataPath, "Tools")))
        {
            string path = "Assets/Tools/" + Path.GetFileName(directory) + "/";
            StringAssert.Contains(path, notices, "a folder under Assets/Tools is not in THIRD-PARTY-NOTICES.md");
        }
    }

    [Test]
    public void EveryPluginBinary_IsNamed()
    {
        string notices = Notices;
        foreach (string platform in Directory.GetDirectories(Path.Combine(Application.dataPath, "Plugins")))
        {
            foreach (string entry in Directory.GetFileSystemEntries(platform).Where(e => !e.EndsWith(".meta")))
            {
                string name = Path.GetFileNameWithoutExtension(entry);
                if (name.StartsWith("lib"))
                    name = name.Substring(3);
                StringAssert.Contains(name, notices, Path.GetFileName(entry) + " ships in the player but is not in THIRD-PARTY-NOTICES.md");
            }
        }
    }
}
