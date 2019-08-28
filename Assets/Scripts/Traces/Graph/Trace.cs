using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void eventsClickedHandler(TraceEvent newVal, int idWin);

public class Trace : MonoBehaviour, IPointerClickHandler
{
    public event eventsClickedHandler eventWasClicked;

    public int TraceId
    {
        get
        {
            return traceID;
        }
    }
    public EegSignal TraceEeg
    {
        get
        {
            return eegSignal;
        }
    }
    public GraphEvents EventsEeg
    {
        get
        {
            return graphEvent;
        }
    }
    public bool hasFocus
    {
        get
        {
            return m_window.hasFocus;
        }
    }

    [SerializeField] BTVMedia media = null;
    [SerializeField] VideoPlayer video = null;
    [SerializeField] EegSignal eegSignal = null;
    [SerializeField] AudioSignal audioSignal = null;
    [SerializeField] GraphLabel graphLabel = null;
    [SerializeField] selectRing ring = null;
    [SerializeField] BrainWarden warden = null;
    [SerializeField] GraphGrid graphGrid = null;
    [SerializeField] GraphEvents graphEvent = null;
    [SerializeField] GraphSonification graphSonif = null;
    [SerializeField] Window m_window = null;
    [SerializeField] Window m_handleOtherTrace = null;
    [SerializeField] int traceID = 0;

    bool m_initDone = false;
    bool m_AddEvents = false;
    RectTransform m_rectTransform = null;
    Vector3[] m_worldCornerOfBrainPanel = new Vector3[4];
    Vector3[] m_worldCorners = new Vector3[4];
    Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    Color blue = new Color(0.6117f, 0.7058f, 0.7960f);
    Color yellow = new Color(0.9058f, 0.8784f, 0.0f);

    Trace m_signalWindow1 = null;
    Trace m_signalWindow2 = null;

    GameObject m_AddEventWindowPrefabs = null;
    GameObject m_DisplayEventWindowPrefabs = null;

    void Awake()
    {
        m_AddEventWindowPrefabs = Resources.Load("Prefabs/EventInfoEdit", typeof(GameObject)) as GameObject;
        m_DisplayEventWindowPrefabs = Resources.Load("Prefabs/EventInfoDisplay", typeof(GameObject)) as GameObject;

        m_signalWindow1 = GameObject.Find("Trace1Window").GetComponent<Trace>();
        m_signalWindow2 = GameObject.Find("Trace2Window").GetComponent<Trace>();

        media.loadTrace += new initTrace(init);
        Messenger.Default.Register<UiToTraceMessage>(this, OnTraceParametersMessage, MessageContext.UiToTrace);
        Messenger.Default.Register<UiToVideoMessage>(this, OnVideoParametersMessage, MessageContext.UiToVideo);
        Messenger.Default.Register<EventsToTraceMessage>(this, OnEventsToTraceMessage, MessageContext.EventsToTraceMessage);

    }

    void OnDestroy()
    {
        media.loadTrace -= new initTrace(init);
        if (m_initDone)
        {
            video.sendTime -= new timeVideo(eegSignal.updateDraw);
            video.sendTimeVideo -= new timeVideoSync(audioSignal.updateDraw);
            video.sendTime -= new timeVideo(graphEvent.UpdateEventsOnTrace);
            video.sendTime -= new timeVideo(graphSonif.updateSonif);
            video.stopTimeVideo -= new stopVideo(graphSonif.muteSonficiation);

            //hub.traceRemotes[traceID].idFileHasChanged -= new idFileChangedEventHandler(changeFileID);
            //hub.traceRemotes[traceID].idElecHasChanged -= new idElecChangedEventHandler(updateElectrodeById);
            warden.plotWasClicked -= new newPlotClicked(plotClicked);
            graphLabel.ElectrodeButton.onClick.RemoveAllListeners();
            //hub.traceRemotes[traceID].deleteElectrodeInPanel();

            Messenger.Default.Unregister(this, MessageContext.UiToTrace);
            Messenger.Default.Unregister(this, MessageContext.UiToVideo);
            Messenger.Default.Unregister(this, MessageContext.EventsToTraceMessage);
        }
    }

    void Update()
    {
        if (m_initDone && isOver(Input.mousePosition) && m_window.hasFocus)
        {
            Vector2 scrollDelta = Input.mouseScrollDelta;
            if (scrollDelta.y != 0)
            {
                if (scrollDelta.y < 0)
                    updateElectrodeById(eegSignal.IdElectrode - 1);
                else
                    updateElectrodeById(eegSignal.IdElectrode + 1);
            }
        }
    }

