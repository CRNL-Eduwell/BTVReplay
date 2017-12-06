using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CielaSpike;

public delegate void newEventToShowHandler(TraceEvent newEvent, int id);

public class eventsOptions : MonoBehaviour
{
    public SortedList<int, TraceEvent> userEvents
    {
        get
        {
            return events;
        }
    }

    public event newEventToShowHandler newEventToShow;

    GameObject eventHubClick = null;
    GameObject eventAddUI = null;
    GameObject eventDispUI = null;
    VideoPlayer v = null;
    TraceCurve win1 = null;
    TraceCurve win2 = null;
    CoroutineManager coMana = null;
    EventInfoEdit infoEdit = null;
    EventInfoDisplay infoDisp = null;
    int traceIDCalled = 0;
    TraceEvent eventCalled = null;
    TraceEvent eventMemory = null;

    Button activateEventsButton = null;
    Image activateEventPic = null;
    Transform panelContent = null;
    Button saveEvents = null;
    Button loadEvents = null;
    Sprite startEventPic = null, stopEventPic = null;

    public bool addEvent = false;
    SortedList<int, TraceEvent> events = new SortedList<int, TraceEvent>();

    GameObject scrollObj = null;
    Texture2D scrollOrig = null, scrollTex = null;
    Color[] dataTexScroll;
    Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
    Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    int currentPos = -1;

