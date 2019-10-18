using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraceEvent
{
    #region Private Members
    EegEvent m_event;
    int m_duration = 0;
    string m_elecOfInterest = "";
    string m_secondElecOfInterest = "";
    string m_comment = "";
    float[] m_correlationData = null;
    float[][] m_correlationData2D = null;
    #endregion

    #region Public Properties
    public int code
    {
        get
        {
            return m_event.Code;
        }
        set
        {
            m_event.Code = value;
        }
    }
    public int sample
    {
        get
        {
            return m_event.Sample;
        }
    }
    public int samplingFrequency
    {
        get
        {
            return m_event.SamplingFrequency;
        }
    }
    public int duration
    {
        get
        {
            return m_duration;
        }
        set
        {
            m_duration = value;
        }
    }
    public string elecOfInterest
    {
        get
        {
            return m_elecOfInterest;
        }
        set
        {
            m_elecOfInterest = value;
        }
    }
    public string secondElecOfInterest
    {
        get
        {
            return m_secondElecOfInterest;
        }
        set
        {
            m_secondElecOfInterest = value;
        }
    }
    public string comment
    {
        get
        {
            return m_comment;
        }
        set
        {
            m_comment = value;
        }
    }
    public float[] correlationArray
    {
        get
        {
            return m_correlationData;
        }
        set
        {
            m_correlationData = value;
        }
    }
    public float[][] correlation2DArray
    {
        get
        {
            return m_correlationData2D;
        }
        set
        {
            m_correlationData2D = value;
        }
    }
    #endregion

    #region Constructors
    public TraceEvent(EegEvent eegEvent, int duration = 0, string elecOfInterest = "", string secondElecOfInterest = "", string comment = "")
    {
        m_event = new EegEvent(eegEvent);
        m_duration = duration;
        m_elecOfInterest = elecOfInterest;
        m_secondElecOfInterest = secondElecOfInterest;
        m_comment = comment;
    }

    public TraceEvent(TraceEvent currentTraceEvent)
    {
        m_event = new EegEvent(currentTraceEvent.m_event);
        m_duration = currentTraceEvent.m_duration;
        m_elecOfInterest = currentTraceEvent.m_elecOfInterest;
        m_secondElecOfInterest = currentTraceEvent.m_secondElecOfInterest;
        m_comment = currentTraceEvent.m_comment;
        if (currentTraceEvent.correlationArray != null)
            Array.Copy(currentTraceEvent.correlationArray, m_correlationData, m_correlationData.Length);
    }
    #endregion

    #region Public Methods
    public float timeSeconds()
    {
        return sample / samplingFrequency;
    } 
    #endregion
}
