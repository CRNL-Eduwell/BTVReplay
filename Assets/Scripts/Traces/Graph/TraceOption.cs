using BTV.Data;
using BTV.Services.EventsService;
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
    public string ElectrodeName
    {
        get
        {
            return m_FileHandle.GetElectrodeNameFromElectrodeID(m_currentElectrodeID);
        }
    }
    public string ElectrodeLabel
    {
        get
        {
            if (FileHandle != null)
            {
                return Gain >= 0 ? ElectrodeName : (" - " + ElectrodeName);
            }
            else
            {
                return "";
            }
        }
    }
    public int ElectrodeID
    {
        get
        {
            return m_currentElectrodeID;
        }
        set
        {
            if (value > -1 && value < FileHandle.NumberOfElectrodes)
            {
                // Every electrode-change path funnels through this setter (trace scroll wheel,
                // brain plot click, keyboard shortcut, toolbar). Navigating to another electrode
                // dismisses the 1D correlation coloring on the brain: it was computed against a
                // site the user is no longer inspecting.
                if (m_currentElectrodeID != value)
                    EventsService.ClearCorrelations();

                m_currentElectrodeID = value;
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
            //TODO : v?rifier que ca sois une valeur correcte sois ici , sois plus haut
            m_WindowInSeconds = value;
            RaisePropertyChanged("WindowInSeconds");
            RaisePropertyChanged("NumberOfPoint");
        }
    }
    public Color Color
    {
        get
        {
            return m_Color;
        }
        set
        {
            m_Color = value;
            RaisePropertyChanged();
        }
    }
    public bool IsGridOn
    {
        get
        {
            return m_IsGridOn;
        }
        set
        {
            m_IsGridOn = value;
            RaisePropertyChanged();
        }
    }
    public int LineWidth
    {
        get 
        {
            return m_LineWidth;
        }
        set
        {
            m_LineWidth = value;
            RaisePropertyChanged();
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
    private int m_currentElectrodeID = 0;
    private Color m_Color = Color.white;
    private int m_LineWidth = 2;
    private bool m_IsGridOn = false;
    public TraceOption(BtvProgram file, Color color, float gain = 1, float offset = 0, int windowInSec = 10)
    {
        m_FileHandle = file;
        m_Color = color;
        m_Gain = gain;
        m_Offset = offset;
        m_WindowInSeconds = windowInSec;
    }

}
