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
    Text activateEventText = null;
    Transform panelContent = null;
    Button saveEvents = null;
    Button loadEvents = null;

    public bool addEvent = false;
    SortedList<int, TraceEvent> events = new SortedList<int, TraceEvent>();

    GameObject scrollObj = null;
    Texture2D scrollOrig = null, scrollTex = null;
    Color[] dataTexScroll;
    Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
    Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    int currentPos = -1;

    public void init(GameObject eventsOptionPanel)
    {
        eventHubClick = Resources.Load("Prefabs/Hub-Event", typeof(GameObject)) as GameObject;
        eventAddUI = Resources.Load("Prefabs/EventInfoEdit", typeof(GameObject)) as GameObject;
        eventDispUI = Resources.Load("Prefabs/EventInfoDisplay", typeof(GameObject)) as GameObject;
        scrollOrig = Resources.Load("Pictures/eventScroll", typeof(Texture2D)) as Texture2D;
        //==
        v = GameObject.Find("PanelR").GetComponent<VideoPlayer>();
        win1 = GameObject.Find("Trace1Window").GetComponent<TraceCurve>();
        win2 = GameObject.Find("Trace2Window").GetComponent<TraceCurve>();
        coMana = GameObject.Find("ringSelect").GetComponent<CoroutineManager>();
        scrollObj = GameObject.Find("TimeScrollBar");
        scrollTex = Instantiate(scrollOrig);
        //==
        activateEventsButton = eventsOptionPanel.transform.GetChild(0).GetComponent<Button>();
        activateEventText = activateEventsButton.transform.GetChild(0).GetComponent<Text>();
        panelContent = eventsOptionPanel.transform.GetChild(1).GetChild(0).GetChild(0);
        saveEvents = eventsOptionPanel.transform.GetChild(2).GetComponent<Button>();
        loadEvents = eventsOptionPanel.transform.GetChild(3).GetComponent<Button>();
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

        if (addEvent == true)
            activateEventText.text = "Stop Adding Events";
        else
            activateEventText.text = "Start Adding Events";
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

        if(h > 0)
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
        string btvPosFile = QtGUI_dll.Instance.getSaveFileName(new string[] { "pos" });
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
        List<TraceEvent> eventLoaded = null;
        string pathFile = QtGUI_dll.Instance.getOpenFileName(new string[] { "btv", "pos" });
        string[] pathSplit = pathFile.Split(new char[] { '.' });

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
                        eventLoaded.Add(new TraceEvent(new eventEeg(int.Parse(resultSplit[1]),int.Parse(resultSplit[0]), win1.samplingFrequency)));
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
                pearsonCoefficientsCorrelation2(events.Values[id].correlationArray,  v.audioWav.getAudioHandle(win1.fileHandle.idFileHandle), win1.fileHandle.eegData, sizes);
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
public delegate void gainAudioChangedEventHandler(int newGain);
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
    int gain = 1;
    //====
    Button[] smButton = null;

    //====
    VideoPlayer vid = null;
    CoroutineManager coMana = null;
    //====
    Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
    Color softBlue = new Color(0.6117f, 0.7058f, 0.7960f, 0.392156f);

    public videoOptions(GameObject videoOptionsPanel)
    {
        removeVideoOffset = videoOptionsPanel.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        offsetScrollBar = videoOptionsPanel.transform.GetChild(0).GetChild(1).GetComponent<Scrollbar>();
        addVideoOffset = videoOptionsPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>();
        videoOffsetLabel = videoOptionsPanel.transform.GetChild(0).GetChild(3).GetComponent<Text>();
        //==
        showAudioTrace = videoOptionsPanel.transform.GetChild(1).GetChild(0).GetComponent<Toggle>();
        filterAudio = videoOptionsPanel.transform.GetChild(1).GetChild(1).GetComponent<Button>();
        loadAudio = videoOptionsPanel.transform.GetChild(1).GetChild(2).GetComponent<Button>();
        //==
        gainLabel = videoOptionsPanel.transform.GetChild(2).GetChild(0).GetComponent<Text>();
        gainAddButton = videoOptionsPanel.transform.GetChild(2).GetChild(1).GetComponent<Button>();
        gainRemoveButton = videoOptionsPanel.transform.GetChild(2).GetChild(2).GetComponent<Button>();
        //==
        smButton = new Button[6];
        smButton[0] = videoOptionsPanel.transform.GetChild(3).GetChild(0).GetChild(0).GetComponent<Button>();
        smButton[1] = videoOptionsPanel.transform.GetChild(3).GetChild(0).GetChild(1).GetComponent<Button>();
        smButton[2] = videoOptionsPanel.transform.GetChild(3).GetChild(0).GetChild(2).GetComponent<Button>();
        smButton[3] = videoOptionsPanel.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<Button>();
        smButton[4] = videoOptionsPanel.transform.GetChild(3).GetChild(1).GetChild(1).GetComponent<Button>();
        smButton[5] = videoOptionsPanel.transform.GetChild(3).GetChild(1).GetChild(2).GetComponent<Button>();
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
        gain += 1;
        gainLabel.text = "Gain : " + gain;
        gainAudioHasChanged(gain);
    }

    void removeGain()
    {
        gain -= 1;
        gainLabel.text = "Gain : " + gain;
        gainAudioHasChanged(gain);
    }

    void connectButtonSM(int id)
    {
        smButton[id].onClick.AddListener(() => 
        {
            smAudioHasChanged(id);
            changeButtonSMColor(id);
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
        timePeriodInputField = perfOptionsPanel.transform.GetChild(1).GetChild(1).GetComponent<InputField>();

        hideMeToggle.onValueChanged.AddListener(delegate {
            if (hideMeToggle.isOn)
                iAmHiden(true);
            else
                iAmHiden(false);
        });
        timePeriodInputField.onEndEdit.AddListener(delegate {
            changeTimePeriod(timePeriodInputField);
        });
    }

    ~perfDataOptions()
    {
        hideMeToggle.onValueChanged.RemoveAllListeners();
        timePeriodInputField.onEndEdit.RemoveAllListeners();
    }

    void changeTimePeriod(InputField timeField)
    {
        int myVal = 0;
        Int32.TryParse(timePeriodInputField.text, out myVal);
        timeHasChanged(myVal);
    }
}

public delegate void gainChangedEventHandler(int newVal);
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

    int gain = 1;
    int offset = 0;
    bool isSonifOn = false;

    Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
    Color softBlue = new Color(0.6117f, 0.7058f, 0.7960f, 0.392156f);
    Color yellow = new Color(0.9058f, 0.8784f, 0.0f);

    public traceXOptions(GameObject traceOptionsPanel, BTVMedia p_media)
    {
        media = p_media;

        elecPlot = Resources.Load("Prefabs/Hub-Elec", typeof(GameObject)) as GameObject;
        sonifON = Resources.Load("Pictures/soundOK", typeof(Texture2D)) as Texture2D;
        sonifOFF = Resources.Load("Pictures/soundNOK", typeof(Texture2D)) as Texture2D;

        electrodeContentPanel = traceOptionsPanel.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

        smButton = new Button[6];
        for (int i = 0; i < 6; i++)
            smButton[i] = traceOptionsPanel.transform.GetChild(1).GetChild(i).GetComponent<Button>();

        connectButtonSM(0);
        connectButtonSM(1);
        connectButtonSM(2);
        connectButtonSM(3);
        connectButtonSM(4);
        connectButtonSM(5);

        gainLabel = traceOptionsPanel.transform.GetChild(2).GetChild(0).GetComponent<Text>();
        gainLabel.text = "Gain : " + gain;

        gainAddButton = traceOptionsPanel.transform.GetChild(2).GetChild(1).GetComponent<Button>();
        gainAddButton.onClick.AddListener(addGain);

        gainRemoveButton = traceOptionsPanel.transform.GetChild(2).GetChild(2).GetComponent<Button>();
        gainRemoveButton.onClick.AddListener(removeGain);

        offsetLabel = traceOptionsPanel.transform.GetChild(3).GetChild(0).GetComponent<Text>();
        offsetLabel.text = "Offset : " + offset + "%";
        offsetAddButton = traceOptionsPanel.transform.GetChild(3).GetChild(1).GetComponent<Button>();
        offsetRemoveButton = traceOptionsPanel.transform.GetChild(3).GetChild(2).GetComponent<Button>();

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

        timeGridToggle = traceOptionsPanel.transform.GetChild(4).GetChild(0).GetComponent<Toggle>();
        timeGridToggle.onValueChanged.AddListener(delegate { gridToggled(timeGridToggle.isOn); });
        timePeriodInputField = traceOptionsPanel.transform.GetChild(4).GetChild(2).GetComponent<InputField>();
        timePeriodInputField.onEndEdit.AddListener(delegate { changeTimePeriod(timePeriodInputField); });

        //child 5 color
        //========

        sonifButton = traceOptionsPanel.transform.GetChild(6).GetChild(0).GetComponent<Button>();
        sonifSoundDropDown = traceOptionsPanel.transform.GetChild(6).GetChild(1).GetComponent<Dropdown>();
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
        gain += 1;
        gainLabel.text = "Gain : " + gain;
        gainHasChanged(gain);
    }

    void removeGain()
    {
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

    Button brainMNI = null;
    Button brainPAT = null;
    Button brainELEC = null;
    Button dispFullBrain = null;
    Button dispLeftBrain = null;
    Button dispRightBrain = null;
    Text gainValue = null;
    Button gainAdd = null;
    Button gainRemove = null;
    int gain = 1;

    public brainOptions(GameObject brainOptionsPanel)
    {
        brainMNI = brainOptionsPanel.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        brainPAT = brainOptionsPanel.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        brainELEC = brainOptionsPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>();
        dispFullBrain = brainOptionsPanel.transform.GetChild(1).GetComponent<Button>();
        dispLeftBrain = brainOptionsPanel.transform.GetChild(2).GetComponent<Button>();
        dispRightBrain = brainOptionsPanel.transform.GetChild(3).GetComponent<Button>();
        gainValue = brainOptionsPanel.transform.GetChild(4).GetChild(0).GetComponent<Text>();
        gainAdd = brainOptionsPanel.transform.GetChild(4).GetChild(1).GetComponent<Button>();
        gainRemove = brainOptionsPanel.transform.GetChild(4).GetChild(2).GetComponent<Button>();

        brainMNI.onClick.AddListener(()=>
        {
            changeBrainDisplay(0);
        });
        brainPAT.onClick.AddListener(() =>
        {
            changeBrainDisplay(1);
        });
        brainELEC.onClick.AddListener(() =>
        {
            changeBrainDisplay(2);
        });

        dispFullBrain.onClick.AddListener(() => Brain.changeVisuBrain(0));
        dispLeftBrain.onClick.AddListener(() => Brain.changeVisuBrain(-1));
        dispRightBrain.onClick.AddListener(() => Brain.changeVisuBrain(1));

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
        brainMNI.onClick.RemoveAllListeners();
        brainPAT.onClick.RemoveAllListeners();
        brainELEC.onClick.RemoveAllListeners();
        dispFullBrain.onClick.RemoveAllListeners();
        dispLeftBrain.onClick.RemoveAllListeners();
        dispRightBrain.onClick.RemoveAllListeners();
        gainAdd.onClick.RemoveAllListeners();
        gainRemove.onClick.RemoveAllListeners();
    }

    public void setBrainInteract(bool isInteractable)
    {
        dispFullBrain.interactable = isInteractable;
        dispLeftBrain.interactable = isInteractable;
        dispRightBrain.interactable = isInteractable;
    }

    void changeBrainDisplay(int idDisplay)
    {
        needToChangeBrain(idDisplay);
    }
}

public class UIOption
{
    public GameObject optionsPanel
    {
        get
        {
            return options;
        }
    }

    #region UIMembers
    Button showButton = null;
    Image showPic = null;
    Text nameText = null;
    GameObject contentPanel = null;
    GameObject options = null;
    eventsOptions eventMenu = null;
    #endregion

    Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    Color blue = new Color(0.6117f, 0.7058f, 0.7960f);
    bool isVisible = false;

    public UIOption(GameObject buttonsPanel, GameObject optionsPanel, int idOpt)
    {
        contentPanel = optionsPanel.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        showButton = buttonsPanel.transform.GetChild(idOpt).GetComponent<Button>();
        showPic = buttonsPanel.transform.GetChild(idOpt).GetComponent<Image>();
        nameText = buttonsPanel.transform.GetChild(idOpt).GetChild(0).GetComponent<Text>();
        options = contentPanel.transform.GetChild(idOpt).gameObject;

        showButton.onClick.AddListener(() =>
        {
            if (optionsPanel.activeSelf == false)
            {
                changeColorOptions();
                optionsPanel.SetActive(true);
                options.SetActive(!options.activeSelf);
            }
            else
            {
                if (options.name == "OptionsEvents")
                {
                    Component[] objects = GameObject.Find("Canvas").GetComponentsInChildren(typeof(eventsOptions), true);
                    eventMenu = (eventsOptions)objects[0];
                    if (eventMenu && eventMenu.addEvent == false)
                    {
                        changeColorOptions();
                        options.SetActive(!options.activeSelf);

                        bool hide = false;
                        for (int i = 0; i < contentPanel.transform.childCount; i++)
                        {
                            hide = hide || contentPanel.transform.GetChild(i).gameObject.activeSelf;
                        }

                        if (!hide)
                        {
                            optionsPanel.SetActive(false);
                        }
                    }
                }
                else
                {
                    changeColorOptions();
                    options.SetActive(!options.activeSelf);

                    bool hide = false;
                    for (int i = 0; i < contentPanel.transform.childCount; i++)
                    {
                        hide = hide || contentPanel.transform.GetChild(i).gameObject.activeSelf;
                    }

                    if (!hide)
                    {
                        optionsPanel.SetActive(false);
                    }
                }


            }
        });
    }

    ~UIOption()
    {
        showButton.onClick.RemoveAllListeners();
    }

    void changeColorOptions()
    {
        isVisible = !isVisible;

        if (isVisible)
            nameText.color = orange;
        else
            nameText.color = blue;
    }
}

public class UIXOption : MonoBehaviour, IPointerClickHandler
{
    int positionCounter = 1;
    Text nameText = null;
    GameObject contentPanel = null;
    public GameObject options = null;
    GameObject optionsPanel = null;
    Color yellow = new Color(0.9058f, 0.8784f, 0.0f);
    Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    Color blue = new Color(0.6117f, 0.7058f, 0.7960f);
    Color blueHide = new Color(0.6117f, 0.7058f, 0.7960f, 0.3921f);
    int idCurve = 0;
    GameObject curve = null;

    public void init(GameObject buttonsPanel, GameObject optionsPanel, int idOpt)
    {
        this.optionsPanel = optionsPanel;
        contentPanel = optionsPanel.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        nameText = buttonsPanel.transform.GetChild(idOpt).GetChild(0).GetComponent<Text>();
        options = contentPanel.transform.GetChild(idOpt).gameObject;

        if (nameText.transform.parent.name == "ButtonTrace1")
            curve = GameObject.Find("Trace1Window");
        else if (nameText.transform.parent.name == "ButtonTrace2")
            curve = GameObject.Find("Trace2Window");

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Right:
                if (positionCounter - 1 >= 0)
                    positionCounter -= 1;
                break;
            case PointerEventData.InputButton.Left:
                if (positionCounter + 1 <= 3)
                    positionCounter += 1;
                break;
        }

        switch (positionCounter)
        {
            case 0:
                curve.SetActive(false);
                nameText.color = blueHide;
                break;
            case 1:
                curve.SetActive(true);
                options.SetActive(false);
                nameText.color = blue;

                bool hide = false;
                for (int i = 0; i < contentPanel.transform.childCount; i++)
                    hide = hide || contentPanel.transform.GetChild(i).gameObject.activeSelf;

                if (!hide)
                    optionsPanel.SetActive(false);

                break;
            case 2:
                optionsPanel.SetActive(true);
                options.SetActive(true);
                nameText.color = yellow;

                if(curve.GetComponent<TraceCurve>().hasFocus)
                    curve.GetComponent<TraceCurve>().manageFocusClick();
                break;
            case 3:
                nameText.color = orange;
                if (!curve.GetComponent<TraceCurve>().hasFocus)
                    curve.GetComponent<TraceCurve>().manageFocusClick();
                break;
        }
    }
}

public class optionsHub : MonoBehaviour
{
    [SerializeField] BTVMedia media = null;
    [SerializeField] GameObject detaileOptionsPanel = null;

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
    UIOption brainOpt = null;
    UIXOption trace1Opt = null;
    UIXOption trace2Opt = null;
    UIOption perfOpt = null;
    UIOption videoOpt = null;
    UIOption eventsOpt = null;
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
        brainOpt = new UIOption(gameObject, detaileOptionsPanel, 0);
        brainOpts = new brainOptions(brainOpt.optionsPanel);
        //==
        trace1Opt = gameObject.transform.GetChild(1).gameObject.AddComponent<UIXOption>();
        trace1Opt.init(gameObject, detaileOptionsPanel, 1);
        traceXOpts[0] = new traceXOptions(trace1Opt.options, media);
        //==
        trace2Opt = gameObject.transform.GetChild(2).gameObject.AddComponent<UIXOption>();
        trace2Opt.init(gameObject, detaileOptionsPanel, 2);
        traceXOpts[1] = new traceXOptions(trace2Opt.options, media);
        //==
        perfOpt = new UIOption(gameObject, detaileOptionsPanel, 3);
        perfOpts = new perfDataOptions(perfOpt.optionsPanel);
        //==
        videoOpt = new UIOption(gameObject, detaileOptionsPanel, 4);
        vidOpts = new videoOptions(videoOpt.optionsPanel);
        //==
        eventsOpt = new UIOption(gameObject, detaileOptionsPanel, 5);
        eventsOpts = gameObject.AddComponent<eventsOptions>();
        eventsOpts.init(eventsOpt.optionsPanel);
    }
}
