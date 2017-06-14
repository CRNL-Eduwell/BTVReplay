using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;//Requiered for Event data.
using System.Collections; //IEnumerator

public delegate void newEventToShowHandler(eventEeg newEvent, int id);

public class eventsOptions : MonoBehaviour
{
    public SortedList<int, eventEeg> userEvents
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

    TraceCurve win1 = null;
    TraceCurve win2 = null;
    EventInfoEdit infoEdit = null;
    EventInfoDisplay infoDisp = null;
    int traceIDCalled = 0;
    eventEeg eventCalled = null;

    Button activateEventsButton = null;
    Text activateEventText = null;
    Transform panelContent = null;
    Button saveEvents = null;
    Button loadEvents = null;

    bool addEvent = false;
    SortedList<int, eventEeg> events = new SortedList<int, eventEeg>();

    public void init(GameObject eventsOptionPanel)
    {
        eventHubClick = Resources.Load("Prefabs/Hub-Event", typeof(GameObject)) as GameObject;
        eventAddUI = Resources.Load("Prefabs/EventInfoEdit", typeof(GameObject)) as GameObject;
        eventDispUI = Resources.Load("Prefabs/EventInfoDisplay", typeof(GameObject)) as GameObject;
        //==
        win1 = GameObject.Find("Trace1Window").GetComponent<TraceCurve>();
        win2 = GameObject.Find("Trace2Window").GetComponent<TraceCurve>();
        activateEventsButton = eventsOptionPanel.transform.GetChild(0).GetComponent<Button>();
        activateEventText = activateEventsButton.transform.GetChild(0).GetComponent<Text>();
        panelContent = eventsOptionPanel.transform.GetChild(1).GetChild(0).GetChild(0);
        saveEvents = eventsOptionPanel.transform.GetChild(2).GetComponent<Button>();
        loadEvents = eventsOptionPanel.transform.GetChild(3).GetComponent<Button>();
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
    }

    void activateEventsMode()
    {
        addEvent = !addEvent;

        if (addEvent == true)
            activateEventText.text = "Stop Adding Events";
        else
            activateEventText.text = "Start Adding Events";
    }

    void openEventAddUI(eventEeg currentEvent, int traceID)
    {
        if (addEvent)
        {
            if (infoEdit == null && infoDisp == null)
            {
                GameObject addUI = Instantiate(eventAddUI);

                if (traceID == 0)
                    addUI.transform.SetParent(win1.gameObject.transform);
                else
                    addUI.transform.SetParent(win2.gameObject.transform);
                addUI.transform.localScale = new Vector3(1, 1, 1);
                addUI.transform.localPosition = new Vector3(0, 0, -402);

                infoEdit = addUI.GetComponent<EventInfoEdit>();
                infoEdit.init(currentEvent, false);
                infoEdit.eventValid += new eventValidated(eventValidatedForUI);
                infoEdit.aaaagh += new imDying(removeConnectionAddUi);
            }
        }
    }

