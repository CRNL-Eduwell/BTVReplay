using BTV.Data;
using BTV.Services.EventsService;
using NUnit.Framework;

/// <summary>
/// Edit-mode tests for the update-event contract around stored correlations. A 1D correlation
/// is computed against the event's site of interest and a duration change moves the data
/// window, so both edits must invalidate the stored results - otherwise the brain keeps
/// displaying the previous electrode's correlations. A cosmetic edit (comment) must NOT throw
/// away a computed result.
/// </summary>
public class EventsServiceUpdateEventTests
{
    [TearDown]
    public void TearDown()
    {
        EventsService.Reset();
    }

    private static BtvEvent AddEventWithCorrelation()
    {
        BtvEvent original = new BtvEvent(5, 1000f, 2000, "A1", "B2", "comment");
        EventsService.AddEvent(original);
        EventsService.Events[0].Correlation = new float[] { 0.1f, 0.2f, 0.3f };
        return original;
    }

    [Test]
    public void UpdateEvent_SiteOfInterestChange_ClearsTheStored1dCorrelation()
    {
        BtvEvent original = AddEventWithCorrelation();
        // The edit window works on a stale copy of the event that can still carry the old
        // correlation - the service must not let it survive a site change.
        BtvEvent modified = new BtvEvent(EventsService.Events[0]);
        modified.SiteOfInterest = "C3";

        EventsService.UpdateEvent(modified, original);

        Assert.IsNull(EventsService.Events[0].Correlation, "a correlation computed against the old site must not survive a site change");
        Assert.AreEqual("C3", EventsService.Events[0].SiteOfInterest);
    }

    [Test]
    public void UpdateEvent_SiteOfInterestChange_Keeps2dCorrelation()
    {
        BtvEvent original = new BtvEvent(5, 1000f, 2000, "A1", "B2", "comment");
        EventsService.AddEvent(original);
        EventsService.Events[0].Correlation2D = new float[][] { new float[] { 0f, 0.5f }, new float[] { 0.5f, 0f } };

        BtvEvent modified = new BtvEvent(EventsService.Events[0]);
        modified.SiteOfInterest = "C3";

        EventsService.UpdateEvent(modified, original);

        Assert.IsNotNull(EventsService.Events[0].Correlation2D, "the all-pairs matrix does not depend on the event site");
    }

    [Test]
    public void UpdateEvent_DurationChange_ClearsBothCorrelations()
    {
        BtvEvent original = AddEventWithCorrelation();
        EventsService.Events[0].Correlation2D = new float[][] { new float[] { 0f } };

        BtvEvent modified = new BtvEvent(EventsService.Events[0]);
        modified.Duration = 3000;

        EventsService.UpdateEvent(modified, original);

        Assert.IsNull(EventsService.Events[0].Correlation);
        Assert.IsNull(EventsService.Events[0].Correlation2D);
    }

    [Test]
    public void UpdateEvent_CommentOnlyChange_KeepsTheStoredCorrelation()
    {
        BtvEvent original = AddEventWithCorrelation();
        BtvEvent modified = new BtvEvent(EventsService.Events[0]);
        modified.Comment = "new comment";

        EventsService.UpdateEvent(modified, original);

        Assert.IsNotNull(EventsService.Events[0].Correlation, "a cosmetic edit must not discard a computed correlation");
    }
}
