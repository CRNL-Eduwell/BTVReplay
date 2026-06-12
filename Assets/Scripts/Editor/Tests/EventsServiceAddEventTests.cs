using System.Collections.Generic;
using BTV.Data;
using BTV.Services.EventsService;
using NUnit.Framework;

/// <summary>
/// Edit-mode tests for the add-event contract. The UI lists and the per-trace GameObject lists
/// are kept index-parallel with EventsService.Events by replaying every add at the index the
/// service reports, so AddEvent must say whether anything was actually added (duplicates are
/// skipped) and the Add -> SortBySample -> GetEventId sequence must yield the exact index where
/// the new event ended up.
/// </summary>
public class EventsServiceAddEventTests
{
    [TearDown]
    public void TearDown()
    {
        EventsService.Reset();
    }

    [Test]
    public void AddEvent_AddsACopy_AndReturnsTrue()
    {
        BtvEvent original = new BtvEvent(5, 1000f, 200, "A1", "B2", "comment");

        Assert.IsTrue(EventsService.AddEvent(original));
        Assert.AreEqual(1, EventsService.Events.Count);
        Assert.AreNotSame(original, EventsService.Events[0], "the service must store a copy");
        Assert.IsTrue(EventsService.Events[0].Equals(original));
    }

    [Test]
    public void AddEvent_SkipsDuplicate_AndReturnsFalse()
    {
        BtvEvent original = new BtvEvent(5, 1000f, 200, "A1", "B2", "comment");
        EventsService.AddEvent(original);

        Assert.IsFalse(EventsService.AddEvent(new BtvEvent(original)));
        Assert.AreEqual(1, EventsService.Events.Count, "a duplicate must not grow the list");
    }

    [Test]
    public void AddEvent_TreatsSameTimeDifferentCode_AsDuplicate()
    {
        // Pins the current BtvEvent.Equals semantics: Code is NOT part of the identity, so two
        // events differing only by code are duplicates. If this test starts failing because
        // Equals was extended, review every Contains/GetEventId/Remove call site first.
        EventsService.AddEvent(new BtvEvent(5, 1000f, 200, "A1", "B2", "comment"));

        Assert.IsFalse(EventsService.AddEvent(new BtvEvent(99, 1000f, 200, "A1", "B2", "comment")));
        Assert.AreEqual(1, EventsService.Events.Count);
    }

    [Test]
    public void AddFlow_YieldsTheIndexWhereTheEventLanded_KeepingParallelListsInSync()
    {
        // Mirrors EventsManager.AddEvent: Add -> SortBySample -> GetEventId, then replay the
        // insert at that index into a parallel list (what every trace does with its GameObjects).
        List<float> parallel = new List<float>();
        float[] times = { 5000f, 1000f, 3000f, 4000f, 2000f, 3500f };

        for (int i = 0; i < times.Length; i++)
        {
            BtvEvent toAdd = new BtvEvent(i, times[i], 0, "A1", "", "evt " + i);
            Assert.IsTrue(EventsService.AddEvent(toAdd));
            EventsService.SortBySample();
            int id = EventsService.GetEventId(toAdd);

            Assert.GreaterOrEqual(id, 0);
            Assert.LessOrEqual(id, parallel.Count, "the reported index must be insertable into the parallel list");
            parallel.Insert(id, toAdd.TimeInMilliSeconds);
        }

        Assert.AreEqual(EventsService.Events.Count, parallel.Count);
        for (int i = 0; i < parallel.Count; i++)
        {
            Assert.AreEqual(EventsService.Events[i].TimeInMilliSeconds, parallel[i],
                "parallel list out of sync at index " + i);
        }
    }
}
