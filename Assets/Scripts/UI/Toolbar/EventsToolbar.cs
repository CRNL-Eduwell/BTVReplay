using CielaSpike;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

//Uncomment when deleting optionHub.cs
//
//public delegate void newEventToShowHandler(TraceEvent newEvent, int id);
//public delegate void showAllEventsHandler(bool show);
namespace BTV.UI.Module3D
{
    public class EventsToolbar : Toolbar
    {
        public TraceEvent[] userEvents
        {
            get
            {
                return m_eventList.Objects;
            }
        }

        public event newEventToShowHandler newEventToShow;
        public event showAllEventsHandler showEvents;

        [SerializeField] EventList m_eventList;
        [SerializeField] VideoPlayer m_videoPlayer = null;
        [SerializeField] Trace m_signalWindow1 = null;
        [SerializeField] Trace m_signalWindow2 = null;
        [SerializeField] CoroutineManager m_coroutineManager = null;
        [SerializeField] RawImage m_timeScrollBarImage = null;

        GameObject eventAddUI = null;
        GameObject eventDispUI = null;

        EventInfoEdit infoEdit = null;
        EventInfoDisplay infoDisp = null;
        int traceIDCalled = 0;
        TraceEvent eventCalled = null;
        TraceEvent eventMemory = null;


        Image activateEventPic = null, showEventPic = null;
        Transform panelContent = null;
        Button saveEvents = null, loadEvents = null, activateEventsButton = null, showEventsButton = null, deleteEventsButton = null;
        Sprite startEventPic = null, stopEventPic = null;
        Sprite showEventSprite = null, hideEventSprite = null;

        public bool addEvent = false, showEvent = true;

        //GameObject scrollObj = null;
        Texture2D scrollOrig = null, scrollTex = null;
        Color[] dataTexScroll;
        Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
        Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
        int currentPos = -1;

        public void init(ButtonUI button)
        {
            eventAddUI = Resources.Load("Prefabs/EventInfoEdit", typeof(GameObject)) as GameObject;
            eventDispUI = Resources.Load("Prefabs/EventInfoDisplay", typeof(GameObject)) as GameObject;
            scrollOrig = Resources.Load("Pictures/eventScroll", typeof(Texture2D)) as Texture2D;
            startEventPic = Resources.Load("Pictures/ConfigBar/StartEvents", typeof(Sprite)) as Sprite;
            stopEventPic = Resources.Load("Pictures/ConfigBar/StopEvents", typeof(Sprite)) as Sprite;
            showEventSprite = Resources.Load("Pictures/ConfigBar/showEvents", typeof(Sprite)) as Sprite;
            hideEventSprite = Resources.Load("Pictures/ConfigBar/hideEvents", typeof(Sprite)) as Sprite;
            //==
            //list = GameObject.Find("Workable Part").transform.GetChild(0).GetChild(2).GetChild(2).GetChild(0).GetChild(1).GetComponent<EventList>();
            //v = GameObject.Find("Workable Part").transform.GetChild(0).GetChild(1).GetComponent<VideoPlayer>();
            //win1 = GameObject.Find("Workable Part").transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Trace>();
            //win2 = GameObject.Find("Workable Part").transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<Trace>();
            //m_coroutineManager = GameObject.Find("ringSelect").GetComponent<CoroutineManager>();
            //scrollObj = GameObject.Find("TimeScrollBar").transform.GetChild(0).GetChild(0).gameObject;
            scrollTex = Instantiate(scrollOrig);
            //==
            panelContent = button.optionsPanel2.transform.GetChild(0).GetChild(0).GetChild(0);
            loadEvents = button.optionsPanel.transform.GetChild(0).GetChild(0).GetComponent<Button>();
            saveEvents = button.optionsPanel.transform.GetChild(0).GetChild(1).GetComponent<Button>();
            activateEventsButton = button.optionsPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>();
            showEventsButton = button.optionsPanel.transform.GetChild(0).GetChild(3).GetComponent<Button>();
            deleteEventsButton = button.optionsPanel.transform.GetChild(0).GetChild(4).GetComponent<Button>();

            activateEventPic = activateEventsButton.transform.GetComponent<Image>();
            showEventPic = showEventsButton.transform.GetComponent<Image>();
            //scrollObj.GetComponent<RawImage>().texture = scrollTex;
            m_timeScrollBarImage.texture = scrollTex;

            //==
            activateEventsButton.onClick.AddListener(activateEventsMode);
            showEventsButton.onClick.AddListener(showEventsMode);
            saveEvents.onClick.AddListener(saveEventsList);
            loadEvents.onClick.AddListener(loadEventList);
            deleteEventsButton.onClick.AddListener(() =>
            {
                ApplicationState.displayConfirmation("Deleting Notes", "You are going to delete " + m_eventList.ObjectsSelected.Length + " Notes, are you sure ? ",
                () =>
                {
                    for (int i = m_eventList.ObjectsSelected.Length - 1; i >= 0; i--)
                        deleteEvents(m_eventList.ObjectsSelected[i], 0);
                },
                () => { });
            });

            m_signalWindow1.eventWasClicked += new eventsClickedHandler(openEventAddUI);
            m_signalWindow1.EventsEeg.eventsToDisplay += new eventsToDisplay(openEventDisplayUI);
            m_signalWindow1.EventsEeg.eventsToDelete += new eventsToDelete(deleteEvents);
            m_signalWindow2.eventWasClicked += new eventsClickedHandler(openEventAddUI);
            m_signalWindow2.EventsEeg.eventsToDisplay += new eventsToDisplay(openEventDisplayUI);
            m_signalWindow2.EventsEeg.eventsToDelete += new eventsToDelete(deleteEvents);

            dataTexScroll = scrollTex.GetPixels();
            m_eventList.Initialize();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
                goToEventLeft();

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
                goToEventRight();
        }

