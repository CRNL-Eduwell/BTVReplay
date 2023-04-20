using BTV.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TfTraceOption : ViewModelBase
{
    public float Alpha
    {
        get
        {
            return m_Alpha;
        }
        set
        {
            m_Alpha = value;
            RaisePropertyChanged();
        }
    }
    public float HighFrequency
    {
        get
        {
            return m_HighFrequency;
        }
        set
        {
            m_HighFrequency = value;
            RaisePropertyChanged();
        }
    }
    public float LowFrequency
    {
        get
        {
            return m_LowFrequency;
        }
        set
        {
            m_LowFrequency = value;
            RaisePropertyChanged();
        }
    }
    public float WindowInMilliseconds
    {
        get
        {
            return m_WindowInMilliseconds;
        }
        set
        {
            m_WindowInMilliseconds = value;
            RaisePropertyChanged();
        }
    }

    private float m_Alpha = 0.5f;
    private float m_WindowInMilliseconds = 500f;
    private float m_LowFrequency = 0f;
    private float m_HighFrequency = 256f;

    public TfTraceOption(float alpha)
    {
        m_Alpha = alpha;
    }
}