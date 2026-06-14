using System.IO;
using NUnit.Framework;

/// <summary>
/// Edit-mode tests for the HasAnat cache: the existence checks hit the disk (possibly UNC
/// network paths), so the result must be computed once per container state and recomputed
/// when any involved path changes - never staying stale after an edit.
/// </summary>
public class BrainDataContainerHasAnatTests
{
    private string m_TempDir;

    [SetUp]
    public void SetUp()
    {
        m_TempDir = Path.Combine(Path.GetTempPath(), "btv-hasanat-tests-" + Path.GetRandomFileName());
        Directory.CreateDirectory(m_TempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(m_TempDir))
            Directory.Delete(m_TempDir, true);
    }

    private string CreateFile(string name)
    {
        string path = Path.Combine(m_TempDir, name);
        File.WriteAllText(path, "x");
        return path;
    }

    [Test]
    public void HasAnat_TrueWhenAllSingleConfigurationFilesExist()
    {
        BrainDataContainer container = new BrainDataContainer
        {
            MeshConfiguration = MeshConfiguration.Single,
            LeftHemisphere = CreateFile("mesh.gii"),
            Pts = CreateFile("sites.pts")
        };

        Assert.IsTrue(container.HasAnat);
    }

    [Test]
    public void HasAnat_FalseWhenAFileIsMissingOrHasTheWrongExtension()
    {
        BrainDataContainer container = new BrainDataContainer
        {
            MeshConfiguration = MeshConfiguration.Single,
            LeftHemisphere = Path.Combine(m_TempDir, "missing.gii"),
            Pts = CreateFile("sites.pts")
        };
        Assert.IsFalse(container.HasAnat, "missing mesh file");

        container.LeftHemisphere = CreateFile("mesh.txt");
        Assert.IsFalse(container.HasAnat, "wrong mesh extension");
    }

    [Test]
    public void HasAnat_IsRecomputedWhenAPathChanges()
    {
        BrainDataContainer container = new BrainDataContainer
        {
            MeshConfiguration = MeshConfiguration.Single,
            LeftHemisphere = Path.Combine(m_TempDir, "missing.gii"),
            Pts = CreateFile("sites.pts")
        };
        Assert.IsFalse(container.HasAnat);

        // Pointing the path at an existing file must invalidate the cached false.
        container.LeftHemisphere = CreateFile("mesh.gii");
        Assert.IsTrue(container.HasAnat, "the cache must not survive a path change");

        // And a configuration change must invalidate too (LeftRight now misses the right mesh).
        container.MeshConfiguration = MeshConfiguration.LeftRight;
        Assert.IsFalse(container.HasAnat, "the cache must not survive a configuration change");
    }
}
