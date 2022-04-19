using BTV.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EegSignal : MonoBehaviour
{
    public int MostRecentTimeInMilliSecs { get; private set; } = 0;
    public float MostRecentValueInPercentOfTrace { get { return m_dataArray[m_Option.NumberOfPoint - 1].y / (2 * m_LimitValue); } }

    [SerializeField] private LineRenderer _LineRenderer = null;
    
    private RectTransform m_ParentRectTransform = null;
    private Vector3[] m_dataArray;
    private float m_WidthOfGameObject = 0.0f;
    private float m_HorizontalScale = 0.0f;
    private float m_LimitValue = 0.0f;
    private float m_m_offsetCoefficient = 0.0f;

    private TraceOption m_Option = null;
    private bool m_initDone = false;
    private BtvChannel m_Channel = null;

    private void Awake()
    {
        m_ParentRectTransform = gameObject.transform.parent.GetComponent<RectTransform>();
    }

    public void Initialize(int electrodeID, TraceOption option)
    {
        m_Option = option;
        m_Option.ElectrodeID = electrodeID;
        m_Channel = m_Option.FileHandle.Channels[m_Option.ElectrodeID];

        m_Option.PropertyChanged += OnTraceOptionPropertyChanged;

        m_dataArray = new Vector3[m_Option.NumberOfPoint];
        _LineRenderer.positionCount = m_Option.NumberOfPoint;
        _LineRenderer.sortingOrder = -1;
        _LineRenderer.startWidth = m_Option.LineWidth;
        _LineRenderer.endWidth = m_Option.LineWidth;

        UpdateHorizontalScale();

        m_initDone = true;
    }

    private void OnDestroy()
    {
        if (m_Option != null) m_Option.PropertyChanged -= OnTraceOptionPropertyChanged;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (m_ParentRectTransform == null) return;
        if (!m_initDone) return;
        
        UpdateHorizontalScale();
    }

    private void OnTraceOptionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Gain":
                {

                    break;
                }
            case "Offset":
                {
                    m_m_offsetCoefficient = (m_Option.Offset / 10) * m_Channel.MaxValue;
                    break;
                }
            case "FileHandle":
                {
                    m_Channel = m_Option.FileHandle.Channels[m_Option.ElectrodeID];
                    break;
                }
            case "ElectrodeID":
                {
                    m_Channel = m_Option.FileHandle.Channels[m_Option.ElectrodeID];
                    break;
                }
            case "WindowInSeconds": //NumberOfPoint
                {
                    //This is old UpdateTimeResolution()
                    m_dataArray = new Vector3[m_Option.NumberOfPoint];
                    _LineRenderer.positionCount = m_Option.NumberOfPoint;
                    _LineRenderer.sortingOrder = -1;
                    break;
                }
            case "Color":
                {
                    _LineRenderer.startColor = m_Option.Color;
                    _LineRenderer.endColor = m_Option.Color;
                    break;
                }

        }
    }

    public void UpdateHorizontalScale()
    {
        m_WidthOfGameObject = m_ParentRectTransform.rect.width - 10;
        m_HorizontalScale = m_WidthOfGameObject / m_dataArray.Length;
        for (int i = 0; i < m_dataArray.Length; i++)
        {
            m_dataArray[i].x = ((-m_WidthOfGameObject / 2) + 1) + i * m_HorizontalScale;
            m_dataArray[i].y = 0;
        }
        _LineRenderer.SetPositions(m_dataArray);
    }

    public void UpdateDraw(int milliSecToLook)
    {
        MostRecentTimeInMilliSecs = milliSecToLook;
        //int mostRecentSample = (int)(milliSecToLook * ((float)m_Option.SamplingFrequency / 1000));
        int mostRecentSample = Mathf.RoundToInt(milliSecToLook * ((float)m_Option.SamplingFrequency / 1000));
        int posInArray = mostRecentSample - m_Option.NumberOfPoint;
        
        m_LimitValue = (m_ParentRectTransform.rect.height - 6.5f) / 2;
        for (int i = 0; i < m_Option.NumberOfPoint; i++)
        {
            if (i + posInArray >= 0)
            {
                float eegValue = m_Channel.GetSample(i + posInArray, true);
                float value = m_Option.Gain * eegValue + m_m_offsetCoefficient;
                if (value >= -m_LimitValue && value <= m_LimitValue)
                {
                    m_dataArray[i].y = value;
                }
                else
                {
                    if (value >= 0)
                        m_dataArray[i].y = m_LimitValue;
                    else
                        m_dataArray[i].y = -m_LimitValue;
                }
            }
            else
            {
                m_dataArray[i].y = 0;
            }
        }
        _LineRenderer.SetPositions(m_dataArray);
    }

    public void UpdateLineWidth()
    {
        float width = _LineRenderer.startWidth;
        if (width + 1 <= 4)
            width += 1;
        else
            width = 2f;

        _LineRenderer.startWidth = width;
        _LineRenderer.endWidth = width;
    }
}
