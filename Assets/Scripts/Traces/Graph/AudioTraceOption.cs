using BTV.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTraceOption : ViewModelBase
{
    public int FileID
    {
        get
        {
            return m_FileID;
        }
        set
        {
            m_FileID = value;
            RaisePropertyChanged();
        }
    }

    public BtvProgram FileHandle
    {
        get
        {
            return m_FileHandle;
        }
        set
        {
            if (m_FileHandle != value)
            {
                m_FileHandle = value;
                RaisePropertyChanged();
            }
        }
    }
    public int SamplingFrequency
    {
        get
        {
            return m_FileHandle.Frequency.Value;
        }
    }
    public float Gain
    {
        get
        {
            return m_Gain;
        }
        set
        {
            m_Gain = value;
            RaisePropertyChanged();
        }
    }
    public float OffsetInMilliSeconds
    {
        get
        {
            return m_OffsetInMilliSeconds;
        }
        set
        {
            m_OffsetInMilliSeconds = value;
            RaisePropertyChanged();
        }
    }
    public int WindowInSeconds
    {
        get
        {
            return m_WindowInSeconds;
        }
        set
        {
            //TODO : v?rifier que ca sois une valeur correcte sois ici , sois plus haut
            m_WindowInSeconds = value;
            RaisePropertyChanged("WindowInSeconds");
            RaisePropertyChanged("NumberOfPoint");
        }
    }
    public int NumberOfPoint
    {
        get
        {
            return SamplingFrequency * WindowInSeconds;
        }
    }

    private BtvProgram m_FileHandle = null;
    private int m_FileID = -1;
    private float m_Gain = 0;
    private float m_OffsetInMilliSeconds = 0;
    private int m_WindowInSeconds = 0;

    public AudioTraceOption(BtvProgram file, float gain = 1, float offset = 0, int windowInSec = 10)
    {
        m_FileHandle = file;
        m_Gain = gain;
        m_OffsetInMilliSeconds = offset;
        m_WindowInSeconds = windowInSec;
    }
}
