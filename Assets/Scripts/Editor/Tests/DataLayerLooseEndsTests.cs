using Assets.Scripts.Data.Factory;
using NUnit.Framework;

/// <summary>
/// Edit-mode tests for the data-layer loose ends of review L-3: the EEG file-info types' hash
/// contract, and the single intracranial label parser that replaced two copies.
/// </summary>
public class DataLayerLooseEndsTests
{
    [Test]
    public void EqualFileInfos_HaveEqualHashCodes()
    {
        Assert.AreEqual(new ElanFileInfo("/d/rec.eeg", "/d/rec.pos").GetHashCode(), new ElanFileInfo("/d/rec.eeg", "/d/rec.pos").GetHashCode());
        Assert.AreEqual(new MicromedFileInfo("/d/rec.TRC").GetHashCode(), new MicromedFileInfo("/d/rec.TRC").GetHashCode());
        Assert.AreEqual(new BrainvisionFileInfo("/d/rec.vhdr").GetHashCode(), new BrainvisionFileInfo("/d/rec.vhdr").GetHashCode());
        Assert.AreEqual(new EdfFileInfo("/d/rec.edf").GetHashCode(), new EdfFileInfo("/d/rec.edf").GetHashCode());
    }

    [Test]
    public void EqualFileInfos_CollapseInAHashSet()
    {
        var set = new System.Collections.Generic.HashSet<object> { new MicromedFileInfo("/d/rec.TRC"), new MicromedFileInfo("/d/rec.TRC") };
        Assert.AreEqual(1, set.Count);
    }

    // One-letter electrodes: unchanged by the multi-letter support.
    [TestCase("A12", "A", 12)]
    [TestCase("a01", "a", 1)]
    [TestCase("B'3", "B'", 3)]
    [TestCase("b'10", "b'", 10)]
    [TestCase("A1-A2", "A", 1)]
    [TestCase("A'1-A'2", "A'", 1)]
    [TestCase("EEG A1", "A", 1)]
    // Electrode names of several letters used to give ("", -1).
    [TestCase("HA1", "HA", 1)]
    [TestCase("ha12", "ha", 12)]
    [TestCase("HA'1", "HA'", 1)]
    [TestCase("ha01", "ha", 1)]
    [TestCase("pa1", "pa", 1)]
    [TestCase("Ap1", "Ap", 1)]
    [TestCase("ecg1", "ecg", 1)]
    [TestCase(" HA1 ", "HA", 1)]
    // A bipolar label of multi-letter electrodes and a label with no contact number stay unsplit.
    [TestCase("HA1-HA2", "", -1)]
    [TestCase("HA'1-HA'2", "", -1)]
    [TestCase("ECG", "", -1)]
    [TestCase("MKR+", "", -1)]
    [TestCase("A", "", -1)]
    [TestCase("a'", "", -1)]
    [TestCase("1", "", -1)]
    [TestCase("a 1", "", -1)]
    [TestCase("", "", -1)]
    public void GetIntraPlotInformation_SplitsNameAndContact(string label, string name, int contact)
    {
        var parsed = IntraContext.GetIntraPlotInformation(label);
        Assert.AreEqual(name, parsed.Item1);
        Assert.AreEqual(contact, parsed.Item2);
    }

    // The layout shown without electrode coordinates: a new column (x) each time the electrode
    // name changes, contacts going down the column (z). Multi-letter electrodes all had the empty
    // name, so they shared one column.
    [Test]
    public void DefaultIntraLayout_GivesEachMultiLetterElectrodeItsOwnColumn()
    {
        var sites = BTV.Services.AnatomicalDataService.AnatomicalDataService.DefaultIntraLayout(
            new[] { "HA1", "HA2", "HB1", "HB2", "A1", "A2" });

        Assert.AreEqual(new UnityEngine.Vector3(5, 0, -5), sites[0].Coordinates);
        Assert.AreEqual(new UnityEngine.Vector3(5, 0, -10), sites[1].Coordinates);
        Assert.AreEqual(new UnityEngine.Vector3(10, 0, -5), sites[2].Coordinates);
        Assert.AreEqual(new UnityEngine.Vector3(10, 0, -10), sites[3].Coordinates);
        Assert.AreEqual(new UnityEngine.Vector3(15, 0, -5), sites[4].Coordinates);
        Assert.AreEqual(new UnityEngine.Vector3(15, 0, -10), sites[5].Coordinates);
        Assert.AreEqual("HB1", sites[2].Label, "the label is kept as the file gives it");
    }

    [Test]
    public void DefaultIntraLayout_KeepsUnsplitLabelsInOneColumn()
    {
        var sites = BTV.Services.AnatomicalDataService.AnatomicalDataService.DefaultIntraLayout(
            new[] { "A1", "HA1-HA2", "HB1-HB2", "ECG" });

        Assert.AreEqual(new UnityEngine.Vector3(5, 0, -5), sites[0].Coordinates);
        Assert.AreEqual(new UnityEngine.Vector3(10, 0, -5), sites[1].Coordinates);
        Assert.AreEqual(new UnityEngine.Vector3(10, 0, -10), sites[2].Coordinates);
        Assert.AreEqual(new UnityEngine.Vector3(10, 0, -15), sites[3].Coordinates);
    }
}
