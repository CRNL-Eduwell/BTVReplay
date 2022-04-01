using BTV.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTraceOption : ViewModelBase
{
    public bool DataLoaded
    {
        get 
        {
            return m_DataLoaded;
        }
    }

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

    private bool m_DataLoaded = false;
    private BtvProgram m_FileHandle = null;
    private int m_FileID = -1;
    private float m_Gain = 0;
    private float m_Offset = 0;

    public AudioTraceOption()
    { 
    
    }
}
