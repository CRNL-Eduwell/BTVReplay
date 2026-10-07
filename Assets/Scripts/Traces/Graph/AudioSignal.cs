using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;

public class AudioSignal : MonoBehaviour
{
    [SerializeField] private LineRenderer _LineRenderer = null;

    private RectTransform m_ParentRectTransform = null;
    private Vector3[] m_dataArray;
    private float[] m_Window;
    private float m_WidthOfGameObject = 0.0f;
    private float m_HorizontalScale = 0.0f;
    private float m_LimitValue = 0.0f;

    private AudioTraceOption m_Option = null;
    private bool m_initDone = false;
    private BtvChannel m_Channel = null;

    private void Awake()
    {
        m_ParentRectTransform = gameObject.transform.parent.GetComponent<RectTransform>();
    }

    public void Initialize(int electrodeID, AudioTraceOption option)
    {
        m_Option = option;
        m_Option.FileID = electrodeID;
        if (m_Option.FileHandle != null && m_Option.FileID > -1)
        {
            m_Channel = m_Option.FileHandle.Channels[m_Option.FileID];

            m_dataArray = new Vector3[m_Option.NumberOfPoint];
            _LineRenderer.positionCount = m_Option.NumberOfPoint;
            _LineRenderer.sortingOrder = -1;
            _LineRenderer.startWidth = m_Option.LineWidth;
            _LineRenderer.endWidth = m_Option.LineWidth;

            UpdateHorizontalScale();
        }
        m_Option.PropertyChanged += OnAudioTraceOptionPropertyChanged;

        m_initDone = true;
    }

    private void OnDestroy()
    {
        if (m_Option != null) m_Option.PropertyChanged -= OnAudioTraceOptionPropertyChanged;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (m_ParentRectTransform == null) return;
        if (!m_initDone) return;

        UpdateHorizontalScale();
    }

    private void OnAudioTraceOptionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Gain":
                {

                    break;
                }
            case "OffsetInMilliSeconds":
                {

                    break;
                }
            case "FileHandle":
                {
                    if (m_Option.FileHandle != null && m_Option.FileID > -1)
                    {
                        m_Channel = m_Option.FileHandle.Channels[m_Option.FileID];

                        m_dataArray = new Vector3[m_Option.NumberOfPoint];
                        _LineRenderer.positionCount = m_Option.NumberOfPoint;
                        _LineRenderer.sortingOrder = -1;
                        _LineRenderer.startWidth = m_Option.LineWidth;
                        _LineRenderer.endWidth = m_Option.LineWidth;

                        UpdateHorizontalScale();
                    }
                    break;
                }
            case "WindowInSeconds":
                {

                    break;
                }
            case "NumberOfPoint":
                {
                    m_dataArray = new Vector3[m_Option.NumberOfPoint];
                    _LineRenderer.positionCount = m_Option.NumberOfPoint;
                    _LineRenderer.sortingOrder = -1;

                    UpdateHorizontalScale();
                    break;
                }
            case "FileID":
                {
                    if (m_Option.FileHandle != null && m_Option.FileID > -1)
                    {
                        m_Channel = m_Option.FileHandle.Channels[m_Option.FileID];
                    }
                    break;
                }
        }
    }

    public void UpdateDraw(int milliSecToLook)
    {
        if (m_Channel == null)
            return;

        int SamplePosition = m_Channel.Frequency.ConvertToCeiledNumberOfSamples((int)(milliSecToLook + m_Option.OffsetInMilliSeconds));
        int PositionInArray = SamplePosition - m_Option.NumberOfPoint;
        float limitVal = (m_ParentRectTransform.rect.height - 6.5f) / 2;

        // One read for the whole window.
        if (m_Window == null || m_Window.Length != m_dataArray.Length)
            m_Window = new float[m_dataArray.Length];
        m_Channel.ReadWindow(PositionInArray, m_dataArray.Length, m_Window);
        for (int i = 0; i < m_dataArray.Length; i++)
        {
            if ((i + PositionInArray >= 0) && (i + PositionInArray < m_Channel.NumberOfSample))
            {
                float value = m_Option.Gain * m_Window[i];
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
            else
            {
                m_dataArray[i].y = 0;
            }
        }
        _LineRenderer.SetPositions(m_dataArray);
    }

    public void Show(bool show)
    {
        _LineRenderer.gameObject.SetActive(show);
    }

    public void UpdateHorizontalScale()
    {
        if (m_dataArray == null) return;

        m_WidthOfGameObject = m_ParentRectTransform.rect.width - 10;
        m_HorizontalScale = m_WidthOfGameObject / m_dataArray.Length;
        for (int i = 0; i < m_dataArray.Length; i++)
        {
            m_dataArray[i].x = ((-m_WidthOfGameObject / 2) + 1) + i * m_HorizontalScale;
            m_dataArray[i].y = 0;
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