        void OnDestroy()
        {
            activateEventsButton.onClick.RemoveAllListeners();
            showEventsButton.onClick.RemoveAllListeners();
            saveEvents.onClick.RemoveAllListeners();
            loadEvents.onClick.RemoveAllListeners();
            deleteEventsButton.onClick.RemoveAllListeners();

            m_signalWindow1.eventWasClicked += new eventsClickedHandler(openEventAddUI);
            m_signalWindow1.EventsEeg.eventsToDisplay += new eventsToDisplay(openEventDisplayUI);
            m_signalWindow1.EventsEeg.eventsToDelete += new eventsToDelete(deleteEvents);
            m_signalWindow2.eventWasClicked += new eventsClickedHandler(openEventAddUI);
            m_signalWindow2.EventsEeg.eventsToDisplay += new eventsToDisplay(openEventDisplayUI);
            m_signalWindow2.EventsEeg.eventsToDelete += new eventsToDelete(deleteEvents);

            for (int i = 0; i < panelContent.transform.childCount; i++)
            {
                Destroy(panelContent.GetChild(i));
            }

            scrollTex = Instantiate(scrollOrig);
        }

        void activateEventsMode()
        {
            addEvent = !addEvent;
            if (addEvent)
                activateEventPic.sprite = stopEventPic;
            else
                activateEventPic.sprite = startEventPic;
        }

        void showEventsMode()
        {
            showEvent = !showEvent;
            if (showEvent)
                showEventPic.sprite = hideEventSprite;
            else
                showEventPic.sprite = showEventSprite;

            showEvents(showEvent);
        }

        void openEventAddUI(TraceEvent currentEvent, int traceID)
        {
            if (addEvent)
            {
                if (infoEdit == null && infoDisp == null)
                {
                    GameObject addUI = Instantiate(eventAddUI);

                    if (traceID == 0)
                    {
                        addUI.transform.SetParent(m_signalWindow1.gameObject.transform);
                        currentEvent.secondElecOfInterest = m_signalWindow2.TraceEeg.LabelElectrode;
                    }
                    else
                    {
                        addUI.transform.SetParent(m_signalWindow2.gameObject.transform);
                        currentEvent.secondElecOfInterest = m_signalWindow1.TraceEeg.LabelElectrode;
                    }
                    addUI.transform.localScale = new Vector3(1, 1, 1);
                    addUI.transform.localPosition = new Vector3(0, 0, -402);

                    infoEdit = addUI.GetComponent<EventInfoEdit>();
                    infoEdit.init(currentEvent, eventMemory, false);
                    infoEdit.eventValid += new eventValidated(eventValidatedForUI);
                    infoEdit.aaaagh += new imDying(removeConnectionAddUi);
                }
            }
        }

