using Assets.Scripts.Data.Factory;
using BTV.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BTV.Services.EventsService
{
    public static class EventsService
    {
        public static List<BtvEvent> Events
        {
            get { return Session.Current.Events; }
            set { Session.Current.Events = value ?? new List<BtvEvent>(); }
        }

        public static void Reset()
        {
            Events = new List<BtvEvent>();
        }

        public static void Load(string filePath, int samplingFrequency = 0)
        {
            Load(Session.Current, filePath, samplingFrequency);
        }

        /// <summary>
        /// Replaces the session's events with the file's. Throws when the file cannot be read,
        /// and in that case leaves the events already loaded untouched: the old version cleared
        /// them first, so a corrupt file silently wiped the clinician's current markings.
        /// </summary>
        public static void Load(Session session, string filePath, int samplingFrequency = 0)
        {
            if (File.Exists(filePath))
            {
                IEventsContext file = EventsFactory.GetEventsContext(filePath, samplingFrequency);

                List<BtvEvent> loaded = new List<BtvEvent>(file.Events.Count);
                for (int i = 0; i < file.Events.Count; i++)
                {
                    loaded.Add(new BtvEvent(file.Events[i]));
                }
                session.Events = loaded;

                // Keep the service sorted by start time the same way AddEvent does. The window
                // queries below binary-search on this order, and the trace GameObject lists are
                // built index-parallel to Events, so an unsorted load would otherwise desync them
                // the first time the user edits an event (AddEvent re-sorts the whole list).
                SortBySample(session);
            }
        }

        /// <summary>Reads a file's events without touching the session; throws when it cannot be read.</summary>
        public static List<BtvEvent> LoadEventsFromFile(string filePath, int samplingFrequency = 0)
        {
            if (File.Exists(filePath))
            {
                IEventsContext file = EventsFactory.GetEventsContext(filePath, samplingFrequency);
                return new List<BtvEvent>(file.Events);
            }
            return new List<BtvEvent>();
        }

        public static void SaveEvents(string filePath, int samplingFrequency = 0)
        {
            SaveEvents(Session.Current, filePath, samplingFrequency);
        }

        /// <summary>
        /// Throws when the file cannot be written, so the caller can tell the user. The writers
        /// used to swallow their own errors, which made a failed save indistinguishable from a
        /// successful one.
        /// </summary>
        public static void SaveEvents(Session session, string filePath, int samplingFrequency = 0)
        {
            EventsFactory.SaveEvents(filePath, session.Events, samplingFrequency);
        }

        /// <summary>
        /// Adds a copy of the event and reports whether anything was actually added: an
        /// Equals-duplicate is skipped and returns false, so callers must not propagate the add
        /// to the UI lists / trace GameObjects (which are kept index-parallel with Events).
        /// </summary>
        public static bool AddEvent(BtvEvent Event)
        {
            return AddEvent(Session.Current, Event);
        }

        public static bool AddEvent(Session session, BtvEvent Event)
        {
            BtvEvent EventToAdd = new BtvEvent(Event);
            if (session.Events.Contains(EventToAdd))
                return false;

            session.Events.Add(EventToAdd);
            return true;
        }

        public static void UpdateEvent(BtvEvent ModifiedEvent, BtvEvent OriginalEvent)
        {
            bool UpdateDuration = ModifiedEvent.Duration != OriginalEvent.Duration;
            bool UpdateSite = ModifiedEvent.SiteOfInterest != OriginalEvent.SiteOfInterest;
            bool UpdateElectrodeDefault = ModifiedEvent.SiteOfInterest == "";
            int Id = GetEventId(OriginalEvent);
            if (Id != -1)
            {
                Events[Id] = new BtvEvent(ModifiedEvent);
                if (UpdateDuration)
                {
                    Events[Id].Correlation = null;
                    Events[Id].Correlation2D = null;
                }

                // The 1D correlation is computed against the site of interest: changing the site
                // makes it stale, and the brain would keep displaying the previous electrode's
                // correlations. The 2D matrix is all-pairs and does not depend on the event site.
                if (UpdateSite)
                {
                    Events[Id].Correlation = null;
                }

                if (UpdateElectrodeDefault)
                {
                    Events[Id].SiteOfInterest = TracesService.ElectrodeName(0);
                    Events[Id].SecondSiteOfInterest = TracesService.ElectrodeName(1);
                }
            }
        }

        /// <summary>
        /// Drops every stored 1D correlation. Called when the user navigates to another
        /// electrode: the brain coloring would otherwise keep displaying correlations computed
        /// against a site the user is no longer inspecting. The 2D matrices stay - their
        /// displayed row follows the selected electrode by design.
        /// </summary>
        public static void ClearCorrelations()
        {
            ClearCorrelations(Session.Current);
        }

        public static void ClearCorrelations(Session session)
        {
            for (int i = 0; i < session.Events.Count; i++)
            {
                session.Events[i].Correlation = null;
            }
        }

        public static void RemoveEvent(BtvEvent Event)
        {
            BtvEvent EventToRemove = new BtvEvent(Event);
            if (Events.Contains(EventToRemove))
            {
                bool result = Events.Remove(EventToRemove);
                BtvLog.Log("Event has been removed : " + result);
            }
        }

        public static void RemoveEventAt(int ID)
        {
            RemoveEventAt(Session.Current, ID);
        }

        public static void RemoveEventAt(Session session, int ID)
        {
            if (ID >= 0 && ID < session.Events.Count)
            {
                session.Events.RemoveAt(ID);
                BtvLog.Log("Event has been removed");
            }
        }

        public static int GetEventId(BtvEvent Event)
        {
            return GetEventId(Session.Current, Event);
        }

        public static int GetEventId(Session session, BtvEvent Event)
        {
            // Match the full event identity (BtvEvent.Equals), not just the timestamp: two events
            // at the same millisecond used to resolve to the wrong index. Returns -1 if absent
            // (the old .First() threw); callers guard on a negative result.
            return session.Events.FindIndex(e => e.Equals(Event));
        }

        public static List<int> FindIndexes(int SearchValue)
        {
            return FindIndexes(Session.Current, SearchValue);
        }

        public static List<int> FindIndexes(Session session, int SearchValue)
        {
            return session.Events.Select((item, index) => new { Item = item, Index = index })
             .Where(x => x.Item.Code == SearchValue)
             .Select(x => x.Index)
             .ToList();
        }

        public static int GetEventCount(Session session)
        {
            return session.Events.Count;
        }

        public static IReadOnlyList<BtvEvent> GetEvents(Session session)
        {
            return session.Events;
        }

        public static BtvEvent GetEvent(Session session, int index)
        {
            return session.Events[index];
        }

        public static List<BtvEvent> FindEvents(Session session, Predicate<BtvEvent> predicate)
        {
            return session.Events.FindAll(predicate);
        }

        // First index whose event start time is >= ms. Relies on Events being sorted ascending by
        // TimeInMilliSeconds - the invariant kept by Load() and by SortBySample() (called after
        // every AddEvent). The window queries below use it to stop scanning once events start at or
        // after the right border, instead of walking the whole list every video tick.
        // Precondition of the queries: left <= right (always true - the window is [ms - period, ms]).
        private static int LowerBoundByTime(float ms)
        {
            return LowerBoundByTime(Events, ms);
        }

        private static int LowerBoundByTime(List<BtvEvent> events, float ms)
        {
            int lo = 0;
            int hi = events.Count;
            while (lo < hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                if (events[mid].TimeInMilliSeconds < ms)
                    lo = mid + 1;
                else
                    hi = mid;
            }
            return lo;
        }

        // The allocating single-category query methods remain as independent reference
        // implementations for EventsServiceWindowQueryTests. Runtime tick paths should use
        // CollectEventIdsForWindow with reusable buffers.
        /// <summary>
        ///
        /// [BeginEvent ---------------------------------------------- EndEvent]
        ///         [LeftBorderWindow ------------ RightBorderWindow]
        ///
        /// Condition => Events.TimeInMilliSeconds < Left && Events.TimeInMilliSeconds + Events.duration > Right
        /// </summary>
        /// <param name="LeftBorderMilliSeconds"></param>
        /// <param name="RightBorderMilliSeconds"></param>
        /// <returns></returns>
        public static List<int> GetEventIdsBiggerThanWindow(int LeftBorderMilliSeconds, int RightBorderMilliSeconds)
        {
            List<int> result = new List<int>();
            int hi = LowerBoundByTime(RightBorderMilliSeconds);
            for (int i = 0; i < hi; i++)
            {
                BtvEvent e = Events[i];
                if (e.TimeInMilliSeconds <= LeftBorderMilliSeconds && e.TimeInMilliSeconds + e.Duration >= RightBorderMilliSeconds)
                    result.Add(i);
            }
            return result;
        }

        /// <summary>
        ///
        ///                     [BeginEvent --- EndEvent]
        ///         [LeftBorderWindow ------------ RightBorderWindow]
        ///
        /// Condition => Left < TimeInMilliSeconds < Right && Left < TimeInMilliSeconds + Events.Duration < Right
        /// </summary>
        /// <param name="LeftBorderMilliSeconds"></param>
        /// <param name="RightBorderMilliSeconds"></param>
        /// <returns></returns>
        public static List<int> GetEventIdsInsideWindow(int LeftBorderMilliSeconds, int RightBorderMilliSeconds)
        {
            List<int> result = new List<int>();
            int hi = LowerBoundByTime(RightBorderMilliSeconds);
            for (int i = 0; i < hi; i++)
            {
                BtvEvent e = Events[i];
                float end = e.TimeInMilliSeconds + e.Duration;
                if (e.TimeInMilliSeconds < RightBorderMilliSeconds &&
                    e.TimeInMilliSeconds > LeftBorderMilliSeconds &&
                    end >= LeftBorderMilliSeconds &&
                    end <= RightBorderMilliSeconds)
                    result.Add(i);
            }
            return result;
        }

        /// <summary>
        ///
        ///                                             [BeginEvent -- EndEvent]
        ///         [LeftBorderWindow ------------ RightBorderWindow]
        ///
        /// Condition => Left < TimeInMilliSeconds < Right && Events.TimeInMilliSeconds + Events.Duration > Right
        /// </summary>
        /// <param name="LeftBorderMilliSeconds"></param>
        /// <param name="RightBorderMilliSeconds"></param>
        /// <returns></returns>
        public static List<int> GetEventIdsEnteringWindow(int LeftBorderMilliSeconds, int RightBorderMilliSeconds)
        {
            List<int> result = new List<int>();
            int hi = LowerBoundByTime(RightBorderMilliSeconds);
            for (int i = 0; i < hi; i++)
            {
                BtvEvent e = Events[i];
                if (e.TimeInMilliSeconds < RightBorderMilliSeconds &&
                    e.TimeInMilliSeconds > LeftBorderMilliSeconds &&
                    e.TimeInMilliSeconds + e.Duration >= RightBorderMilliSeconds)
                    result.Add(i);
            }
            return result;
        }

        /// <summary>
        ///
        /// [BeginEvent -- EndEvent]
        ///         [LeftBorderWindow ------------ RightBorderWindow]
        ///
        /// Condition => Events.TimeInMilliSeconds > Left && Left < Events.TimeInMilliSeconds + Events.Duration < Right
        /// </summary>
        /// <param name="LeftBorderMilliSeconds"></param>
        /// <param name="RightBorderMilliSeconds"></param>
        /// <returns></returns>
        public static List<int> GetEventIdsExitingWindow(int LeftBorderMilliSeconds, int RightBorderMilliSeconds)
        {
            List<int> result = new List<int>();
            int hi = LowerBoundByTime(RightBorderMilliSeconds);
            for (int i = 0; i < hi; i++)
            {
                BtvEvent e = Events[i];
                float end = e.TimeInMilliSeconds + e.Duration;
                if (e.TimeInMilliSeconds < LeftBorderMilliSeconds &&
                    end >= LeftBorderMilliSeconds &&
                    end <= RightBorderMilliSeconds)
                    result.Add(i);
            }
            return result;
        }

        /// <summary>
        /// Single-pass, allocation-free equivalent of calling all four GetEventIds*Window queries
        /// with the same window. Clears then fills the caller-owned lists with indexes into
        /// <see cref="Events"/> for the events that, respectively, span / enter / sit inside / exit
        /// the [left, right] window. One binary search plus one bounded scan replaces four full
        /// list scans per trace per video tick.
        ///
        /// Each event is tested against all four predicates independently (not else-if), so an
        /// event that satisfies more than one - e.g. starting before the window and ending exactly
        /// on the right border, which matches both "bigger" and "exiting" - lands in every matching
        /// bucket, exactly as the four separate queries did.
        /// </summary>
        public static void CollectEventIdsForWindow(int LeftBorderMilliSeconds, int RightBorderMilliSeconds,
            List<int> biggerThanWindow, List<int> enteringWindow, List<int> insideWindow, List<int> exitingWindow)
        {
            CollectEventIdsForWindow(Session.Current, LeftBorderMilliSeconds, RightBorderMilliSeconds,
                biggerThanWindow, enteringWindow, insideWindow, exitingWindow);
        }

        public static void CollectEventIdsForWindow(Session session, int LeftBorderMilliSeconds, int RightBorderMilliSeconds,
            List<int> biggerThanWindow, List<int> enteringWindow, List<int> insideWindow, List<int> exitingWindow)
        {
            biggerThanWindow.Clear();
            enteringWindow.Clear();
            insideWindow.Clear();
            exitingWindow.Clear();

            int hi = LowerBoundByTime(session.Events, RightBorderMilliSeconds);
            for (int i = 0; i < hi; i++)
            {
                BtvEvent e = session.Events[i];
                float start = e.TimeInMilliSeconds;
                float end = start + e.Duration;

                if (start <= LeftBorderMilliSeconds && end >= RightBorderMilliSeconds)
                    biggerThanWindow.Add(i);
                if (start > LeftBorderMilliSeconds && start < RightBorderMilliSeconds && end >= RightBorderMilliSeconds)
                    enteringWindow.Add(i);
                if (start > LeftBorderMilliSeconds && start < RightBorderMilliSeconds && end >= LeftBorderMilliSeconds && end <= RightBorderMilliSeconds)
                    insideWindow.Add(i);
                if (start < LeftBorderMilliSeconds && end >= LeftBorderMilliSeconds && end <= RightBorderMilliSeconds)
                    exitingWindow.Add(i);
            }
        }

        public static void SortBySample()
        {
            SortBySample(Session.Current);
        }

        public static void SortBySample(Session session)
        {
            session.Events = session.Events.OrderBy(x => x.TimeInMilliSeconds).ToList();
        }
    }
}
