using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class eventEeg
{
    #region Private Members
    int m_code = -1;
    int m_sample = -1;
    int m_samplingFreq = -1;
    #endregion

    #region Public Properties
    public int code
    {
        get
        {
            return m_code;
        }
        set
        {
            m_code = value;
        }
    }
    public int sample
    {
        get
        {
            return m_sample;
        }
        set
        {
            m_sample = value;
        }
    }
    public int samplingFrequency
    {
        get
        {
            return m_samplingFreq;
        }
        set
        {
            m_samplingFreq = value;
        }
    }
    #endregion

    #region Constructors
    public eventEeg(int code, int sample = -1, int samplingFreq = -1)
    {
        m_code = code;
        m_sample = sample;
        m_samplingFreq = samplingFreq;
    }

    public eventEeg(eventEeg currentEvent)
    {
        m_code = currentEvent.m_code;
        m_sample = currentEvent.m_sample;
        m_samplingFreq = currentEvent.m_samplingFreq;
    }
    #endregion

    #region Public Methods
    public float timeSec()
    {
        return (float)m_sample / m_samplingFreq;
    }

    public float timeMilliSec()
    {
        return timeSec() * 1000;
    }
    #endregion
}

public class trigg
{
    #region Private Members
    eventEeg m_trigger;
    eventEeg m_response;
    int m_rtSample = -1;
    int m_rtMs = -1;
    #endregion

    #region Public Properties
    public eventEeg trigger
    {
        get
        {
            return m_trigger;
        }
        set
        {
            m_trigger = value;
        }
    }

    public eventEeg response
    {
        get
        {
            return m_response;
        }
        set
        {
            m_response = value;
        }
    }

    public int rtSample
    {
        get
        {
            return m_rtSample = m_response.sample - m_trigger.sample;
        }
        set
        {
            m_rtSample = value;
        }
    }
    #endregion

    #region Constructors
    public trigg(trigg trigger)
    {
        m_trigger = new eventEeg(trigger.trigger);
        m_response = new eventEeg(trigger.response);
    }

    public trigg(eventEeg trigger)
    {
        m_trigger = new eventEeg(trigger);
    }

    public trigg(eventEeg trigger, eventEeg response)
    {
        m_trigger = new eventEeg(trigger);
        m_response = new eventEeg(response);
    }

    ~trigg()
    {

    }
    #endregion

    #region Public Methods
    public int rtMs(int samplingFreq)
    {
        return m_rtMs = (int)(((float)rtSample / samplingFreq) * 1000);
    }

    public int rtMs()
    {
        return m_rtMs;
    }

    public static bool operator !=(trigg c1, trigg c2)
    {
        return !(c1 == c2);
    }

    public static bool operator ==(trigg c1, trigg c2)
    {
        if (c1.trigger.code == c2.trigger.code)
            return true;
        else
            return false;
    }

    public override bool Equals(object obj)
    {
        trigg triggObj = obj as trigg;
        if (triggObj == null)
            return false;
        else
            return trigger.code.Equals(triggObj.trigger.code);
    }

    public override int GetHashCode()
    {
        return trigger.GetHashCode();
    }
    #endregion
}

