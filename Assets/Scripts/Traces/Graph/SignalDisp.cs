using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SignalDisp : MonoBehaviour
{
    public int PeriodInSeconds { get { return m_PeriodInSeconds; } set { m_PeriodInSeconds = value; UpdateTimeResolution(); } }
    public int SamplingFrequency { get; set; } = 64;
    public int NumberOfPoint { get; set; } = 64 * 10;
    public float Gain { get; set; } = 1;
    public float WidthOfGameObject { get; set; }
    public float HorizontalScale { get; set; }
    public Vector3[] Data { get { return m_dataArray; } }
    public float LineWidth
    {
        get
        {
            return _LineRenderer.startWidth;
        }
        set
        {
            _LineRenderer.startWidth = value;
            _LineRenderer.endWidth = value;
        }
    }
    public Color Color
    {
        get
        {
            return _LineRenderer.startColor;
        }
        set
        {
            if (_LineRenderer.startColor != value && _LineRenderer.endColor != value)
            {
                _LineRenderer.startColor = value;
                _LineRenderer.endColor = value;
            }
        }
    }

    [SerializeField]
    protected LineRenderer _LineRenderer = null;
    protected RectTransform m_parentRectTransform = null;
    protected Vector3[] m_dataArray;
    protected float m_previousGain = 1;
    protected bool m_initDone = false;

    private int m_PeriodInSeconds = 10;

    void Awake()
    {
        m_parentRectTransform = gameObject.transform.parent.GetComponent<RectTransform>();
    }

    public virtual void Initialize()
    {
        NumberOfPoint = SamplingFrequency * m_PeriodInSeconds;
        m_dataArray = new Vector3[NumberOfPoint];
        _LineRenderer.positionCount = NumberOfPoint;
        _LineRenderer.startWidth = 1f;
        _LineRenderer.endWidth = 1f;
        UpdateHorizontalScale();

        m_initDone = true;
    }

    void OnRectTransformDimensionsChange()
    {
        if (m_parentRectTransform != null && m_initDone)
            UpdateHorizontalScale();
    }

    public void UpdateHorizontalScale()
    {
        WidthOfGameObject = m_parentRectTransform.rect.width - 10;
        HorizontalScale = WidthOfGameObject / m_dataArray.Length;
        for (int i = 0; i < m_dataArray.Length; i++)
        {
            m_dataArray[i].x = ((-WidthOfGameObject / 2) + 1) + i * HorizontalScale;
            m_dataArray[i].y = 0;
        }
        _LineRenderer.SetPositions(m_dataArray);
    }

    private void UpdateTimeResolution(/*int period*/)
    {
        //PeriodInSeconds = period;
        NumberOfPoint = SamplingFrequency * m_PeriodInSeconds;
        m_dataArray = new Vector3[NumberOfPoint];
        _LineRenderer.positionCount = NumberOfPoint;
        _LineRenderer.sortingOrder = -1;
    }

    public void UpdateLineWidth()
    {
        if (_LineRenderer.startWidth == 1f)
        {
            _LineRenderer.startWidth = 2f;
            _LineRenderer.endWidth = 2f;
        }
        else
        {
            _LineRenderer.startWidth = 1f;
            _LineRenderer.endWidth = 1f;
        }
    }

    public void UpdateGain(float gain)
    {
        m_previousGain = Gain;
        Gain = gain;

        for (int i = 0; i < NumberOfPoint; i++)
            m_dataArray[i].y = (m_dataArray[i].y / m_previousGain) * Gain;
        _LineRenderer.SetPositions(m_dataArray);
    }

    public abstract void UpdateDraw(int milliSecToLook);
}
