using BTV.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Trace : MonoBehaviour, IPointerClickHandler
{
    public int TraceId
    {
        get
        {
            return traceID;
        }
    }
    public AudioSignal TraceAudio
    {
        get
        {
            return audioSignal;
        }
    }
    public EegSignal TraceEeg
    {
        get
        {
            return eegSignal;
        }
    }
    public GraphGrid GraphGrid
    {
        get
        {
            return graphGrid;
        }
    }

    [SerializeField] EegSignal eegSignal = null;
    [SerializeField] AudioSignal audioSignal = null;
    [SerializeField] GraphLabel graphLabel = null;
    [SerializeField] selectRing ring = null;
    [SerializeField] GraphGrid graphGrid = null;
    [SerializeField] GraphEvents graphEvent = null;
    [SerializeField] GraphSonification graphSonif = null;
    [SerializeField] Window m_window = null;
    [SerializeField] Window m_handleOtherTrace = null;
    [SerializeField] int traceID = 0;

    bool m_initDone = false;
    int m_State = 1;
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
    GameObject m_PopUpAddWindow = null, m_PopUpEditWindow = null, m_PopUpDisplayWindow = null;

    void Awake()
    {
        m_AddEventWindowPrefabs = Resources.Load("Prefabs/EventInfoEdit", typeof(GameObject)) as GameObject;
        m_DisplayEventWindowPrefabs = Resources.Load("Prefabs/EventInfoDisplay", typeof(GameObject)) as GameObject;

        m_signalWindow1 = GameObject.Find("Trace1Window").GetComponent<Trace>();
        m_signalWindow2 = GameObject.Find("Trace2Window").GetComponent<Trace>();

        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
        Messenger.Default.Register<UiToTraceMessage>(this, OnTraceParametersMessage, MessageContext.UiToTrace);
        Messenger.Default.Register<UiToVideoMessage>(this, OnVideoParametersMessage, MessageContext.UiToVideo);
        Messenger.Default.Register<EventsToTraceMessage>(this, OnEventsToTraceMessage, MessageContext.EventsToTraceMessage);
        Messenger.Default.Register<BrainWardenToTraceMessage>(this, OnBrainWardenToTraceMessage, MessageContext.BrainWardenToTraceMessage);
        Messenger.Default.Register<VideoToModulesMessage>(this, OnVideoToModulesMessage, MessageContext.VideoToModulesMessage);
        
    }

    void OnDestroy()
    {
        if (m_initDone)
        {
            graphLabel.ElectrodeButton.onClick.RemoveAllListeners();
            
            Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
            Messenger.Default.Unregister(this, MessageContext.UiToTrace);
            Messenger.Default.Unregister(this, MessageContext.UiToVideo);
            Messenger.Default.Unregister(this, MessageContext.EventsToTraceMessage);
            Messenger.Default.Unregister(this, MessageContext.BrainWardenToTraceMessage);
            Messenger.Default.Unregister(this, MessageContext.VideoToModulesMessage);
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
                    updateElectrodeById(eegSignal.ElectrodeID - 1);
                else
                    updateElectrodeById(eegSignal.ElectrodeID + 1);
            }
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (m_rectTransform == null) return;

        if (m_State > 0)
        {
            gameObject.SetActive(m_rectTransform.rect.width > 100);
        }
    }

    private void OnLoaderMessage(LoaderMessage message)
    {
        if (message.Task == LoaderMessage.LoaderTask.LoadTrace)
        {
            init();
        }
    }

    void init()
    {
        m_rectTransform = gameObject.GetComponent<RectTransform>();

        eegSignal.Initialize();
        audioSignal.Initialize();
        graphLabel.Initialize(eegSignal.ElectrodeLabel, eegSignal.FileHandle.Description);
        graphGrid.init(eegSignal.PeriodInSeconds);
        graphEvent.init(this);
        graphSonif.init(this);

        graphLabel.ElectrodeButton.onClick.AddListener(updateTracesWidth);
        m_initDone = true;
    }

    private void OnTraceParametersMessage(UiToTraceMessage message)
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
                eegSignal.UpdateOffset(message.Offset);
                break;
            case 2:
                Debug.Log("Toggle Grid");
                graphGrid.IsOn = message.IsGridOn;
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
                changeFileID(message.FileID);
                break;
            default:
                Debug.LogError("Trace.cs : Id of action to execute does not exist : " + message.TaskToExecute);
                break;
        }
    }

    private void OnVideoParametersMessage(UiToVideoMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                Debug.Log("Update Trace Gain");
                audioSignal.UpdateGain(message.Gain);
                break;
            case 1:
                Debug.Log("Update Trace Offset");
                audioSignal.OffsetInMilliseconds = message.Offset;
                break;
            case 2:
                Debug.Log("Toggle Audio Trace");
                audioSignal.Show(message.IsTraceOn);
                break;
            case 3:
                Debug.Log("Update Trace Audio File");
                audioSignal.UpdateAudioID(message.TraceID);
                break;
        }
    }

    private void OnEventsToTraceMessage(EventsToTraceMessage message)
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
                if (message.ParentWindowIndex == traceID)
                {
                    bool ind = RectTransformUtility.RectangleContainsScreenPoint(m_rectTransform, Input.mousePosition, Camera.main);
                    if (ind)
                        OpenEventModify(message.Event);
                }
                break;
            case 3://Add Event
                graphEvent.AddEventToTrace(message.Event, message.EventIndex);
                break;
            case 4://Delete Event
                graphEvent.DeleteEventFromTrace(message.EventIndex);
                break;
            case 5:
                if (message.ParentWindowIndex == traceID)
                {
                    bool ind = RectTransformUtility.RectangleContainsScreenPoint(m_rectTransform, Input.mousePosition, Camera.main);
                    if (ind)
                        OpenEventDisplay(message.Event);
                }
                break;
        }
    }

    private void OnBrainWardenToTraceMessage(BrainWardenToTraceMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                plotClicked(message.ClickedElectrode);
                break;
            default:
                Debug.LogError("Trace.cs : Id of action to execute does not exist : " + message.TaskToExecute);
                break;
        }
    }

    private void OnVideoToModulesMessage(VideoToModulesMessage message)
    {
        if (message.IsStopped) graphSonif.muteSonficiation();

        eegSignal.UpdateDraw((int)message.TimeMilliseconds);
        audioSignal.UpdateDraw((int)message.TimeMilliseconds);
        graphEvent.UpdateEventsOnTrace((int)message.TimeMilliseconds);
        graphSonif.updateSonif((int)message.TimeMilliseconds);
    }

    public void UpdateWindowState(int state)
    {
        UnityEngine.Debug.Log("Updating Trace " + TraceId + " Ui State");
        m_State = state;
        switch (m_State)
        {
            case 0: //Hide 3D Module
                gameObject.SetActive(false);
                break;
            case 1: //Show 3D Module
            case 2: //Module is visible but special selection mode
                    //for electrodes is disabled if it was on before
                gameObject.SetActive(true);

                plotClicked(null);
                m_window.setBorderColor(blue);
                m_window.hasFocus = false;
                break;
            case 3: //Module is visible and special selection mode
                    //allowing to change electrode by clicking on the brain
                    //is on
                m_window.setBorderColor(orange);
                m_window.hasFocus = true;

                if (m_handleOtherTrace != null)
                {
                    m_handleOtherTrace.hasFocus = false;
                    m_handleOtherTrace.setBorderColor(blue);
                }

                plotClicked(GameObject.Find(eegSignal.ElectrodeName.ToLower()));
                break;
        }
    }

    void changeFileID(int newId)
    {
        eegSignal.UpdateFileId(newId);
        graphLabel.Description = eegSignal.FileHandle.Description;
        updateTimeResolution(eegSignal.PeriodInSeconds);
    }

    void updateTimeResolution(int newPeriod)
    {
        eegSignal.PeriodInSeconds = newPeriod;
        eegSignal.UpdateHorizontalScale();
        audioSignal.PeriodInSeconds = newPeriod;
        audioSignal.UpdateHorizontalScale();
        graphGrid.updateGridScale(newPeriod);
    }

    void UpdateTraceGain(float newGain)
    {
        eegSignal.UpdateGain(newGain);
        graphLabel.Electrode = eegSignal.ElectrodeLabel;
    }

    void updateElectrodeById(int newId)
    {
        eegSignal.ElectrodeID = newId;
        eegSignal.UpdateOffset();
        graphLabel.Electrode = eegSignal.ElectrodeLabel;
    }

    void updateTracesWidth()
    {
        eegSignal.UpdateLineWidth();
        audioSignal.UpdateLineWidth();
    }

    void updateColors(Color color)
    {
        graphLabel.Color = color;
        eegSignal.Color = color;
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
        float msClicked = (eegSignal.MostRecentTimeInMilliSecs - (eegSignal.PeriodInSeconds * 1000)) + (perCentX * (eegSignal.PeriodInSeconds * 1000));
        if (msClicked >= 0)
        {
            BtvEvent currentEvent = new BtvEvent(0, (int)msClicked, elecOfInterest: eegSignal.ElectrodeLabel);
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

            plotClicked(GameObject.Find(eegSignal.ElectrodeName.ToLower()));
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
            if (hit.collider.name == "Electrode_" + (traceID))
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
    private void OpenEventAdd(BtvEvent Event)
    {
        if (m_AddEvents && m_PopUpAddWindow == null)
        {
            m_PopUpAddWindow = Instantiate(m_AddEventWindowPrefabs);

            if (traceID == 0)
            {
                m_PopUpAddWindow.transform.SetParent(m_signalWindow1.gameObject.transform);
                Event.SecondSiteOfInterest = m_signalWindow2.TraceEeg.ElectrodeLabel;
            }
            else
            {
                m_PopUpAddWindow.transform.SetParent(m_signalWindow2.gameObject.transform);
                Event.SecondSiteOfInterest = m_signalWindow1.TraceEeg.ElectrodeLabel;
            }
            m_PopUpAddWindow.transform.localScale = new Vector3(1, 1, 1);
            m_PopUpAddWindow.transform.localPosition = new Vector3(0, 0, -402);

            EventInfoEdit infoEdit = m_PopUpAddWindow.GetComponent<EventInfoEdit>();
            infoEdit.init(Event, false);
        }
    }

    private void OpenEventDisplay(BtvEvent Event)
    {
        if (m_PopUpDisplayWindow == null)
        {
            m_PopUpDisplayWindow = Instantiate(m_DisplayEventWindowPrefabs);

            if (traceID == 0)
                m_PopUpDisplayWindow.transform.SetParent(m_signalWindow1.gameObject.transform);
            else
                m_PopUpDisplayWindow.transform.SetParent(m_signalWindow2.gameObject.transform);
            m_PopUpDisplayWindow.transform.localScale = new Vector3(1, 1, 1);
            m_PopUpDisplayWindow.transform.localPosition = new Vector3(0, 0, -402);

            EventInfoDisplay infoDisp = m_PopUpDisplayWindow.GetComponent<EventInfoDisplay>();
            infoDisp.init(Event);
        }
    }

    private void OpenEventModify(BtvEvent Event)
    {
        ApplicationState.Module3D.MemoryEvent = null;
        if (m_PopUpEditWindow == null)
        {
            m_PopUpEditWindow = Instantiate(m_AddEventWindowPrefabs);

            if (traceID == 0)
                m_PopUpEditWindow.transform.SetParent(m_signalWindow1.gameObject.transform);
            else
                m_PopUpEditWindow.transform.SetParent(m_signalWindow2.gameObject.transform);
            m_PopUpEditWindow.transform.localScale = new Vector3(1, 1, 1);
            m_PopUpEditWindow.transform.localPosition = new Vector3(0, 0, -402);

            EventInfoEdit infoEdit = m_PopUpEditWindow.GetComponent<EventInfoEdit>();
            infoEdit.init(Event, true);
        }
    }
}