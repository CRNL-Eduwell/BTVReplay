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

    private float m_SamplingFrequency = 0;
    private int m_frequencyBinCount = 0;
    private int m_timeFrameCount = 0;
    private Dictionary<int, float[]> Freq_TimeFrame = null;

    public TimeFrequencyDataStructure(TimeFrequencyDataStructure tfds)
    {
        m_SamplingFrequency = tfds.m_SamplingFrequency;
        m_frequencyBinCount = tfds.m_frequencyBinCount;
        m_timeFrameCount = tfds.m_timeFrameCount;

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
        }
    }

    public float[] GetFrequencyBinData(int binIndex)
    {
        return Freq_TimeFrame[binIndex];
    }

    public float RequestValue(int binIndex, int timeIndex)
    {
        return Freq_TimeFrame[binIndex][timeIndex];
    }

    //public float RequestFrequency(int binIndex, float freqVisu)
    //{
    //    return (freqVisu / m_frequencyBinCount) * binIndex;
    //}

    public float RequestFrequency(int binIndex, float lowFrequency, float highFrequency)
    {
        return (((highFrequency - lowFrequency) / m_frequencyBinCount) * binIndex) + lowFrequency;
    }
}