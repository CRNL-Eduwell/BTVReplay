using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Edit-mode tests for the portable path token logic: "${NAME}/..." forms stored in patient
/// bases resolve against machine-configured roots, and absolute paths under a known root
/// tokenize back. The rules pinned here: legacy absolute paths pass through untouched, the
/// longest matching root wins, matching is separator-agnostic and case-insensitive, and an
/// unconfigured token warns and passes through instead of failing silently.
/// </summary>
public class PathTokensTests
{
    private static readonly char S = Path.DirectorySeparatorChar;

    private static List<PathRoot> Roots(params (string name, string path)[] roots)
    {
        List<PathRoot> list = new List<PathRoot>();
        foreach ((string name, string path) in roots)
            list.Add(new PathRoot(name, path));
        return list;
    }

    [Test]
    public void Resolve_ExpandsAConfiguredRoot_WithNativeSeparators()
    {
        string resolved = PathTokens.Resolve("${EEG}/sub1/file.trc", Roots(("EEG", "/data/eeg")), "/app/Config");

        Assert.AreEqual("/data/eeg" + S + "sub1" + S + "file.trc", resolved);
    }

    [Test]
    public void Resolve_ExpandsTheBuiltInAppConfigToken()
    {
        string resolved = PathTokens.Resolve("${APPCONFIG}/Data/MNI/mesh.tri", Roots(), "/app/Config");

        Assert.AreEqual("/app/Config" + S + "Data" + S + "MNI" + S + "mesh.tri", resolved);
    }

    [Test]
    public void Resolve_LeavesLegacyAbsolutePathsUntouched()
    {
        const string absolute = @"\\10.69.168.1\intra\BrainVisaDB\file.pts";

        Assert.AreEqual(absolute, PathTokens.Resolve(absolute, Roots(("DATA", "/data")), "/app/Config"));
        Assert.AreEqual("", PathTokens.Resolve("", Roots(), "/app/Config"));
    }

    [Test]
    public void Resolve_UnconfiguredToken_WarnsAndReturnsThePathAsIs()
    {
        LogAssert.Expect(LogType.Warning, new Regex("\"ANAT\" is not configured"));
        string resolved = PathTokens.Resolve("${ANAT}/mesh.gii", Roots(("EEG", "/data/eeg")), "/app/Config");

        Assert.AreEqual("${ANAT}/mesh.gii", resolved);
    }

    [Test]
    public void Resolve_TokenNameIsCaseInsensitive()
    {
        string resolved = PathTokens.Resolve("${eeg}/file.trc", Roots(("EEG", "/data/eeg")), "/app/Config");

        Assert.AreEqual("/data/eeg" + S + "file.trc", resolved);
    }

    [Test]
    public void Tokenize_RewritesAPathUnderARoot_WithForwardSlashes()
    {
        string tokenized = PathTokens.Tokenize(@"\\server\share\Epilepsy\PAT1\file.trc", Roots(("DATA", @"\\server\share")), "/app/Config");

        Assert.AreEqual("${DATA}/Epilepsy/PAT1/file.trc", tokenized);
    }

    [Test]
    public void Tokenize_MatchesAcrossSeparatorsAndCase()
    {
        string tokenized = PathTokens.Tokenize("/Data/EEG/sub1/file.trc", Roots(("EEG", @"\data\eeg")), "/app/Config");

        Assert.AreEqual("${EEG}/sub1/file.trc", tokenized, "root matching must ignore separator style and case, keeping the remainder's casing");
    }

    [Test]
    public void Tokenize_PrefersTheLongestMatchingRoot()
    {
        List<PathRoot> roots = Roots(("DATA", "/data"), ("EEG", "/data/eeg"));

        Assert.AreEqual("${EEG}/file.trc", PathTokens.Tokenize("/data/eeg/file.trc", roots, "/app/Config"));
        Assert.AreEqual("${DATA}/anat/mesh.gii", PathTokens.Tokenize("/data/anat/mesh.gii", roots, "/app/Config"));
    }

    [Test]
    public void Tokenize_UsesAppConfigForApplicationAssets()
    {
        string tokenized = PathTokens.Tokenize("/app/Config/Data/MNI/mesh.tri", Roots(), "/app/Config");

        Assert.AreEqual("${APPCONFIG}/Data/MNI/mesh.tri", tokenized);
    }

    [Test]
    public void Tokenize_LeavesPathsUnderNoRootUntouched()
    {
        const string foreign = "/somewhere/else/file.trc";

        Assert.AreEqual(foreign, PathTokens.Tokenize(foreign, Roots(("DATA", "/data")), "/app/Config"));
        Assert.AreEqual("", PathTokens.Tokenize("", Roots(), "/app/Config"));
    }

    [Test]
    public void Tokenize_DoesNotMatchASiblingFolderSharingThePrefix()
    {
        // "/data/eeg-backup" must not tokenize against the "/data/eeg" root.
        string tokenized = PathTokens.Tokenize("/data/eeg-backup/file.trc", Roots(("EEG", "/data/eeg")), "/app/Config");

        Assert.AreEqual("/data/eeg-backup/file.trc", tokenized);
    }

    [Test]
    public void TokenizeThenResolve_RoundTripsToTheNativeForm()
    {
        List<PathRoot> roots = Roots(("DATA", "/data/clinical"));
        string original = "/data/clinical/PAT1/eeg/file.trc";

        string roundTripped = PathTokens.Resolve(PathTokens.Tokenize(original, roots, "/app/Config"), roots, "/app/Config");

        Assert.AreEqual(original.Replace('/', S), roundTripped);
    }
}