    public void init(ButtonUI button)
    {
        eventHubClick = Resources.Load("Prefabs/Hub-Event", typeof(GameObject)) as GameObject;
        eventAddUI = Resources.Load("Prefabs/EventInfoEdit", typeof(GameObject)) as GameObject;
        eventDispUI = Resources.Load("Prefabs/EventInfoDisplay", typeof(GameObject)) as GameObject;
        scrollOrig = Resources.Load("Pictures/eventScroll", typeof(Texture2D)) as Texture2D;
        startEventPic = Resources.Load("Pictures/ConfigBar/StartEvents", typeof(Sprite)) as Sprite;
        stopEventPic = Resources.Load("Pictures/ConfigBar/StopEvents", typeof(Sprite)) as Sprite;

        //==
        v = GameObject.Find("PanelR").GetComponent<VideoPlayer>();
        win1 = GameObject.Find("Trace1Window").GetComponent<TraceCurve>();
        win2 = GameObject.Find("Trace2Window").GetComponent<TraceCurve>();
        coMana = GameObject.Find("ringSelect").GetComponent<CoroutineManager>();
        scrollObj = GameObject.Find("TimeScrollBar");
        scrollTex = Instantiate(scrollOrig);
        //==
        panelContent = button.optionsPanel2.transform.GetChild(0).GetChild(0).GetChild(0);
        loadEvents = button.optionsPanel.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        saveEvents = button.optionsPanel.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        activateEventsButton = button.optionsPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>();
        activateEventPic = button.optionsPanel.transform.GetChild(0).GetChild(2).GetComponent<Image>();
        scrollObj.GetComponent<RawImage>().texture = scrollTex;
        //==
        activateEventsButton.onClick.AddListener(activateEventsMode);
        saveEvents.onClick.AddListener(saveEventsList);
        loadEvents.onClick.AddListener(loadEventList);
        win1.eventWasClicked += new eventsClickedHandler(openEventAddUI);
        win2.eventWasClicked += new eventsClickedHandler(openEventAddUI);
        win1.eventsToDisplay += new eventsToDisplay(openEventDisplayUI);
        win2.eventsToDisplay += new eventsToDisplay(openEventDisplayUI);
        win1.eventsToDelete += new eventsToDelete(deleteEvents);
        win2.eventsToDelete += new eventsToDelete(deleteEvents);
        //==
        win1.eventCode = 0;
        win2.eventCode = 0;

        dataTexScroll = scrollTex.GetPixels();
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
        saveEvents.onClick.RemoveAllListeners();
        loadEvents.onClick.RemoveAllListeners();
        win1.eventWasClicked -= new eventsClickedHandler(openEventAddUI);
        win2.eventWasClicked -= new eventsClickedHandler(openEventAddUI);
        win1.eventsToDisplay -= new eventsToDisplay(openEventDisplayUI);
        win2.eventsToDisplay -= new eventsToDisplay(openEventDisplayUI);
        win1.eventsToDelete -= new eventsToDelete(deleteEvents);
        win2.eventsToDelete -= new eventsToDelete(deleteEvents);

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

    void openEventAddUI(TraceEvent currentEvent, int traceID)
    {
        if (addEvent)
        {
            if (infoEdit == null && infoDisp == null)
            {
                GameObject addUI = Instantiate(eventAddUI);

                if (traceID == 0)
                {
                    addUI.transform.SetParent(win1.gameObject.transform);
                    currentEvent.secondElecOfInterest = win2.labelElectrode.text;
                }
                else
                {
                    addUI.transform.SetParent(win2.gameObject.transform);
                    currentEvent.secondElecOfInterest = win1.labelElectrode.text;
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
                addUI.transform.SetParent(win1.gameObject.transform);
            else
                addUI.transform.SetParent(win2.gameObject.transform);
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
                dispUI.transform.SetParent(win1.gameObject.transform);
            else
                dispUI.transform.SetParent(win2.gameObject.transform);
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
            infoDisp.eventModifed += new eventModifPlot((TraceEvent modifiedOne, TraceEvent previousOne) =>
            {
                applyChangeToEvent(modifiedOne, previousOne);
            });
            infoDisp.aaaagh += new imDying(removeConnectionDispUI);
        }
    }

    void deleteEvents(TraceEvent eventToDelete, int traceID)
    {
        removeEventToTexture(eventToDelete);
        //Debug.Log("del ev " + eventToDelete.sample);
        //==List behind the scene
        //int id = events.IndexOfKey(eventToDelete.sample);
        events.Remove(eventToDelete.sample);

        //Remove : 
        //  -event connection
        //  -then destroy object 
        //  -then the reference in list of gameobject 
        int index1 = win1.eventsAdded.FindIndex(x => x.name == "Event - " + eventToDelete.sample);
        win1.removeEventConnections(win1.eventsAdded[index1].gameObject);
        Destroy(win1.eventsAdded[index1].gameObject);
        win1.eventsAdded.RemoveAt(index1);
        win2.removeEventConnections(win2.eventsAdded[index1].gameObject);
        Destroy(win2.eventsAdded[index1].gameObject);
        win2.eventsAdded.RemoveAt(index1);

        //== Event in Hub
        var hubObj = GameObject.Find("HubEvent - " + eventToDelete.sample);
        if (hubObj != null)
        {
            hubObj.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(hubObj);
        }
    }

    void eventValidatedForUI(TraceEvent currentEvent)
    {
        eventMemory = new TraceEvent(currentEvent);

        events.Add(currentEvent.sample, currentEvent);
        int id = events.IndexOfKey(currentEvent.sample);

        GameObject currentEventGO = Instantiate(eventHubClick);
        currentEventGO.name = "HubEvent - " + currentEvent.sample;
        currentEventGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            v.setTime((currentEvent.sample / win1.samplingFrequency) * 1000);
        });

        int timeInSec = currentEvent.sample / win1.samplingFrequency;
        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        if (h > 0)
            currentEventGO.transform.GetChild(0).GetComponent<Text>().text = h + ":" + m + ":" + s;
        else
            currentEventGO.transform.GetChild(0).GetComponent<Text>().text = "00:" + m + ":" + s;

        currentEventGO.transform.GetChild(1).GetComponent<Text>().text = currentEvent.elecOfInterest;
        currentEventGO.transform.GetChild(2).GetComponent<Text>().text = currentEvent.comment;
        currentEventGO.transform.SetParent(panelContent);
        currentEventGO.transform.localScale = new Vector3(1, 1, 1);
        currentEventGO.transform.position = currentEventGO.transform.parent.position;
        currentEventGO.transform.SetSiblingIndex(id);

        addEventToTexture(currentEvent);
        newEventToShow(currentEvent, id);
    }

    void applyChangeToEvent(TraceEvent modifyiedEvent, TraceEvent previousEvent)
    {
        var eventToChangeObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(
                                   obj => obj.name == "Event - " + previousEvent.sample);

        if (eventToChangeObjects.Count() > 0)
        {
            int id = events.IndexOfKey(previousEvent.sample);
            removeEventToTexture(events.Values[id]);
            int memDuration = events.Values[id].duration;
            events.Values[id].elecOfInterest = modifyiedEvent.elecOfInterest;
            events.Values[id].code = modifyiedEvent.code;
            events.Values[id].comment = modifyiedEvent.comment;

            if (modifyiedEvent.duration != events.Values[id].duration)
                events.Values[id].correlationArray = null;

            if (modifyiedEvent.elecOfInterest == "")
            {
                modifyiedEvent.elecOfInterest = win1.labelElectrode.text;
                modifyiedEvent.secondElecOfInterest = win2.labelElectrode.text;
            }

            events.Values[id].duration = modifyiedEvent.duration;
            addEventToTexture(events.Values[id]);

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
        }
    }

    void saveEventsList()
    {
        string btvPosFile = QtGUI_dll.Instance.getSaveFileName(new string[] { "pos" }, "Save Event File", win1.fileHandle.fileFolder);
        btvPosFile = btvPosFile.Replace(".pos", "_btv.pos");

        try
        {
            using (StreamWriter sw = new StreamWriter(btvPosFile))
            {
                foreach (KeyValuePair<int, TraceEvent> kvp in events)
                {
                    sw.Write(kvp.Value.sample.ToString().PadRight(10));
                    sw.Write(kvp.Value.code.ToString().PadRight(10));
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
                foreach (KeyValuePair<int, TraceEvent> kvp in events)
                {
                    int timeInSec = kvp.Value.sample / win1.samplingFrequency;
                    int h = timeInSec / 3600;
                    int m = (timeInSec / 60) % 60;
                    int s = timeInSec % 60;

                    string timeString = "";
                    if (h > 0)
                        timeString = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
                    else
                        timeString = "00:" + returnTimeString(m) + ":" + returnTimeString(s);

                    sw.Write(timeString.PadRight(10));
                    sw.Write(kvp.Value.comment.PadRight(40));
                    sw.Write(kvp.Value.code.ToString().PadRight(10));
                    sw.Write(kvp.Value.sample.ToString().PadRight(10));
                    sw.Write(kvp.Value.duration.ToString().PadRight(10));
                    sw.Write(kvp.Value.elecOfInterest.PadRight(10));
                    sw.WriteLine(kvp.Value.secondElecOfInterest);
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
        string pathFile = QtGUI_dll.Instance.getOpenFileName(new string[] { "btv", "pos" }, "Select an Event File", win1.fileHandle.fileFolder);
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
            for (int i = events.Count - 1; i >= 0; i--)
                deleteEvents(events.Values.ElementAt(i), -1);

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
                        eventEeg currentEvent = new eventEeg(int.Parse(resultSplit[2]), int.Parse(resultSplit[3]), win1.samplingFrequency);
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
                        eventLoaded.Add(new TraceEvent(new eventEeg(int.Parse(resultSplit[1]), int.Parse(resultSplit[0]), win1.samplingFrequency)));
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
        infoDisp.eventModifed -= new eventModifPlot((TraceEvent modifiedOne, TraceEvent previousOne) =>
        {
            applyChangeToEvent(modifiedOne, previousOne);
        });
        infoDisp.aaaagh -= new imDying(removeConnectionDispUI);
        infoDisp = null;
    }

    void addEventToTexture(TraceEvent currentEvent)
    {
        float perC = ((((float)currentEvent.sample / win1.samplingFrequency) / v.videoInterface.totalVideoTime) * 1000);
        int pixelID = (int)(perC * scrollTex.width);

        if (currentEvent.duration > 0)
        {
            if (currentEvent.duration > 1000)
            {
                float durationInSample = (currentEvent.duration * ((float)win1.samplingFrequency / 1000));
                float perCDuration = (((((float)currentEvent.sample + durationInSample) / win1.samplingFrequency) / v.videoInterface.totalVideoTime) * 1000);
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
        float perC = ((((float)currentEvent.sample / win1.samplingFrequency) / v.videoInterface.totalVideoTime) * 1000);
        int pixelID = (int)(perC * scrollTex.width);

        if (currentEvent.duration > 0)
        {
            if (currentEvent.duration > 1000)
            {
                float durationInSample = (currentEvent.duration * ((float)win1.samplingFrequency / 1000));
                float perCDuration = (((((float)currentEvent.sample + durationInSample) / win1.samplingFrequency) / v.videoInterface.totalVideoTime) * 1000);
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
        if (events.Count > 0)
        {
            long timeSec = v.videoInterface.currentTime / 1000;
            long timeSample = timeSec * win1.samplingFrequency;
            var keys = new List<int>(events.Keys);
            var index = keys.BinarySearch((int)timeSample);

            if (Math.Abs(index) - 1 == 0)
            {
                currentPos = 0;
                v.changeTimeClick((events[keys[currentPos]].sample / win1.samplingFrequency) * 1000);
                v.setTime((events[keys[currentPos]].sample / win1.samplingFrequency) * 1000);
            }
            else
            {
                currentPos = Math.Abs(index) - 1;
                v.changeTimeClick((events[keys[currentPos - 1]].sample / win1.samplingFrequency) * 1000);
                v.setTime((events[keys[currentPos - 1]].sample / win1.samplingFrequency) * 1000);
            }
        }
    }

    void goToEventRight()
    {
        if (events.Count > 0)
        {
            long timeSec = v.videoInterface.currentTime / 1000;
            long timeSample = timeSec * win1.samplingFrequency;
            var keys = new List<int>(events.Keys);
            var index = keys.BinarySearch((int)timeSample);

            if (Math.Abs(index) < keys.Count)
            {
                currentPos = Math.Abs(index) - 1;
                v.changeTimeClick((events[keys[currentPos + 1]].sample / win1.samplingFrequency) * 1000);
                v.setTime((events[keys[currentPos + 1]].sample / win1.samplingFrequency) * 1000);
            }
        }
    }

    IEnumerator calcCorr(TraceEvent currentEvent)
    {
        yield return Ninja.JumpBack;
        coMana.StartCoroutine(c_correlation(currentEvent));
        yield return Ninja.JumpToUnity;
    }

    IEnumerator c_correlation(TraceEvent currentEvent)
    {
        int nbElec = win1.fileHandle.electrodes.Length;
        int id = events.IndexOfKey(currentEvent.sample);
        events.Values[id].correlationArray = new float[nbElec];

        int beginSample = events.Values[id].sample;
        int durationSample = (events.Values[id].duration / 1000) * events.Values[id].samplingFrequency;

        int idBase = win1.fileHandle.electrodes.ToList().FindIndex(x => x.name == currentEvent.elecOfInterest);
        if (idBase != -1)
        {
            int[] sizes = new int[5] { idBase, nbElec, beginSample, durationSample, win1.fileHandle.nbSam };
            pearsonCoefficientsCorrelation(events.Values[id].correlationArray, win1.fileHandle.eegData, sizes);
        }
        else
        {
            if (currentEvent.elecOfInterest.StartsWith("AUD"))
            {
                int[] sizes = new int[4] { nbElec, beginSample, durationSample, win1.fileHandle.nbSam };
                pearsonCoefficientsCorrelation2(events.Values[id].correlationArray, v.audioWav.getAudioHandle(win1.fileHandle.idFileHandle), win1.fileHandle.eegData, sizes);
            }
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

public delegate void offsetVideoChangedEventHandler(float newVal);
public delegate void toggleAudioTraceEventHandler(bool isTraceOn);
public delegate void gainAudioChangedEventHandler(float newGain);
public delegate void idAudioSmChangedEventHandler(int newIdSm);

public class videoOptions
{
    public event offsetVideoChangedEventHandler offsetVideoHasChanged;
    public event toggleAudioTraceEventHandler audioToggled;
    public event gainAudioChangedEventHandler gainAudioHasChanged;
    public event idAudioSmChangedEventHandler smAudioHasChanged;

    Button removeVideoOffset = null;
    Scrollbar offsetScrollBar = null;
    Button addVideoOffset = null;
    Text videoOffsetLabel = null;
    EventTrigger trigger = null;
    float offsetMilliSec = 0;
    //====
    Toggle showAudioTrace = null;
    Button filterAudio = null;
    Button loadAudio = null;
    //====
    Text gainLabel = null;
    Button gainAddButton = null;
    Button gainRemoveButton = null;
    float gain = 1;
    //====
    Button[] smButton = null;
    bool sm_ChoicePending = false;
    //====
    VideoPlayer vid = null;
    CoroutineManager coMana = null;
    //====
    Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
    Color softBlue = new Color(0.6117f, 0.7058f, 0.7960f, 0.627450f);

    public videoOptions(GameObject videoOptionsPanel)
    {
        gainLabel = videoOptionsPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>();
        gainAddButton = videoOptionsPanel.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        gainRemoveButton = videoOptionsPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>();
        //==
        videoOffsetLabel = videoOptionsPanel.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        removeVideoOffset = videoOptionsPanel.transform.GetChild(1).GetChild(1).GetComponent<Button>();
        offsetScrollBar = videoOptionsPanel.transform.GetChild(1).GetChild(2).GetComponent<Scrollbar>();
        addVideoOffset = videoOptionsPanel.transform.GetChild(1).GetChild(3).GetComponent<Button>();
        //==
        showAudioTrace = videoOptionsPanel.transform.GetChild(2).GetChild(0).GetComponent<Toggle>();
        filterAudio = videoOptionsPanel.transform.GetChild(2).GetChild(1).GetComponent<Button>();
        loadAudio = videoOptionsPanel.transform.GetChild(2).GetChild(2).GetComponent<Button>();
        //==

        smButton = new Button[6];
        smButton[0] = videoOptionsPanel.transform.GetChild(3).GetComponent<Button>();
        smButton[1] = videoOptionsPanel.transform.GetChild(4).GetComponent<Button>();
        smButton[2] = videoOptionsPanel.transform.GetChild(5).GetComponent<Button>();
        smButton[3] = videoOptionsPanel.transform.GetChild(6).GetComponent<Button>();
        smButton[4] = videoOptionsPanel.transform.GetChild(7).GetComponent<Button>();
        smButton[5] = videoOptionsPanel.transform.GetChild(8).GetComponent<Button>();
        //==
        vid = GameObject.Find("PanelR").transform.GetComponent<VideoPlayer>();
        coMana = GameObject.Find("ringSelect").GetComponent<CoroutineManager>();

        //[===]
        removeVideoOffset.onClick.AddListener(() =>
        {
            if (offsetMilliSec - 10 >= -60000)
            {
                offsetMilliSec -= 10;
                offsetScrollBar.value = ((offsetMilliSec / 1000) / 120) + 0.5f;
                setOffsetText(offsetMilliSec);
                offsetVideoHasChanged(offsetMilliSec);
            }
        });
        addVideoOffset.onClick.AddListener(() =>
        {
            if (offsetMilliSec + 10 <= 60000)
            {
                offsetMilliSec += 10;
                offsetScrollBar.value = ((offsetMilliSec / 1000) / 120) + 0.5f;
                setOffsetText(offsetMilliSec);
                offsetVideoHasChanged(offsetMilliSec);
            }
        });
        //==
        showAudioTrace.onValueChanged.AddListener((bool isChecked) =>
        {
            audioToggled(isChecked);
        });
        filterAudio.onClick.AddListener(() =>
        {
            coMana.StartCoroutine(vid.c_filterAudio());
            filterAudio.interactable = false;
            loadAudio.interactable = false;
            showAudioTrace.isOn = true;
        });
        loadAudio.onClick.AddListener(() =>
        {
            coMana.StartCoroutine(vid.c_loadAudio());
            filterAudio.interactable = false;
            loadAudio.interactable = false;
            showAudioTrace.isOn = true;
        });
        //==
        gainLabel.text = "Gain : " + gain;
        gainAddButton.onClick.AddListener(addGain);
        gainRemoveButton.onClick.AddListener(removeGain);
        //==
        for (int i = 0; i < 6; i++)
            connectButtonSM(i);
        changeButtonSMColor(0);
        //==
        trigger = offsetScrollBar.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerUp;
        entry2.callback.AddListener((eventData) => { setOffsetScrollBar(); });
        trigger.triggers.Add(entry2);

        videoOffsetLabel.text = "Offset : 00 m: 00 s: 00ms";
    }

    ~videoOptions()
    {
        removeVideoOffset.onClick.RemoveAllListeners();
        addVideoOffset.onClick.RemoveAllListeners();
        showAudioTrace.onValueChanged.RemoveAllListeners();
        filterAudio.onClick.RemoveAllListeners();
        loadAudio.onClick.RemoveAllListeners();
        gainAddButton.onClick.RemoveAllListeners();
        gainRemoveButton.onClick.RemoveAllListeners();

        for (int i = 0; i < 6; i++)
            smButton[i].onClick.RemoveAllListeners();

        for (int i = 0; i < trigger.triggers.Count; i++)
            trigger.triggers[i].callback.RemoveAllListeners();
    }

    void setOffsetScrollBar()
    {
        float offsetBar = offsetScrollBar.value - 0.5f;
        offsetMilliSec = (int)(offsetBar * 120) * 1000;
        setOffsetText(offsetMilliSec);
        offsetVideoHasChanged(offsetMilliSec);
    }

    void setOffsetText(float milliSec)
    {
        int m = ((int)milliSec / 1000) / 60;
        int s = ((int)milliSec / 1000) % 60;
        int ms = (int)milliSec - (((int)milliSec / 1000) * 1000);
        videoOffsetLabel.text = "Offset : " + m + "m: " + s + "s:" + ms + "ms";
    }

    public void setButtonsInteractable(bool value)
    {
        if (value)
        {
            filterAudio.interactable = true;
            loadAudio.interactable = false;
        }
        else
        {
            filterAudio.interactable = false;
            loadAudio.interactable = true;
        }
    }

    void addGain()
    {
        if (gain < 1 && gain >= -1)
            gain += 0.25f;
        else
            gain += 1;
        gainLabel.text = "Gain : " + gain;
        gainAudioHasChanged(gain);
    }

    void removeGain()
    {
        if (gain <= 1 && gain > -1)
            gain -= 0.25f;
        else
            gain -= 1;
        gainLabel.text = "Gain : " + gain;
        gainAudioHasChanged(gain);
    }

    void connectButtonSM(int id)
    {
        smButton[id].onClick.AddListener(() =>
        {
            if (!sm_ChoicePending)
            {
                setButtonsVisible(smButton, true, -1);
                sm_ChoicePending = true;
            }
            else
            {
                smAudioHasChanged(id);
                changeButtonSMColor(id);
                setButtonsVisible(smButton, true, id);
                sm_ChoicePending = false;
            }
        });
    }

    public void changeButtonSMColor(int id = -1)
    {
        for (int i = 0; i < 6; i++)
        {
            if (i == id)
                smButton[i].gameObject.GetComponent<Image>().color = hardBlue;
            else
                smButton[i].gameObject.GetComponent<Image>().color = softBlue;
        }
    }

    void setButtonsVisible(Button[] buttons, bool isVisible, int Id)
    {
        if (Id != -1)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == Id)
                    buttons[i].gameObject.SetActive(isVisible);
                else
                    buttons[i].gameObject.SetActive(!isVisible);
            }
        }
        else
        {
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].gameObject.SetActive(isVisible);
        }
    }
}

public delegate void hideMe(bool isHidden);

public class perfDataOptions
{
    public event timePeriodChangedEventHandler timeHasChanged;
    public event hideMe iAmHiden;

    public Toggle hideTog
    {
        get
        {
            return hideMeToggle;
        }
    }

    Toggle hideMeToggle = null;
    InputField timePeriodInputField = null;

    public perfDataOptions(GameObject perfOptionsPanel)
    {
        hideMeToggle = perfOptionsPanel.transform.GetChild(0).GetChild(0).GetComponent<Toggle>();
        timePeriodInputField = perfOptionsPanel.transform.GetChild(0).GetChild(1).GetComponent<InputField>();

        //hideMeToggle.onValueChanged.AddListener(delegate {
        //    if (hideMeToggle.isOn)
        //        iAmHiden(true);
        //    else
        //        iAmHiden(false);
        //});
        timePeriodInputField.onEndEdit.AddListener(delegate
        {
            changeTimePeriod(timePeriodInputField);
        });
    }

    ~perfDataOptions()
    {
        //hideMeToggle.onValueChanged.RemoveAllListeners();
        timePeriodInputField.onEndEdit.RemoveAllListeners();
    }

    void changeTimePeriod(InputField timeField)
    {
        int myVal = 0;
        Int32.TryParse(timePeriodInputField.text, out myVal);
        timeHasChanged(myVal);
    }
}

public delegate void gainChangedEventHandler(float newVal);
public delegate void offsetChangedEventHandler(float newVal);
public delegate void idFileChangedEventHandler(int newIdHandle);
public delegate void idElecChangedEventHandler(int newIDElec);
public delegate void timePeriodChangedEventHandler(int newPeriod);
public delegate void toggleGridDisplay(bool isGridOn);
public delegate void toggleSonification(bool isSonifOn);
public delegate void newSoundSonif(int newIDSound);

public class traceXOptions
{
    public event gainChangedEventHandler gainHasChanged;
    public event offsetChangedEventHandler offsetHasChanged;
    public event idFileChangedEventHandler idFileHasChanged;
    public event idElecChangedEventHandler idElecHasChanged;
    public event timePeriodChangedEventHandler timeHasChanged;
    public event toggleGridDisplay gridToggled;
    public event toggleSonification sonifToggled;
    public event newSoundSonif soundChanged;

    GameObject elecPlot = null;
    GameObject electrodeContentPanel = null;
    BTVMedia media = null;

    Button[] smButton = null;

    Text gainLabel = null;
    Button gainAddButton = null;
    Button gainRemoveButton = null;

    Text offsetLabel = null;
    Button offsetAddButton = null;
    Button offsetRemoveButton = null;

    Toggle timeGridToggle = null;
    InputField timePeriodInputField = null;

    Button sonifButton = null;
    Dropdown sonifSoundDropDown = null;
    Text sonifDDownText = null;
    Texture2D sonifON = null;
    Texture2D sonifOFF = null;

    //audiosource extention file
    //https://docs.unity3d.com/Manual/AudioFiles.html
    string[] fileExtentionAUDIO = new string[] { ".mp3", ".ogg", ".wav", ".aiff", ".aif", ".mod", ".it", ".s3m", ".xm" };
    public List<string> soundFilesAbsPath = null;
    public string[] soundFileShort;

    float gain = 1;
    int offset = 0;
    bool isSonifOn = false;

    Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
    Color softBlue = new Color(0.6117f, 0.7058f, 0.7960f, 0.392156f);
    Color yellow = new Color(0.9058f, 0.8784f, 0.0f);

    public traceXOptions(ButtonUI_show buttonOpt, BTVMedia p_media)
    {
        media = p_media;

        elecPlot = Resources.Load("Prefabs/Hub-Elec", typeof(GameObject)) as GameObject;
        sonifON = Resources.Load("Pictures/soundOK", typeof(Texture2D)) as Texture2D;
        sonifOFF = Resources.Load("Pictures/soundNOK", typeof(Texture2D)) as Texture2D;

        electrodeContentPanel = buttonOpt.optionsPanel2.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

        smButton = new Button[6];
        for (int i = 0; i < 6; i++)
            smButton[i] = buttonOpt.optionsPanel2.transform.GetChild(1).GetChild(i).GetComponent<Button>();

        connectButtonSM(0);
        connectButtonSM(1);
        connectButtonSM(2);
        connectButtonSM(3);
        connectButtonSM(4);
        connectButtonSM(5);

        gainLabel = buttonOpt.optionsPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>();
        gainLabel.text = "Gain : " + gain;

        gainAddButton = buttonOpt.optionsPanel.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        gainAddButton.onClick.AddListener(addGain);

        gainRemoveButton = buttonOpt.optionsPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>();
        gainRemoveButton.onClick.AddListener(removeGain);

        offsetLabel = buttonOpt.optionsPanel.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        offsetLabel.text = "Offset : " + offset + "%";
        offsetAddButton = buttonOpt.optionsPanel.transform.GetChild(1).GetChild(1).GetComponent<Button>();
        offsetRemoveButton = buttonOpt.optionsPanel.transform.GetChild(1).GetChild(2).GetComponent<Button>();

        offsetAddButton.onClick.AddListener(() =>
        {
            if (offset + 1 <= 5)
            {
                offset += 1;
                offsetLabel.text = "Offset : " + (offset * 10) + "%";
                offsetHasChanged(offset);
            }
        });
        offsetRemoveButton.onClick.AddListener(() =>
        {
            if (offset - 1 >= -5)
            {
                offset -= 1;
                offsetLabel.text = "Offset : " + (offset * 10) + "%";
                offsetHasChanged(offset);
            }
        });

        timeGridToggle = buttonOpt.optionsPanel.transform.GetChild(2).GetChild(0).GetComponent<Toggle>();
        timeGridToggle.onValueChanged.AddListener(delegate { gridToggled(timeGridToggle.isOn); });
        timePeriodInputField = buttonOpt.optionsPanel.transform.GetChild(2).GetChild(2).GetComponent<InputField>();
        timePeriodInputField.onEndEdit.AddListener(delegate { changeTimePeriod(timePeriodInputField); });

        //child 5 color
        //========

        sonifButton = buttonOpt.optionsPanel.transform.GetChild(3).GetChild(0).GetComponent<Button>();
        sonifSoundDropDown = buttonOpt.optionsPanel.transform.GetChild(3).GetChild(1).GetComponent<Dropdown>();
        sonifSoundDropDown.interactable = false;
        sonifDDownText = sonifSoundDropDown.transform.GetChild(0).GetComponent<Text>();

        sonifButton.onClick.AddListener(toggleSonification);

        soundFilesAbsPath = Directory.GetFiles(Application.dataPath + @"/Config/Sounds/", "*.*")
                                .Where(n => fileExtentionAUDIO.Contains(System.IO.Path.GetExtension(n), StringComparer.OrdinalIgnoreCase))
                                .ToList();

        soundFileShort = new string[soundFilesAbsPath.Count()];
        for (int i = 0; i < soundFilesAbsPath.Count(); i++)
        {
            string[] splitPath = soundFilesAbsPath[i].Split(new char[] { '/', '.' });
            soundFileShort[i] = splitPath[splitPath.Count() - 2];
        }

        sonifSoundDropDown.options.Clear();
        for (int i = 0; i < soundFileShort.Count(); i++)
        {
            sonifSoundDropDown.options.Add(new Dropdown.OptionData(soundFileShort[i]));
        }
        sonifDDownText.text = sonifSoundDropDown.options[sonifSoundDropDown.value].text;

        sonifSoundDropDown.onValueChanged.AddListener(changeSonifSound);
    }

    ~traceXOptions()
    {
        for (int i = 0; i < 6; i++)
        {
            smButton[i].onClick.RemoveAllListeners();
        }

        gainAddButton.onClick.RemoveAllListeners();
        gainRemoveButton.onClick.RemoveAllListeners();

        offsetAddButton.onClick.RemoveAllListeners();
        offsetRemoveButton.onClick.RemoveAllListeners();

        timeGridToggle.onValueChanged.RemoveAllListeners();
        timePeriodInputField.onEndEdit.RemoveAllListeners();

        sonifButton.onClick.RemoveAllListeners();
        sonifSoundDropDown.onValueChanged.RemoveAllListeners();
    }

    public void loadElectrodeInPanel(elecFile[] electrodeList)
    {
        deleteElectrodeInPanel();
        for (int i = 0; i < electrodeList.Length; i++)
        {
            GameObject currentElectrode = GameObject.Instantiate(elecPlot);
            Button currentElecButton = currentElectrode.GetComponent<Button>();
            currentElecButton.onClick.AddListener(() =>
            {
                for (int j = 0; j < electrodeContentPanel.transform.childCount; j++)
                {
                    if (electrodeContentPanel.transform.GetChild(j).name == currentElecButton.name)
                    {
                        //if (sphereColor.isValidForChange(currentElecButton.name))
                        //{
                        idElecHasChanged(j);
                        //    changeColorElec();
                        break;
                        //}
                    }
                }
            });

            Text currentElecText = currentElectrode.transform.GetChild(0).GetComponent<Text>();
            currentElecText.text = electrodeList[i].name;
            currentElectrode.name = electrodeList[i].name;
            currentElectrode.transform.SetParent(electrodeContentPanel.transform);
            currentElectrode.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void deleteElectrodeInPanel()
    {
        if (electrodeContentPanel.transform.childCount > 0)
        {
            for (int i = 0; i < electrodeContentPanel.transform.childCount; i++)
            {
                electrodeContentPanel.transform.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
                GameObject.Destroy(electrodeContentPanel.transform.GetChild(i).gameObject);
            }
        }
    }

    public void changeButtonSMColor(int id = -1)
    {
        if (id == -1)
            id = ELAN.returnFirstValidHandleId(media.elanFiles);

        for (int i = 0; i < 6; i++)
        {
            if (i == id)
                smButton[i].gameObject.GetComponent<Image>().color = hardBlue;
            else
                smButton[i].gameObject.GetComponent<Image>().color = softBlue;
        }
    }

    void connectButtonSM(int id)
    {
        if (ELAN.checkHandle(media.elanFiles, id))
        {
            smButton[id].onClick.AddListener(() =>
            {
                idFileHasChanged(id);
                changeButtonSMColor(id);
            });
        }
        else
        {
            smButton[id].interactable = false;
        }
    }

    void changeTimePeriod(InputField timeField)
    {
        int myVal = 0;
        Int32.TryParse(timePeriodInputField.text, out myVal);
        timeHasChanged(myVal);
    }

    void addGain()
    {
        if (gain < 1 && gain >= -1)
            gain += 0.25f;
        else
            gain += 1;
        gainLabel.text = "Gain : " + gain;
        gainHasChanged(gain);
    }

    void removeGain()
    {
        if (gain <= 1 && gain > -1)
            gain -= 0.25f;
        else
            gain -= 1;
        gainLabel.text = "Gain : " + gain;
        gainHasChanged(gain);
    }

    void toggleSonification()
    {
        isSonifOn = !isSonifOn;
        if (isSonifOn)
        {
            sonifButton.GetComponent<RawImage>().texture = sonifON;
            sonifSoundDropDown.interactable = true;
        }
        else
        {
            sonifButton.GetComponent<RawImage>().texture = sonifOFF;
            sonifSoundDropDown.interactable = false;
        }

        sonifToggled(isSonifOn);
    }

    void changeSonifSound(int newIDDropDown)
    {
        soundChanged(newIDDropDown);
    }
}

public delegate void brainChangeEventHandler(int idBrain);

public class brainOptions
{
    public event gainChangedEventHandler gainHasChanged;
    public event brainChangeEventHandler needToChangeBrain;

    Button[] brainButtons = null;
    Button[] visBrainButtons = null;
    Text gainValue = null;
    Button gainAdd = null;
    Button gainRemove = null;
    int gain = 1;
    bool referential_choicePending = false;
    bool visu_choicePending = false;

    public brainOptions(GameObject brainOptionsPanel)
    {
        brainButtons = new Button[3];
        brainButtons[0] = brainOptionsPanel.transform.GetChild(0).GetComponent<Button>();
        brainButtons[1] = brainOptionsPanel.transform.GetChild(1).GetComponent<Button>();
        brainButtons[2] = brainOptionsPanel.transform.GetChild(2).GetComponent<Button>();

        visBrainButtons = new Button[3];
        visBrainButtons[0] = brainOptionsPanel.transform.GetChild(3).GetComponent<Button>();
        visBrainButtons[1] = brainOptionsPanel.transform.GetChild(4).GetComponent<Button>();
        visBrainButtons[2] = brainOptionsPanel.transform.GetChild(5).GetComponent<Button>();
        gainValue = brainOptionsPanel.transform.GetChild(6).GetChild(0).GetComponent<Text>();

        gainAdd = brainOptionsPanel.transform.GetChild(6).GetChild(1).GetComponent<Button>();
        gainRemove = brainOptionsPanel.transform.GetChild(6).GetChild(2).GetComponent<Button>();

        brainButtons[0].onClick.AddListener(() =>
        {
            if (!referential_choicePending)
            {
                setButtonsVisible(brainButtons, true, true, true);
                referential_choicePending = true;
            }
            else
            {
                needToChangeBrain(0);
                setButtonsVisible(brainButtons, true, false, false);
                referential_choicePending = false;
            }
        });
        brainButtons[1].onClick.AddListener(() =>
        {
            if (!referential_choicePending)
            {
                setButtonsVisible(brainButtons, true, true, true);
                referential_choicePending = true;
            }
            else
            {
                needToChangeBrain(1);
                setButtonsVisible(brainButtons, false, true, false);
                referential_choicePending = false;
            }
        });
        brainButtons[2].onClick.AddListener(() =>
        {
            if (!referential_choicePending)
            {
                setButtonsVisible(brainButtons, true, true, true);
                referential_choicePending = true;
            }
            else
            {
                needToChangeBrain(2);
                setButtonsVisible(brainButtons, false, false, true);
                referential_choicePending = false;
            }
        });

        visBrainButtons[0].onClick.AddListener(() =>
        {
            if (!visu_choicePending)
            {
                setButtonsVisible(visBrainButtons, true, true, true);
                visu_choicePending = true;
            }
            else
            {
                Brain.changeVisuBrain(0);
                setButtonsVisible(visBrainButtons, true, false, false);
                visu_choicePending = false;
            }
        });
        visBrainButtons[1].onClick.AddListener(() =>
        {
            if (!visu_choicePending)
            {
                setButtonsVisible(visBrainButtons, true, true, true);
                visu_choicePending = true;
            }
            else
            {
                Brain.changeVisuBrain(-1);
                setButtonsVisible(visBrainButtons, false, true, false);
                visu_choicePending = false;
            }
        });
        visBrainButtons[2].onClick.AddListener(() =>
        {
            if (!visu_choicePending)
            {
                setButtonsVisible(visBrainButtons, true, true, true);
                visu_choicePending = true;
            }
            else
            {
                Brain.changeVisuBrain(1);
                setButtonsVisible(visBrainButtons, false, false, true);
                visu_choicePending = false;
            }
        });

        gainValue.text = "Gain : " + gain;
        gainAdd.onClick.AddListener(() =>
        {
            gain += 1;
            gainValue.text = "Gain : " + gain;
            gainHasChanged(gain);
        });
        gainRemove.onClick.AddListener(() =>
        {
            if (gain - 1 > 0)
            {
                gain -= 1;
                gainValue.text = "Gain : " + gain;
                gainHasChanged(gain);
            }
        });
    }

    ~brainOptions()
    {
        brainButtons[0].onClick.RemoveAllListeners();
        brainButtons[1].onClick.RemoveAllListeners();
        brainButtons[2].onClick.RemoveAllListeners();
        brainButtons = null;
        visBrainButtons[0].onClick.RemoveAllListeners();
        visBrainButtons[1].onClick.RemoveAllListeners();
        visBrainButtons[2].onClick.RemoveAllListeners();
        visBrainButtons = null;
        gainAdd.onClick.RemoveAllListeners();
        gainRemove.onClick.RemoveAllListeners();
    }

    void setButtonsVisible(Button[] buttons, bool isVisible1, bool isVisible2, bool isVisible3)
    {
        buttons[0].gameObject.SetActive(isVisible1);
        buttons[1].gameObject.SetActive(isVisible2);
        buttons[2].gameObject.SetActive(isVisible3);
    }

    public void setBrainInteract(bool isInteractable)
    {
        visBrainButtons[0].interactable = isInteractable;
        visBrainButtons[1].interactable = isInteractable;
        visBrainButtons[2].interactable = isInteractable;
    }
}

public class ButtonUI_show : ButtonUI
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Right:
                if (m_positionCounter - 1 >= 0)
                    m_positionCounter -= 1;
                break;
            case PointerEventData.InputButton.Left:
                if (m_positionCounter + 1 <= 3)
                    m_positionCounter += 1;
                break;
        }

        if (m_curve != null)
        {
            switch (m_positionCounter)
            {
                case 0:
                    m_curve.SetActive(false); //ni courbe ni option
                    for (int i = 0; i < m_optionsPanel.transform.childCount; i++)
                    {
                        if (i != m_idOpt)
                        {
                            m_buttonsPanel.transform.GetChild(i).GetComponent<Image>().color = Color.black;
                            m_buttonsPanel.transform.GetChild(i).GetComponent<ButtonUI>().showExtraPanels(false);
                            m_optionsPanel.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                    changeColorOptions(true);
                    m_options.SetActive(m_isVisible);
                    break;
                case 1:
                    m_curve.SetActive(true); //courbe et option

                    changeColorOptions(true);
                    m_options.SetActive(m_isVisible);
                    showExtraPanels(m_isVisible);
                    break;
                case 2:
                    m_curve.SetActive(true); //courbe et option
                    for (int i = 0; i < m_optionsPanel.transform.childCount; i++)
                    {
                        if (i != m_idOpt)
                        {
                            m_buttonsPanel.transform.GetChild(i).GetComponent<Image>().color = Color.black;
                            m_buttonsPanel.transform.GetChild(i).GetComponent<ButtonUI>().showExtraPanels(false);
                            m_optionsPanel.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                    changeColorOptions(false);
                    m_options.SetActive(m_isVisible);
                    showExtraPanels(m_isVisible);

                    if (m_curve.transform.GetComponent<TraceCurve>().hasFocus)
                        m_curve.transform.GetComponent<TraceCurve>().manageFocusClick();
                    break;
                case 3:
                    if (!m_curve.transform.GetComponent<TraceCurve>().hasFocus)
                        m_curve.transform.GetComponent<TraceCurve>().manageFocusClick();
                    break;
            }
        }
    }
}

public class ButtonUI_hide : ButtonUI
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Right:
                if (m_positionCounter - 1 >= 0)
                    m_positionCounter -= 1;
                break;
            case PointerEventData.InputButton.Left:
                if (m_positionCounter + 1 <= 2)
                    m_positionCounter += 1;
                break;
        }

        if (m_curve != null)
        {
            switch (m_positionCounter)
            {
                case 0:
                    m_curve.SetActive(false); //ni courbe ni option
                    for (int i = 0; i < m_optionsPanel.transform.childCount; i++)
                    {
                        if (i != m_idOpt)
                        {
                            m_buttonsPanel.transform.GetChild(i).GetComponent<Image>().color = Color.black;
                            m_buttonsPanel.transform.GetChild(i).GetComponent<ButtonUI>().showExtraPanels(false);
                            m_optionsPanel.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                    changeColorOptions(true);
                    m_options.SetActive(m_isVisible);
                    break;
                case 1:
                    m_curve.SetActive(true); //courbe et option

                    changeColorOptions(true);
                    m_options.SetActive(m_isVisible);
                    showExtraPanels(m_isVisible);
                    break;
                case 2:
                    for (int i = 0; i < m_optionsPanel.transform.childCount; i++)
                    {
                        if (i != m_idOpt)
                        {
                            m_buttonsPanel.transform.GetChild(i).GetComponent<Image>().color = Color.black;
                            m_buttonsPanel.transform.GetChild(i).GetComponent<ButtonUI>().showExtraPanels(false);
                            m_optionsPanel.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                    changeColorOptions(false);
                    m_options.SetActive(m_isVisible);
                    showExtraPanels(m_isVisible);
                    break;
            }
        }
    }
}

public class ButtonUI : MonoBehaviour, IPointerClickHandler
{
    public GameObject optionsPanel
    {
        get
        {
            return m_options;
        }
    }
    public GameObject optionsPanel2
    {
        get
        {
            return m_options2Panel;
        }
    }
    //==
    protected GameObject m_buttonsPanel = null;
    protected GameObject m_optionsPanel = null;
    protected Image m_showPic = null;
    protected GameObject m_options = null;
    protected int m_idOpt = -2;
    //==
    protected GameObject m_curve = null;
    protected GameObject m_options2Panel = null;
    protected int m_positionCounter = 1;
    //==
    protected bool m_isVisible = false;
    protected Color yellow = new Color(0.9058f, 0.8784f, 0.0f);
    protected Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    protected Color blue = new Color(0.6117f, 0.7058f, 0.7960f);
    protected Color blueHide = new Color(0.6117f, 0.7058f, 0.7960f, 0.3921f);

    public void init(GameObject p_buttonsPanel, GameObject p_optionsPanel, int p_idOpt)
    {
        m_buttonsPanel = p_buttonsPanel;
        m_optionsPanel = p_optionsPanel;
        m_idOpt = p_idOpt;

        m_showPic = p_buttonsPanel.transform.GetChild(m_idOpt).GetComponent<Image>();
        m_options = m_optionsPanel.transform.GetChild(m_idOpt).gameObject;
    }

    public void initExtraData(GameObject p_options2Panel)
    {
        m_options2Panel = p_options2Panel;
    }

    public void initUIElement(string name)
    {
        m_curve = GameObject.Find(name);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            for (int i = 0; i < m_optionsPanel.transform.childCount; i++)
            {
                if (i != m_idOpt)
                {
                    m_buttonsPanel.transform.GetChild(i).GetComponent<Image>().color = Color.black;
                    m_buttonsPanel.transform.GetChild(i).GetComponent<ButtonUI>().showExtraPanels(false);
                    m_optionsPanel.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            changeColorOptions(m_options.activeSelf);
            m_options.SetActive(m_isVisible);
            showExtraPanels(m_isVisible);
        }
    }

    protected void changeColorOptions(bool currentStatus)
    {
        m_isVisible = !currentStatus;

        if (m_isVisible)
            m_showPic.color = blue;
        else
            m_showPic.color = Color.black;
    }

    public virtual void showExtraPanels(bool show)
    {
        if (m_options2Panel != null)
        {
            m_options2Panel.SetActive(show);
            m_options2Panel.transform.parent.gameObject.SetActive(show);
            if (show == false)
                m_positionCounter = 1;
            if (m_curve != null)
            {
                if (m_curve.transform.GetComponent<TraceCurve>() != null)
                {
                    if (m_curve.transform.GetComponent<TraceCurve>().hasFocus)
                        m_curve.transform.GetComponent<TraceCurve>().manageFocusClick();
                }
            }
        }
    }
}

public class optionsHub : MonoBehaviour
{
    [SerializeField] BTVMedia media = null;
    [SerializeField] GameObject detaileOptionsPanel = null; //before => View=>PanelOption

    public brainOptions brainRemote
    {
        get
        {
            return brainOpts;
        }
    }
    public traceXOptions[] traceRemotes
    {
        get
        {
            return traceXOpts;
        }
    }
    public perfDataOptions perfRemote
    {
        get
        {
            return perfOpts;
        }
    }
    public videoOptions videoRemote
    {
        get
        {
            return vidOpts;
        }
    }
    public eventsOptions eventRemote
    {
        get
        {
            return eventsOpts;
        }
    }

    #region UIMembers
    //== Pannel Options 
    ButtonUI brainOpt = null;
    ButtonUI_show trace1Opt = null;
    ButtonUI_show trace2Opt = null;
    ButtonUI_hide perfOpt = null;
    ButtonUI videoOpt = null;
    ButtonUI eventsOpt = null;
    //== Detailed Options
    brainOptions brainOpts = null;
    traceXOptions[] traceXOpts = new traceXOptions[2];
    perfDataOptions perfOpts = null;
    videoOptions vidOpts = null;
    eventsOptions eventsOpts = null;
    #endregion

    void Awake()
    {
        media.mediaLoaded += new mediaLoadedEventHandler(initOptionMenu);
    }

    void OnDestroy()
    {
        media.mediaLoaded -= new mediaLoadedEventHandler(initOptionMenu);
    }

    void initOptionMenu()
    {
        brainOpt = gameObject.transform.GetChild(0).gameObject.AddComponent<ButtonUI>();
        brainOpt.init(gameObject, detaileOptionsPanel, 0);
        brainOpts = new brainOptions(brainOpt.optionsPanel);
        ////==
        trace1Opt = gameObject.transform.GetChild(1).gameObject.AddComponent<ButtonUI_show>();
        trace1Opt.init(gameObject, detaileOptionsPanel, 1);
        trace1Opt.initUIElement("Trace1Window");
        trace1Opt.initExtraData(GameObject.Find("Canvas").transform.GetChild(1).GetChild(2).GetChild(0).gameObject);
        traceXOpts[0] = new traceXOptions(trace1Opt, media);
        //////==
        trace2Opt = gameObject.transform.GetChild(2).gameObject.AddComponent<ButtonUI_show>();
        trace2Opt.init(gameObject, detaileOptionsPanel, 2);
        trace2Opt.initUIElement("Trace2Window");
        trace2Opt.initExtraData(GameObject.Find("Canvas").transform.GetChild(1).GetChild(2).GetChild(1).gameObject);
        traceXOpts[1] = new traceXOptions(trace2Opt, media);
        ////==
        perfOpt = gameObject.transform.GetChild(3).gameObject.AddComponent<ButtonUI_hide>();
        perfOpt.init(gameObject, detaileOptionsPanel, 3);
        perfOpt.initUIElement("TracePerfWindow");
        perfOpts = new perfDataOptions(perfOpt.optionsPanel);
        ////==
        videoOpt = gameObject.transform.GetChild(4).gameObject.AddComponent<ButtonUI>();
        videoOpt.init(gameObject, detaileOptionsPanel, 4);
        vidOpts = new videoOptions(videoOpt.optionsPanel);
        ////==
        eventsOpt = gameObject.transform.GetChild(5).gameObject.AddComponent<ButtonUI>();
        eventsOpt.init(gameObject, detaileOptionsPanel, 5);
        eventsOpt.initExtraData(GameObject.Find("Canvas").transform.GetChild(1).GetChild(2).GetChild(2).gameObject);
        eventsOpts = gameObject.AddComponent<eventsOptions>();
        eventsOpts.init(eventsOpt);
    }
}
