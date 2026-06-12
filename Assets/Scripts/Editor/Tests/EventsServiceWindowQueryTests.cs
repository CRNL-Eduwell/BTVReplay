using System;
using System.Collections.Generic;
using BTV.Data;
using BTV.Services.EventsService;
using NUnit.Framework;

/// <summary>
/// Edit-mode tests for the event-window queries that the trace redraw runs every video tick.
/// They pin the new binary-search-bounded implementations (and the single-pass
/// CollectEventIdsForWindow) against a brute-force full-scan reference that encodes the original
/// LINQ predicates, so the optimisation can never silently drop or add an event.
/// </summary>
public class EventsServiceWindowQueryTests
{
    // Brute-force references: the exact predicates of the four original LINQ queries, scanned over
    // the whole list with no binary-search bound. start = TimeInMilliSeconds, end = start + Duration.
    private static bool RefBigger(BtvEvent e, int l, int r)
        => e.TimeInMilliSeconds <= l && e.TimeInMilliSeconds + e.Duration >= r;
    private static bool RefEntering(BtvEvent e, int l, int r)
        => e.TimeInMilliSeconds < r && e.TimeInMilliSeconds > l && e.TimeInMilliSeconds + e.Duration >= r;
    private static bool RefInside(BtvEvent e, int l, int r)
        => e.TimeInMilliSeconds < r && e.TimeInMilliSeconds > l
           && e.TimeInMilliSeconds + e.Duration >= l && e.TimeInMilliSeconds + e.Duration <= r;
    private static bool RefExiting(BtvEvent e, int l, int r)
        => e.TimeInMilliSeconds < l
           && e.TimeInMilliSeconds + e.Duration >= l && e.TimeInMilliSeconds + e.Duration <= r;

    private static List<int> Brute(Func<BtvEvent, int, int, bool> predicate, int l, int r)
    {
        List<int> result = new List<int>();
        for (int i = 0; i < EventsService.Events.Count; i++)
        {
            if (predicate(EventsService.Events[i], l, r))
                result.Add(i);
        }
        return result;
    }

    private static void SetSortedEvents(IEnumerable<BtvEvent> events)
    {
        List<BtvEvent> list = new List<BtvEvent>(events);
        list.Sort((a, b) => a.TimeInMilliSeconds.CompareTo(b.TimeInMilliSeconds));
        EventsService.Events = list;
    }

    [TearDown]
    public void TearDown()
    {
        EventsService.Reset();
    }

    [Test]
    public void BoundedQueries_MatchBruteForce_AcrossRandomEventsAndWindows()
    {
        // Deterministic seed so a failure is reproducible.
        Random rng = new Random(20260612);

        for (int trial = 0; trial < 200; trial++)
        {
            int count = rng.Next(0, 40);
            List<BtvEvent> events = new List<BtvEvent>();
            for (int i = 0; i < count; i++)
            {
                int start = rng.Next(0, 10000);
                int duration = rng.Next(0, 3000);
                events.Add(new BtvEvent(0, start, duration));
            }
            SetSortedEvents(events);

            // A real playback window is strictly positive (left = ms - period*1000, period >= 1;
            // the render path itself divides by right - left), so keep left < right.
            int right = rng.Next(0, 10000);
            int left = right - rng.Next(1, 5001);

            CollectionAssert.AreEqual(Brute(RefBigger, left, right),
                EventsService.GetEventIdsBiggerThanWindow(left, right), $"bigger trial {trial} [{left},{right}]");
            CollectionAssert.AreEqual(Brute(RefEntering, left, right),
                EventsService.GetEventIdsEnteringWindow(left, right), $"entering trial {trial} [{left},{right}]");
            CollectionAssert.AreEqual(Brute(RefInside, left, right),
                EventsService.GetEventIdsInsideWindow(left, right), $"inside trial {trial} [{left},{right}]");
            CollectionAssert.AreEqual(Brute(RefExiting, left, right),
                EventsService.GetEventIdsExitingWindow(left, right), $"exiting trial {trial} [{left},{right}]");
        }
    }

