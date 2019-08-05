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

    private void Awake()
    {
        scrollOrig = Resources.Load("Pictures/eventScroll", typeof(Texture2D)) as Texture2D;


        Messenger.Default.Register<UiToEventsMessage>(this, OnEventsParametersMessage, MessageContext.UiToEvents);
    }

    private void Start()
    {
        m_checkAll.onValueChanged.AddListener(ToggleAllEvents);

        scrollTex = Instantiate(scrollOrig);
        m_timeScrollBarImage.texture = scrollTex;

        //m_signalWindow1.eventWasClicked += new eventsClickedHandler(openEventAddUI);
        //m_signalWindow1.EventsEeg.eventsToDisplay += new eventsToDisplay(openEventDisplayUI);
        //m_signalWindow1.EventsEeg.eventsToDelete += new eventsToDelete(deleteEvents);
        //m_signalWindow2.eventWasClicked += new eventsClickedHandler(openEventAddUI);
        //m_signalWindow2.EventsEeg.eventsToDisplay += new eventsToDisplay(openEventDisplayUI);
        //m_signalWindow2.EventsEeg.eventsToDelete += new eventsToDelete(deleteEvents);

        dataTexScroll = scrollTex.GetPixels();
        Initialize(); //Init the list class
    }

    private void OnDestroy()
    {
        m_checkAll.onValueChanged.RemoveAllListeners();
        Messenger.Default.Unregister(this, MessageContext.UiToEvents);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
            GoToPreviousEvent();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
            GoToNextEvent();
    }

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
            long timeSample = (long)(timeSec * ApplicationState.CurrentSelectedFile.sampFreq);
            var keys = sampleValues;
            var index = keys.BinarySearch((int)timeSample);

            if (Math.Abs(index) - 1 == 0)
            {
                int currentPos = 0;
                m_videoPlayer.changeTimeClick((int)(Objects[currentPos].sample / ApplicationState.CurrentSelectedFile.sampFreq) * 1000);
                m_videoPlayer.setTime((int)(Objects[currentPos].sample / ApplicationState.CurrentSelectedFile.sampFreq) * 1000);
            }
            else
            {
                int currentPos = Math.Abs(index) - 1;
                m_videoPlayer.changeTimeClick((int)(Objects[currentPos - 1].sample / ApplicationState.CurrentSelectedFile.sampFreq) * 1000);
                m_videoPlayer.setTime((int)(Objects[currentPos - 1].sample / ApplicationState.CurrentSelectedFile.sampFreq) * 1000);
            }
        }
    }

    private void GoToNextEvent()
    {
        if (Objects.Length > 0)
        {
            long timeSec = m_videoPlayer.videoInterface.currentTime / 1000;
            long timeSample = (long)(timeSec * ApplicationState.CurrentSelectedFile.sampFreq);
            var keys = sampleValues;
            var index = keys.BinarySearch((int)timeSample);

            int currentPos = Math.Abs(index) - 1;
            if (currentPos + 1 < Objects.Length)
            {
                m_videoPlayer.changeTimeClick((int)(Objects[currentPos + 1].sample / ApplicationState.CurrentSelectedFile.sampFreq) * 1000);
                m_videoPlayer.setTime((int)(Objects[currentPos + 1].sample / ApplicationState.CurrentSelectedFile.sampFreq) * 1000);
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

    private void LoadEvents(string filePath)
    {
        if (File.Exists(filePath))
        {
            IEventsContext file = EventsFactory.GetEventsContext(filePath);
            List<TraceEvent> eventLoaded = file.Events;

            //if elements already loaded , delete everything
            for (int i = Objects.Length - 1; i >= 0; i--)
                DeleteEvents(Objects.ElementAt(i));

            //for (int i = 0; i < eventLoaded.Count; i++)
            //    eventValidatedForUI(eventLoaded[i]);
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
        List<int> ids = Objects.Select((item, index) => new { Item = item, Index = index })
                         .Where(x => x.Item.sample == eventToDelete.sample)
                         .Select(x => x.Index)
                         .ToList();
        
        //  /!!!!!!!!!!!!!!!!\
        //  SEND MESSAGE TO TEXTURE TO DELETE EVENTS
        //  /!!!!!!!!!!!!!!!!\
        //removeEventToTexture(eventToDelete);

        Remove(Objects[ids[0]]); //par ref

        //Remove : 
        //  -event connection
        //  -then destroy object 
        //  -then the reference in list of gameobject 
        //int index1 = win1.eventsAdded.FindIndex(x => x.name == "Event - " + eventToDelete.sample);

        //  /!!!!!!!!!!!!!!!!\
        //  SEND MESSAGE TO TRACES TO DELETE EVENTS
        //  /!!!!!!!!!!!!!!!!\

        //m_signalWindow1.EventsEeg.removeEventConnections(m_signalWindow1.EventsEeg.eventsAdded[ids[0]].gameObject);
        //Destroy(m_signalWindow1.EventsEeg.eventsAdded[ids[0]].gameObject);
        //m_signalWindow1.EventsEeg.eventsAdded.RemoveAt(ids[0]);
        //m_signalWindow2.EventsEeg.removeEventConnections(m_signalWindow2.EventsEeg.eventsAdded[ids[0]].gameObject);
        //Destroy(m_signalWindow2.EventsEeg.eventsAdded[ids[0]].gameObject);
        //m_signalWindow2.EventsEeg.eventsAdded.RemoveAt(ids[0]);
    }

    public void SortBySample()
    {
        m_Objects = m_Objects.OrderBy(x => x.sample).ToList();
        Refresh();
    }
}