    void init()
    {
        m_rectTransform = gameObject.GetComponent<RectTransform>();

        eegSignal.init();
        audioSignal.init();
        graphLabel.init(eegSignal.LabelElectrode);
        graphGrid.init(eegSignal.PeriodInSeconds);
        graphEvent.init(this);
        graphSonif.init(this);

        #region plugEvents
        video.sendTime += new timeVideo(eegSignal.updateDraw);
        video.sendTimeVideo += new timeVideoSync(audioSignal.updateDraw);
        video.sendTime += new timeVideo(graphEvent.UpdateEventsOnTrace);
        video.sendTime += new timeVideo(graphSonif.updateSonif);
        video.stopTimeVideo += new stopVideo(graphSonif.muteSonficiation);

        //hub.traceRemotes[traceID].idFileHasChanged += new idFileChangedEventHandler(changeFileID);
        //hub.traceRemotes[traceID].idElecHasChanged += new idElecChangedEventHandler(updateElectrodeById);
        warden.plotWasClicked += new newPlotClicked(plotClicked);
        graphLabel.ElectrodeButton.onClick.AddListener(updateTracesWidth);
        //hub.traceRemotes[traceID].loadElectrodeInPanel(eegSignal.fileHandle.electrodes);
        #endregion

        m_initDone = true;
    }

    void OnTraceParametersMessage(UiToTraceMessage message)
    {
        if (message.TraceID != traceID)
            return;

        switch (message.TaskToExecute)
        {
            case 0:
                Debug.Log("Update Trace Gain");
                UpdateTraceGain(message.Gain);
                break;
            case 1:
                Debug.Log("Update Trace Offset");
                eegSignal.updateOffset(message.Gain);
                break;
            case 2:
                Debug.Log("Toggle Grid");
                graphGrid.displayTimeGrid(message.IsGridOn);
                break;
            case 3:
                Debug.Log("Update WIndow Period");
                updateTimeResolution(message.TimeWindow);
                break;
            case 4:
                Debug.Log("Toggle Sonification");
                graphSonif.toggleSonification(message.IsSonificationOn);
                break;
            case 5:
                Debug.Log("Update Sonification Sound");
                graphSonif.changeAudioSonification(message.NewSonificationId);
                break;
            case 6:
                Debug.Log("Update ColorPicker");
                updateColors(message.Color);
                break;
            case 7:
                Debug.Log("Update File Switcher");
                break;
            default:
                Debug.LogError("Trace.cs : Id of action to execute does not exist : " + message.TaskToExecute);
                break;
        }
    }

