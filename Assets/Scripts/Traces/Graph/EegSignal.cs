using BTV.Data;
using BTV.Services.EegFileService;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EegSignal : SignalDisp
{
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
                m_currentElectrodeID = value;
                m_Channel = FileHandle.Channels[m_currentElectrodeID];
            }
        }
    }
    public string ElectrodeLabel
    {
        get
        {
            if (FileHandle != null)
            {
                return m_gain >= 0 ? ElectrodeName : (" - " + ElectrodeName);
            }
            else
            {
                return "";
            }
        }
    }
    public string ElectrodeName
    {
        get
        {
            return FileHandle.GetElectrodeNameFromElectrodeID(m_currentElectrodeID);
        }
    }
    public BtvProgram FileHandle{ get; private set; } = null;
    public int MostRecentSample { get; private set; } = 0;
    public int MostRecentTimeInMilliSecs { get; private set; } = 0;
    public float MostRecentValue
    {
        get
        {
            return m_Channel.GetSample(MostRecentSample, true);
        }
    }
    public float Offset
    {
        get
        {
            return m_offsetCoefficient;
        }
        set
        {
            m_offsetCoefficient = value;
        }
    }
    [SerializeField] int m_currentElectrodeID = 0;

    private float m_offsetCoefficient = 0;
    private float m_offsetPerTen = 0;
    private int m_NumberSample = 0;
    private BtvChannel m_Channel = null;

    public override void init()
    {
        FileHandle = EegFileService.ReturnFirstValidContainer();
        m_samplingFreq = FileHandle.Frequency.Value;
        m_NumberSample = FileHandle.Channels[m_currentElectrodeID].NumberOfSample;
        m_numberPoint = m_samplingFreq * m_periodSec;
        m_Channel = FileHandle.Channels[m_currentElectrodeID];

        base.init();
    }

    public void updateFileId(int newId)
    {
        FileHandle = EegFileService.ChangeContainerHandle(FileHandle, newId);
        m_samplingFreq = FileHandle.Frequency.Value;
        m_NumberSample = FileHandle.Channels[m_currentElectrodeID].NumberOfSample;
        m_numberPoint = m_samplingFreq * m_periodSec;
        m_Channel = FileHandle.Channels[m_currentElectrodeID];
    }

    public void updateOffset(float newOffset)
    {
        m_offsetPerTen = newOffset;
        m_offsetCoefficient = (m_offsetPerTen / 10) * m_Channel.MaxValue;
    }

    public void updateOffset()
    {
        m_offsetCoefficient = (m_offsetPerTen / 10) * m_Channel.MaxValue;
    }

    public override void UpdateDraw(int milliSecToLook)
    {
        MostRecentTimeInMilliSecs = milliSecToLook;
        MostRecentSample = (int)(milliSecToLook * ((float)m_samplingFreq / 1000));
        int posInArray = MostRecentSample - m_numberPoint;
        float limitVal = (m_parentRectTransform.rect.height - 6.5f) / 2;

        for (int i = 0; i < m_numberPoint; i++)
        {
            if (i + posInArray >= 0)
            {
                float eegValue = m_Channel.GetSample(i + posInArray, true);
                float value = m_gain * eegValue + m_offsetCoefficient;
                if (value >= -limitVal && value <= limitVal)
                {
                    m_dataArray[i].y = value;
                }
                else
                {
                    if (value >= 0)
                        m_dataArray[i].y = limitVal;
                    else
                        m_dataArray[i].y = -limitVal;
                }
            }
            else
            {
                m_dataArray[i].y = 0;
            }
        }
        lineRenderer.SetPositions(m_dataArray);
    }
}
