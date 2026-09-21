using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BTV.Data;
using BTV.Services.CalculationService;
using BTV.Services.EventsService;
using BTV.Services.CodeMatchingService;
using BTV.Services;
using BTV.UI;
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
    EventMatchList m_EventsMatchList = null;
    [SerializeField]
    EventsTexture m_EventsTexture = null;
    [SerializeField]
    TracesDisplayer m_TracesDisplayer = null;
    [SerializeField]
    CustomVideoPlayer m_videoPlayer = null;

    private GameObject m_InputFieldWindowPrefabs = null;
    private Session m_PatientSession = null;

    private void Awake()
    {
        m_InputFieldWindowPrefabs = Resources.Load("Prefabs/UIElements/InputFieldWindow", typeof(GameObject)) as GameObject;
        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
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
        Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
    }

    private void OnLoaderMessage(LoaderMessage message)
    {
        if (message.Task == LoaderMessage.LoaderTask.MediaLoader && Session.IsCurrent(message.PatientSession))
            m_PatientSession = message.PatientSession;
    }

    private void Update()
    {
        if (!Session.IsCurrent(m_PatientSession)) return;
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
            GoToPreviousEvent();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
            GoToNextEvent();
    }

    private void OnEventsParametersMessage(UiToEventsMessage message)
    {
        if (!Session.IsCurrent(m_PatientSession)) return;
        switch (message.TaskToExecute)
        {
            case UiToEventsMessage.Task.LoadEventsFile:
                {
                    BtvLog.Log("Load File");
                    ApplicationState.displayConfirmation("Loading Events", "Do you want to delete all previous events or add to them ?",
                        () =>
                        {
                            LoadEventsFile(message.FilePathToLoad, true);
                        },
                        () =>
                        {
                            LoadEventsFile(message.FilePathToLoad, false);
                        });
                    break;
                }
            case UiToEventsMessage.Task.SaveEventsFile:
                {
                    BtvLog.Log("Save File");
                    FileInfo file = new FileInfo(message.FilePathToSave);
                    if (file.Extension == ".pos")
                    {
                        string path = message.FilePathToSave.Replace(".pos", "_btv.pos");
                        InputFieldWindow window = SpawnFrequencyChoiceWindow();
                        window.Initialize("File Sample Rate", "Sampling Frequency (in Hz) ?", () => { EventsService.SaveEvents(m_PatientSession, path, window.IntValue); window.Close(); }, () => { window.Close(); });
                    }
                    else
                    {
                        EventsService.SaveEvents(m_PatientSession, file.FullName);
                    }
                    break;
                }
            case UiToEventsMessage.Task.ToggleAddEvents:
                {
                    BtvLog.Log("Toggle Add Event");
                    EventsToTraceMessage EventsMessage = new EventsToTraceMessage
                    {
                        TaskToExecute = EventsToTraceMessage.Task.ToggleAddEvents,
                        IsAddEventsOn = message.IsAddEventsOn
                    };
                    Messenger.Default.Send(EventsMessage, MessageContext.EventsToTraceMessage);
                    break;
                }
            case UiToEventsMessage.Task.ToggleShowEvents:
                {
                    BtvLog.Log("Toggle Show Event");
                    EventsToTraceMessage EventsMessage = new EventsToTraceMessage
                    {
                        TaskToExecute = EventsToTraceMessage.Task.ToggleShowEvents,
                        IsShowEventsOn = message.IsShowEventsOn
                    };
                    Messenger.Default.Send(EventsMessage, MessageContext.EventsToTraceMessage);
                    break;
                }
            case UiToEventsMessage.Task.DeleteSelectedEvents:
                {
                    BtvLog.Log("Delete Selected Notes");
                    ApplicationState.displayConfirmation("Deleting Notes", "You are going to delete " + ((ISelectionCountable)m_EventsList).NumberOfItemSelected + " Notes, are you sure ? ", () => { DeleteSelectedEvents(); }, () => { });
                    break;
                }
            case UiToEventsMessage.Task.LoadCodeMatchingFile:
                {
                    BtvLog.Log("Load CodeMatching file");
                    CodeMatchingService.Load(m_PatientSession, message.FilePathToLoad);
                    m_EventsMatchList.DeleteAllEvents();
                    m_EventsMatchList.LoadEvents(CodeMatchingService.GetCodesAndComment(m_PatientSession));
                    break;
                }
        }
    }

    private void OnEventsModificationMessage(EventsModificationMessage message)
    {
        if (!Session.IsCurrent(m_PatientSession)) return;
        switch (message.TaskToExecute)
        {
            case EventsModificationMessage.Task.AddEvent:
                {
                    BtvLog.Log("Add Event");
                    AddEvent(message.Event);
                    break;
                }
            case EventsModificationMessage.Task.ModifyEvent:
                {
                    BtvLog.Log("Modify Event");
                    UpdateEvent(message.Event, message.EventMemory);
                    break;
                }
            case EventsModificationMessage.Task.DeleteEvent:
                {
                    BtvLog.Log("Delete Event");
                    DeleteEvent(message.Event);
                    break;
                }
            case EventsModificationMessage.Task.EditEvent:
                {
                    BtvLog.Log("Edit Event");
                    //Events to trace with parent gameobject (or mouse position) and event
                    EventsToTraceMessage messageToTrace = new EventsToTraceMessage
                    {
                        TaskToExecute = EventsToTraceMessage.Task.EditEvent,
                        Event = message.Event,
                        ParentWindowIndex = message.ParentWindowIndex
                    };
                    Messenger.Default.Send(messageToTrace, MessageContext.EventsToTraceMessage);
                    break;
                }
            case EventsModificationMessage.Task.ComputeCorrelation:
                {
                    BtvLog.Log("Correlation 1D");
                    ProcessCorrelation(message.Event);
                    break;
                }
            case EventsModificationMessage.Task.ComputeCorrelation2D:
                {
                    BtvLog.Log("Correlation 2D");
                    Process2dCorrelation(message.Event);
                    break;
                }
        }
    }

    private void GoToPreviousEvent()
    {
        IReadOnlyList<BtvEvent> events = EventsService.GetEvents(m_PatientSession);
        if (events.Count > 0)
        {
            long videoTimeInMs = m_videoPlayer.VideoInterface.ClockTime;
            int index = events.Select(x => x.TimeInMilliSeconds).ToList().BinarySearch(videoTimeInMs);
            if (index < 0) index = ~index - 1;

            if (index - 1 >= 0)
            {
                int eventTimeInMs = (int)events[index - 1].TimeInMilliSeconds;
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
        IReadOnlyList<BtvEvent> events = EventsService.GetEvents(m_PatientSession);
        if (events.Count > 0)
        {
            long videoTimeInMs = m_videoPlayer.VideoInterface.ClockTime;
            int index = events.Select(x => x.TimeInMilliSeconds).ToList().BinarySearch(videoTimeInMs);
            index = (index < 0) ? ~index : index + 1;

            if (index + 1 <= events.Count)
            {
                int eventTimeInMs = (int)events[index].TimeInMilliSeconds;
                ModulesToVideoMessage messageToVideo = new ModulesToVideoMessage
                {
                    UpdateClickPosition = true,
                    TimeMilliseconds = eventTimeInMs
                };
                Messenger.Default.Send(messageToVideo, MessageContext.ModulesToVideoMessage);
            }
        }
    }

    private InputFieldWindow SpawnFrequencyChoiceWindow()
    {
        GameObject viewGameObject = GameObject.Find("Windows");
        GameObject inputField = Instantiate(m_InputFieldWindowPrefabs, viewGameObject.transform);
        InputFieldWindow window = inputField.GetComponent<InputFieldWindow>();
        return window;
    }

    private void LoadEventsFile(string filePath, bool clearPreviousEvents)
    {
        FileInfo file = new FileInfo(filePath);
        if (file.Extension.Equals(".pos"))
        {
            InputFieldWindow window = SpawnFrequencyChoiceWindow();
            window.Initialize("File Sample Rate", "Sampling Frequency (in Hz) ?", () => { LoadEvents(file.FullName, window.IntValue, clearPreviousEvents); window.Close(); }, () => { window.Close(); });
        }
        else
        {
            LoadEvents(filePath, 0, clearPreviousEvents);
        }
    }

    //TODO : Reset Everything or allow to load data over already existing events ? 
    private void LoadEvents(string filePath, int SamplingFrequency = 0, bool ClearPreviousEvents = false)
    {
        if (!m_videoPlayer.VideoInterface.IsPrepared && SamplingFrequency != 0)
        {
            ApplicationState.displayMessage("Video not started yet", "NOK", "You need to start the video in order for the total length of the file/video to be known");
            return;
        }

        if (File.Exists(filePath))
        {
            if (ClearPreviousEvents)
            {
                EventsService.Load(m_PatientSession, filePath, SamplingFrequency);
                List<BtvEvent> events = EventsService.GetEvents(m_PatientSession).ToList();
                //load in UI List
                m_EventsList.DeleteAllEvents();
                m_EventsList.LoadEvents(events);
                //load in Scrollbar Texture
                m_EventsTexture.RemoveAllEvents();
                m_EventsTexture.AddEvents(events);
                //load in TraceDisplayer Texture [TODO : might need to put some other messages or refactor existing one]
                m_TracesDisplayer.RemoveAllEvents();
                m_TracesDisplayer.AddEvents(events);
                //send events to traces
                for (int i = 0; i < events.Count; i++)
                {
                    EventsToTraceMessage message = new EventsToTraceMessage
                    {
                        TaskToExecute = EventsToTraceMessage.Task.AddEventToTrace,
                        Event = events[i],
                        EventIndex = i
                    };
                    Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
                }

                EventsToTaskPerformanceMessage resetMessage = new EventsToTaskPerformanceMessage
                {
                    TaskToExecute = EventsToTaskPerformanceMessage.Task.ResetAll
                };
                Messenger.Default.Send(resetMessage, MessageContext.EventsToTaskPerformanceMessage);
            }
            else
            {
                List<BtvEvent> list = EventsService.LoadEventsFromFile(filePath, SamplingFrequency);
                foreach (BtvEvent btvEvent in list)
                {
                    AddEvent(btvEvent);
                }
            }
        }
    }

    private void AddEvent(BtvEvent Event)
    {
        if (!EventsService.AddEvent(m_PatientSession, Event))
        {
            // The service skipped a duplicate: stop here, otherwise the UI lists and the
            // per-trace GameObject lists would each gain an entry the service does not have
            // and drift out of sync with EventsService.Events.
            UnityEngine.Debug.LogWarning("AddEvent: an identical event already exists, nothing added.");
            return;
        }
        EventsService.SortBySample(m_PatientSession);
        int id = EventsService.GetEventId(m_PatientSession, Event);

        m_EventsTexture.AddEvent(Event);
        m_EventsList.AddEvent(Event);
        m_TracesDisplayer.AddEvent(Event);

        //Send message to Add to traces
        EventsToTraceMessage message = new EventsToTraceMessage
        {
            TaskToExecute = EventsToTraceMessage.Task.AddEventToTrace,
            EventIndex = id,
            Event = Event
        };
        Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
        //Send message to task perf that an event was added
        EventsToTaskPerformanceMessage addedMessage = new EventsToTaskPerformanceMessage
        {
            TaskToExecute = EventsToTaskPerformanceMessage.Task.MarkOutOfDate
        };
        Messenger.Default.Send(addedMessage, MessageContext.EventsToTaskPerformanceMessage);
    }

    private void UpdateEvent(BtvEvent modifyiedEvent, BtvEvent previousEvent)
    {
        DeleteEvent(previousEvent);
        AddEvent(modifyiedEvent);

        EventsToTaskPerformanceMessage modifiedMessage = new EventsToTaskPerformanceMessage
        {
            TaskToExecute = EventsToTaskPerformanceMessage.Task.MarkOutOfDate
        };
        Messenger.Default.Send(modifiedMessage, MessageContext.EventsToTaskPerformanceMessage);
    }

    private void DeleteEvent(BtvEvent Event)
    {
        int id = EventsService.GetEventId(m_PatientSession, Event);
        if (id < 0)
        {
            UnityEngine.Debug.LogWarning("DeleteEvent: event not found in the service, nothing to delete.");
            return;
        }
        EventsService.RemoveEventAt(m_PatientSession, id);

        m_EventsTexture.RemoveEvent(Event);
        m_EventsList.DeleteEvent(id);
        m_TracesDisplayer.RemoveEvent(Event);

        //Send message to delete from traces
        EventsToTraceMessage message = new EventsToTraceMessage
        {
            TaskToExecute = EventsToTraceMessage.Task.RemoveEventFromTrace,
            EventIndex = id
        };
        Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);

        EventsToTaskPerformanceMessage deletedMessage = new EventsToTaskPerformanceMessage
        {
            TaskToExecute = EventsService.GetEventCount(m_PatientSession) == 0
                ? EventsToTaskPerformanceMessage.Task.ResetAll
                : EventsToTaskPerformanceMessage.Task.MarkOutOfDate
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

    private async void ProcessCorrelation(BtvEvent currentEvent)
    {
        Session patientSession = m_PatientSession;
        if (!Session.IsCurrent(patientSession)) return;
        try
        {
            int eventIndex = EventsService.GetEventId(patientSession, currentEvent);
            if (eventIndex < 0)
            {
                UnityEngine.Debug.LogWarning("ProcessCorrelation: event not found in EventsService, nothing computed.");
                return;
            }

            // Gather everything on the main thread; the worker only reads what was gathered.
            BtvEvent eventToProcess = EventsService.GetEvent(patientSession, eventIndex);
            int samplingFrequency = TracesService.SamplingFrequency(patientSession, 0);
            int electrodeCount = TracesService.ElectrodeCount(patientSession, 0);
            int beginTimeSample = (int)(eventToProcess.TimeInSeconds * samplingFrequency);
            int durationInSample = (eventToProcess.Duration / 1000) * samplingFrequency;
            int[] sizes = { beginTimeSample, durationInSample };

            int indexBaseline = TracesService.GetOptionsFor(patientSession, 0).FileHandle.GetElectrodeIDFromElectrodeName(currentEvent.SiteOfInterest);
            float[] baseline = null;
            if (indexBaseline != -1)
                baseline = TracesService.ChannelData(patientSession, 0, indexBaseline);
            else if (currentEvent.SiteOfInterest.StartsWith("AUD")) //Run Correlation against Audio trace
                baseline = TracesService.AudioChannelData(patientSession);

            float[] correlation;
            if (baseline == null)
            {
                // Unknown site of interest: keep the historical result, an all-zero correlation.
                correlation = new float[electrodeCount];
            }
            else
            {
                float[][] channels = new float[electrodeCount][];
                for (int i = 0; i < electrodeCount; i++)
                    channels[i] = TracesService.ChannelData(patientSession, 0, i);

                // -1 when correlating against audio: every channel is processed.
                int channelToSkip = indexBaseline;
                correlation = await Task.Run(() => ComputeCorrelation(baseline, channels, sizes, channelToSkip));
            }

            if (this == null || !Session.IsCurrent(patientSession)) return;

            // Publish on the main thread, re-resolving the event: it may have been deleted while
            // the computation was running. The old code wrote into the event from the worker
            // thread while BrainWarden was reading it on every video tick.
            int targetIndex = EventsService.GetEventId(patientSession, currentEvent);
            if (targetIndex < 0)
            {
                BtvLog.Log("ProcessCorrelation: event removed during computation, result discarded.");
                return;
            }
            BtvEvent targetEvent = EventsService.GetEvent(patientSession, targetIndex);
            targetEvent.Correlation = correlation;
            targetEvent.Correlation2D = null;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error processing correlations");
            UnityEngine.Debug.LogException(ex);
            ApplicationState.displayMessage("Error Processing Correlations", "NOK", ex.Message);
        }
    }

    private static float[] ComputeCorrelation(float[] baseline, float[][] channels, int[] sizes, int channelToSkip)
    {
        float[] correlation = new float[channels.Length];
        for (int i = 0; i < channels.Length; i++)
        {
            if (i == channelToSkip)
                continue;

            correlation[i] = CalculationService.PearsonCorrelationCoefficients(baseline, channels[i], sizes);
        }
        return correlation;
    }

    private async void Process2dCorrelation(BtvEvent currentEvent)
    {
        Session patientSession = m_PatientSession;
        if (!Session.IsCurrent(patientSession)) return;
        try
        {
            int eventIndex = EventsService.GetEventId(patientSession, currentEvent);
            if (eventIndex < 0)
            {
                UnityEngine.Debug.LogWarning("Process2dCorrelation: event not found in EventsService, nothing computed.");
                return;
            }

            BtvProgram container1 = TracesService.GetOptionsFor(patientSession, 0).FileHandle;
            BtvProgram container2 = TracesService.GetOptionsFor(patientSession, 1).FileHandle;

            bool sameDescription = container1.Description == container2.Description;
            bool sameElectrodeCount = container1.NumberOfElectrodes == container2.NumberOfElectrodes;
            bool sameSamplingFrequency = container1.Frequency.Value == container2.Frequency.Value;

            if (!sameDescription && !sameElectrodeCount) throw new ArgumentException("Process2dCorrelation : Number of electrode is not the same in the two files used");
            if (!sameDescription && !sameSamplingFrequency) throw new ArgumentException("Process2dCorrelation : Sampling Frequency is different beetween the two files used");

            int samplingFrequency = container2.Frequency.Value;
            int electrodeCount = container2.NumberOfElectrodes;

            BtvEvent eventToProcess = EventsService.GetEvent(patientSession, eventIndex);
            int beginTimeSample = (int)(eventToProcess.TimeInSeconds * samplingFrequency);
            int durationInSample = (eventToProcess.Duration / 1000) * samplingFrequency;
            int[] sizes = { beginTimeSample, durationInSample };

            float[][] channels1 = new float[electrodeCount][];
            float[][] channels2 = new float[electrodeCount][];
            for (int i = 0; i < electrodeCount; i++)
            {
                channels1[i] = container1.Channels[i].Data;
                channels2[i] = container2.Channels[i].Data;
            }

            float[][] correlation2d = await Task.Run(() => ComputeCorrelation2d(channels1, channels2, sizes));

            if (this == null || !Session.IsCurrent(patientSession)) return;

            int targetIndex = EventsService.GetEventId(patientSession, currentEvent);
            if (targetIndex < 0)
            {
                BtvLog.Log("Process2dCorrelation: event removed during computation, result discarded.");
                return;
            }
            BtvEvent targetEvent = EventsService.GetEvent(patientSession, targetIndex);
            targetEvent.Correlation = null;
            targetEvent.Correlation2D = correlation2d;

            bool sameFile = container1 == container2;
            if (!sameFile)
            {
                string message = "Correlations have been processed" + "\nJust a reminder, you correlated data from two different files";
                ApplicationState.displayMessage("Correlations Processing succeeded", "OK", message);
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error processing 2D correlations");
            UnityEngine.Debug.LogException(ex);
            ApplicationState.displayMessage("Error Processing Correlations", "NOK", ex.Message);
        }
    }

    private static float[][] ComputeCorrelation2d(float[][] channels1, float[][] channels2, int[] sizes)
    {
        int electrodeCount = channels1.Length;
        float[][] correlation2d = new float[electrodeCount][];
        for (int i = 0; i < electrodeCount; i++)
            correlation2d[i] = new float[electrodeCount];

        for (int i = 0; i < electrodeCount; i++)
        {
            for (int j = 0; j < electrodeCount; j++)
            {
                if (i == j)
                    continue;
                correlation2d[i][j] = CalculationService.PearsonCorrelationCoefficients(channels1[i], channels2[j], sizes);
            }
        }
        return correlation2d;
    }
}
