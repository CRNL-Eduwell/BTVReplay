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
        public static List<BtvEvent> Events { get; set; } = new List<BtvEvent>();

        public static void Reset()
        {
            Events = new List<BtvEvent>();
        }

        public static void Load(string filePath, int samplingFrequency = 0)
        {
            if (File.Exists(filePath))
            {
                IEventsContext file = EventsFactory.GetEventsContext(filePath, samplingFrequency);

                Events = new List<BtvEvent>();
                for (int i = 0; i < file.Events.Count; i++)
                {
                    Events.Add(new BtvEvent(file.Events[i]));
                }

                // Keep the service sorted by start time the same way AddEvent does. The window
                // queries below binary-search on this order, and the trace GameObject lists are
                // built index-parallel to Events, so an unsorted load would otherwise desync them
                // the first time the user edits an event (AddEvent re-sorts the whole list).
                SortBySample();
            }
        }

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
            try
            {
                EventsFactory.SaveEvents(filePath, Events, samplingFrequency);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Error while saving file events.");
                UnityEngine.Debug.LogError(ex.Message);
            }
        }

        public static void AddEvent(BtvEvent Event)
        {
            BtvEvent EventToAdd = new BtvEvent(Event);
            if (!Events.Contains(EventToAdd))
            {
                Events.Add(EventToAdd);
            }
        }

        public static void UpdateEvent(BtvEvent ModifiedEvent, BtvEvent OriginalEvent)
        {
            bool UpdateDuration = ModifiedEvent.Duration != OriginalEvent.Duration;
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

                if (UpdateElectrodeDefault)
                {
                    Events[Id].SiteOfInterest = TracesService.ElectrodeName(0);
                    Events[Id].SecondSiteOfInterest = TracesService.ElectrodeName(1);
                }
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
            if (ID >= 0 && ID < Events.Count)
            {
                Events.RemoveAt(ID);
                BtvLog.Log("Event has been removed");
            }
        }

        public static int GetEventId(BtvEvent Event)
        {
            // Match the full event identity (BtvEvent.Equals), not just the timestamp: two events
            // at the same millisecond used to resolve to the wrong index. Returns -1 if absent
            // (the old .First() threw); callers guard on a negative result.
            return Events.FindIndex(e => e.Equals(Event));
        }

        public static List<int> FindIndexes(int SearchValue)
        {
            return Events.Select((item, index) => new { Item = item, Index = index })
             .Where(x => x.Item.Code == SearchValue)
             .Select(x => x.Index)
             .ToList();
        }

        // First index whose event start time is >= ms. Relies on Events being sorted ascending by
        // TimeInMilliSeconds - the invariant kept by Load() and by SortBySample() (called after
        // every AddEvent). The window queries below use it to stop scanning once events start at or
        // after the right border, instead of walking the whole list every video tick.
        // Precondition of the queries: left <= right (always true - the window is [ms - period, ms]).
        private static int LowerBoundByTime(float ms)
        {
            int lo = 0;
            int hi = Events.Count;
            while (lo < hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                if (Events[mid].TimeInMilliSeconds < ms)
                    lo = mid + 1;
                else
                    hi = mid;
            }
            return lo;
        }

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
            biggerThanWindow.Clear();
            enteringWindow.Clear();
            insideWindow.Clear();
            exitingWindow.Clear();

            int hi = LowerBoundByTime(RightBorderMilliSeconds);
            for (int i = 0; i < hi; i++)
            {
                BtvEvent e = Events[i];
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
            Events = Events.OrderBy(x => x.TimeInMilliSeconds).ToList();
        }
    }
}
