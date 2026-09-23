using NUnit.Framework;

/// <summary>
/// Edit-mode tests for the saved dock ids (review M-6). Workspaces used to store the dock
/// parent's live scene-object name and look it up with GameObject.Find; the ids are now stable
/// constants that keep the legacy values, so old and new workspaces restore the same way.
/// </summary>
public class DockSideTests
{
    [Test]
    public void LegacyAndCurrentIds_AreTheSame()
    {
        // Workspaces written before the change stored these scene-object names verbatim.
        Assert.AreEqual("Pannel", DockSide.Left);
        Assert.AreEqual("RightPannel", DockSide.Right);
    }

    [TestCase("Pannel", true)]
    [TestCase("RightPannel", false)]
    [TestCase("SomeRenamedPanel", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void IsLeft_DocksUnknownIdsRight_AsTheOldLookupDid(string savedId, bool left)
    {
        Assert.AreEqual(left, DockSide.IsLeft(savedId));
    }
}
