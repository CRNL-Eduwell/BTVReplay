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
                UnityEngine.Debug.Log("Event has been removed : " + result);
            }
        }

        public static void RemoveEventAt(int ID)
        {
            if (ID >= 0 && ID < Events.Count)
            {
                Events.RemoveAt(ID);
                UnityEngine.Debug.Log("Event has been removed");
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
            return Events.Select((item, index) => new { Item = item, Index = index })
                    .Where(x => (x.Item.TimeInMilliSeconds <= LeftBorderMilliSeconds && (x.Item.TimeInMilliSeconds + x.Item.Duration >= RightBorderMilliSeconds)))
                    .Select(x => x.Index)
                    .ToList();
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
           return Events.Select((item, index) => new { Item = item, Index = index })
                    .Where(x => ((x.Item.TimeInMilliSeconds < RightBorderMilliSeconds) &&
                                (x.Item.TimeInMilliSeconds > LeftBorderMilliSeconds) &&
                                (x.Item.TimeInMilliSeconds + x.Item.Duration >= LeftBorderMilliSeconds) &&
                                (x.Item.TimeInMilliSeconds + x.Item.Duration <= RightBorderMilliSeconds)))
                    .Select(x => x.Index)
                    .ToList();
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
           return Events.Select((item, index) => new { Item = item, Index = index })
                    .Where(x => (x.Item.TimeInMilliSeconds < RightBorderMilliSeconds && x.Item.TimeInMilliSeconds > LeftBorderMilliSeconds && (x.Item.TimeInMilliSeconds + x.Item.Duration) >= RightBorderMilliSeconds))
                    .Select(x => x.Index)
                    .ToList();
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
            return Events.Select((item, index) => new { Item = item, Index = index })
                                          .Where(x => (x.Item.TimeInMilliSeconds < LeftBorderMilliSeconds) &&
                                                      (x.Item.TimeInMilliSeconds + x.Item.Duration >= LeftBorderMilliSeconds) &&
                                                      (x.Item.TimeInMilliSeconds + x.Item.Duration <= RightBorderMilliSeconds))
                                          .Select(x => x.Index)
                                          .ToList();
        }

        public static void SortBySample()
        {
            Events = Events.OrderBy(x => x.TimeInMilliSeconds).ToList();
        }
    }
}
