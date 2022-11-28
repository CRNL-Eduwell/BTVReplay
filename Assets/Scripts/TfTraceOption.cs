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

    public float FrequencySlider
    {
        get
        {
            return m_FrequencySlider;
        }
        set
        {
            m_FrequencySlider = value;
            RaisePropertyChanged();
        }
    }

    private float m_Alpha = 0.5f;
    private float m_FrequencySlider = 1f;

    public TfTraceOption(float alpha)
    {
        m_Alpha = alpha;
    }
}