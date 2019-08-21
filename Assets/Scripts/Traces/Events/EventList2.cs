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
    [SerializeField] VideoPlayer m_videoPlayer = null;
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
                    SaveEvents(message.FilePathToSave);
                    break;
                }
            case 2:
                {
                    Debug.Log("Toggle Add Event");
                    EventsToTraceMessage EventsMessage = new EventsToTraceMessage
                    {
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
                    eventValidatedForUI(message.Event);
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
            IEventsContext file = EventsFactory.GetEventsContext(filePath);
            List<TraceEvent> eventLoaded = file.Events;

            //if elements already loaded , delete everything
            for (int i = Objects.Length - 1; i >= 0; i--)
                DeleteEvents(Objects.ElementAt(i));

            for (int i = 0; i < eventLoaded.Count; i++)
                eventValidatedForUI(eventLoaded[i]);
        }
    }

    private void SaveEvents(string filePath)
    {
        string posFilePath = filePath.Replace(".pos", "_btv.pos");
        EventsFactory.SaveEvents(posFilePath, Objects.ToList());
        string btvFilePath = filePath.Replace(".pos", ".btv");
        EventsFactory.SaveEvents(btvFilePath, Objects.ToList());
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

        //Delete from internal List 
        List<int> ids = Objects.Select((item, index) => new { Item = item, Index = index })
                         .Where(x => x.Item.sample == eventToDelete.sample)
                         .Select(x => x.Index)
                         .ToList();
        Remove(Objects[ids[0]]); //par ref

        //Send message to delete from traces
        EventsToTraceMessage message = new EventsToTraceMessage
        {
            TaskToExecute = 4,
            EventIndex = ids[0]
        };
        Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
    }

    public void SortBySample()
    {
        m_Objects = m_Objects.OrderBy(x => x.sample).ToList();
        Refresh();
    }

    private void eventValidatedForUI(TraceEvent currentEvent)
    {
        ApplicationState.MemoryEvent = new TraceEvent(currentEvent);

        Add(currentEvent);
        SortBySample();
        AddEventToTexture(currentEvent);

        List<int> ids = Objects.Select((item, index) => new { Item = item, Index = index })
                                 .Where(x => x.Item.sample == currentEvent.sample)
                                 .Select(x => x.Index)
                                 .ToList();

        //Send message to Add to traces
        EventsToTraceMessage message = new EventsToTraceMessage
        {
            TaskToExecute = 3,
            EventIndex = ids[0],
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
        var eventToChangeObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(
                                   obj => obj.name == "Event - " + previousEvent.sample);

        List<int> ids = Objects.Select((item, index) => new { Item = item, Index = index })
                            .Where(x => x.Item.sample == previousEvent.sample)
                            .Select(x => x.Index)
                            .ToList();
        if (ids.Count > 0)
        {
            TraceEvent eventFound = Objects[ids[0]];
            RemoveEventToTexture(eventFound);

            //Modify event in list and reset all needed arrays 
            int memDuration = eventFound.duration;
            eventFound.elecOfInterest = modifyiedEvent.elecOfInterest;
            eventFound.code = modifyiedEvent.code;
            eventFound.comment = modifyiedEvent.comment;

            if (modifyiedEvent.duration != eventFound.duration)
            {
                eventFound.correlationArray = null;
                eventFound.correlation2DArray = null;
            }

            if (modifyiedEvent.elecOfInterest == "")
            {
                modifyiedEvent.elecOfInterest = ApplicationState.Window1.TraceEeg.LabelElectrode;
                modifyiedEvent.secondElecOfInterest = ApplicationState.Window2.TraceEeg.LabelElectrode;
            }

            eventFound.duration = modifyiedEvent.duration;
            AddEventToTexture(eventFound);

            //if event goes from no duration to with duration or the other way around we switch it
            if ((modifyiedEvent.duration - memDuration == modifyiedEvent.duration) ||
                (modifyiedEvent.duration - memDuration == -memDuration))
            {
                eventToChangeObjects.ElementAt(0).GetComponent<EventTrace>().DeleteMe();
                eventValidatedForUI(modifyiedEvent);
            }

            //Update object on Traces UI
            foreach (var eventToChange in eventToChangeObjects)
            {
                eventToChange.GetComponent<EventTrace>().UpdateEvent(modifyiedEvent);
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
