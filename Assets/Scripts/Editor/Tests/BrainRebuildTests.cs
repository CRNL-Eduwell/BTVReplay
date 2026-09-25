using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Assets.Scripts.Data.Factory;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode tests for the brain rebuild path (review L-4). Electrode contacts used to be placed
/// in world space, so Brain moved itself back from its off-canvas pose around every rebuild; it
/// also found the objects to destroy by name, which skips inactive ones. Sites each held an
/// instanced material that nothing destroyed.
/// </summary>
public class BrainRebuildTests
{
    private GameObject m_Root;

    [SetUp]
    public void SetUp()
    {
        // The pose BrainCamera gives the brain after the first load.
        m_Root = new GameObject("brain-rebuild-test");
        m_Root.transform.position = new Vector3(-10000, 0, 0);
        m_Root.transform.Rotate(new Vector3(270, 0, 0));
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_Root);
    }

    private static List<AnatomicalSite> Sites()
    {
        return new List<AnatomicalSite>
        {
            new AnatomicalSite("A1", new Vector3(10, 20, 30)),
            new AnatomicalSite("A2", new Vector3(11, 21, 31)),
            new AnatomicalSite("B1", new Vector3(-5, 0, 2)),
        };
    }

    private GameObject Electrodes()
    {
        GameObject electrodes = new GameObject("Electrodes");
        electrodes.transform.SetParent(m_Root.transform, false);
        return electrodes;
    }

    private static void AssertPlacedInBrainSpace(GameObject electrodes, List<AnatomicalSite> sites, List<Site> placed)
    {
        Assert.AreEqual(sites.Count, placed.Count, "one site per contact");
        for (int i = 0; i < sites.Count; i++)
        {
            // The x axis is mirrored on the way in (Unity is left-handed).
            Vector3 expected = new Vector3(-sites[i].Coordinates.x, sites[i].Coordinates.y, sites[i].Coordinates.z);
            Transform contact = placed[i].transform;
            Assert.That(Vector3.Distance(expected, contact.localPosition), Is.LessThan(1e-3f),
                sites[i].Label + " keeps its anatomical coordinates relative to the brain");
            Assert.That(Quaternion.Angle(Quaternion.identity, contact.localRotation), Is.LessThan(1e-3f));
            Assert.That(Vector3.Distance(electrodes.transform.TransformPoint(expected), contact.position), Is.LessThan(1e-2f),
                sites[i].Label + " moves with the brain");
        }
    }

    [Test]
    public void IntraContext_PlacesContactsInBrainSpace_WhereverTheBrainSits()
    {
        GameObject electrodes = Electrodes();
        List<AnatomicalSite> sites = Sites();
        List<Site> placed = new List<Site>();

        new IntraContext().LoadElectrodesOnBrain(electrodes, sites, (site, anatomicalSite) => placed.Add(site));

        AssertPlacedInBrainSpace(electrodes, sites, placed);
    }

    [Test]
    public void ScalpContext_PlacesContactsInBrainSpace_AndToleratesAMissingProjector()
    {
        // No HalfSphere / ScalpDataProjector under the root: the old GetChild(2) lookup returned
        // null here (as it did mid-rebuild) and InitArrays threw.
        GameObject electrodes = Electrodes();
        List<AnatomicalSite> sites = Sites();
        List<Site> placed = new List<Site>();

        new ScalpContext().LoadElectrodesOnBrain(electrodes, sites, (site, anatomicalSite) => placed.Add(site));

        AssertPlacedInBrainSpace(electrodes, sites, placed);
    }

    [Test]
    public void SiteColor_UsesAPropertyBlock_AndKeepsTheSharedMaterial()
    {
        GameObject prefab = Resources.Load("Prefabs/Brain-ElecPlot", typeof(GameObject)) as GameObject;
        Assert.IsNotNull(prefab);
        Material shared = prefab.GetComponent<MeshRenderer>().sharedMaterial;

        GameObject plot = Object.Instantiate(prefab, m_Root.transform);
        MeshRenderer renderer = plot.GetComponent<MeshRenderer>();
        Site site = plot.GetComponent<Site>();
        // Site.Init needs a loaded patient; it only caches the renderer for the colour.
        typeof(Site).GetField("m_MeshRenderer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(site, renderer);

        site.Color = Color.red;

        Assert.IsTrue(renderer.HasPropertyBlock(), "the colour goes through a property block");
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        Assert.AreEqual(Color.red, block.GetColor("_Color"));
        Assert.AreSame(shared, renderer.sharedMaterial, "no per-site material instance to leak");
    }

    [Test]
    public void Brain_RebuildsWithoutNameLookupsOrTheOffsetDance()
    {
        // Comments may describe the old code; only code counts.
        string source = Regex.Replace(
            File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/Brain/Brain.cs")),
            @"//[^\n]*", "");

        StringAssert.DoesNotContain("Destroy(GameObject.Find(", source,
            "previous hemispheres and electrodes are destroyed by reference (Find skips inactive objects)");
        StringAssert.DoesNotContain("10000", source,
            "children are built in local space, so a rebuild no longer moves the brain back to the origin");
    }
}
