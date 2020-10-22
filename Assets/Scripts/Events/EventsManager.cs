using System;
using System.Collections;
using System.IO;
using System.Linq;
using BTV.Data;
using BTV.Services.CalculationService;
using BTV.Services.EventsService;
using BTV.UI;
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
///     - TracesDisplayer
///     - EventsTexture
/// </remarks>
public class EventsManager : MonoBehaviour
{
    [SerializeField]
    EventList m_EventsList = null;
    [SerializeField]
    EventsTexture m_EventsTexture = null;
    [SerializeField]
    TracesDisplayer m_TracesDisplayer = null;
    [SerializeField]
    CustomVideoPlayer m_videoPlayer = null;

    private GameObject m_InputFieldWindowPrefabs = null;

    private void Awake()
    {
        m_InputFieldWindowPrefabs = Resources.Load("Prefabs/UIElements/InputFieldWindow", typeof(GameObject)) as GameObject;
    }

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
                    FileInfo file = new FileInfo(message.FilePathToLoad);
                    if (file.Extension.Equals(".pos"))
                    {
                        InputFieldWindow window = SpawFrequencyChoiceWindow();
                        window.Initialize("File Sample Rate", "Sampling Frequency (in Hz) ?", () => { LoadEvents(file.FullName, window.IntValue); window.Close(); }, () => { window.Close(); });
                    }
                    else
                    {
                        LoadEvents(message.FilePathToLoad);
                    }
                    break;
                }
            case 1:
                {
                    Debug.Log("Save File");
                    FileInfo file = new FileInfo(message.FilePathToSave);
                    if (file.Extension == ".pos")
                    {
                        string path = message.FilePathToSave.Replace(".pos", "_btv.pos");
                        InputFieldWindow window = SpawFrequencyChoiceWindow();
                        window.Initialize("File Sample Rate", "Sampling Frequency (in Hz) ?", () => { EventsService.SaveEvents(path, window.IntValue); window.Close(); }, () => { window.Close(); });
                    }
                    else
                    {
                        EventsService.SaveEvents(file.FullName);
                    }
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
                    ApplicationState.displayConfirmation("Deleting Notes", "You are going to delete " + ((ISelectionCountable)m_EventsList).NumberOfItemSelected + " Notes, are you sure ? ", () => { DeleteSelectedEvents(); }, () => { });
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
                        Event = message.Event,
                        ParentWindowIndex = message.ParentWindowIndex
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
            long videoTimeInMs = m_videoPlayer.VideoInterface.CurrentTime;
            int index = EventsService.Events.Select(x => x.TimeInMilliSeconds).ToList().BinarySearch(videoTimeInMs);
            if (index < 0) index = ~index - 1;

            if (index - 1 >= 0)
            {
                int eventTimeInMs = (int)EventsService.Events[index - 1].TimeInMilliSeconds;
                ModulesToVideoMessage messageToVideo = new ModulesToVideoMessage
                {
                    UpdateClickPosition = true,
                    TimeMilliseconds = eventTimeInMs
                };
                Messenger.Default.Send(messageToVideo, MessageContext.ModulesToVideoMessage);
            }
        }
    }

    private void GoToNextEvent()
    {
        if (EventsService.Events.Count > 0)
        {
            long videoTimeInMs = m_videoPlayer.VideoInterface.CurrentTime;
            int index = EventsService.Events.Select(x => x.TimeInMilliSeconds).ToList().BinarySearch(videoTimeInMs);
            index = (index < 0) ? ~index : index + 1;

            if (index + 1 <= EventsService.Events.Count)
            {
                int eventTimeInMs = (int)EventsService.Events[index].TimeInMilliSeconds;
                ModulesToVideoMessage messageToVideo = new ModulesToVideoMessage
                {
                    UpdateClickPosition = true,
                    TimeMilliseconds = eventTimeInMs
                };
                Messenger.Default.Send(messageToVideo, MessageContext.ModulesToVideoMessage);
            }
        }
    }

    private InputFieldWindow SpawFrequencyChoiceWindow()
    {
        GameObject viewGameObject = GameObject.Find("Windows");
        GameObject inputField = Instantiate(m_InputFieldWindowPrefabs, viewGameObject.transform);
        InputFieldWindow window = inputField.GetComponent<InputFieldWindow>();
        return window;
    }

    //TODO : Reset Everything or allow to load data over already existing events ? 
    private void LoadEvents(string filePath, int SamplingFrequency = 0)
    {
        if (File.Exists(filePath))
        {
            EventsService.Load(filePath, SamplingFrequency);
            //load in UI List
            m_EventsList.DeleteAllEvents();
            m_EventsList.LoadEvents(EventsService.Events);
            //load in Scrollbar Texture
            m_EventsTexture.RemoveAllEvents();
            m_EventsTexture.AddEvents(EventsService.Events);
            //load in TraceDisplayer Texture [TODO : might need to put some other messages or refactor existing one]
            m_TracesDisplayer.RemoveAllEvents();
            m_TracesDisplayer.AddEvents(EventsService.Events);
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

            EventsToTaskPerformanceMessage resetMessage = new EventsToTaskPerformanceMessage
            {
                TaskToExecute = 0
            };
            Messenger.Default.Send(resetMessage, MessageContext.EventsToTaskPerformanceMessage);
        }
    }

    private void AddEvent(BtvEvent Event)
    {
        EventsService.AddEvent(Event);
        EventsService.SortBySample();
        int id = EventsService.GetEventId(Event);

        m_EventsTexture.AddEvent(Event);
        m_EventsList.AddEvent(Event);
        m_TracesDisplayer.AddEvent(Event);

        //Send message to Add to traces
        EventsToTraceMessage message = new EventsToTraceMessage
        {
            TaskToExecute = 3,
            EventIndex = id,
            Event = Event
        };
        Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
        //Send message to task perf that an event was added
        EventsToTaskPerformanceMessage addedMessage = new EventsToTaskPerformanceMessage
        {
            TaskToExecute = 1
        };
        Messenger.Default.Send(addedMessage, MessageContext.EventsToTaskPerformanceMessage);
    }

    private void UpdateEvent(BtvEvent modifyiedEvent, BtvEvent previousEvent)
    {
        DeleteEvent(previousEvent);
        AddEvent(modifyiedEvent);

        EventsToTaskPerformanceMessage modifiedMessage = new EventsToTaskPerformanceMessage
        {
            TaskToExecute = 1
        };
        Messenger.Default.Send(modifiedMessage, MessageContext.EventsToTaskPerformanceMessage);
    }

    private void DeleteEvent(BtvEvent Event)
    {
        int id = EventsService.GetEventId(Event);
        EventsService.RemoveEventAt(id);

        m_EventsTexture.RemoveEvent(Event);
        m_EventsList.DeleteEvent(id);
        m_TracesDisplayer.RemoveEvent(Event);

        //Send message to delete from traces
        EventsToTraceMessage message = new EventsToTraceMessage
        {
            TaskToExecute = 4,
            EventIndex = id
        };
        Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);

        EventsToTaskPerformanceMessage deletedMessage = new EventsToTaskPerformanceMessage
        {
            TaskToExecute = EventsService.Events.Count == 0 ? 0 : 1
        };
        Messenger.Default.Send(deletedMessage, MessageContext.EventsToTaskPerformanceMessage);
    }

    private void DeleteSelectedEvents()
    {
        BtvEvent[] selectedEvents = m_EventsList.ObjectsSelected;
        int selectedCount = selectedEvents.Length;
        for (int i = selectedCount - 1; i >= 0; i--)
        {
            DeleteEvent(selectedEvents[i]);
        }
    }

    private IEnumerator ProcessCorrelation(BtvEvent currentEvent)
    {
        yield return Ninja.JumpBack;
        this.StartCoroutineAsync(c_Correlation(currentEvent));
        yield return Ninja.JumpToUnity;
    }

    private IEnumerator c_Correlation(BtvEvent currentEvent)
    {
        int samplingFrequency = ApplicationState.Module3D.Window1.TraceEeg.SamplingFrequency;
        int electrodeCount = ApplicationState.Module3D.Window1.TraceEeg.FileHandle.NumberOfElectrodes;
        int eventIndex = EventsService.GetEventId(currentEvent);

        EventsService.Events[eventIndex].Correlation = new float[electrodeCount];
        EventsService.Events[eventIndex].Correlation2D = null;

        int beginTimeSample = (int)(EventsService.Events[eventIndex].TimeInSeconds * samplingFrequency);
        int durationInSample = (EventsService.Events[eventIndex].Duration / 1000) * samplingFrequency;

        int indexBaseline = ApplicationState.Module3D.Window1.TraceEeg.FileHandle.GetElectrodeIDFromElectrodeName(currentEvent.SiteOfInterest);
        if (indexBaseline != -1)
        {
            int[] sizes = { beginTimeSample, durationInSample };
            BtvProgram container = ApplicationState.Module3D.Window1.TraceEeg.FileHandle;
            float[] baseline = container.Channels[indexBaseline].Data;
            for (int i = 0; i < electrodeCount; i++)
            {
                if (i == indexBaseline)
                    continue;

                float[] channel = container.Channels[i].Data;
                EventsService.Events[eventIndex].Correlation[i] = CalculationService.PearsonCorrelationCoefficients(baseline, channel, sizes);
            }
        }
        else
        {
            //Run Correlation against Audio trace
            if (currentEvent.SiteOfInterest.StartsWith("AUD"))
            {
                int[] sizes = { beginTimeSample, durationInSample };
                BtvChannel audioChannel = ApplicationState.Module3D.Window1.TraceAudio.ChannelHandle;
                float[] baseline = audioChannel.Data;

                for (int i = 0; i < electrodeCount; i++)
                {
                    float[] channel = ApplicationState.Module3D.Window1.TraceEeg.FileHandle.Channels[i].Data;
                    EventsService.Events[eventIndex].Correlation[i] = CalculationService.PearsonCorrelationCoefficients(baseline, channel, sizes);
                }
            }
        }
        yield return null;
    }

    private IEnumerator Process2dCorrelation(BtvEvent currentEvent)
    {
        yield return this.StartCoroutineAsync(c_Correlation2d(currentEvent), out Task AudioFilteringTask);
        switch (AudioFilteringTask.State)
        {
            case TaskState.Done:
                {
                    bool sameFile = ApplicationState.Module3D.Window1.TraceEeg.FileHandle == ApplicationState.Module3D.Window2.TraceEeg.FileHandle;
                    if (sameFile) break;

                    yield return Ninja.JumpToUnity;
                    string message = "Correlations have been processed" + "\nJust a reminder, you correlated data from two different files";
                    ApplicationState.displayMessage("Correlations Processing succeeded", "OK", message);
                    yield return Ninja.JumpBack;
                    break;
                }
            case TaskState.Error:
                {
                    yield return Ninja.JumpToUnity;
                    ApplicationState.displayMessage("Error Processing Correlations", "NOK", AudioFilteringTask.Exception.Message.ToString());
                    yield return Ninja.JumpBack;
                    break;
                }
        }
    }

    private IEnumerator c_Correlation2d(BtvEvent currentEvent)
    {
        int eventIndex = EventsService.GetEventId(currentEvent);

        BtvProgram container1 = ApplicationState.Module3D.Window1.TraceEeg.FileHandle;
        BtvProgram container2 = ApplicationState.Module3D.Window2.TraceEeg.FileHandle;

        bool sameDescription = container1.Description == container2.Description;
        bool sameElectrodeCount = container1.NumberOfElectrodes == container2.NumberOfElectrodes;
        bool sameSamplingFrequency = container1.Frequency.Value == container2.Frequency.Value;

        if (!sameDescription && !sameElectrodeCount) throw new ArgumentException("c_Correlation2d : Number of electrode is not the same in the two files used");
        if (!sameDescription && !sameSamplingFrequency) throw new ArgumentException("c_Correlation2d : Sampling Frequency is different beetween the two files used");

        int samplingFrequency = container2.Frequency.Value;
        int electrodeCount = container2.NumberOfElectrodes;

        EventsService.Events[eventIndex].Correlation = null;
        EventsService.Events[eventIndex].Correlation2D = new float[electrodeCount][];
        for (int i = 0; i < electrodeCount; i++)
            EventsService.Events[eventIndex].Correlation2D[i] = new float[electrodeCount];

        int beginTimeSample = (int)(EventsService.Events[eventIndex].TimeInSeconds * samplingFrequency);
        int durationInSample = (EventsService.Events[eventIndex].Duration / 1000) * samplingFrequency;

        int[] sizes = { beginTimeSample, durationInSample };
        for (int i = 0; i < electrodeCount; i++)
        {
            for (int j = 0; j < electrodeCount; j++)
            {
                if (i == j)
                    continue;
                float[] baseline = container1.Channels[i].Data;
                float[] channel = container2.Channels[j].Data;
                EventsService.Events[eventIndex].Correlation2D[i][j] = CalculationService.PearsonCorrelationCoefficients(baseline, channel, sizes);
            }
        }

        yield return null;
    }
}