    void openEventModifyUI(eventEeg currentEvent, int traceID)
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
            infoEdit.init(currentEvent, true);
            infoEdit.eventModifed += new eventModifValidated((modifyedEvent) =>
            {
                applyChangeToEvent(modifyedEvent, eventCalled);
            });
            infoEdit.aaaagh += new imDying(removeConnectionEditUI);
        }
    }

    void openEventDisplayUI(eventEeg currentEvent, int traceID)
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
            infoDisp.aaaagh += new imDying(removeConnectionDispUI);
        }
    }

    void deleteEvents(eventEeg eventToDelete, int traceID)
    {
        Debug.Log("del ev " + eventToDelete.sample);
        //==List behind the scene
        int id = events.IndexOfKey(eventToDelete.sample);
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
            Destroy(hubObj);
        }
    }

    void eventValidatedForUI(eventEeg currentEvent)
    {
        events.Add(currentEvent.sample, currentEvent);
        int id = events.IndexOfKey(currentEvent.sample);

        GameObject currentEventGO = Instantiate(eventHubClick);
        currentEventGO.name = "HubEvent - " + currentEvent.sample;

        int timeInSec = currentEvent.sample / 64;
        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        if(h > 0)
            currentEventGO.transform.GetChild(0).GetComponent<Text>().text = h + ":" + m + ":" + s;
        else
            currentEventGO.transform.GetChild(0).GetComponent<Text>().text = m + ":" + s;

        currentEventGO.transform.GetChild(1).GetComponent<Text>().text = currentEvent.elecOfInterest;
        currentEventGO.transform.SetParent(panelContent);
        currentEventGO.transform.localScale = new Vector3(1, 1, 1);
        currentEventGO.transform.position = currentEventGO.transform.parent.position;
        currentEventGO.transform.SetSiblingIndex(id);

        newEventToShow(currentEvent, id);
    }

    void applyChangeToEvent(eventEeg modifyiedEvent, eventEeg previousEvent)
    {
        var eventToChangeObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(
                                   obj => obj.name == "Event - " + previousEvent.sample);

        if(eventToChangeObjects.Count() > 0)
        {
            events[previousEvent.sample].code = modifyiedEvent.code;
            events[previousEvent.sample].comment = modifyiedEvent.comment;

            foreach (var eventToChange in eventToChangeObjects)
            {
                eventToChange.GetComponent<EventTrace>().UpdateEvent(modifyiedEvent); // events[previousEvent.sample]);
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
                foreach (KeyValuePair<int, eventEeg> kvp in events)
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
                foreach (KeyValuePair<int, eventEeg> kvp in events)
                {
                    int timeInSec = kvp.Value.sample / 64;
                    int h = timeInSec / 3600;
                    int m = (timeInSec / 60) % 60;
                    int s = timeInSec % 60;

                    string dd = "";
                    if (h > 0)
                        dd = h + ":" + m + ":" + s;
                    else
                        dd = m + ":" + s;

                    sw.Write(dd.PadRight(10));
                    sw.Write(kvp.Value.comment.PadRight(40));
                    sw.Write(kvp.Value.code.ToString().PadRight(10));
                    sw.Write(kvp.Value.sample.ToString().PadRight(10));
                    sw.Write(kvp.Value.duration.ToString().PadRight(10));
                    sw.WriteLine(kvp.Value.elecOfInterest);
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

    void loadEventList()
    {
        List<eventEeg> eventLoaded = new List<eventEeg>();
        string pathFile = QtGUI_dll.Instance.getOpenFileName(new string[] { "btv" });

        try
        {
            using (StreamReader sr = new StreamReader(pathFile))
            {
                string r;

                while ((r = sr.ReadLine()) != null)
                {
                    string[] resultSplit = r.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (resultSplit.Count() == 6)
                    {
                        eventLoaded.Add(new eventEeg(int.Parse(resultSplit[2]), int.Parse(resultSplit[3]), int.Parse(resultSplit[4]), resultSplit[5], resultSplit[1]));
                    }
                }
                sr.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The pos file could not be read:");
            Console.WriteLine(e.Message);
        }

        for (int i = 0; i < eventLoaded.Count; i++) 
        {
            eventValidatedForUI(eventLoaded[i]);
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
        infoEdit.aaaagh -= new imDying(removeConnectionAddUi);
        infoEdit = null;
    }

    void removeConnectionDispUI()
    {
        infoDisp.editEvent -= new eventToEditHandler((eventToEdit) =>
        {
            openEventAddUI(eventToEdit, traceIDCalled);
        });
        infoDisp.aaaagh -= new imDying(removeConnectionDispUI);
        infoDisp = null;
    }
}

public delegate void offsetVideoChangedEventHandler(int newVal);

public class videoOptions
{
    public event offsetVideoChangedEventHandler offsetVideoHasChanged;

    Button removeVideoOffset = null;
    Scrollbar offsetScrollBar = null;
    Button addVideoOffset = null;
    Text videoOffsetLabel = null;
    Button sampleMode = null;

    EventTrigger trigger = null;
    //bool scrollBarNotClicked = true;
    int offsetSec = 0;

    public videoOptions(GameObject videoOptionsPanel)
    {
        removeVideoOffset = videoOptionsPanel.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        offsetScrollBar = videoOptionsPanel.transform.GetChild(0).GetChild(1).GetComponent<Scrollbar>();
        addVideoOffset = videoOptionsPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>();
        videoOffsetLabel = videoOptionsPanel.transform.GetChild(0).GetChild(3).GetComponent<Text>();
        sampleMode = videoOptionsPanel.transform.GetChild(1).GetComponent<Button>();

        removeVideoOffset.onClick.AddListener(() =>
        {
            if (offsetSec - 1 >= -60)
            {
                offsetSec -= 1;
                offsetScrollBar.value = ((float)offsetSec / 120) + 0.5f;
                setOffsetText(offsetSec);
                offsetVideoHasChanged(offsetSec);
            }
        });

        addVideoOffset.onClick.AddListener(() =>
        {
            if (offsetSec + 1 <= 60)
            {
                offsetSec += 1;
                offsetScrollBar.value = ((float)offsetSec / 120) + 0.5f;
                setOffsetText(offsetSec);
                offsetVideoHasChanged(offsetSec);
            }
        });

        trigger = offsetScrollBar.gameObject.AddComponent<EventTrigger>();
        //EventTrigger.Entry entry = new EventTrigger.Entry();
        //entry.eventID = EventTriggerType.PointerDown;
        //entry.callback.AddListener((eventData) => { scrollBarNotClicked = false; });
        //trigger.triggers.Add(entry);

        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerUp;
        entry2.callback.AddListener((eventData) => { setOffsetScrollBar(); });
        trigger.triggers.Add(entry2);

        videoOffsetLabel.text = "Offset : 00: 00 s";
    }

    ~videoOptions()
    {
        removeVideoOffset.onClick.RemoveAllListeners();
        addVideoOffset.onClick.RemoveAllListeners();

        for (int i = 0; i < trigger.triggers.Count; i++)
            trigger.triggers[i].callback.RemoveAllListeners();
    }

    void setOffsetScrollBar()
    {
        float offsetBar = offsetScrollBar.value - 0.5f;
        offsetSec = (int)(offsetBar * 120);
        setOffsetText(offsetSec);
    }

    void setOffsetText(int sec)
    {
        int m = sec / 60;
        int s = sec % 60;

        videoOffsetLabel.text = "Offset : " + m + ": " + s + "s";
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
public delegate void toggleSonification(bool isSonifOn);
public delegate void newSoundSonif(int newIDSound);

public class traceXOptions
{
    public event gainChangedEventHandler gainHasChanged;
    public event offsetChangedEventHandler offsetHasChanged;
    public event idFileChangedEventHandler idFileHasChanged;
    public event idElecChangedEventHandler idElecHasChanged;
    public event timePeriodChangedEventHandler timeHasChanged;
    public event toggleSonification sonifToggled;
    public event newSoundSonif soundChanged;

    GameObject elecPlot = null;
    GameObject electrodeContentPanel = null;

    Button sm0Button = null;
    Button sm250Button = null;
    Button sm500Button = null;
    Button sm1000Button = null;
    Button sm2500Button = null;
    Button sm5000Button = null;

    Text gainLabel = null;
    Button gainAddButton = null;
    Button gainRemoveButton = null;

    Text offsetLabel = null;
    Button offsetAddButton = null;
    Button offsetRemoveButton = null;

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

    public traceXOptions(GameObject traceOptionsPanel, BTVMedia media)
    {
        elecPlot = Resources.Load("Prefabs/Hub-Elec", typeof(GameObject)) as GameObject;
        sonifON = Resources.Load("Pictures/soundOK", typeof(Texture2D)) as Texture2D;
        sonifOFF = Resources.Load("Pictures/soundNOK", typeof(Texture2D)) as Texture2D;

        electrodeContentPanel = traceOptionsPanel.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

        sm0Button = traceOptionsPanel.transform.GetChild(1).GetChild(0).GetComponent<Button>();
        sm250Button = traceOptionsPanel.transform.GetChild(1).GetChild(1).GetComponent<Button>();
        sm500Button = traceOptionsPanel.transform.GetChild(1).GetChild(2).GetComponent<Button>();
        sm1000Button = traceOptionsPanel.transform.GetChild(1).GetChild(3).GetComponent<Button>();
        sm2500Button = traceOptionsPanel.transform.GetChild(1).GetChild(4).GetComponent<Button>();
        sm5000Button = traceOptionsPanel.transform.GetChild(1).GetChild(5).GetComponent<Button>();

        sm0Button.onClick.AddListener(() =>
        {
            if (ELAN.checkHandle(media.elanFiles, 0))
            {
                idFileHasChanged(0);
            }
        });

        sm250Button.onClick.AddListener(() =>
        {
            if (ELAN.checkHandle(media.elanFiles, 1))
            {
                idFileHasChanged(1);
            }
        });

        sm500Button.onClick.AddListener(() =>
        {
            if (ELAN.checkHandle(media.elanFiles, 2))
            {
                idFileHasChanged(2);
            }
        });

        sm1000Button.onClick.AddListener(() =>
        {
            if (ELAN.checkHandle(media.elanFiles, 3))
            {
                idFileHasChanged(3);
            }
        });

        sm2500Button.onClick.AddListener(() =>
        {
            if (ELAN.checkHandle(media.elanFiles, 4))
            {
                idFileHasChanged(4);
            }
        });

        sm5000Button.onClick.AddListener(() =>
        {
            if (ELAN.checkHandle(media.elanFiles, 5))
            {
                idFileHasChanged(5);
            }
        });

        gainLabel = traceOptionsPanel.transform.GetChild(2).GetChild(0).GetComponent<Text>();
        gainLabel.text = "Gain : " + gain;
        gainAddButton = traceOptionsPanel.transform.GetChild(2).GetChild(1).GetComponent<Button>();
        gainRemoveButton = traceOptionsPanel.transform.GetChild(2).GetChild(2).GetComponent<Button>();

        gainAddButton.onClick.AddListener(() =>
        {
            gain += 1;
            gainLabel.text = "Gain : " + gain;
            gainHasChanged(gain);
        });
        gainRemoveButton.onClick.AddListener(() =>
        {
            if (gain - 1 > 0)
            {
                gain -= 1;
                gainLabel.text = "Gain : " + gain;
                gainHasChanged(gain);
            }
        });

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

        timePeriodInputField = traceOptionsPanel.transform.GetChild(4).GetChild(1).GetComponent<InputField>();
        timePeriodInputField.onEndEdit.AddListener(delegate { changeTimePeriod(timePeriodInputField); });

        sonifButton = traceOptionsPanel.transform.GetChild(5).GetChild(0).GetComponent<Button>();
        sonifSoundDropDown = traceOptionsPanel.transform.GetChild(5).GetChild(1).GetComponent<Dropdown>();
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
        sm0Button.onClick.RemoveAllListeners();
        sm250Button.onClick.RemoveAllListeners();
        sm500Button.onClick.RemoveAllListeners();
        sm1000Button.onClick.RemoveAllListeners();
        sm2500Button.onClick.RemoveAllListeners();
        sm5000Button.onClick.RemoveAllListeners();

        gainAddButton.onClick.RemoveAllListeners();
        gainRemoveButton.onClick.RemoveAllListeners();

        offsetAddButton.onClick.RemoveAllListeners();
        offsetRemoveButton.onClick.RemoveAllListeners();

        timePeriodInputField.onEndEdit.RemoveAllListeners();

        sonifButton.onClick.RemoveAllListeners();
        sonifSoundDropDown.onValueChanged.RemoveAllListeners();
    }

    public void loadElectrodeInPanel(List<string> electrodeList)
    {
        deleteElectrodeInPanel();
        for (int i = 0; i < electrodeList.Count; i++)
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
            currentElecText.text = electrodeList[i];
            currentElectrode.name = electrodeList[i];
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

    void changeTimePeriod(InputField timeField)
    {
        int myVal = 0;
        Int32.TryParse(timePeriodInputField.text, out myVal);
        timeHasChanged(myVal);
    }

    void toggleSonification()
    {
        isSonifOn = !isSonifOn;
        if (isSonifOn)
            sonifButton.GetComponent<RawImage>().texture = sonifON;
        else
            sonifButton.GetComponent<RawImage>().texture = sonifOFF;

        sonifToggled(isSonifOn);
    }

    void changeSonifSound(int newIDDropDown)
    {
        soundChanged(newIDDropDown);
    }
}

public class brainOptions
{
    public event gainChangedEventHandler gainHasChanged;

    Button mniButton = null;
    Button patButton = null;
    Button dispFullBrain = null;
    Button dispLeftBrain = null;
    Button dispRightBrain = null;
    Text gainValue = null;
    Button gainAdd = null;
    Button gainRemove = null;
    int gain = 1;

    public brainOptions(GameObject brainOptionsPanel)
    {
        mniButton = brainOptionsPanel.transform.GetChild(0).GetComponent<Button>();
        patButton = brainOptionsPanel.transform.GetChild(1).GetComponent<Button>();
        dispFullBrain = brainOptionsPanel.transform.GetChild(2).GetComponent<Button>();
        dispLeftBrain = brainOptionsPanel.transform.GetChild(3).GetComponent<Button>();
        dispRightBrain = brainOptionsPanel.transform.GetChild(4).GetComponent<Button>();
        gainValue = brainOptionsPanel.transform.GetChild(5).GetChild(0).GetComponent<Text>();
        gainAdd = brainOptionsPanel.transform.GetChild(5).GetChild(1).GetComponent<Button>();
        gainRemove = brainOptionsPanel.transform.GetChild(5).GetChild(2).GetComponent<Button>();

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
        dispFullBrain.onClick.RemoveAllListeners();
        dispLeftBrain.onClick.RemoveAllListeners();
        dispRightBrain.onClick.RemoveAllListeners();
        gainAdd.onClick.RemoveAllListeners();
        gainRemove.onClick.RemoveAllListeners();
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
            isVisible = !isVisible;

            if (isVisible)
                nameText.color = orange;
            else
                nameText.color = blue;

            if (optionsPanel.activeSelf == false)
            {
                optionsPanel.SetActive(true);
                options.SetActive(!options.activeSelf);
            }
            else
            {
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
        });
    }

    ~UIOption()
    {
        showButton.onClick.RemoveAllListeners();
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
    UIOption brainOpt = null;
    UIOption trace1Opt = null;
    UIOption trace2Opt = null;
    UIOption perfOpt = null;
    UIOption videoOpt = null;
    UIOption eventsOpt = null;
    //==
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
        trace1Opt = new UIOption(gameObject, detaileOptionsPanel, 1);
        traceXOpts[0] = new traceXOptions(trace1Opt.optionsPanel, media);
        trace2Opt = new UIOption(gameObject, detaileOptionsPanel, 2);
        traceXOpts[1] = new traceXOptions(trace2Opt.optionsPanel, media);
        perfOpt = new UIOption(gameObject, detaileOptionsPanel, 3);
        perfOpts = new perfDataOptions(perfOpt.optionsPanel);
        videoOpt = new UIOption(gameObject, detaileOptionsPanel, 4);
        vidOpts = new videoOptions(videoOpt.optionsPanel);
        eventsOpt = new UIOption(gameObject, detaileOptionsPanel, 5);
        eventsOpts = gameObject.AddComponent<eventsOptions>();
        eventsOpts.init(eventsOpt.optionsPanel);
    }
}
