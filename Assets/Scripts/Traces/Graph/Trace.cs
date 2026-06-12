using BTV.Data;
using BTV.Services.EegFileService;
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
    public float MostRecentValueInPercentOfTrace
    {
        get
        {
            return eegSignal.MostRecentValueInPercentOfTrace;
        }
    }
    public GraphGrid GraphGrid
    {
        get
        {
            return graphGrid;
        }
    }
    public bool IsMouseOver { get; private set; }

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
    GameObject m_EditEventWindowPrefabs = null;
    GameObject m_DisplayEventWindowPrefabs = null;
    GameObject m_PopUpAddWindow = null, m_PopUpEditWindow = null, m_PopUpDisplayWindow = null;

    private float m_WheelSum = 0;

    private TraceOption m_TraceOption = null;
    private AudioTraceOption m_AudioOption = null;

    void Awake()
    {
        m_EditEventWindowPrefabs = Resources.Load("Prefabs/EventInfoEdit", typeof(GameObject)) as GameObject;
        m_AddEventWindowPrefabs = Resources.Load("Prefabs/EventInfoAdd", typeof(GameObject)) as GameObject;
        m_DisplayEventWindowPrefabs = Resources.Load("Prefabs/EventInfoDisplay", typeof(GameObject)) as GameObject;

        m_signalWindow1 = GameObject.Find("Trace1Window").GetComponent<Trace>();
        m_signalWindow2 = GameObject.Find("Trace2Window").GetComponent<Trace>();

        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
        Messenger.Default.Register<UiToTraceMessage>(this, OnTraceParametersMessage, MessageContext.UiToTrace);
        Messenger.Default.Register<UiToVideoMessage>(this, OnVideoParametersMessage, MessageContext.UiToVideo);
        Messenger.Default.Register<EventsToTraceMessage>(this, OnEventsToTraceMessage, MessageContext.EventsToTraceMessage);
        Messenger.Default.Register<BrainWardenToTraceMessage>(this, OnBrainWardenToTraceMessage, MessageContext.BrainWardenToTraceMessage);
        Messenger.Default.Register<VideoToModulesMessage>(this, OnVideoToModulesMessage, MessageContext.VideoToModulesMessage);
        Messenger.Default.Register<MontageMessage>(this, OnMontageMessage, MessageContext.MontageMessage);
    }

    void OnDestroy()
    {
        if (m_initDone)
            graphLabel.ElectrodeButton.onClick.RemoveAllListeners();

        // Unregister unconditionally: Awake always registers these, so they must always be
        // released even if no subject was loaded (m_initDone == false), or the Messenger keeps
        // invoking handlers on a destroyed object. MontageMessage was previously never released.
        Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
        Messenger.Default.Unregister(this, MessageContext.UiToTrace);
        Messenger.Default.Unregister(this, MessageContext.UiToVideo);
        Messenger.Default.Unregister(this, MessageContext.EventsToTraceMessage);
        Messenger.Default.Unregister(this, MessageContext.BrainWardenToTraceMessage);
        Messenger.Default.Unregister(this, MessageContext.VideoToModulesMessage);
        Messenger.Default.Unregister(this, MessageContext.MontageMessage);
    }

    private void OnGUI()
    {
        if (m_initDone == false) return;

        IsMouseOver = IsOver(Input.mousePosition);
        if (m_window.hasFocus)
        {
            if (IsMouseOver)
            {
                if (Event.current.type == EventType.ScrollWheel)
                {
                    Vector2 scrollDelta = Input.mouseScrollDelta;
                    if (scrollDelta.y != 0)
                    {
                        if (IsAlmostEqual(Mathf.Abs(Event.current.delta.y), Mathf.Abs(scrollDelta.y)))
                        {
                            //BtvLog.Log("ismouse");
                            UpdateElectrodeById(scrollDelta.y > 0 ? m_TraceOption.ElectrodeID + 1 : m_TraceOption.ElectrodeID - 1);
                        }
                        else
                        {
                            //BtvLog.Log("ispad");
                            m_WheelSum += scrollDelta.y;
                            if (m_WheelSum <= -0.1f)
                            {
                                m_WheelSum = 0;
                                UpdateElectrodeById(m_TraceOption.ElectrodeID - 1);
                            }
                            else if (m_WheelSum >= 0.1f)
                            {
                                m_WheelSum = 0;
                                UpdateElectrodeById(m_TraceOption.ElectrodeID + 1);
                            }
                        }
                    }
                }
            }
        }
    }

    private bool IsAlmostEqual(float a, float b)
    {
        if (a >= b - 0.0001f && a <= b + 0.0001f)
        {
            return true;
        }
        else
        {
            return false;
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
            Initialization();
        }
    }

    private void Initialization()
    {
        m_rectTransform = gameObject.GetComponent<RectTransform>();

        m_TraceOption = TracesService.GetOptionsFor(traceID);
        m_AudioOption = TracesService.GetAudioOptions();

        eegSignal.Initialize(traceID, m_TraceOption);
        audioSignal.Initialize(0, m_AudioOption);
        graphLabel.Initialize(m_TraceOption);

        graphGrid.init(m_TraceOption.WindowInSeconds);
        graphEvent.init(this);
        graphSonif.Init(this);

        graphLabel.ElectrodeButton.onClick.AddListener(UpdateTracesWidth);
        m_initDone = true;
    }

    private void OnTraceParametersMessage(UiToTraceMessage message)
    {
        if (message.TraceID != traceID)
            return;

        switch (message.TaskToExecute)
        {
            case UiToTraceMessage.Task.UpdateGain:
                BtvLog.Log("Update Trace Gain");
                m_TraceOption.Gain = message.Gain;
                graphLabel.Electrode = m_TraceOption.ElectrodeLabel;
                break;
            case UiToTraceMessage.Task.UpdateOffset:
                BtvLog.Log("Update Trace Offset");
                m_TraceOption.Offset = message.Offset;
                break;
            case UiToTraceMessage.Task.ToggleGrid:
                BtvLog.Log("Toggle Grid");
                graphGrid.IsOn = message.IsGridOn;
                break;
            case UiToTraceMessage.Task.UpdateWindowSize:
                BtvLog.Log("Update WIndow Period");
                UpdateTimeResolution(message.TimeWindow);
                break;
            case UiToTraceMessage.Task.ToggleSonification:
                BtvLog.Log("Toggle Sonification");
                graphSonif.Toggle(message.IsSonificationOn);
                break;
            case UiToTraceMessage.Task.UpdateSonificationSound:
                BtvLog.Log("Update Sonification Sound");
                graphSonif.ChangeAudioClip(message.NewSonificationId);
                break;
            case UiToTraceMessage.Task.UpdateColor:
                BtvLog.Log("Update ColorPicker");
                graphLabel.Color = message.Color;
                m_TraceOption.Color = message.Color;
                break;
            case UiToTraceMessage.Task.UpdateFile:
                BtvLog.Log("Update File Switcher");
                m_TraceOption.FileHandle = EegFileService.ChangeContainerHandle(m_TraceOption.FileHandle, message.FileID);
                graphLabel.Electrode = m_TraceOption.ElectrodeLabel;
                graphLabel.Description = m_TraceOption.FileHandle.Description;
                UpdateTimeResolution(m_TraceOption.WindowInSeconds);
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
            case UiToVideoMessage.Task.UpdateGain:
                BtvLog.Log("Update Trace Gain");
                m_AudioOption.Gain = message.Gain;
                break;
            case UiToVideoMessage.Task.UpdateOffset:
                BtvLog.Log("Update Trace Offset");
                m_AudioOption.OffsetInMilliSeconds = message.Offset;
                break;
            case UiToVideoMessage.Task.ToggleAudioTrace:
                BtvLog.Log("Toggle Audio Trace");
                audioSignal.Show(message.IsTraceOn);
                break;
            case UiToVideoMessage.Task.UpdateAudioFile:
                BtvLog.Log("Update Trace Audio File");
                m_AudioOption.FileID = message.TraceID;
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
            case BrainWardenToTraceMessage.Task.PlotClicked:
                PlotWasClicked(message.ClickedElectrode);
                break;
            default:
                Debug.LogError("Trace.cs : Id of action to execute does not exist : " + message.TaskToExecute);
                break;
        }
    }

    private void OnVideoToModulesMessage(VideoToModulesMessage message)
    {
        if (message.IsStopped)
        {
            graphSonif.Mute();
            return;
        }
        else
        {
            eegSignal.UpdateDraw((int)message.TimeMilliseconds);
            audioSignal.UpdateDraw((int)message.TimeMilliseconds);
            graphEvent.UpdateEventsOnTrace((int)message.TimeMilliseconds);
            graphSonif.UpdateSonification((int)message.TimeMilliseconds);
        }
    }

    private void OnMontageMessage(MontageMessage message)
    {
        if (message.TaskToExecute == MontageMessage.Task.SelectMontage)
        {
            m_TraceOption.FileHandle = EegFileService.ReturnFirstValidContainer(); // FIXME : keep ID of selected file
            graphLabel.Electrode = m_TraceOption.ElectrodeLabel;
            graphLabel.Description = m_TraceOption.FileHandle.Description;
            UpdateTimeResolution(m_TraceOption.WindowInSeconds);
        }
    }

    public void UpdateWindowState(int state)
    {
        BtvLog.Log("Updating Trace " + TraceId + " Ui State");
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

                PlotWasClicked(null);
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

                PlotWasClicked(GameObject.Find(m_TraceOption.ElectrodeName.ToLower()));
                break;
        }
    }

    private void UpdateTimeResolution(int newPeriod)
    {
        m_TraceOption.WindowInSeconds = newPeriod;
        eegSignal.UpdateHorizontalScale();
        m_AudioOption.WindowInSeconds = newPeriod;
        audioSignal.UpdateHorizontalScale();
        graphGrid.updateGridScale(newPeriod);
    }

    public void UpdateElectrodeById(int newId)
    {
        m_TraceOption.ElectrodeID = newId;
        m_TraceOption.Offset = m_TraceOption.Offset; //update offset, see for autoupdate somewhere ???
        graphLabel.Electrode = m_TraceOption.ElectrodeLabel;
    }

    private void UpdateTracesWidth()
    {
        eegSignal.UpdateLineWidth();
        audioSignal.UpdateLineWidth();
    }

    private void PlotWasClicked(GameObject plot)
    {
        if (m_window.hasFocus)
        {
            if (plot != null)
            {
                int hitID = plot.GetComponent<Site>().ID;
                UpdateElectrodeById(hitID);
            }
            ring.setSelectedPlot(plot);
        }
    }

    private bool IsOver(Vector3 mousePos)
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
        {
            ForceToggleToolbar toggleMessage = new ForceToggleToolbar
            {
                toolbar = "EEG" + (TraceId + 1).ToString()
            };
            Messenger.Default.Send(toggleMessage, MessageContext.ForceToggleToolbar);
        }

        FocusClickElecLabel();

        m_rectTransform.GetWorldCorners(m_worldCorners);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float perCentX = (worldClick.x - m_worldCorners[1].x) / (m_worldCorners[2].x - m_worldCorners[1].x);
        float msClicked = (eegSignal.MostRecentTimeInMilliSecs - (m_TraceOption.WindowInSeconds * 1000)) + (perCentX * (m_TraceOption.WindowInSeconds * 1000));
        if (msClicked >= 0)
        {
            BtvEvent currentEvent = new BtvEvent(0, (int)msClicked, elecOfInterest: m_TraceOption.ElectrodeLabel);
            //eventWasClicked(currentEvent, traceID);
            OpenEventAdd(currentEvent);
        }
    }

    private void FocusClickElecLabel()
    {
        Ray r = new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit))
        {
            if (hit.collider.name.Contains("Electrode_"))
            {
                string[] split = hit.collider.name.Split(new string[] { "Electrode_" }, System.StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 0)
                {
                    bool parseOk = int.TryParse(split[0], out int index);
                    if (parseOk)
                    {
                        ForceToggleToolbar toggleMessage = new ForceToggleToolbar
                        {
                            toolbar = "EEG" + (index + 1).ToString()
                        };
                        Messenger.Default.Send(toggleMessage, MessageContext.ForceToggleToolbar);

                        GameObject moveSiblingPosition = (index == TraceId) ? gameObject : m_handleOtherTrace.gameObject;
                        if (m_window.transform.position == m_handleOtherTrace.transform.position)
                        {
                            moveSiblingPosition.transform.SetAsLastSibling();
                        }
                    }
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
            Transform parent = traceID == 0 ? m_signalWindow1.gameObject.transform : m_signalWindow2.gameObject.transform;
            Event.SecondSiteOfInterest = traceID == 0 ? TracesService.ElectrodeName(1) : TracesService.ElectrodeName(0);

            m_PopUpAddWindow = Instantiate(m_AddEventWindowPrefabs, parent);
            EventInfoAdd infoAdd = m_PopUpAddWindow.GetComponent<EventInfoAdd>();
            infoAdd.Init(Event);
        }
    }

    private void OpenEventDisplay(BtvEvent Event)
    {
        if (m_PopUpDisplayWindow == null)
        {
            Transform parent = traceID == 0 ? m_signalWindow1.gameObject.transform : m_signalWindow2.gameObject.transform;
            m_PopUpDisplayWindow = Instantiate(m_DisplayEventWindowPrefabs, parent);
            EventInfoDisplay infoDisp = m_PopUpDisplayWindow.GetComponent<EventInfoDisplay>();
            infoDisp.init(Event);
        }
    }

    private void OpenEventModify(BtvEvent Event)
    {
        if (m_PopUpEditWindow == null)
        {
            Transform parent = traceID == 0 ? m_signalWindow1.gameObject.transform : m_signalWindow2.gameObject.transform;
            m_PopUpEditWindow = Instantiate(m_EditEventWindowPrefabs, parent);
            EventInfoEdit infoEdit = m_PopUpEditWindow.GetComponent<EventInfoEdit>();
            infoEdit.Init(Event);
        }
    }
}