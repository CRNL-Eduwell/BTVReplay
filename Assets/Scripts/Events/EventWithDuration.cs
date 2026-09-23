using System.Collections.Generic;
using System.IO;
using System.Linq;
using BTV.Services;
using UnityEngine;
using UnityEngine.UI;

public class EventWithDuration : EventTrace
{
    [SerializeField] private Toggle _ShowEvent = null;
    [SerializeField] private Toggle _ShowTimeFrequency = null;
    [SerializeField] private Toggle _NormalizeTimeFrequency = null;
    [SerializeField] private EventTFCursor m_Cursor = null;
    [SerializeField] private EventTfValueDisplay m_Display = null;

    private Color m_Blue = new Color(0.6117f, 0.7058f, 0.7960f);
    private RawImage m_Image = null;
    private TimeFrequencyDataStructure m_TfDataStruct = null;

    private Color[] m_ColorJetMap = null;
    private Texture2D m_TfTexture = null;
    private bool m_HasDataToDisplay = false;
    private float m_begMemory = -1;
    private float m_endMemory = -1;
    private TfTraceOption m_TfTraceOption = null;
    private Session m_Session = null;
    private float Fs_Max_Visu = 0;
    private int LeftTimeMemoryMs = 0, RightTimeMemoryMs = 0;

    private GameObject m_InputFieldWindowPrefabs = null;

    private void Awake()
    {
        m_InputFieldWindowPrefabs = Resources.Load("Prefabs/NormalizeTF", typeof(GameObject)) as GameObject;

        m_Image = transform.GetComponent<RawImage>();
        m_ColorJetMap = TfMapMath.JetColorMap();

        _ShowEvent.onValueChanged.AddListener(ToggleEventView);
        _ShowTimeFrequency.onValueChanged.AddListener(ToggleTimeFrequencyView);
        _NormalizeTimeFrequency.onValueChanged.AddListener(NormalizeTimeFrequency);
        Messenger.Default.Register<UiToTFEventsMessage>(this, OnUiToTFEventsMessage, MessageContext.UiToTFEvents);
        Messenger.Default.Register<TimeFrequencyResultMessage>(this, OnTimeFrequencyResultMessage, MessageContext.TimeFrequencyResultMessage);
    }

    private void OnDestroy()
    {
        // Guard: Initialize (which sets m_TfTraceOption) may never have run; the bare deref
        // used to NRE here and skip the unregisters below it.
        if (m_TfTraceOption != null)
            m_TfTraceOption.PropertyChanged -= OnTimeFrequencyTraceOption_PropertyChanged;
        _ShowEvent.onValueChanged.RemoveAllListeners();
        _ShowTimeFrequency.onValueChanged.RemoveAllListeners();
        _NormalizeTimeFrequency.onValueChanged.RemoveAllListeners();
        Messenger.Default.Unregister(this, MessageContext.UiToTFEvents);
        Messenger.Default.Unregister(this, MessageContext.TimeFrequencyResultMessage);
        if (m_TfTexture != null) Destroy(m_TfTexture);
    }

    public void Initialize(BTV.Data.BtvEvent currentEvent, int winID, Session session)
    {
        base.Init(currentEvent, winID);
        m_Session = session;

        m_TfTraceOption = TimeFrequencyService.GetOptionsFor(m_Session, ParentWindowIndex);
        m_TfTraceOption.PropertyChanged += OnTimeFrequencyTraceOption_PropertyChanged;

        Fs_Max_Visu = TracesService.SamplingFrequency(m_Session, ParentWindowIndex) / 2;
        Fs_Max_Visu = (Fs_Max_Visu / (1000f / m_TfTraceOption.WindowInMilliseconds)) + 1;
    }

    private void ToggleEventView(bool isViewable)
    {
        m_Image.color = new Color(m_Blue.r, m_Blue.g, m_Blue.b, isViewable ? 0.5f : 0f);
    }

