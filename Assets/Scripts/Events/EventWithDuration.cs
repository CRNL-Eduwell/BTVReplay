using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EventWithDuration : EventTrace
{
    [SerializeField] private Toggle _ShowEvent = null;
    [SerializeField] private Toggle _ShowTimeFrequency = null;
    [SerializeField] private EventTFCursor m_Cursor = null;

    private Color m_Blue = new Color(0.6117f, 0.7058f, 0.7960f);
    private RawImage m_Image = null;
    private float[][] m_TfData = null;
    private Color[] m_ColorJetMap = null;
    private bool m_HasDataToDisplay = false;
    private float m_begMemory = -1;
    private float m_endMemory = -1;
    private TfTraceOption m_TfTraceOption = null;
    private float Fs_Max_Visu = 0;
    private int LeftTimeMemoryMs = 0, RightTimeMemoryMs = 0;

    private void Awake()
    {
        m_Image = transform.GetComponent<RawImage>();
        m_ColorJetMap = DefineColorMap();

        _ShowEvent.onValueChanged.AddListener(ToggleEventView);
        _ShowTimeFrequency.onValueChanged.AddListener(ToggleTimeFrequencyView);

        Messenger.Default.Register<UiToTFEventsMessage>(this, OnUiToTFEventsMessage, MessageContext.UiToTFEvents);
        Messenger.Default.Register<TimeFrequencyResultMessage>(this, OnTimeFrequencyResultMessage, MessageContext.TimeFrequencyResultMessage);
    }

    private void OnDestroy()
    {
        m_TfTraceOption.PropertyChanged -= OnTimeFrequencyTraceOption_PropertyChanged;
        _ShowEvent.onValueChanged.RemoveAllListeners();
        _ShowTimeFrequency.onValueChanged.RemoveAllListeners();
        Messenger.Default.Unregister(this, MessageContext.UiToTFEvents);
        Messenger.Default.Unregister(this, MessageContext.TimeFrequencyResultMessage);
    }

    public void Initialize(BTV.Data.BtvEvent currentEvent, int winID)
    {
        base.Init(currentEvent, winID);

        Fs_Max_Visu = TracesService.SamplingFrequency(ParentWindowIndex) / 2;
        m_TfTraceOption = TimeFrequencyService.GetOptionsFor(ParentWindowIndex);
        m_TfTraceOption.PropertyChanged += OnTimeFrequencyTraceOption_PropertyChanged;
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
            case "FrequencySlider":
                {
                    float samplingFreq = TracesService.SamplingFrequency(ParentWindowIndex);
                    float Fs_Max = samplingFreq / 2;
                    Fs_Max_Visu = m_TfTraceOption.FrequencySlider * Fs_Max;
                    UpdateTfMap(LeftTimeMemoryMs, RightTimeMemoryMs, true);
                    break;
                }
        }
    }

    private void OnUiToTFEventsMessage(UiToTFEventsMessage message)
    {
        if (message.TaskToExecute == 0)
        {
            m_Cursor.IsSlaved = message.IsSlaved;
        }
        else if (message.TaskToExecute == 1) //alpha => 2 will be frequency
        {
            if (message.ParentWindowIndex != ParentWindowIndex) return;

            m_TfTraceOption.Alpha = message.Alpha;
        }
        else if (message.TaskToExecute == 2)
        {
            if (message.ParentWindowIndex != ParentWindowIndex) return;

            m_TfTraceOption.FrequencySlider = message.FrequencySlider;
        }
    }

    private void OnTimeFrequencyResultMessage(TimeFrequencyResultMessage message)
    {
        if (message.TraceIndex != ParentWindowIndex) return;
        if (message.EventOfInterest.Code != EventOfInterest.Code) return;
        if (message.EventOfInterest.TimeInMilliSeconds != EventOfInterest.TimeInMilliSeconds) return;
        if (message.EventOfInterest.Duration != EventOfInterest.Duration) return;

        //SetTf Data in Viewer
        UnityEngine.Debug.Log("OnTimeFrequencyResultMessage : Setting tf ");
        SetTfData(message.TFData, EventOfInterest);
    }

    public void UpdateTfMap(int LeftTimekInMs, int RightTimeInMs, bool overrideCheck = false)
    {
        LeftTimeMemoryMs = LeftTimekInMs;
        RightTimeMemoryMs = RightTimeInMs;

        if (!m_HasDataToDisplay) return;

        float samplingFreq = TracesService.SamplingFrequency(ParentWindowIndex);

        float leftClockInSample = (LeftTimekInMs * samplingFreq) / 1000;
        float rightClockInSample = (RightTimeInMs * samplingFreq) / 1000;
        //
        float begInSample = EventOfInterest.TimeInSeconds * samplingFreq;
        float endInSample = begInSample + ((EventOfInterest.Duration * samplingFreq) / 1000);
        //
        float beg = leftClockInSample - begInSample < 0 ? 0 : leftClockInSample - begInSample;
        float end = rightClockInSample - endInSample < 0 ? (rightClockInSample - begInSample) : (endInSample - begInSample);

        if (beg != m_begMemory || end != m_endMemory || overrideCheck)
        {
            m_begMemory = beg;
            m_endMemory = end;

            int begI = Mathf.RoundToInt((beg / 512) * (512 / 256));
            int endI = Mathf.RoundToInt((end / 512) * (512 / 256)) - 1;

            bool enterInWindow = (begI == 0 && endI <= 0);
            bool cameOutOfWindow = (begI >= m_TfData.Length) && (endI >= m_TfData.Length);
            bool isInsideWindow = (begI >= 0) && (endI <= m_TfData.Length);
            if (isInsideWindow && !cameOutOfWindow && !enterInWindow)
            {
                UnityEngine.Debug.Log(Mathf.RoundToInt(Fs_Max_Visu));
                m_Image.texture = EegData2Colors(m_TfData, begI, endI, m_ColorJetMap, Mathf.RoundToInt(Fs_Max_Visu));
            }
        }
    }

    private void SetTfData(float[][] data, BTV.Data.BtvEvent btvEvent)
    {
        m_HasDataToDisplay = false;
        UnityEngine.Debug.Log("Dim 0 : " + data.Length);
        UnityEngine.Debug.Log("Dim 1 : " + data[0].Length);

        m_TfData = null;
        m_TfData = new float[data.Length][];
        for (int i = 0; i < data.Length; i++)
        {
            m_TfData[i] = new float[data[i].Length];
            for (int j = 0; j < data[i].Length; j++)
            {
                m_TfData[i][j] = data[i][j];
            }
        }

        EventOfInterest = new BTV.Data.BtvEvent(btvEvent);
        m_HasDataToDisplay = true;
    }

    private void ResetTfOptions()
    {
        m_Image.texture = null;
        m_begMemory = -1;
        m_endMemory = -1;
        m_HasDataToDisplay = false;
    }

    private Color[] DefineColorMap()
    {
        Color[] colorMap = new Color[512];

        int compteur = 0;
        for (int i = 0; i < 57; i++)
        {
            float r = 0;
            float g = 0;
            float b = 143.4375f + (i * 1.9649f);
            colorMap[i] = new Color(r, g, b);
        }

        compteur = 57;
        for (int i = 0; i < 130; i++)
        {
            float r = 0;
            float g = 0.4366f + (i * 1.9649f);
            float b = 255;
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        compteur = 187;
        for (int i = 0; i < 130; i++)
        {
            float r = 0.8733f + (i * 1.9649f);
            float g = 255;
            float b = 254.1267f - (i * 1.9649f);
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        compteur = 317;
        for (int i = 0; i < 130; i++)
        {
            float r = 255;
            float g = 253.6901f - (i * 1.9649f);
            float b = 0;
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        compteur = 447;
        for (int i = 0; i < 65; i++)
        {
            float r = 253.2534f - (i * 1.9649f);
            float g = 0;
            float b = 0;
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        return colorMap;
    }

    private Texture2D EegData2Colors(float[][] eegData, Color[] colormap)
    {
        float maxValue = 256;
        float minValue = 0;

        Texture2D cursor = new Texture2D(eegData.Length, eegData[0].Length);
        for (int l = 0; l < eegData[0].Length; l++)
        {
            for (int m = 0; m < eegData.Length; m++)
            {
                float r = (eegData[m][l] - minValue) / (maxValue - minValue);

                int col = Mathf.RoundToInt(0 + (511 * r));
                if (col < 0)
                    col = 0;
                else if (col > 511)
                    col = 511;

                cursor.SetPixel(m, l, colormap[col]);
            }
        }
        cursor.Apply();

        ////Debug texture generated
        //byte[] d = ImageConversion.EncodeToPNG(cursor);
        //File.WriteAllBytes("/Users/florian/Desktop/dd.png", d);

        return cursor;
    }

    private Texture2D EegData2Colors(float[][] eegData, int beg, int end, Color[] colormap, int freqMax = -1)
    {
        float maxValue = 256;
        float minValue = 0;

        int test = freqMax == -1 ? eegData[0].Length : freqMax;
        UnityEngine.Debug.Log("test " + test);
        Texture2D cursor = new Texture2D((end - beg), test);
        for (int l = 0; l < test; l++) //x
        {
            int count = 0;
            for (int m = beg; m < end; m++) //y
            {
                float r = (eegData[m][l] - minValue) / (maxValue - minValue);

                int col = Mathf.RoundToInt(0 + (511 * r));
                if (col < 0)
                    col = 0;
                else if (col > 511)
                    col = 511;

                cursor.SetPixel(count, l, colormap[col]);
                count++;
            }
        }
        cursor.Apply();

        //Debug texture generated
        //byte[] d = ImageConversion.EncodeToPNG(cursor);
        //File.WriteAllBytes("/Users/florian/Desktop/dd.png", d);

        return cursor;
    }
}
