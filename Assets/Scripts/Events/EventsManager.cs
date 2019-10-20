using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BTV.Services.EventsService;
using CielaSpike;
using UnityEngine;

/// <summary>
/// Class responsible for managing the events of the scene
/// </summary>
/// <remarks>
/// This class is the entry point for all messages coming from other modules and can load, save, add, delete and edit events.
/// It is also used to send events to sub-modules :
///     - Traces
///     - EventsList
///     - EventsTexture
/// </remarks>
public class EventsManager : MonoBehaviour
{
    [SerializeField]
    EventList m_EventsList = null;
    [SerializeField]
    EventsTexture m_EventsTexture = null;
    [SerializeField]
    CustomVideoPlayer m_videoPlayer = null;

    private void Start()
    {
        Messenger.Default.Register<UiToEventsMessage>(this, OnEventsParametersMessage, MessageContext.UiToEvents);
        Messenger.Default.Register<EventsModificationMessage>(this, OnEventsModificationMessage, MessageContext.EventsModificationMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.UiToEvents);
        Messenger.Default.Unregister(this, MessageContext.EventsModificationMessage);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
            GoToPreviousEvent();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
            GoToNextEvent();
    }

    private void OnEventsParametersMessage(UiToEventsMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                {
                    Debug.Log("Load File");
                    LoadEvents(message.FilePathToLoad);
                    break;
                }
            case 1:
                {
                    Debug.Log("Save File");
                    EventsService.SaveEvents(message.FilePathToSave);
                    break;
                }
            case 2:
                {
                    Debug.Log("Toggle Add Event");
                    EventsToTraceMessage EventsMessage = new EventsToTraceMessage
                    {
                        TaskToExecute = 0,
                        IsAddEventsOn = message.IsAddEventsOn
                    };
                    Messenger.Default.Send(EventsMessage, MessageContext.EventsToTraceMessage);
                    break;
                }
            case 3:
                {
                    Debug.Log("Toggle Show Event");
                    EventsToTraceMessage EventsMessage = new EventsToTraceMessage
                    {
                        TaskToExecute = 1,
                        IsShowEventsOn = message.IsShowEventsOn
                    };
                    Messenger.Default.Send(EventsMessage, MessageContext.EventsToTraceMessage);
                    break;
                }
            case 4:
                {
                    Debug.Log("Delete Selected Notes");
                    ApplicationState.displayConfirmation("Deleting Notes", "You are going to delete " + m_EventsList.NumberOfItemSelected + " Notes, are you sure ? ", m_EventsList.DeleteSelectedEvents, () => { });
                    break;
                }
        }
    }

    private void OnEventsModificationMessage(EventsModificationMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                {
                    Debug.Log("Add Event");
                    AddEvent(message.Event);
                    break;
                }
            case 1:
                {
                    Debug.Log("Modify Event");
                    UpdateEvent(message.Event, message.EventMemory);
                    break;
                }
            case 2:
                {
                    Debug.Log("Delete Event");
                    DeleteEvent(message.Event);
                    break;
                }
            case 3:
                {
                    Debug.Log("Edit Event");
                    //Events to trace with parent gameobject (or mouse position) and event
                    EventsToTraceMessage messageToTrace = new EventsToTraceMessage
                    {
                        TaskToExecute = 2,
                        Event = message.Event
                    };
                    Messenger.Default.Send(messageToTrace, MessageContext.EventsToTraceMessage);
                    break;
                }
            case 4:
                {
                    Debug.Log("Correlation 1D");
                    StartCoroutine(ProcessCorrelation(message.Event));
                    break;
                }
            case 5:
                {
                    Debug.Log("Correlation 2D");
                    StartCoroutine(Process2dCorrelation(message.Event));
                    break;
                }
        }
    }

    private void GoToPreviousEvent()
    {
        if (EventsService.Events.Count > 0)
        {
            long TimeInSec = m_videoPlayer.videoInterface.currentTime / 1000;
            long TimeInSample = TimeInSec * ApplicationState.Window1.TraceEeg.SamplingFrequency;
            int index = EventsService.Events.Select(x=>x.sample).ToList().BinarySearch((int)TimeInSample);
            if (Math.Abs(index) - 1 == 0)
            {
                int TimeInMilliSec = ((EventsService.Events[0].sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) * 1000);
                m_videoPlayer.changeTimeClick(TimeInMilliSec);
                m_videoPlayer.setTime(TimeInMilliSec);
            }
            else
            {
                int currentPos = Math.Abs(index) - 1;
                int TimeInMilliSec = ((EventsService.Events[currentPos - 1].sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) * 1000);
                m_videoPlayer.changeTimeClick(TimeInMilliSec);
                m_videoPlayer.setTime(TimeInMilliSec);
            }
        }
    }

    private void GoToNextEvent()
    {
        if (EventsService.Events.Count > 0)
        {
            long TimeInSec = m_videoPlayer.videoInterface.currentTime / 1000;
            long TimeInSample = TimeInSec * ApplicationState.Window1.TraceEeg.SamplingFrequency;
            int index = EventsService.Events.Select(x => x.sample).ToList().BinarySearch((int)TimeInSample);
            int currentPos = Math.Abs(index) - 1;
            if (currentPos + 1 < EventsService.Events.Count)
            {
                int TimeInMilliSec = ((EventsService.Events[currentPos + 1].sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) * 1000);
                m_videoPlayer.changeTimeClick(TimeInMilliSec);
                m_videoPlayer.setTime(TimeInMilliSec);
            }
        }
    }

    //TODO : Reset Everything or allow to load data over already existing events ? 
    private void LoadEvents(string filePath)
    {
        if (File.Exists(filePath))
        {
            EventsService.Load(filePath);
            //load in UI List
            m_EventsList.DeleteAllEvents();
            m_EventsList.LoadEvents(EventsService.Events);
            //load in Scrollbar Texture
            m_EventsTexture.RemoveAllEvents();
            m_EventsTexture.AddEvents(EventsService.Events);
            //send events to traces
            for (int i = 0; i < EventsService.Events.Count; i++)
            {
                EventsToTraceMessage message = new EventsToTraceMessage
                {
                    TaskToExecute = 3,
                    Event = EventsService.Events[i],
                    EventIndex = i
                };
                Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
            }
        }
    }

    private void AddEvent(TraceEvent Event)
    {
        EventsService.AddEvent(Event);
        EventsService.SortBySample();
        int Id = EventsService.GetEventId(Event);

        m_EventsTexture.AddEvent(Event);
        m_EventsList.AddEvent(Event);

        //Send message to Add to traces
        EventsToTraceMessage message = new EventsToTraceMessage
        {
            TaskToExecute = 3,
            EventIndex = Id,
            Event = Event
        };
        Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
    }

    private void UpdateEvent(TraceEvent modifyiedEvent, TraceEvent previousEvent)
    {
        int Id = EventsService.GetEventId(previousEvent);
        if (Id != -1)
        {
            bool UpdateEventUI = (modifyiedEvent.duration - previousEvent.duration == modifyiedEvent.duration) || (modifyiedEvent.duration - previousEvent.duration == -previousEvent.duration);

            m_EventsTexture.RemoveEvent(previousEvent);
            EventsService.UpdateEvent(modifyiedEvent, previousEvent);
            m_EventsTexture.AddEvent(EventsService.Events[Id]);

            var eventToChangeObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Event - " + previousEvent.sample);
            if (UpdateEventUI)
            {
                eventToChangeObjects.ElementAt(0).GetComponent<EventTrace>().DeleteMe();
                m_EventsList.AddEvent(EventsService.Events[Id]);
            }

            //Update object on Traces UI
            foreach (var eventToChange in eventToChangeObjects)
            {
                eventToChange.GetComponent<EventTrace>().UpdateEvent(EventsService.Events[Id]);
            }

            m_EventsList.Refresh();
        }
    }

    private void DeleteEvent(TraceEvent Event)
    {
        int Id = EventsService.GetEventId(Event);
        EventsService.RemoveEventAt(Id);

        m_EventsTexture.RemoveEvent(Event);
        m_EventsList.DeleteEvent(Id);

        //Send message to delete from traces
        EventsToTraceMessage message = new EventsToTraceMessage
        {
            TaskToExecute = 4,
            EventIndex = Id
        };
        Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
    }

    IEnumerator ProcessCorrelation(TraceEvent currentEvent)
    {
        yield return Ninja.JumpBack;
        ApplicationState.coroutineManager.StartCoroutine(c_Correlation(currentEvent));
        yield return Ninja.JumpToUnity;
    }

    IEnumerator c_Correlation(TraceEvent currentEvent)
    {
        int nbElec = ApplicationState.Window1.TraceEeg.FileHandle.NumberOfElectrodes;
        //List<int> ids = Objects.Select((item, index) => new { Item = item, Index = index })
        //                         .Where(x => x.Item.sample == currentEvent.sample)
        //                         .Select(x => x.Index)
        //                         .ToList();

        //Objects[ids[0]].correlationArray = new float[nbElec];
        //Objects[ids[0]].correlation2DArray = null;

        //int beginSample = Objects[ids[0]].sample;
        //int durationSample = (Objects[ids[0]].duration / 1000) * Objects[ids[0]].samplingFrequency;

        //int idBase = ApplicationState.Window1.TraceEeg.FileHandle.GetElectrodeIDFromElectrodeName(currentEvent.elecOfInterest);
        //if (idBase != -1)
        //{
        //    int[] sizes = new int[5] { idBase, nbElec, beginSample, durationSample, ApplicationState.Window1.TraceEeg.FileHandle.NumberOfSample };
        //    //pearsonCoefficientsCorrelation(Objects[ids[0]].correlationArray, ApplicationState.Window1.TraceEeg.FileHandle.eegData, sizes);
        //}
        //else
        //{
        //    if (currentEvent.elecOfInterest.StartsWith("AUD"))
        //    {
        //        int[] sizes = new int[4] { nbElec, beginSample, durationSample, ApplicationState.Window1.TraceEeg.FileHandle.NumberOfSample };
        //        //pearsonCoefficientsCorrelation2(Objects[ids[0]].correlationArray, m_videoPlayer.audioWav.getAudioHandle(m_videoPlayer.audioWav.idAudioHandle), ApplicationState.Window1.TraceEeg.FileHandle.eegData, sizes);
        //    }
        //}
        yield return null;
    }

    IEnumerator Process2dCorrelation(TraceEvent currentEvent)
    {
        yield return Ninja.JumpBack;
        ApplicationState.coroutineManager.StartCoroutine(c_Correlation2d(currentEvent));
        yield return Ninja.JumpToUnity;
    }

    IEnumerator c_Correlation2d(TraceEvent currentEvent)
    {
        int nbElec = ApplicationState.Window1.TraceEeg.FileHandle.NumberOfElectrodes;
        //List<int> ids = Objects.Select((item, index) => new { Item = item, Index = index })
        //                         .Where(x => x.Item.sample == currentEvent.sample)
        //                         .Select(x => x.Index)
        //                         .ToList();

        //Objects[ids[0]].correlationArray = null;
        //Objects[ids[0]].correlation2DArray = new float[nbElec][];
        //for (int i = 0; i < Objects[ids[0]].correlation2DArray.Length; i++)
        //    Objects[ids[0]].correlation2DArray[i] = new float[nbElec];

        //int beginSample = Objects[ids[0]].sample;
        //int durationSample = (Objects[ids[0]].duration / 1000) * Objects[ids[0]].samplingFrequency;

        //for (int i = 0; i < Objects[ids[0]].correlation2DArray.Length; i++)
        //{
        //    int[] sizes = new int[5] { i, nbElec, beginSample, durationSample, ApplicationState.Window1.TraceEeg.FileHandle.NumberOfSample };
        //    //pearsonCoefficientsCorrelation(Objects[ids[0]].correlation2DArray[i], ApplicationState.Window1.TraceEeg.FileHandle.eegData, sizes);
        //}

        yield return null;
    }

    #region DLLImport
    [DllImport("BTVReplayLibraryC++", EntryPoint = "pearsonCoefficientsCorrelation", CallingConvention = CallingConvention.Cdecl)]
    static private extern void pearsonCoefficientsCorrelation(float[] coeffs, float[] eegData, int[] sizes);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "pearsonCoefficientsCorrelation2", CallingConvention = CallingConvention.Cdecl)]
    static private extern void pearsonCoefficientsCorrelation2(float[] coeffs, float[] baseArray, float[] eegData, int[] sizes);
    #endregion
}
