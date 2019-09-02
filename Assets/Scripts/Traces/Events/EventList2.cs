using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assets.Scripts.Data.Factory;
using CielaSpike;
using BTV.Services.EventsService;

public class EventList2 : Tools.SelectableList<TraceEvent>
{
    public List<int> sampleValues
    {
        get
        {
            List<int> sample = new List<int>(m_Items.Count);
            foreach (TraceEvent Event in m_Objects)
                sample.Add(Event.sample);

            return sample;
        }
    }

    [SerializeField] Toggle m_checkAll = null;
    [SerializeField] CustomVideoPlayer m_videoPlayer = null;
    [SerializeField] RawImage m_timeScrollBarImage = null;

    Texture2D scrollOrig = null, scrollTex = null;
    Color[] dataTexScroll;
    Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);

    private void Start()
    {
        scrollOrig = Resources.Load("Pictures/eventScroll", typeof(Texture2D)) as Texture2D;


        Messenger.Default.Register<UiToEventsMessage>(this, OnEventsParametersMessage, MessageContext.UiToEvents);
        Messenger.Default.Register<EventsModificationMessage>(this, OnEventsModificationMessage, MessageContext.EventsModificationMessage);
        m_checkAll.onValueChanged.AddListener(ToggleAllEvents);

        scrollTex = Instantiate(scrollOrig);
        m_timeScrollBarImage.texture = scrollTex;

        dataTexScroll = scrollTex.GetPixels();
        Initialize(); //Init the list class
    }

    private void OnDestroy()
    {
        m_checkAll.onValueChanged.RemoveAllListeners();
        Messenger.Default.Unregister(this, MessageContext.UiToEvents);
        Messenger.Default.Unregister(this, MessageContext.EventsModificationMessage);
    }

    //private void Update()
    //{
    //    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
    //        GoToPreviousEvent();

    //    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
    //        GoToNextEvent();
    //}

    private void ToggleAllEvents(bool isChecked)
    {
        if (isChecked)
            SelectAll();
        else
            DeselectAll();
    }

    private void GoToPreviousEvent()
    {
        if (Objects.Length > 0)
        {
            long timeSec = m_videoPlayer.videoInterface.currentTime / 1000;
            long timeSample = (long)(timeSec * ApplicationState.Window1.TraceEeg.SamplingFrequency);
            var keys = sampleValues;
            var index = keys.BinarySearch((int)timeSample);

            if (Math.Abs(index) - 1 == 0)
            {
                int currentPos = 0;
                m_videoPlayer.changeTimeClick((int)(Objects[currentPos].sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) * 1000);
                m_videoPlayer.setTime((int)(Objects[currentPos].sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) * 1000);
            }
            else
            {
                int currentPos = Math.Abs(index) - 1;
                m_videoPlayer.changeTimeClick((int)(Objects[currentPos - 1].sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) * 1000);
                m_videoPlayer.setTime((int)(Objects[currentPos - 1].sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) * 1000);
            }
        }
    }

    private void GoToNextEvent()
    {
        if (Objects.Length > 0)
        {
            long timeSec = m_videoPlayer.videoInterface.currentTime / 1000;
            long timeSample = (long)(timeSec * ApplicationState.Window1.TraceEeg.SamplingFrequency);
            var keys = sampleValues;
            var index = keys.BinarySearch((int)timeSample);

            int currentPos = Math.Abs(index) - 1;
            if (currentPos + 1 < Objects.Length)
            {
                m_videoPlayer.changeTimeClick((int)(Objects[currentPos + 1].sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) * 1000);
                m_videoPlayer.setTime((int)(Objects[currentPos + 1].sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) * 1000);
            }
        }
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
                    ApplicationState.displayConfirmation("Deleting Notes", "You are going to delete " + ObjectsSelected.Length + " Notes, are you sure ? ", DeleteSelectedEvents, () => { });
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
                    EventsService.AddEvent(message.Event);
                    AddEventToUi(message.Event);
                    break;
                }
            case 1:
                {
                    Debug.Log("Modify Event");
                    applyChangeToEvent(message.Event, message.EventMemory);
                    break;
                }
            case 2:
                {
                    Debug.Log("Delete Event");
                    DeleteEvents(message.Event);
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

    private void LoadEvents(string filePath)
    {
        if (File.Exists(filePath))
        {
            EventsService.Load(filePath);
            
            //if elements already loaded , delete everything
            for (int i = Objects.Length - 1; i >= 0; i--)
                DeleteEvents(Objects.ElementAt(i));

            int EventCount = EventsService.Events.Count;
            for (int i = 0; i < EventCount; i++)
                AddEventToUi(EventsService.Events[i]);
        }
    }

    private void DeleteSelectedEvents()
    {
        for (int i = ObjectsSelected.Length - 1; i >= 0; i--)
            DeleteEvents(ObjectsSelected[i]);
    }

    private void DeleteEvents(TraceEvent eventToDelete)
    {
        //Delete from Texture
        RemoveEventToTexture(eventToDelete);

        int Id = EventsService.GetEventId(eventToDelete);
        //Delete from internal List 
        EventsService.RemoveEvent(eventToDelete);
        //Delete from Ui List by ref
        Remove(Objects[Id]);

        //Send message to delete from traces
        EventsToTraceMessage message = new EventsToTraceMessage
        {
            TaskToExecute = 4,
            EventIndex = Id
        };
        Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
    }

    public void SortBySample()
    {
        EventsService.SortBySample();
        m_Objects = m_Objects.OrderBy(x => x.sample).ToList();
        Refresh();
    }

    private void AddEventToUi(TraceEvent currentEvent)
    {
        ApplicationState.MemoryEvent = new TraceEvent(currentEvent);

        Add(currentEvent);
        SortBySample();
        AddEventToTexture(currentEvent);

        int Id = EventsService.GetEventId(currentEvent);

        //Send message to Add to traces
        EventsToTraceMessage message = new EventsToTraceMessage
        {
            TaskToExecute = 3,
            EventIndex = Id,
            Event = currentEvent
        };
        Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
    }

    private void AddEventToTexture(TraceEvent currentEvent)
    {
        float perC = ((((float)currentEvent.sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) / m_videoPlayer.videoInterface.totalVideoTime) * 1000);
        int pixelID = (int)(perC * scrollTex.width);

        if (currentEvent.duration > 0)
        {
            if (currentEvent.duration > 1000)
            {
                float durationInSample = (currentEvent.duration * ((float)ApplicationState.Window1.TraceEeg.SamplingFrequency / 1000));
                float perCDuration = (((((float)currentEvent.sample + durationInSample) / ApplicationState.Window1.TraceEeg.SamplingFrequency) / m_videoPlayer.videoInterface.totalVideoTime) * 1000);
                int pixelIDDuration = (int)(perCDuration * scrollTex.width);
                for (int i = 0; i < scrollTex.height / 2; i++)
                {
                    for (int j = 0; j < pixelIDDuration - pixelID; j++)
                        dataTexScroll[(pixelID + j) + (i * scrollTex.width)] = orange;
                }
            }
            else //if duration < 1000ms, too thin to see the red streak on the scrollbar
            {
                for (int i = 0; i < scrollTex.height; i++)
                    dataTexScroll[pixelID + (i * scrollTex.width)] = orange;
            }
        }
        else
        {
            for (int i = 0; i < scrollTex.height; i++)
                dataTexScroll[pixelID + (i * scrollTex.width)] = Color.red;
        }
        scrollTex.SetPixels(dataTexScroll);
        scrollTex.Apply();
    }

    private void RemoveEventToTexture(TraceEvent currentEvent)
    {
        float perC = ((((float)currentEvent.sample / ApplicationState.Window1.TraceEeg.SamplingFrequency) / m_videoPlayer.videoInterface.totalVideoTime) * 1000);
        int pixelID = (int)(perC * scrollTex.width);

        if (currentEvent.duration > 0)
        {
            if (currentEvent.duration > 1000)
            {
                float durationInSample = (currentEvent.duration * ((float)ApplicationState.Window1.TraceEeg.SamplingFrequency / 1000));
                float perCDuration = (((((float)currentEvent.sample + durationInSample) / ApplicationState.Window1.TraceEeg.SamplingFrequency) / m_videoPlayer.videoInterface.totalVideoTime) * 1000);
                int pixelIDDuration = (int)(perCDuration * scrollTex.width);
                for (int i = 0; i < scrollTex.height / 2; i++)
                {
                    for (int j = 0; j < pixelIDDuration - pixelID; j++)
                        dataTexScroll[(pixelID + j) + (i * scrollTex.width)] = hardBlue;
                }
            }
            else //if duration < 1000ms, too thin to see the red streak on the scrollbar
            {
                for (int i = 0; i < scrollTex.height; i++)
                    dataTexScroll[pixelID + (i * scrollTex.width)] = hardBlue;
            }
        }
        else
        {
            for (int i = 0; i < scrollTex.height; i++)
                dataTexScroll[pixelID + (i * scrollTex.width)] = hardBlue;
        }
        scrollTex.SetPixels(dataTexScroll);
        scrollTex.Apply();
    }

    private void applyChangeToEvent(TraceEvent modifyiedEvent, TraceEvent previousEvent)
    {
        int Id = EventsService.GetEventId(previousEvent);
        if (Id != -1)
        {
            bool UpdateEventUI = (modifyiedEvent.duration - previousEvent.duration == modifyiedEvent.duration) || (modifyiedEvent.duration - previousEvent.duration == -previousEvent.duration);

            RemoveEventToTexture(previousEvent);
            EventsService.UpdateEvent(modifyiedEvent, previousEvent);
            TraceEvent currentObject = Objects[Id];
            currentObject = new TraceEvent(EventsService.Events[Id]);
            AddEventToTexture(currentObject);

            var eventToChangeObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Event - " + previousEvent.sample);
            if (UpdateEventUI)
            {
                eventToChangeObjects.ElementAt(0).GetComponent<EventTrace>().DeleteMe();
                AddEventToUi(currentObject);
            }

            //Update object on Traces UI
            foreach (var eventToChange in eventToChangeObjects)
            {
                eventToChange.GetComponent<EventTrace>().UpdateEvent(currentObject);
            }

            //Refresh EventList
            Refresh();
        }
    }

    IEnumerator ProcessCorrelation(TraceEvent currentEvent)
    {
        yield return Ninja.JumpBack;
        ApplicationState.coroutineManager.StartCoroutine(c_Correlation(currentEvent));
        yield return Ninja.JumpToUnity;
    }

    IEnumerator c_Correlation(TraceEvent currentEvent)
    {
        int nbElec = ApplicationState.Window1.TraceEeg.fileHandle.electrodes.Length;
        List<int> ids = Objects.Select((item, index) => new { Item = item, Index = index })
                                 .Where(x => x.Item.sample == currentEvent.sample)
                                 .Select(x => x.Index)
                                 .ToList();

        Objects[ids[0]].correlationArray = new float[nbElec];
        Objects[ids[0]].correlation2DArray = null;

        int beginSample = Objects[ids[0]].sample;
        int durationSample = (Objects[ids[0]].duration / 1000) * Objects[ids[0]].samplingFrequency;

        int idBase = ApplicationState.Window1.TraceEeg.fileHandle.electrodes.ToList().FindIndex(x => x.name == currentEvent.elecOfInterest);
        if (idBase != -1)
        {
            int[] sizes = new int[5] { idBase, nbElec, beginSample, durationSample, ApplicationState.Window1.TraceEeg.fileHandle.nbSam };
            pearsonCoefficientsCorrelation(Objects[ids[0]].correlationArray, ApplicationState.Window1.TraceEeg.fileHandle.eegData, sizes);
        }
        else
        {
            if (currentEvent.elecOfInterest.StartsWith("AUD"))
            {
                int[] sizes = new int[4] { nbElec, beginSample, durationSample, ApplicationState.Window1.TraceEeg.fileHandle.nbSam };
                pearsonCoefficientsCorrelation2(Objects[ids[0]].correlationArray, m_videoPlayer.audioWav.getAudioHandle(m_videoPlayer.audioWav.idAudioHandle), ApplicationState.Window1.TraceEeg.fileHandle.eegData, sizes);
            }
        }
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
        int nbElec = ApplicationState.Window1.TraceEeg.fileHandle.electrodes.Length;
        List<int> ids = Objects.Select((item, index) => new { Item = item, Index = index })
                                 .Where(x => x.Item.sample == currentEvent.sample)
                                 .Select(x => x.Index)
                                 .ToList();

        Objects[ids[0]].correlationArray = null;
        Objects[ids[0]].correlation2DArray = new float[nbElec][];
        for (int i = 0; i < Objects[ids[0]].correlation2DArray.Length; i++)
            Objects[ids[0]].correlation2DArray[i] = new float[nbElec];

        int beginSample = Objects[ids[0]].sample;
        int durationSample = (Objects[ids[0]].duration / 1000) * Objects[ids[0]].samplingFrequency;

        for (int i = 0; i < Objects[ids[0]].correlation2DArray.Length; i++)
        {
            int[] sizes = new int[5] { i, nbElec, beginSample, durationSample, ApplicationState.Window1.TraceEeg.fileHandle.nbSam };
            pearsonCoefficientsCorrelation(Objects[ids[0]].correlation2DArray[i], ApplicationState.Window1.TraceEeg.fileHandle.eegData, sizes);
        }

        yield return null;
    }

    #region DLLImport
    [DllImport("BTVReplayLibraryC++", EntryPoint = "pearsonCoefficientsCorrelation", CallingConvention = CallingConvention.Cdecl)]
    static private extern void pearsonCoefficientsCorrelation(float[] coeffs, float[] eegData, int[] sizes);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "pearsonCoefficientsCorrelation2", CallingConvention = CallingConvention.Cdecl)]
    static private extern void pearsonCoefficientsCorrelation2(float[] coeffs, float[] baseArray, float[] eegData, int[] sizes);
    #endregion
}
