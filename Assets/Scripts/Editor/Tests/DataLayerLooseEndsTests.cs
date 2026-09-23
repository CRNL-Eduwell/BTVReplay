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

    [TestCase("A12", "A", 12)]
    [TestCase("B'3", "B'", 3)]
    public void GetIntraPlotInformation_SplitsNameAndContact(string label, string name, int contact)
    {
        var parsed = IntraContext.GetIntraPlotInformation(label);
        Assert.AreEqual(name, parsed.Item1);
        Assert.AreEqual(contact, parsed.Item2);
    }
}
