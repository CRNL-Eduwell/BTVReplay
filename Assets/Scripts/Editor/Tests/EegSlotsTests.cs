using BTV.Data;
using BTV.Services;
using BTV.Services.EegFileService;
using NUnit.Framework;

/// <summary>
/// Pins the single EEG slot count (review M-3): montages are sized from it and the slot checks
/// bound on it, instead of each repeating a literal 6.
/// </summary>
public class EegSlotsTests
{
    [Test]
    public void DefaultMontage_HasOneProgramPerSlot()
    {
        Assert.AreEqual(EegSlots.Count, EegFileService.GetDefaultMontage(Session.Current).EegFiles.Length);
    }

    [TestCase(-1)]
    [TestCase(EegSlots.Count)]
    public void IsFileIdValid_OutOfRangeSlot_IsFalse(int fileId)
    {
        Assert.IsFalse(EegFileService.IsFileIdValid(Session.Current, fileId));
    }
}