    private void ToggleTimeFrequencyView(bool isViewable)
    {
        if (isViewable)
        {
            ProcessCalculationMessage message = new ProcessCalculationMessage
            {
                Task = Calculations.TF,
                EventOfInterest = new BTV.Data.BtvEvent(EventOfInterest),
                TraceIndex = ParentWindowIndex
            };
            Messenger.Default.Send(message, MessageContext.ProcessCalculationMessage);

            m_Image.color = Color.white;
            m_Cursor.ShowCursor = true;
        }
        else
        {
            m_Image.color = new Color(m_Blue.r, m_Blue.g, m_Blue.b, 0f);
            m_Cursor.ShowCursor = false;
            ResetTfOptions();
        }
    }

    private void NormalizeTimeFrequency(bool shoudNormalize)
    {
        BtvLog.Log("Should Normalize " + shoudNormalize);

        if (shoudNormalize)
        {
            NormalizeTF window = SpawnNormalizeTFWindow();
            window.Initialize(m_Session, () =>
            {
                if (TimeFrequencyService.GetBaselineEvent(m_Session) == null)
                {
                    if (window.Baseline == null)
                    {
                        UnityEngine.Debug.LogError("No baseline events selected, normalized tf will not be processed");
                    }
                    else
                    {
                        //window baseline
                        ProcessCalculationMessage message = new ProcessCalculationMessage
                        {
                            Task = Calculations.NormalizedTF,
                            BaselineEvent = new BTV.Data.BtvEvent(window.Baseline),
                            EventOfInterest = new BTV.Data.BtvEvent(EventOfInterest),
                            TraceIndex = ParentWindowIndex
                        };
                        Messenger.Default.Send(message, MessageContext.ProcessCalculationMessage);

                        m_Image.color = Color.white;
                        m_Cursor.ShowCursor = true;
                    }
                }
                else
                {
                    ProcessCalculationMessage message = new ProcessCalculationMessage
                    {
                        Task = Calculations.NormalizedTF,
                        BaselineEvent = new BTV.Data.BtvEvent(TimeFrequencyService.GetBaselineEvent(m_Session)),
                        EventOfInterest = new BTV.Data.BtvEvent(EventOfInterest),
                        TraceIndex = ParentWindowIndex
                    };
                    Messenger.Default.Send(message, MessageContext.ProcessCalculationMessage);

                    m_Image.color = Color.white;
                    m_Cursor.ShowCursor = true;
                }

                window.Close();
            }, () =>
            {
                if (window.Baseline == null)
                {
                    UnityEngine.Debug.LogError("No baseline events selected, baseline event can not be set");
                }
                else
                {
                    TimeFrequencyService.SetBaselineEvent(m_Session, new BTV.Data.BtvEvent(window.Baseline));
                }
            });
        }
        else
        {
            m_Image.color = new Color(m_Blue.r, m_Blue.g, m_Blue.b, 0f);
            m_Cursor.ShowCursor = false;
            ResetTfOptions();
        }
    }

    private NormalizeTF SpawnNormalizeTFWindow()
    {
        GameObject viewGameObject = GameObject.Find("Windows");
        GameObject inputField = Instantiate(m_InputFieldWindowPrefabs, viewGameObject.transform);
        return inputField.GetComponent<NormalizeTF>();
    }

