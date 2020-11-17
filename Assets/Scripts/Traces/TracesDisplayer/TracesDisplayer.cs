using BTV.Data;
using BTV.Services.CalculationService;
using BTV.Services.EegFileService;
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

    float MaxValue { get; set; } = 150;
    float MinValue { get; set; } = 50;

    private BtvProgram FileHandle = null;
    private BtvChannel Channel = null;

    private Vector3[] m_dataArray = null;
    protected RectTransform m_rectTransform = null;

    private bool m_IsBig = false;
    private int m_currentElectrodeID = 0;
    private float m_Gain = 1;
    private int m_ContainerId = 0;
    private float m_Timer = 0.0f;
    private float m_WheelSum = 0;

    private Texture2D m_DefaultTexturePrefab = null;
    private Color[] m_TextureColorData;
    private Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 0.20784f);
    private Color darkGrey = new Color(0.20784f, 0.20784f, 0.20784f, 0.20784f);
    private List<BtvEvent> m_events = new List<BtvEvent>();


    private void Awake()
    {
        m_rectTransform = gameObject.transform.GetComponent<RectTransform>();

        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
    }

    private void Start()
    {
        m_DefaultTexturePrefab = Resources.Load("Pictures/TraceDisplayer", typeof(Texture2D)) as Texture2D;
        m_TextureRawImage.texture = Instantiate(m_DefaultTexturePrefab);
        m_TextureColorData = ((Texture2D)m_TextureRawImage.texture).GetPixels();
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
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
                        //UnityEngine.Debug.Log("ismouse");
                        UpdateTracesParameters(scrollDelta.y > 0 ? true : false);
                    }
                    else
                    {
                        //UnityEngine.Debug.Log("ispad");
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

    private void Update()
    {
        m_Timer += Time.deltaTime;

        //If OnGui Management of the difference wheel / trackpad cause issue, put that back
        //and delete gui function
        //bool isOver = RectTransformUtility.RectangleContainsScreenPoint(m_rectTransform, Input.mousePosition, Camera.main);
        //if (isOver)
        //{
        //    float yDeltaScroll = Input.mouseScrollDelta.y;
        //    if (yDeltaScroll != 0 && m_Timer >= 0.2f)
        //    {
        //        m_Timer = 0;
        //        UpdateTracesParameters(yDeltaScroll > 0 ? true : false);
        //    }
        //}

        KeyboardActions();
    }

    private void KeyboardActions()
    {
        if ((Input.GetKey(KeyCode.UpArrow) && m_Timer >= 0.2f) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_Timer = 0;
            UpdateTracesParameters(true);
        }
        else if ((Input.GetKey(KeyCode.DownArrow) && m_Timer >= 0.2f) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            m_Timer = 0;
            UpdateTracesParameters(false);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
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
        else if (Input.GetKeyDown(KeyCode.RightArrow))
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

    private void Init()
    {
        FileHandle = EegFileService.ReturnFirstValidContainer();
        m_currentElectrodeID = 0;
        Channel = FileHandle.Channels[m_currentElectrodeID];
        //==
        m_dataArray = new Vector3[Channel.NumberOfSample];
        m_LineRenderer.positionCount = Channel.NumberOfSample;
        m_LineRenderer.startWidth = 1f;
        m_LineRenderer.endWidth = 1f;
        //==
        UpdateElectrode(m_currentElectrodeID);
        m_GainLabel.Label = m_Gain.ToString();
        UpdateFile(-1);
        //==
        UpdateState(m_IsBig);
        //==
        UpdateHorizontalScale();
        UpdateDraw(m_ParentLayoutElement.minHeight);
    }

    private void UpdateState(bool isBig)
    {
        m_ElectrodeLabel.IsVisible = isBig;
        m_GainLabel.IsVisible = isBig;
        m_FileLabel.IsVisible = isBig;
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

    private void UpdateDraw(float height)
    {
        if (m_LineRenderer == null) return;
        if (m_dataArray == null) return;
        if (Channel == null) return;
        
        float limitVal = height / 2;

        for (int i = 0; i < m_dataArray.Length; i++)
        {
            float value = m_Gain * (Channel.GetSample(i, true) / (MaxValue - MinValue)) * limitVal;
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
            m_currentElectrodeID = Index;
            Channel = FileHandle.Channels[m_currentElectrodeID];
            m_ElectrodeLabel.Label = Channel.Label;
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
        m_FileLabel.Label = FileHandle.Description;
    }

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
        Texture2D texture = ((Texture2D)m_TextureRawImage.texture);
        float perC = ((btvEvent.TimeInMilliSeconds / m_VideoPlayer.VideoInterface.TotalVideoTime));// * 1000);
        int pixelID = (int)(perC * texture.width);

        if (btvEvent.Duration > 0)
        {
            if (btvEvent.Duration > 1000)
            {
                float perCDuration = ((btvEvent.TimeInMilliSeconds + btvEvent.Duration) / m_VideoPlayer.VideoInterface.TotalVideoTime);// * 1000;
                int pixelIDDuration = (int)(perCDuration * texture.width);
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
        Texture2D texture = ((Texture2D)m_TextureRawImage.texture);
        float perC = (btvEvent.TimeInMilliSeconds / m_VideoPlayer.VideoInterface.TotalVideoTime);// * 1000);
        int pixelID = (int)(perC * texture.width);

        if (btvEvent.Duration > 0)
        {
            if (btvEvent.Duration > 1000)
            {
                float perCDuration = ((btvEvent.TimeInMilliSeconds + btvEvent.Duration) / m_VideoPlayer.VideoInterface.TotalVideoTime);// * 1000;
                int pixelIDDuration = (int)(perCDuration * texture.width);
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