    void OnVideoParametersMessage(UiToVideoMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                Debug.Log("Update Trace Gain");
                audioSignal.updateGain(message.Gain);
                break;
            case 1:
                Debug.Log("Update Trace Offset");
                //At the moment offset is just used to calculate video time
                //by reading scrollbar value, maybe need to separate that
                //better
                break;
            case 2:
                Debug.Log("Toggle Audio Trace");
                audioSignal.Show(message.IsTraceOn);
                break;
            case 3:
                Debug.Log("Update Trace Audio File");
                audioSignal.changeAudioId(message.TraceID);
                break;
        }
    }

    void OnEventsToTraceMessage(EventsToTraceMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0: //add toggle
                m_AddEvents = message.IsAddEventsOn;
                break;
            case 1://show toggle
                graphEvent.DisplayEvents = message.IsShowEventsOn;
                break;
            case 2://Edit Events
                OpenEventModify(message.Event);
                break;
            case 3://Add Event
                graphEvent.AddEventToTrace(message.Event, message.EventIndex);
                break;
            case 4://Delete Event
                graphEvent.DeleteEventFromTrace(message.EventIndex);
                break;
        }
    }

    void changeFileID(int newId)
    {
        eegSignal.updateFileId(newId);
        updateTimeResolution(eegSignal.PeriodInSeconds);
    }

    void updateTimeResolution(int newPeriod)
    {
        eegSignal.updateTimeResolution(newPeriod);
        eegSignal.updateHorizontalScale();
        audioSignal.updateTimeResolution(newPeriod);
        audioSignal.updateHorizontalScale();
        graphGrid.updateGridScale(newPeriod);
    }

    void UpdateTraceGain(float newGain)
    {
        eegSignal.updateGain(newGain);
        graphLabel.setName(eegSignal.LabelElectrode);
    }

    void updateElectrodeById(int newId)
    {
        eegSignal.IdElectrode = newId;
        eegSignal.updateOffset();
        graphLabel.setName(eegSignal.LabelElectrode);
    }

    void updateTracesWidth()
    {
        eegSignal.updateLineWidth();
        audioSignal.updateLineWidth();
    }

    void updateColors(Color color)
    {
        graphLabel.setColor(color);
        eegSignal.updateLineColor(color);
    }

    void plotClicked(GameObject plot)
    {
        if (m_window.hasFocus)
        {
            if (plot != null)
            {
                int hitID = plot.GetComponent<Site>().ID;
                updateElectrodeById(hitID);
            }
            ring.setSelectedPlot(plot);
        }
    }

    bool isOver(Vector3 mousePos)
    {
        m_rectTransform.GetWorldCorners(m_worldCornerOfBrainPanel);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (worldClick.x > m_worldCornerOfBrainPanel[1].x && worldClick.x < m_worldCornerOfBrainPanel[2].x
            && worldClick.y > m_worldCornerOfBrainPanel[3].y && worldClick.y < m_worldCornerOfBrainPanel[2].y)
            return true;
        else
            return false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
            manageFocusClick();

        focusClickElecLabel();

        m_rectTransform.GetWorldCorners(m_worldCorners);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float perCentX = (worldClick.x - m_worldCorners[1].x) / (m_worldCorners[2].x - m_worldCorners[1].x);
        float sampleClicked = (eegSignal.mostRecentSample - eegSignal.numberOfPoint) + (perCentX * eegSignal.numberOfPoint);
        if (sampleClicked >= 0)
        {
            TraceEvent currentEvent = new TraceEvent(new eventEeg(0, (int)sampleClicked, eegSignal.SamplingFrequency), elecOfInterest:eegSignal.LabelElectrode);
            //eventWasClicked(currentEvent, traceID);
            OpenEventAdd(currentEvent);
        }
    }

    public void manageFocusClick()
    {
        if (!m_window.hasFocus)
        {
            m_window.setBorderColor(orange);
            m_window.hasFocus = !m_window.hasFocus;

            if (m_handleOtherTrace != null)
            {
                m_handleOtherTrace.hasFocus = false;
                m_handleOtherTrace.setBorderColor(blue);
            }

            plotClicked(GameObject.Find(eegSignal.nameElectrode.ToLower()));
        }
        else
        {
            plotClicked(null);
            m_window.setBorderColor(blue);
            m_window.hasFocus = !m_window.hasFocus;
        }
    }

    void focusClickElecLabel()
    {
        Ray r = new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit))
        {
            if (hit.collider.name == "ElecLabel" + (traceID + 1))
            {
                manageFocusClick();
                if (m_window.transform.position == m_handleOtherTrace.transform.position)
                {
                    gameObject.transform.SetSiblingIndex(1);
                    m_handleOtherTrace.gameObject.transform.SetSiblingIndex(0);
                }
            }
            else
            {
                m_handleOtherTrace.gameObject.GetComponent<Trace>().manageFocusClick();
                if (m_window.transform.position == m_handleOtherTrace.transform.position)
                {
                    gameObject.transform.SetSiblingIndex(0);
                    m_handleOtherTrace.gameObject.transform.SetSiblingIndex(1);
                }
            }
        }
    }

    //===
    //Those 3 needs to be connected via reception of a message from messenger
    private void OpenEventAdd(TraceEvent Event)
    {
        if (m_AddEvents)
        {
            GameObject AddEventWindow = Instantiate(m_AddEventWindowPrefabs);

            if (traceID == 0)
            {
                AddEventWindow.transform.SetParent(m_signalWindow1.gameObject.transform);
                Event.secondElecOfInterest = m_signalWindow2.TraceEeg.LabelElectrode;
            }
            else
            {
                AddEventWindow.transform.SetParent(m_signalWindow2.gameObject.transform);
                Event.secondElecOfInterest = m_signalWindow1.TraceEeg.LabelElectrode;
            }
            AddEventWindow.transform.localScale = new Vector3(1, 1, 1);
            AddEventWindow.transform.localPosition = new Vector3(0, 0, -402);

            EventInfoEdit infoEdit = AddEventWindow.GetComponent<EventInfoEdit>();
            infoEdit.init(Event, false);
        }
    }

    private void OpenEventDisplay(TraceEvent Event)
    {
        GameObject DisplayEventWindow = Instantiate(m_DisplayEventWindowPrefabs);

        if (traceID == 0)
            DisplayEventWindow.transform.SetParent(m_signalWindow1.gameObject.transform);
        else
            DisplayEventWindow.transform.SetParent(m_signalWindow2.gameObject.transform);
        DisplayEventWindow.transform.localScale = new Vector3(1, 1, 1);
        DisplayEventWindow.transform.localPosition = new Vector3(0, 0, -402);

        EventInfoDisplay infoDisp = DisplayEventWindow.GetComponent<EventInfoDisplay>();
        infoDisp.init(Event);
    }

    private void OpenEventModify(TraceEvent Event)
    {
        ApplicationState.MemoryEvent = null;

        GameObject AddEventWindow = Instantiate(m_AddEventWindowPrefabs);

        if (traceID == 0)
            AddEventWindow.transform.SetParent(m_signalWindow1.gameObject.transform);
        else
            AddEventWindow.transform.SetParent(m_signalWindow2.gameObject.transform);
        AddEventWindow.transform.localScale = new Vector3(1, 1, 1);
        AddEventWindow.transform.localPosition = new Vector3(0, 0, -402);

        EventInfoEdit infoEdit = AddEventWindow.GetComponent<EventInfoEdit>();
        infoEdit.init(Event, true);
    }
}