    private void OnTimeFrequencyTraceOption_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Alpha":
                {
                    Color currColor = m_Image.color;
                    currColor.a = m_TfTraceOption.Alpha;
                    m_Image.color = currColor;
                    break;
                }
            case "HighFrequency":
            case "LowFrequency":
                {
                    Fs_Max_Visu = m_TfTraceOption.HighFrequency - m_TfTraceOption.LowFrequency;
                    Fs_Max_Visu = (Fs_Max_Visu / (1000f / m_TfTraceOption.WindowInMilliseconds)) + 1;
                    UpdateTfMap(LeftTimeMemoryMs, RightTimeMemoryMs, true);
                    break;
                }
            case "WindowInMilliseconds":
                {
                    Fs_Max_Visu = m_TfTraceOption.HighFrequency - m_TfTraceOption.LowFrequency;
                    Fs_Max_Visu = (Fs_Max_Visu / (1000f / m_TfTraceOption.WindowInMilliseconds)) + 1;

                    ResetTfOptions();
                    ProcessCalculationMessage message = new ProcessCalculationMessage
                    {
                        Task = Calculations.TF,
                        EventOfInterest = new BTV.Data.BtvEvent(EventOfInterest),
                        TraceIndex = ParentWindowIndex
                    };
                    Messenger.Default.Send(message, MessageContext.ProcessCalculationMessage);

                    m_Image.color = Color.white;
                    m_Cursor.ShowCursor = true;

                    break;
                }
            case "MinValueFactor":
            case "MaxValueFactor":
                {
                    UpdateTfMap(LeftTimeMemoryMs, RightTimeMemoryMs, true);
                    break;
                }
        }
    }

    private void OnUiToTFEventsMessage(UiToTFEventsMessage message)
    {
        if (message.TaskToExecute == UiToTFEventsMessage.Task.ToggleCursorSlave)
        {
            m_Cursor.IsSlaved = message.IsSlaved;
        }
        else if (message.TaskToExecute == UiToTFEventsMessage.Task.UpdateAlpha)
        {
            if (message.ParentWindowIndex != ParentWindowIndex) return;

            m_TfTraceOption.Alpha = message.Alpha;
        }
        else if (message.TaskToExecute == UiToTFEventsMessage.Task.UpdateFrequencyBand)
        {
            if (message.ParentWindowIndex != ParentWindowIndex) return;

            m_TfTraceOption.LowFrequency = message.LowFrequency;
            m_TfTraceOption.HighFrequency = message.HighFrequency;
        }
        else if (message.TaskToExecute == UiToTFEventsMessage.Task.UpdateTfWindow)
        {
            if (message.ParentWindowIndex != ParentWindowIndex) return;

            m_TfTraceOption.WindowInMilliseconds = message.WindowInMs;
        }
        else if (message.TaskToExecute == UiToTFEventsMessage.Task.UpdateAmplitude)
        {
            if (message.ParentWindowIndex != ParentWindowIndex) return;

            m_TfTraceOption.MinValueFactor = message.MinValueFactor;
            m_TfTraceOption.MaxValueFactor = message.MaxValueFactor;
        }
    }

    private void OnTimeFrequencyResultMessage(TimeFrequencyResultMessage message)
    {
        if (message.TraceIndex != ParentWindowIndex) return;
        if (message.EventOfInterest.Code != EventOfInterest.Code) return;
        if (message.EventOfInterest.TimeInMilliSeconds != EventOfInterest.TimeInMilliSeconds) return;
        if (message.EventOfInterest.Duration != EventOfInterest.Duration) return;

        //SetTf Data in Viewer
        BtvLog.Log("OnTimeFrequencyResultMessage : Setting tf ");
        SetTfData(message.TFDataStructure, EventOfInterest);
    }

    public void UpdateTfMap(int LeftTimekInMs, int RightTimeInMs, bool overrideCheck = false)
    {
        LeftTimeMemoryMs = LeftTimekInMs;
        RightTimeMemoryMs = RightTimeInMs;

        if (!m_HasDataToDisplay) return;

        float samplingFreq = TracesService.SamplingFrequency(m_Session, ParentWindowIndex);

        (float beg, float end) = TfMapMath.VisibleSampleRange(LeftTimekInMs, RightTimeInMs, samplingFreq, EventOfInterest.TimeInSeconds, EventOfInterest.Duration);

        int frameSize = TimeFrequencyService.GetFrameSizeFor(m_Session, ParentWindowIndex);
        if (beg != m_begMemory || end != m_endMemory || overrideCheck)
        {
            m_begMemory = beg;
            m_endMemory = end;

            (int begI, int endI) = TfMapMath.FrameRange(beg, end, frameSize);

            bool enterInWindow = (begI == 0 && endI <= 0);
            bool cameOutOfWindow = (begI >= m_TfDataStruct.TimeFrameCount) && (endI >= m_TfDataStruct.TimeFrameCount);
            bool isInsideWindow = (begI >= 0) && (endI <= m_TfDataStruct.TimeFrameCount);
            if (isInsideWindow && !cameOutOfWindow && !enterInWindow)
            {
                // Destroy the previous texture before replacing it; this runs as the playback
                // window shifts, so without it each new Texture2D leaked native memory.
                Texture2D newTexture = EegData2Colors(m_TfDataStruct, begI, endI);
                if (m_TfTexture != null) Destroy(m_TfTexture);
                m_TfTexture = newTexture;
                m_Image.texture = newTexture;
            }
        }
    }

    public void DisplayTfInfo(float xperc, float yperc)
    {
        if (!m_HasDataToDisplay) return;

        int x_index = Mathf.CeilToInt(xperc * (m_TfDataStruct.TimeFrameCount - 1));
        int y_index = Mathf.CeilToInt(yperc * (m_TfDataStruct.FrequencyBinCount - 1));

        float freq = m_TfDataStruct.RequestFrequency(y_index, m_TfTraceOption.LowFrequency, m_TfTraceOption.HighFrequency);
        float tfvalue = m_TfDataStruct.RequestValue(y_index, x_index);

        m_Display.UpdateDisplayInformation(freq, tfvalue);
    }

    private void SetTfData(TimeFrequencyDataStructure data, BTV.Data.BtvEvent btvEvent)
    {
        m_HasDataToDisplay = false;
        m_TfDataStruct = new TimeFrequencyDataStructure(data);
        EventOfInterest = new BTV.Data.BtvEvent(btvEvent);
        m_HasDataToDisplay = true;
    }

    private void ResetTfOptions()
    {
        m_Image.texture = null;
        if (m_TfTexture != null) { Destroy(m_TfTexture); m_TfTexture = null; }
        m_begMemory = -1;
        m_endMemory = -1;
        m_HasDataToDisplay = false;
    }


    private static readonly Color s_NoDataColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private Texture2D EegData2Colors(TimeFrequencyDataStructure eegData, int beg, int end)
    {
        (float minValue, float maxValue) = TfMapMath.ScaleBounds(eegData.TopValue, m_TfTraceOption.MinValueFactor, m_TfTraceOption.MaxValueFactor);

        int lowBinIndex = Mathf.RoundToInt(m_TfTraceOption.LowFrequency / (1000f / m_TfTraceOption.WindowInMilliseconds)) + 1;
        int highBinIndex = Mathf.RoundToInt(m_TfTraceOption.HighFrequency / (1000f / m_TfTraceOption.WindowInMilliseconds)) + 1;

        if (end - beg <= 0 || highBinIndex - lowBinIndex <= 0)
            return new Texture2D(1, 1);

        Texture2D cursor = new Texture2D((end - beg), highBinIndex - lowBinIndex);
        for (int l = lowBinIndex; l < highBinIndex; l++)
        {
            float[] data = eegData.GetFrequencyBinData(l);
            for (int m = beg; m < end; m++)
            {
                // -1 = no defined value (flat z-score baseline): neutral, never a colour that
                // reads as activity.
                int col = TfMapMath.ColorIndex(data[m], minValue, maxValue);
                cursor.SetPixel(m - beg, l - lowBinIndex, col < 0 ? s_NoDataColor : m_ColorJetMap[col]);
            }
        }
        cursor.Apply();

        //Debug texture generated
        //byte[] d = ImageConversion.EncodeToPNG(cursor);
        //File.WriteAllBytes("/Users/florian/Desktop/dd.png", d);

        return cursor;
    }
}
