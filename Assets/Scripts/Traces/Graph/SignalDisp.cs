using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SignalDisp : MonoBehaviour
{
    public int PeriodInSeconds
    {
        get
        {
            return m_periodSec;
        }
    }
    public int SamplingFrequency
    {
        get
        {
            return m_samplingFreq;
        }
    }
    public int numberOfPoint
    {
        get
        {
            return m_numberPoint;
        }
    }
    public float Gain
    {
        get
        {
            return m_gain;
        }
    }
    public float widthOfGameObject
    {
        get;
        set;
    }
    public float horizontalScale
    {
        get;
        set;
    }
    public Vector3[] dataArray
    {
        get
        {
            return m_dataArray;
        }
    }

    [SerializeField]
    protected LineRenderer lineRenderer = null;
    protected RectTransform m_parentRectTransform = null;
    protected Vector3[] m_dataArray;
    protected float m_gain = 1, m_previousGain = 1;
    protected int m_periodSec = 10, m_samplingFreq = 64, m_numberPoint = 64 * 10;
    protected bool m_initDone = false;

    void Awake()
    {
        m_parentRectTransform = gameObject.transform.parent.GetComponent<RectTransform>();
    }

    public virtual void init()
    {
        m_dataArray = new Vector3[m_numberPoint];
        lineRenderer.positionCount = m_numberPoint;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        updateHorizontalScale();

        m_initDone = true;
    }

    void OnRectTransformDimensionsChange()
    {
        if (m_parentRectTransform != null && m_initDone)
            updateHorizontalScale();
    }

    public void updateHorizontalScale()
    {
        widthOfGameObject = m_parentRectTransform.rect.width - 10;
        horizontalScale = widthOfGameObject / m_dataArray.Length;
        for (int i = 0; i < m_dataArray.Length; i++)
        {
            m_dataArray[i].x = ((-widthOfGameObject / 2) + 1) + i * horizontalScale;
            m_dataArray[i].y = 0;
        }
        lineRenderer.SetPositions(m_dataArray);
    }

    public void updateTimeResolution(int newPeriod)
    {
        m_periodSec = newPeriod;
        m_numberPoint = m_samplingFreq * m_periodSec;
        m_dataArray = new Vector3[m_numberPoint];
        lineRenderer.positionCount = m_numberPoint;
        lineRenderer.sortingOrder = -1;
    }

    public void updateLineWidth()
    {
        if (lineRenderer.startWidth == 0.02f)
        {
            lineRenderer.startWidth = 0.04f;
            lineRenderer.endWidth = 0.04f;
        }
        else
        {
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
        }
    }

    public void updateLineColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    public void updateGain(float newGain)
    {
        m_previousGain = m_gain;
        m_gain = newGain;

        for (int i = 0; i < m_numberPoint; i++)
            m_dataArray[i].y = (m_dataArray[i].y / m_previousGain) * m_gain;
        lineRenderer.SetPositions(m_dataArray);
    }

    public abstract void updateDraw(int milliSecToLook);
}
