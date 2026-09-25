using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimeFrequencyDataStructure
{
    public float FrequencyBinCount
    {
        get
        {
            return m_frequencyBinCount;
        }
    }
    public int TimeFrameCount
    {
        get
        {
            return m_timeFrameCount;
        }
    }
    public float TopValue
    {
        get
        {
            if (m_TopValueNeedsUpdate)
            {
                // NaN marks bins with no defined value (a flat z-score baseline); they must not
                // decide the colour scale.
                var values = Freq_TimeFrame.Values.SelectMany(v => v).Where(v => !float.IsNaN(v) && !float.IsInfinity(v)).OrderBy(v => v).ToArray();
                int index = (int)(values.Length * 0.90f);
                if (index <= 0) index = 0;
                if (index >= values.Length) index = values.Length - 1;
                m_TopValue = values.Length > 0 ? values[index] : 0f;
                m_TopValueNeedsUpdate = false;
            }
            return m_TopValue;
        }
    }

    private float m_SamplingFrequency = 0;
    private int m_frequencyBinCount = 0;
    private int m_timeFrameCount = 0;
    private float m_TopValue = 0;
    private bool m_TopValueNeedsUpdate = false;
    private Dictionary<int, float[]> Freq_TimeFrame = null;

    public TimeFrequencyDataStructure(TimeFrequencyDataStructure tfds)
    {
        m_SamplingFrequency = tfds.m_SamplingFrequency;
        m_frequencyBinCount = tfds.m_frequencyBinCount;
        m_timeFrameCount = tfds.m_timeFrameCount;
        m_TopValue = tfds.TopValue;

        Freq_TimeFrame = new Dictionary<int, float[]>();
        Freq_TimeFrame = tfds.Freq_TimeFrame.ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    public TimeFrequencyDataStructure(float samplingFrequency, int frequencyBinCount, int timeFrameCount)
    {
        m_SamplingFrequency = samplingFrequency;
        m_frequencyBinCount = frequencyBinCount;
        m_timeFrameCount = timeFrameCount;

        Freq_TimeFrame = new Dictionary<int, float[]>();
        for (int i = 0; i < frequencyBinCount; i++)
        {
            Freq_TimeFrame.Add(i, new float[timeFrameCount]);
        }
    }

    public void SetTf_Frame(int frame, float[] data)
    {
        for (int i = 0; i < Freq_TimeFrame.Count; i++)
        {
            Freq_TimeFrame[i].SetValue(data[i], frame);
            m_TopValueNeedsUpdate = true;
        }
    }

    public void SetFrequencyBinData(int binIndex, float[] data)
    {
        int elementCount = Freq_TimeFrame[binIndex].Length;
        for (int i = 0; i < elementCount; i++)
        {
            Freq_TimeFrame[binIndex][i] = data[i];
            m_TopValueNeedsUpdate = true;
        }
    }

    public float[] GetFrequencyBinData(int binIndex)
    {
        if (Freq_TimeFrame.TryGetValue(binIndex, out float[] values))
        {
            return values;
        }
        return new float[m_timeFrameCount];
    }

    public float RequestValue(int binIndex, int timeIndex)
    {
        return Freq_TimeFrame[binIndex][timeIndex];
    }

    public float RequestFrequency(int binIndex, float lowFrequency, float highFrequency)
    {
        return (((highFrequency - lowFrequency) / m_frequencyBinCount) * binIndex) + lowFrequency;
    }
}