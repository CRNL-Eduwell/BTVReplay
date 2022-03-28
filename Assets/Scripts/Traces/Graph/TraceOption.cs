using BTV.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraceOption : ViewModelBase
{
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
                RaisePropertyChanged("FileHandle");
                RaisePropertyChanged("SamplingFrequency");
                RaisePropertyChanged("NumberOfPoint");
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
    public float Offset
    {
        get
        {
            return m_Offset;
        }
        set
        {
            m_Offset = value;
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
            //TODO : vérifier que ca sois une valeur correcte sois ici , sois plus haut
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
    private float m_Gain = 0;
    private float m_Offset = 0;
    private int m_WindowInSeconds = 0;

    public TraceOption(BtvProgram file, float gain = 1, float offset = 0, int windowInSec = 10)
    {
        m_FileHandle = file;
        m_Gain = gain;
        m_Offset = offset;
        m_WindowInSeconds = windowInSec;
    }

}