        void openEventModifyUI(TraceEvent currentEvent, int traceID)
        {
            traceIDCalled = traceID;
            eventCalled = currentEvent;

            if (infoEdit == null)
            {
                GameObject addUI = Instantiate(eventAddUI);

                if (traceIDCalled == 0)
                    addUI.transform.SetParent(m_signalWindow1.gameObject.transform);
                else
                    addUI.transform.SetParent(m_signalWindow2.gameObject.transform);
                addUI.transform.localScale = new Vector3(1, 1, 1);
                addUI.transform.localPosition = new Vector3(0, 0, -402);

                infoEdit = addUI.GetComponent<EventInfoEdit>();
                infoEdit.init(currentEvent, eventMemory, true);
                infoEdit.eventModifed += new eventModifValidated((modifyedEvent) =>
                {
                    applyChangeToEvent(modifyedEvent, eventCalled);
                });
                infoEdit.eventsToDelete += new eventsToDelete((eventToDel, id) => { deleteEvents(eventToDel, id); });
                infoEdit.aaaagh += new imDying(removeConnectionEditUI);
            }
        }

        void openEventDisplayUI(TraceEvent currentEvent, int traceID)
        {
            traceIDCalled = traceID;
            if (infoDisp == null && infoEdit == null)
            {
                GameObject dispUI = Instantiate(eventDispUI);

                if (traceIDCalled == 0)
                    dispUI.transform.SetParent(m_signalWindow1.gameObject.transform);
                else
                    dispUI.transform.SetParent(m_signalWindow2.gameObject.transform);
                dispUI.transform.localScale = new Vector3(1, 1, 1);
                dispUI.transform.localPosition = new Vector3(0, 0, -402);

                infoDisp = dispUI.GetComponent<EventInfoDisplay>();
                infoDisp.init(currentEvent);
                infoDisp.editEvent += new eventToEditHandler((eventToEdit) =>
                {
                    openEventModifyUI(eventToEdit, traceIDCalled);
                });
                infoDisp.processCorrelation += new calculateCorrelation((TraceEvent e) =>
                {
                    StartCoroutine(calcCorr(e));
                });
                infoDisp.processCorrelation2D += new calculateCorrelation2D((TraceEvent e) =>
                {
                    StartCoroutine(calcCorr2D(e));
                });
                infoDisp.eventModifed += new eventModifPlot((TraceEvent modifiedOne, TraceEvent previousOne) =>
                {
                    applyChangeToEvent(modifiedOne, previousOne);
                });
                infoDisp.aaaagh += new imDying(removeConnectionDispUI);
            }
        }

        void deleteEvents(TraceEvent eventToDelete, int traceID)
        {
            List<int> ids = m_eventList.Objects.Select((item, index) => new { Item = item, Index = index })
                             .Where(x => x.Item.sample == eventToDelete.sample)
                             .Select(x => x.Index)
                             .ToList();

            removeEventToTexture(eventToDelete);
            m_eventList.Remove(m_eventList.Objects[ids[0]]); //par ref

            //Remove : 
            //  -event connection
            //  -then destroy object 
            //  -then the reference in list of gameobject 
            //int index1 = win1.eventsAdded.FindIndex(x => x.name == "Event - " + eventToDelete.sample);
            m_signalWindow1.EventsEeg.removeEventConnections(m_signalWindow1.EventsEeg.eventsAdded[ids[0]].gameObject);
            Destroy(m_signalWindow1.EventsEeg.eventsAdded[ids[0]].gameObject);
            m_signalWindow1.EventsEeg.eventsAdded.RemoveAt(ids[0]);
            m_signalWindow2.EventsEeg.removeEventConnections(m_signalWindow2.EventsEeg.eventsAdded[ids[0]].gameObject);
            Destroy(m_signalWindow2.EventsEeg.eventsAdded[ids[0]].gameObject);
            m_signalWindow2.EventsEeg.eventsAdded.RemoveAt(ids[0]);
        }