    [Test]
    public void CollectEventIdsForWindow_MatchesTheFourSeparateQueries()
    {
        Random rng = new Random(987654321);
        List<int> bigger = new List<int>(), entering = new List<int>(), inside = new List<int>(), exiting = new List<int>();

        for (int trial = 0; trial < 200; trial++)
        {
            int count = rng.Next(0, 40);
            List<BtvEvent> events = new List<BtvEvent>();
            for (int i = 0; i < count; i++)
                events.Add(new BtvEvent(0, rng.Next(0, 10000), rng.Next(0, 3000)));
            SetSortedEvents(events);

            int right = rng.Next(0, 10000);
            int left = right - rng.Next(1, 5001);

            EventsService.CollectEventIdsForWindow(left, right, bigger, entering, inside, exiting);

            CollectionAssert.AreEqual(EventsService.GetEventIdsBiggerThanWindow(left, right), bigger, "bigger");
            CollectionAssert.AreEqual(EventsService.GetEventIdsEnteringWindow(left, right), entering, "entering");
            CollectionAssert.AreEqual(EventsService.GetEventIdsInsideWindow(left, right), inside, "inside");
            CollectionAssert.AreEqual(EventsService.GetEventIdsExitingWindow(left, right), exiting, "exiting");
        }
    }

    [Test]
    public void CollectEventIdsForWindow_ReusesBuffers_ClearingPreviousContents()
    {
        List<int> bigger = new List<int> { 99 }, entering = new List<int> { 99 },
                  inside = new List<int> { 99 }, exiting = new List<int> { 99 };
        SetSortedEvents(new List<BtvEvent>());

        EventsService.CollectEventIdsForWindow(0, 1000, bigger, entering, inside, exiting);

        Assert.IsEmpty(bigger);
        Assert.IsEmpty(entering);
        Assert.IsEmpty(inside);
        Assert.IsEmpty(exiting);
    }

    [Test]
    public void EventEndingExactlyOnRightBorder_LandsInBothBiggerAndExiting()
    {
        // start 100, end 500. Window [200, 500]: spans the window (bigger) AND ends inside it after
        // starting before the left border (exiting). The original predicates are not mutually
        // exclusive here, so the event must appear in both buckets.
        SetSortedEvents(new List<BtvEvent> { new BtvEvent(0, 100, 400) });

        CollectionAssert.AreEqual(new[] { 0 }, EventsService.GetEventIdsBiggerThanWindow(200, 500));
        CollectionAssert.AreEqual(new[] { 0 }, EventsService.GetEventIdsExitingWindow(200, 500));

        List<int> bigger = new List<int>(), entering = new List<int>(), inside = new List<int>(), exiting = new List<int>();
        EventsService.CollectEventIdsForWindow(200, 500, bigger, entering, inside, exiting);
        CollectionAssert.AreEqual(new[] { 0 }, bigger);
        CollectionAssert.AreEqual(new[] { 0 }, exiting);
        Assert.IsEmpty(entering);
        Assert.IsEmpty(inside);
    }

    [Test]
    public void EventsStartingAtOrAfterRightBorder_AreExcluded()
    {
        // start == right and start > right: never overlap a [left, right] window, and the
        // binary-search bound must skip them.
        SetSortedEvents(new List<BtvEvent>
        {
            new BtvEvent(0, 300, 50),   // inside window [200,500]
            new BtvEvent(0, 500, 100),  // starts exactly on the right border
            new BtvEvent(0, 800, 100),  // starts after the right border
        });

        CollectionAssert.AreEqual(new[] { 0 }, EventsService.GetEventIdsInsideWindow(200, 500));
        Assert.IsEmpty(EventsService.GetEventIdsBiggerThanWindow(200, 500));
        Assert.IsEmpty(EventsService.GetEventIdsEnteringWindow(200, 500));
        Assert.IsEmpty(EventsService.GetEventIdsExitingWindow(200, 500));
    }
}
