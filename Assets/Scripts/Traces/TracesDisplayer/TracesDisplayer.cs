using BTV.Data;
using BTV.Services.CalculationService;
using BTV.Services.EegFileService;
using BTV.Services.EventsService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TracesDisplayer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private LayoutElement m_ParentLayoutElement = null;
    [SerializeField]
    private RawImage m_TextureRawImage = null;
    [SerializeField]
    private CustomVideoPlayer m_VideoPlayer = null;
    [SerializeField]
    private LineRenderer m_LineRenderer = null;
    [SerializeField]
    private TextToggle m_ElectrodeLabel = null;
    [SerializeField]
    private TextToggle m_GainLabel = null;
    [SerializeField]
    private TextToggle m_FileLabel = null;
    [SerializeField]
    private Button m_NormalizeData = null;

    private BtvProgram FileHandle = null;
    private BtvChannel Channel = null;

    private float[] m_dataProcessed = null;
    private Vector3[] m_dataArray = null;
    protected RectTransform m_rectTransform = null;

    private bool m_IsBig = false;
    private int m_currentElectrodeID = 0;
    private float m_Gain = 1;
    private int m_ContainerId = 0;
    private float m_WheelSum = 0;

    private Texture2D m_DefaultTexturePrefab = null;
    private Color[] m_TextureColorData;
    private Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 0.20784f);
    private Color darkGrey = new Color(0.20784f, 0.20784f, 0.20784f, 0.20784f);
    private Color orangeSelect = new Color(1.0f, 0.619608f, 0, 1);

    private float m_NumberOfPixelsByPoint = 0.2f;
    private int m_downsamplingFactor = 1;

    private bool m_Normalized = false;
    private float m_BeginSampleNormalize = -1;
    private float m_EndSampleNormalize = -1;

    private void Awake()
    {
        m_rectTransform = gameObject.transform.GetComponent<RectTransform>();

        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
        Messenger.Default.Register<ShortcutMessage>(this, OnShortcutMessage, MessageContext.ShortcutMessage);
        Messenger.Default.Register<TraceDisplayerNormalize>(this, OnTraceDisplayerNormalize, MessageContext.TraceDisplayerNormalize);
    }

    private void Start()
    {
        m_DefaultTexturePrefab = Resources.Load("Pictures/TraceDisplayer", typeof(Texture2D)) as Texture2D;
        m_TextureRawImage.texture = Instantiate(m_DefaultTexturePrefab);
        m_TextureColorData = ((Texture2D)m_TextureRawImage.texture).GetPixels();
    }

    private void OnDestroy()
    {
        m_NormalizeData.onClick.RemoveAllListeners();

        Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
        Messenger.Default.Unregister(this, MessageContext.ShortcutMessage);
        Messenger.Default.Unregister(this, MessageContext.TraceDisplayerNormalize);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if ((eventData.button == PointerEventData.InputButton.Left) && (eventData.clickCount == 2))
        {
            m_IsBig = !m_IsBig;
            UpdateState(m_IsBig);
            m_ParentLayoutElement.minHeight = m_IsBig ? 120 : 30;
        }
    }

    private void OnGUI()
    {
        if (m_VideoPlayer.VideoInterface == null) return;

        bool isOver = RectTransformUtility.RectangleContainsScreenPoint(m_rectTransform, Input.mousePosition, Camera.main);
        if (isOver)
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                Vector2 scrollDelta = Input.mouseScrollDelta;
                if (scrollDelta.y != 0)
                {
                    if (IsAlmostEqual(Mathf.Abs(Event.current.delta.y), Mathf.Abs(scrollDelta.y)))
                    {
                        //BtvLog.Log("ismouse");
                        UpdateTracesParameters(scrollDelta.y > 0 ? true : false);
                    }
                    else
                    {
                        //BtvLog.Log("ispad");
                        m_WheelSum += scrollDelta.y;
                        if (m_WheelSum <= -0.1f)
                        {
                            m_WheelSum = 0;
                            UpdateTracesParameters(false);
                        }
                        else if (m_WheelSum >= 0.1f)
                        {
                            m_WheelSum = 0;
                            UpdateTracesParameters(true);
                        }
                    }
                }
            }


            //Check if mouse is over an event or not, tooltip autohides after a delay
            Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectTransform, Input.mousePosition, Camera.main, out Vector2 localPosition);
            float perc = ((localPosition.x + (0.5f * m_rectTransform.rect.width)) / m_rectTransform.rect.width);
            float mouseTime = perc * m_VideoPlayer.VideoInterface.TotalVideoTime;
            List<BtvEvent> eventsIndexes = EventsService.Events.FindAll(x => x.Duration > 0 && (mouseTime >= x.TimeInMilliSeconds && mouseTime <= x.TimeInMilliSeconds + x.Duration));
            if (eventsIndexes.Count > 0)
            {
                TraceDisplayerPointerMessage message = new TraceDisplayerPointerMessage
                {
                    TaskToExecute = TraceDisplayerPointerMessage.Task.ShowAndUpdate,
                    PointerPosition = new Vector3(worldClick.x, worldClick.y, 0),
                    ShowPointer = true,
                    Code = eventsIndexes[0].Code.ToString(),
                    Description = eventsIndexes[0].Comment.ToString(),
                    DurationTimeMs = eventsIndexes[0].Duration.ToString()
                };
                Messenger.Default.Send(message, MessageContext.TraceDisplayerPointerMessage);
            }
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateHorizontalScale();
        UpdateDraw(m_ParentLayoutElement.minHeight);
    }

    private void OnLoaderMessage(LoaderMessage message)
    {
        if (message.Task == LoaderMessage.LoaderTask.LoadTrace)
        {
            Init();
        }
    }

    private void Init()
    {
        m_NormalizeData.onClick.AddListener(OnNormalizeButtonClick);

        FileHandle = EegFileService.ReturnFirstValidContainer();
        m_currentElectrodeID = 0;
        Channel = FileHandle.Channels[m_currentElectrodeID];
        //==
        m_downsamplingFactor = Mathf.Max(1, Mathf.CeilToInt(m_NumberOfPixelsByPoint * (float)Channel.NumberOfSample / (m_rectTransform.rect.width)));
        //m_downsamplingFactor = 1; => to force full resolution , warning it's very slow on midlong files
        //==
        m_dataArray = new Vector3[Channel.NumberOfSample / m_downsamplingFactor];
        m_LineRenderer.positionCount = m_dataArray.Length;
        m_LineRenderer.startWidth = 1f;
        m_LineRenderer.endWidth = 1f;

        m_dataProcessed = new float[m_dataArray.Length];
        //==
        UpdateElectrode(m_currentElectrodeID);
        m_GainLabel.Label = m_Gain.ToString();
        UpdateFile(-1);
        m_NormalizeData.transform.GetChild(0).GetComponent<Text>().text = "Norm";
        //==
        UpdateState(m_IsBig);
        //==
        UpdateHorizontalScale();
        //==
        GetDataToDisplay();
        UpdateDraw(m_ParentLayoutElement.minHeight);
    }

    private void OnShortcutMessage(ShortcutMessage message)
    {
        if (GetType() == message.RecipientType)
        {
            if (message.Action == ShortcutActions.ChangeOption)
            {
                if (message.Parameter == ShortcutActionsParameters.Left)
                {
                    if (m_GainLabel.HasFocus)
                    {
                        m_ElectrodeLabel.HasFocus = true;
                        UpdateDraw(m_ParentLayoutElement.minHeight);
                        return;
                    }
                    else if (m_FileLabel.HasFocus)
                    {
                        m_GainLabel.HasFocus = true;
                        UpdateDraw(m_ParentLayoutElement.minHeight);
                        return;
                    }
                }
                else if (message.Parameter == ShortcutActionsParameters.Right)
                {
                    if (m_ElectrodeLabel.HasFocus)
                    {
                        m_GainLabel.HasFocus = true;
                        UpdateDraw(m_ParentLayoutElement.minHeight);
                        return;
                    }
                    else if (m_GainLabel.HasFocus)
                    {
                        m_FileLabel.HasFocus = true;
                        UpdateDraw(m_ParentLayoutElement.minHeight);
                        return;
                    }
                }
            }
            else if (message.Action == ShortcutActions.UpdateOptionValue)
            {
                if (message.Parameter == ShortcutActionsParameters.Up)
                    UpdateTracesParameters(true);
                else if (message.Parameter == ShortcutActionsParameters.Down)
                    UpdateTracesParameters(false);
            }
            else if (message.Action == ShortcutActions.Focus)
            {
                m_IsBig = !m_IsBig;
                UpdateState(m_IsBig);
                m_ElectrodeLabel.HasFocus = m_IsBig;
                m_ParentLayoutElement.minHeight = m_IsBig ? 120 : 30;
            }
        }
    }

    private void OnTraceDisplayerNormalize(TraceDisplayerNormalize message)
    {
        switch (message.TaskToExecute)
        {
            case TraceDisplayerNormalize.Task.Reset:
                {
                    BtvLog.Log("Reseting Data");
                    break;
                }
            case TraceDisplayerNormalize.Task.Normalize:
                {
                    BtvLog.Log("Normalizing Data");
                    if (message.EndTimeBaseline * FileHandle.Frequency.RawValue > Channel.NumberOfSample)
                    {
                        ApplicationState.displayMessage("Can not normalize data", "NOK", "You can not normlize data using an event that finishes after the end of the file");
                        return;
                    }

                    m_Normalized = true;
                    m_NormalizeData.transform.GetChild(0).GetComponent<Text>().color = orangeSelect;
                    m_BeginSampleNormalize = message.BeginTimeBaseline * FileHandle.Frequency.RawValue;
                    m_EndSampleNormalize = message.EndTimeBaseline * FileHandle.Frequency.RawValue;
                    GetDataToDisplay();
                    UpdateDraw(m_ParentLayoutElement.minHeight);
                    break;
                }
            default:
                {
                    UnityEngine.Debug.LogError("TraceDisplayer.cs : OnTraceDisplayerNormalize TaskToExecute value not recognized => " + message.TaskToExecute.ToString());
                    break;
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

    private void OnNormalizeButtonClick()
    {
        if (!m_Normalized)
        {
            ShowWindowMessage mess = new ShowWindowMessage
            {
                WindowName = "NormalizeWindow"
            };

            Messenger.Default.Send(mess, MessageContext.ShowWindowMessage);
        }
        else
        {
            m_Normalized = false;
            m_NormalizeData.transform.GetChild(0).GetComponent<Text>().color = Color.white;
            GetDataToDisplay();
            UpdateDraw(m_ParentLayoutElement.minHeight);
        }
    }

    private void GetDataToDisplay()
    {
        if (m_Normalized)
        {
            float[] normData = Channel.GetBaselineNormalizedValues((int)m_BeginSampleNormalize, (int)m_EndSampleNormalize);
            for (int i = 0; i < m_dataProcessed.Length; i++)
            {
                m_dataProcessed[i] = (normData[i * m_downsamplingFactor] - 0.5f) * 2;
            }
        }
        else
        {
            for (int i = 0; i < m_dataProcessed.Length; i++)
            {
                m_dataProcessed[i] = Channel.GetNormalizedSample(i * m_downsamplingFactor, true);
            }
        }
    }

    private void UpdateState(bool isBig)
    {
        m_ElectrodeLabel.IsVisible = isBig;
        m_GainLabel.IsVisible = isBig;
        m_FileLabel.IsVisible = isBig;
        m_NormalizeData.gameObject.SetActive(isBig);

        if (isBig == false)
        {
            m_ElectrodeLabel.HasFocus = false;
            m_GainLabel.HasFocus = false;
            m_FileLabel.HasFocus = false;
        }
    }

    private void UpdateHorizontalScale()
    {
        if (m_rectTransform == null) return;
        if (m_dataArray == null) return;

        float widthOfGameObject = m_rectTransform.rect.width;
        float horizontalScale = widthOfGameObject / m_dataArray.Length;
        for (int i = 0; i < m_dataArray.Length; i++)
        {
            m_dataArray[i].x = ((-widthOfGameObject / 2) + 1) + i * horizontalScale;
        }
        m_LineRenderer.SetPositions(m_dataArray);
    }

    private void UpdateTracesParameters(bool isUp)
    {
        if (m_ElectrodeLabel.HasFocus)
        {
            int newId = isUp ? m_currentElectrodeID + 1 : m_currentElectrodeID - 1;
            UpdateElectrode(newId);
        }
        else if (m_GainLabel.HasFocus)
        {
            m_Gain = isUp ? m_Gain + 0.25f : m_Gain - 0.25f;
            m_GainLabel.Label = m_Gain.ToString();
        }
        else if (m_FileLabel.HasFocus)
        {
            UpdateFile(isUp ? 1 : -1);
        }
        UpdateDraw(m_ParentLayoutElement.minHeight);
    }

    private void UpdateDraw(float height)
    {
        if (m_LineRenderer == null) return;
        if (m_dataArray == null) return;
        if (Channel == null) return;

        float limitVal = height / 2;
        for (int i = 0; i < m_dataProcessed.Length; i++)
        {
            float value = m_Gain * m_dataProcessed[i] * limitVal;
            if (value >= -limitVal && value <= limitVal)
            {
                m_dataArray[i].y = value;
            }
            else
            {
                if (value >= 0)
                    m_dataArray[i].y = limitVal;
                else
                    m_dataArray[i].y = -limitVal;
            }
        }
        m_LineRenderer.SetPositions(m_dataArray);
    }

    private void UpdateElectrode(int Index)
    {
        if (Index > -1 && Index < FileHandle.NumberOfElectrodes)
        {
            // Navigating to another electrode dismisses the 1D correlation coloring on the
            // brain: it was computed against a site the user is no longer inspecting.
            if (Index != m_currentElectrodeID)
                EventsService.ClearCorrelations();

            m_currentElectrodeID = Index;
            Channel = FileHandle.Channels[m_currentElectrodeID];
            m_ElectrodeLabel.Label = Channel.Label;
            GetDataToDisplay();
        }
    }

    private void UpdateFile(float yDelta)
    {
        if (yDelta < 0)
        {
            if (EegFileService.IsFileIdValid(m_ContainerId - 1))
            {
                m_ContainerId -= 1;
            }
        }
        else
        {
            if (EegFileService.IsFileIdValid(m_ContainerId + 1))
            {
                m_ContainerId += 1;
            }
        }
        FileHandle = EegFileService.ChangeContainerHandle(FileHandle, m_ContainerId);
        Channel = FileHandle.Channels[m_currentElectrodeID];

        m_downsamplingFactor = Mathf.Max(1, Mathf.CeilToInt(m_NumberOfPixelsByPoint * (float)Channel.NumberOfSample / (m_rectTransform.rect.width)));
        m_dataArray = new Vector3[Channel.NumberOfSample / m_downsamplingFactor];
        m_dataProcessed = new float[m_dataArray.Length];
        m_LineRenderer.positionCount = m_dataArray.Length;
        UpdateHorizontalScale();

        m_FileLabel.Label = FileHandle.Description;
        GetDataToDisplay();
    }

    //==Event Display
    public void AddEvents(List<BtvEvent> btvEvents)
    {
        int eventsCount = btvEvents.Count;
        for (int i = 0; i < eventsCount; i++)
        {
            AddEvent(btvEvents[i]);
        }
    }

    public void AddEvent(BtvEvent btvEvent)
    {
        if (m_VideoPlayer.VideoInterface.TotalVideoTime <= 0)
            return;

        Texture2D texture = ((Texture2D)m_TextureRawImage.texture);
        int pixelID = PixelForTime(btvEvent.TimeInMilliSeconds, texture);

        if (btvEvent.Duration > 0)
        {
            if (btvEvent.Duration > 1000)
            {
                int pixelIDDuration = PixelForTime(btvEvent.TimeInMilliSeconds + btvEvent.Duration, texture);
                for (int i = 0; i < texture.height; i++)
                {
                    for (int j = 0; j < pixelIDDuration - pixelID; j++)
                        m_TextureColorData[(pixelID + j) + (i * texture.width)] = hardBlue;
                }
            }
            else //if duration < 1000ms, too thin to see the red streak on the scrollbar
            {
                for (int i = 0; i < texture.height; i++)
                    m_TextureColorData[pixelID + (i * texture.width)] = hardBlue;
            }
        }

        texture.SetPixels(m_TextureColorData);
        texture.Apply();
    }

    public void RemoveEvent(BtvEvent btvEvent)
    {
        if (m_VideoPlayer.VideoInterface.TotalVideoTime <= 0)
            return;

        Texture2D texture = ((Texture2D)m_TextureRawImage.texture);
        int pixelID = PixelForTime(btvEvent.TimeInMilliSeconds, texture);

        if (btvEvent.Duration > 0)
        {
            if (btvEvent.Duration > 1000)
            {
                int pixelIDDuration = PixelForTime(btvEvent.TimeInMilliSeconds + btvEvent.Duration, texture);
                for (int i = 0; i < texture.height; i++)
                {
                    for (int j = 0; j < pixelIDDuration - pixelID; j++)
                        m_TextureColorData[(pixelID + j) + (i * texture.width)] = darkGrey;
                }
            }
            else //if duration < 1000ms, too thin to see the red streak on the scrollbar
            {
                for (int i = 0; i < texture.height; i++)
                    m_TextureColorData[pixelID + (i * texture.width)] = darkGrey;
            }
        }
        texture.SetPixels(m_TextureColorData);
        texture.Apply();
    }

    // Events can sit outside the video timeline (an event past the end of the recording, or no
    // video prepared yet): clamp to the texture bounds so painting can never throw. An exception
    // here aborts the caller's add/delete flow halfway through and desyncs the event lists kept
    // by the other modules.
    private int PixelForTime(float timeInMilliSeconds, Texture2D texture)
    {
        float perC = timeInMilliSeconds / m_VideoPlayer.VideoInterface.TotalVideoTime;
        return Mathf.Clamp((int)(perC * texture.width), 0, texture.width - 1);
    }

    public void RemoveAllEvents()
    {
        Texture2D texture = ((Texture2D)m_TextureRawImage.texture);

        int textureSize = m_TextureColorData.Length;
        for (int i = 0; i < textureSize; i++)
            m_TextureColorData[i] = darkGrey;

        texture.SetPixels(m_TextureColorData);
        texture.Apply();
    }
}