        void eventValidatedForUI(TraceEvent currentEvent)
        {
            eventMemory = new TraceEvent(currentEvent);

            m_eventList.Add(currentEvent);
            m_eventList.sortBySample();
            List<int> ids = m_eventList.Objects.Select((item, index) => new { Item = item, Index = index })
                                             .Where(x => x.Item.sample == currentEvent.sample)
                                             .Select(x => x.Index)
                                             .ToList();
            addEventToTexture(currentEvent);
            newEventToShow(currentEvent, ids[0]);
        }

        void applyChangeToEvent(TraceEvent modifyiedEvent, TraceEvent previousEvent)
        {
            var eventToChangeObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(
                                       obj => obj.name == "Event - " + previousEvent.sample);

            List<int> ids = m_eventList.Objects.Select((item, index) => new { Item = item, Index = index })
                                             .Where(x => x.Item.sample == previousEvent.sample)
                                             .Select(x => x.Index)
                                             .ToList();

            if (ids.Count > 0)
            {
                TraceEvent eventFound = m_eventList.Objects[ids[0]];
                removeEventToTexture(eventFound);

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
                    modifyiedEvent.elecOfInterest = m_signalWindow1.TraceEeg.LabelElectrode;
                    modifyiedEvent.secondElecOfInterest = m_signalWindow2.TraceEeg.LabelElectrode;
                }

                eventFound.duration = modifyiedEvent.duration;
                addEventToTexture(eventFound);

                //if event goes from no duration to with duration or the other way around we switch it
                if ((modifyiedEvent.duration - memDuration == modifyiedEvent.duration) ||
                    (modifyiedEvent.duration - memDuration == -memDuration))
                {
                    eventToChangeObjects.ElementAt(0).GetComponent<EventTrace>().deleteMe();
                    eventValidatedForUI(modifyiedEvent);
                }

                foreach (var eventToChange in eventToChangeObjects)
                {
                    eventToChange.GetComponent<EventTrace>().UpdateEvent(modifyiedEvent);
                }

                m_eventList.Refresh();
            }
        }

