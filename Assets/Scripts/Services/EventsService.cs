using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTV.Services.EventsService
{
    public static class EventsService
    {
        public static List<TraceEvent> Events { get; set; } = new List<TraceEvent>();

        public static void Load(string filePath)
        {
            if (File.Exists(filePath))
            {
                IEventsContext file = EventsFactory.GetEventsContext(filePath);

                Events = new List<TraceEvent>();
                for (int i = 0; i < file.Events.Count; i++)
                {
                    Events.Add(new TraceEvent(file.Events[i]));
                    UnityEngine.Debug.Log(Events[i].code + " et " + Events[i].sample);
                }
            }
        }

        public static void SaveEvents(string filePath)
        {
            string posFilePath = filePath.Replace(".pos", "_btv.pos");
            EventsFactory.SaveEvents(posFilePath, Events);
            string btvFilePath = filePath.Replace(".pos", ".btv");
            EventsFactory.SaveEvents(btvFilePath, Events);
        }

        public static void AddEvent(TraceEvent Event)
        {
            TraceEvent EventToAdd = new TraceEvent(Event);
            if (!Events.Contains(EventToAdd))
            {
                Events.Add(EventToAdd);
            }
        }

        public static void UpdateEvent(TraceEvent ModifiedEvent, TraceEvent OriginalEvent)
        {
            bool UpdateDuration = ModifiedEvent.duration != OriginalEvent.duration;
            bool UpdateElectrodeDefault = ModifiedEvent.elecOfInterest == "";
            int Id = GetEventId(OriginalEvent);
            if (Id != -1)
            {
                Events[Id] = new TraceEvent(ModifiedEvent);
                if (UpdateDuration)
                {
                    Events[Id].correlationArray = null;
                    Events[Id].correlation2DArray = null;
                }

                if (UpdateElectrodeDefault)
                {
                    Events[Id].elecOfInterest = ApplicationState.Window1.TraceEeg.LabelElectrode;
                    Events[Id].secondElecOfInterest = ApplicationState.Window2.TraceEeg.LabelElectrode;
                }
            }
        }

        public static void RemoveEvent(TraceEvent Event)
        {
            TraceEvent EventToRemove = new TraceEvent(Event);
            if (Events.Contains(EventToRemove))
            {
                bool result = Events.Remove(EventToRemove);
                UnityEngine.Debug.Log("Event has been removed : " + result);
            }
        }

        public static int GetEventId(TraceEvent Event)
        {
            return Events.Select((item, index) => new { Item = item, Index = index })
                         .Where(x => x.Item.sample == Event.sample)
                         .Select(x => x.Index)
                         .First();
        }

        /// <summary>
        /// 
        /// [BeginEvent ---------------------------------------------- EndEvent]
        ///         [LeftBorderWindow ------------ RightBorderWindow]
        /// 
        /// Condition => Events.Sample < Left && Events.sample + Events.duration > Right
        /// </summary>
        /// <param name="LeftBorderSample"></param>
        /// <param name="RightBorderSample"></param>
        /// <param name="SamplingFrequency"></param>
        /// <returns></returns>
        public static List<int> GetEventIdsBiggerThanWindow(int LeftBorderSample, int RightBorderSample, float SamplingFrequency)
        {
            return Events.Select((item, index) => new { Item = item, Index = index })
                    .Where(x => (x.Item.sample <= LeftBorderSample && (x.Item.sample + (x.Item.duration * (SamplingFrequency / 1000)) >= RightBorderSample)))
                    .Select(x => x.Index)
                    .ToList();
        }

        /// <summary>
        /// 
        ///                     [BeginEvent --- EndEvent]
        ///         [LeftBorderWindow ------------ RightBorderWindow]
        /// 
        /// Condition => Left < Events.Sample < Right && Left < Events.Sample + Events.Duration < Right 
        /// </summary>
        /// <param name="LeftBorderSample"></param>
        /// <param name="RightBorderSample"></param>
        /// <param name="SamplingFrequency"></param>
        /// <returns></returns>
        public static List<int> GetEventIdsInsideWindow(int LeftBorderSample, int RightBorderSample, float SamplingFrequency)
        {
           return Events.Select((item, index) => new { Item = item, Index = index })
                    .Where(x => ((x.Item.sample < RightBorderSample) &&
                                (x.Item.sample > LeftBorderSample) &&
                                (x.Item.sample + (x.Item.duration * ((float)x.Item.samplingFrequency / 1000)) >= LeftBorderSample) &&
                                (x.Item.sample + (x.Item.duration * ((float)SamplingFrequency / 1000)) <= RightBorderSample)))
                    .Select(x => x.Index)
                    .ToList();
        }

        /// <summary>
        /// 
        ///                                             [BeginEvent -- EndEvent]
        ///         [LeftBorderWindow ------------ RightBorderWindow]
        /// 
        /// Condition => Left < Events.Sample < Right && Events.Sample + Events.Duration > Right 
        /// </summary>
        /// <param name="LeftBorderSample"></param>
        /// <param name="RightBorderSample"></param>
        /// <param name="SamplingFrequency"></param>
        /// <returns></returns>
        public static List<int> GetEventIdsEnteringWindow(int LeftBorderSample, int RightBorderSample, float SamplingFrequency)
        {
           return Events.Select((item, index) => new { Item = item, Index = index })
                    .Where(x => (x.Item.sample < RightBorderSample && x.Item.sample > LeftBorderSample && (x.Item.sample + (x.Item.duration * (SamplingFrequency / 1000)) >= RightBorderSample)))
                    .Select(x => x.Index)
                    .ToList();
        }

        /// <summary>
        /// 
        /// [BeginEvent -- EndEvent]
        ///         [LeftBorderWindow ------------ RightBorderWindow]
        /// 
        /// Condition => Events.Sample > Left && Left < Events.Sample + Events.Duration < Right 
        /// </summary>
        /// <param name="LeftBorderSample"></param>
        /// <param name="RightBorderSample"></param>
        /// <param name="SamplingFrequency"></param>
        /// <returns></returns>
        public static List<int> GetEventIdsExitingWindow(int LeftBorderSample, int RightBorderSample, float SamplingFrequency)
        {
            return Events.Select((item, index) => new { Item = item, Index = index })
                                          .Where(x => ((x.Item.sample < LeftBorderSample) &&
                                                       (x.Item.sample + (x.Item.duration * (SamplingFrequency / 1000)) >= LeftBorderSample) &&
                                                       (x.Item.sample + (x.Item.duration * (SamplingFrequency / 1000)) <= RightBorderSample)))
                                          .Select(x => x.Index)
                                          .ToList();
        }

        public static void SortBySample()
        {
            Events = Events.OrderBy(x => x.sample).ToList();
        }
    }
}