        void saveEventsList()
        {
            string btvPosFile = FileBrowser.getSaveFileName(new string[] { "pos" }, "Save Event File", m_signalWindow1.TraceEeg.fileHandle.fileFolder);
            btvPosFile = btvPosFile.Replace(".pos", "_btv.pos");

            try
            {
                using (StreamWriter sw = new StreamWriter(btvPosFile))
                {
                    foreach (TraceEvent eegEvent in m_eventList.Objects)
                    {
                        sw.Write(eegEvent.sample.ToString().PadRight(10));
                        sw.Write(eegEvent.code.ToString().PadRight(10));
                        sw.Write("0\n");
                    }

                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not write btv pos file");
                Console.WriteLine(e.Message);
            }

            btvPosFile = btvPosFile.Replace("_btv.pos", ".btv");
            try
            {
                using (StreamWriter sw = new StreamWriter(btvPosFile))
                {
                    foreach (TraceEvent eegEvent in m_eventList.Objects)
                    {
                        int timeInSec = eegEvent.sample / m_signalWindow1.TraceEeg.SamplingFrequency;
                        int h = timeInSec / 3600;
                        int m = (timeInSec / 60) % 60;
                        int s = timeInSec % 60;

                        string timeString = "";
                        if (h > 0)
                            timeString = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
                        else
                            timeString = "00:" + returnTimeString(m) + ":" + returnTimeString(s);

                        sw.Write(timeString.PadRight(10));
                        sw.Write(eegEvent.comment.PadRight(40));
                        sw.Write(eegEvent.code.ToString().PadRight(10));
                        sw.Write(eegEvent.sample.ToString().PadRight(10));
                        sw.Write(eegEvent.duration.ToString().PadRight(10));
                        sw.Write(eegEvent.elecOfInterest.PadRight(10));
                        sw.WriteLine(eegEvent.secondElecOfInterest);
                    }

                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not write btv pos file");
                Console.WriteLine(e.Message);
            }
        }

        string returnTimeString(int time)
        {
            if (time < 10)
            {
                return "0" + time;
            }
            else
            {
                return time.ToString();
            }
        }

        void loadEventList()
        {
            string pathFile = FileBrowser.getOpenFileName(new string[] { "btv", "pos" }, "Select an Event File", m_signalWindow1.TraceEeg.fileHandle.fileFolder);
            if (File.Exists(pathFile))
            {
                string[] pathSplit = pathFile.Split(new char[] { '.' });

                List<TraceEvent> eventLoaded = null;
                switch (pathSplit[pathSplit.Length - 1])
                {
                    case "btv":
                        eventLoaded = loadBTVFile(pathFile);
                        break;
                    case "pos":
                        eventLoaded = loadPOSFile(pathFile);
                        break;
                }

                //if elements already loaded , delete everything
                for (int i = m_eventList.Objects.Length - 1; i >= 0; i--)
                    deleteEvents(m_eventList.Objects.ElementAt(i), -1);

                for (int i = 0; i < eventLoaded.Count; i++)
                    eventValidatedForUI(eventLoaded[i]);
            }
        }

        List<TraceEvent> loadBTVFile(string pathFile)
        {
            try
            {
                using (StreamReader sr = new StreamReader(pathFile))
                {
                    List<TraceEvent> eventLoaded = new List<TraceEvent>();
                    string r;

                    while ((r = sr.ReadLine()) != null)
                    {
                        //the regex mean you split by everything but a single white space
                        string[] resultSplit = System.Text.RegularExpressions.Regex.Split(r, @"\s{2,}");
                        if (resultSplit.Count() == 7)
                        {
                            eventEeg currentEvent = new eventEeg(int.Parse(resultSplit[2]), int.Parse(resultSplit[3]), m_signalWindow1.TraceEeg.SamplingFrequency);
                            eventLoaded.Add(new TraceEvent(currentEvent, int.Parse(resultSplit[4]), resultSplit[5], resultSplit[6], resultSplit[1]));
                        }
                    }
                    sr.Close();
                    return eventLoaded;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The btv file could not be read:");
                Console.WriteLine(e.Message);
                return new List<TraceEvent>();
            }
        }

        List<TraceEvent> loadPOSFile(string pathFile)
        {
            try
            {
                using (StreamReader sr = new StreamReader(pathFile))
                {
                    List<TraceEvent> eventLoaded = new List<TraceEvent>();
                    string r;

                    while ((r = sr.ReadLine()) != null)
                    {
                        string[] resultSplit = r.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (resultSplit.Count() == 3)
                        {
                            eventLoaded.Add(new TraceEvent(new eventEeg(int.Parse(resultSplit[1]), int.Parse(resultSplit[0]), m_signalWindow1.TraceEeg.SamplingFrequency)));
                        }
                    }
                    sr.Close();
                    return eventLoaded;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The pos file could not be read:");
                Console.WriteLine(e.Message);
                return new List<TraceEvent>();
            }
        }

        void removeConnectionAddUi()
        {
            infoEdit.eventValid -= new eventValidated(eventValidatedForUI);
            infoEdit.aaaagh -= new imDying(removeConnectionAddUi);
            infoEdit = null;
        }

        void removeConnectionEditUI()
        {
            infoEdit.eventModifed -= new eventModifValidated((modifyedEvent) =>
            {
                applyChangeToEvent(modifyedEvent, eventCalled);
            });
            infoEdit.eventsToDelete -= new eventsToDelete((eventToDel, id) => { deleteEvents(eventToDel, id); });
            infoEdit.aaaagh -= new imDying(removeConnectionAddUi);
            infoEdit = null;
        }

        void removeConnectionDispUI()
        {
            infoDisp.editEvent -= new eventToEditHandler((eventToEdit) =>
            {
                openEventAddUI(eventToEdit, traceIDCalled);
            });
            infoDisp.processCorrelation -= new calculateCorrelation((TraceEvent e) =>
            {
                StartCoroutine(calcCorr(e));
            });
            infoDisp.processCorrelation2D -= new calculateCorrelation2D((TraceEvent e) =>
            {
                StartCoroutine(calcCorr2D(e));
            });
            infoDisp.eventModifed -= new eventModifPlot((TraceEvent modifiedOne, TraceEvent previousOne) =>
            {
                applyChangeToEvent(modifiedOne, previousOne);
            });
            infoDisp.aaaagh -= new imDying(removeConnectionDispUI);
            infoDisp = null;
        }

        void addEventToTexture(TraceEvent currentEvent)
        {
            float perC = ((((float)currentEvent.sample / m_signalWindow1.TraceEeg.SamplingFrequency) / m_videoPlayer.videoInterface.totalVideoTime) * 1000);
            int pixelID = (int)(perC * scrollTex.width);

            if (currentEvent.duration > 0)
            {
                if (currentEvent.duration > 1000)
                {
                    float durationInSample = (currentEvent.duration * ((float)m_signalWindow1.TraceEeg.SamplingFrequency / 1000));
                    float perCDuration = (((((float)currentEvent.sample + durationInSample) / m_signalWindow1.TraceEeg.SamplingFrequency) / m_videoPlayer.videoInterface.totalVideoTime) * 1000);
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

        void removeEventToTexture(TraceEvent currentEvent)
        {
            float perC = ((((float)currentEvent.sample / m_signalWindow1.TraceEeg.SamplingFrequency) / m_videoPlayer.videoInterface.totalVideoTime) * 1000);
            int pixelID = (int)(perC * scrollTex.width);

            if (currentEvent.duration > 0)
            {
                if (currentEvent.duration > 1000)
                {
                    float durationInSample = (currentEvent.duration * ((float)m_signalWindow1.TraceEeg.SamplingFrequency / 1000));
                    float perCDuration = (((((float)currentEvent.sample + durationInSample) / m_signalWindow1.TraceEeg.SamplingFrequency) / m_videoPlayer.videoInterface.totalVideoTime) * 1000);
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

        void goToEventLeft()
        {
            if (m_eventList.Objects.Length > 0)
            {
                long timeSec = m_videoPlayer.videoInterface.currentTime / 1000;
                long timeSample = timeSec * m_signalWindow1.TraceEeg.SamplingFrequency;
                var keys = m_eventList.sampleValues;
                var index = keys.BinarySearch((int)timeSample);

                if (Math.Abs(index) - 1 == 0)
                {
                    currentPos = 0;
                    m_videoPlayer.changeTimeClick((m_eventList.Objects[currentPos].sample / m_signalWindow1.TraceEeg.SamplingFrequency) * 1000);
                    m_videoPlayer.setTime((m_eventList.Objects[currentPos].sample / m_signalWindow1.TraceEeg.SamplingFrequency) * 1000);
                }
                else
                {
                    currentPos = Math.Abs(index) - 1;
                    m_videoPlayer.changeTimeClick((m_eventList.Objects[currentPos - 1].sample / m_signalWindow1.TraceEeg.SamplingFrequency) * 1000);
                    m_videoPlayer.setTime((m_eventList.Objects[currentPos - 1].sample / m_signalWindow1.TraceEeg.SamplingFrequency) * 1000);
                }
            }
        }

        void goToEventRight()
        {
            if (m_eventList.Objects.Length > 0)
            {
                long timeSec = m_videoPlayer.videoInterface.currentTime / 1000;
                long timeSample = timeSec * m_signalWindow1.TraceEeg.SamplingFrequency;
                var keys = m_eventList.sampleValues;
                var index = keys.BinarySearch((int)timeSample);

                currentPos = Math.Abs(index) - 1;
                if (currentPos + 1 < m_eventList.Objects.Length)
                {
                    m_videoPlayer.changeTimeClick((m_eventList.Objects[currentPos + 1].sample / m_signalWindow1.TraceEeg.SamplingFrequency) * 1000);
                    m_videoPlayer.setTime((m_eventList.Objects[currentPos + 1].sample / m_signalWindow1.TraceEeg.SamplingFrequency) * 1000);
                }
            }
        }

        IEnumerator calcCorr(TraceEvent currentEvent)
        {
            yield return Ninja.JumpBack;
            m_coroutineManager.StartCoroutine(c_correlation(currentEvent));
            yield return Ninja.JumpToUnity;
        }

        IEnumerator c_correlation(TraceEvent currentEvent)
        {
            int nbElec = m_signalWindow1.TraceEeg.fileHandle.electrodes.Length;
            List<int> ids = m_eventList.Objects.Select((item, index) => new { Item = item, Index = index })
                                     .Where(x => x.Item.sample == currentEvent.sample)
                                     .Select(x => x.Index)
                                     .ToList();

            m_eventList.Objects[ids[0]].correlationArray = new float[nbElec];
            m_eventList.Objects[ids[0]].correlation2DArray = null;

            int beginSample = m_eventList.Objects[ids[0]].sample;
            int durationSample = (m_eventList.Objects[ids[0]].duration / 1000) * m_eventList.Objects[ids[0]].samplingFrequency;

            int idBase = m_signalWindow1.TraceEeg.fileHandle.electrodes.ToList().FindIndex(x => x.name == currentEvent.elecOfInterest);
            if (idBase != -1)
            {
                int[] sizes = new int[5] { idBase, nbElec, beginSample, durationSample, m_signalWindow1.TraceEeg.fileHandle.nbSam };
                pearsonCoefficientsCorrelation(m_eventList.Objects[ids[0]].correlationArray, m_signalWindow1.TraceEeg.fileHandle.eegData, sizes);
            }
            else
            {
                if (currentEvent.elecOfInterest.StartsWith("AUD"))
                {
                    int[] sizes = new int[4] { nbElec, beginSample, durationSample, m_signalWindow1.TraceEeg.fileHandle.nbSam };
                    pearsonCoefficientsCorrelation2(m_eventList.Objects[ids[0]].correlationArray, m_videoPlayer.audioWav.getAudioHandle(m_videoPlayer.audioWav.idAudioHandle), m_signalWindow1.TraceEeg.fileHandle.eegData, sizes);
                }
            }
            yield return null;
        }

        IEnumerator calcCorr2D(TraceEvent currentEvent)
        {
            yield return Ninja.JumpBack;
            m_coroutineManager.StartCoroutine(c_correlation2D(currentEvent));
            yield return Ninja.JumpToUnity;
        }

        IEnumerator c_correlation2D(TraceEvent currentEvent)
        {
            int nbElec = m_signalWindow1.TraceEeg.fileHandle.electrodes.Length;
            List<int> ids = m_eventList.Objects.Select((item, index) => new { Item = item, Index = index })
                                     .Where(x => x.Item.sample == currentEvent.sample)
                                     .Select(x => x.Index)
                                     .ToList();

            m_eventList.Objects[ids[0]].correlationArray = null;
            m_eventList.Objects[ids[0]].correlation2DArray = new float[nbElec][];
            for (int i = 0; i < m_eventList.Objects[ids[0]].correlation2DArray.Length; i++)
                m_eventList.Objects[ids[0]].correlation2DArray[i] = new float[nbElec];

            int beginSample = m_eventList.Objects[ids[0]].sample;
            int durationSample = (m_eventList.Objects[ids[0]].duration / 1000) * m_eventList.Objects[ids[0]].samplingFrequency;

            for (int i = 0; i < m_eventList.Objects[ids[0]].correlation2DArray.Length; i++)
            {
                int[] sizes = new int[5] { i, nbElec, beginSample, durationSample, m_signalWindow1.TraceEeg.fileHandle.nbSam };
                pearsonCoefficientsCorrelation(m_eventList.Objects[ids[0]].correlation2DArray[i], m_signalWindow1.TraceEeg.fileHandle.eegData, sizes);
            }

            yield return null;
        }

        #region DLLImport
        [DllImport("BTVReplayLibraryC++", EntryPoint = "pearsonCoefficientsCorrelation", CallingConvention = CallingConvention.Cdecl)]
        static private extern void pearsonCoefficientsCorrelation(float[] coeffs, float[] eegData, int[] sizes);

        [DllImport("BTVReplayLibraryC++", EntryPoint = "pearsonCoefficientsCorrelation2", CallingConvention = CallingConvention.Cdecl)]
        static private extern void pearsonCoefficientsCorrelation2(float[] coeffs, float[] baseArray, float[] eegData, int[] sizes);

        protected override void AddTools()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}