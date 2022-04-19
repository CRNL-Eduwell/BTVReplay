using BTV.Data;
using BTV.Services.EegFileService;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EegSignal3 : SignalDisp
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
                return Gain >= 0 ? ElectrodeName : (" - " + ElectrodeName);
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
    public float MostRecentValueInPercentOfTrace
    {
        get
        {
            return m_dataArray[NumberOfPoint - 1].y / (2 * m_limitValue);
        }
    }
    public float Offset
    {
        get
        {
            return m_offsetPerTen;
        }
    }
    [SerializeField] int m_currentElectrodeID = 0;

    private float m_offsetCoefficient = 0;
    private float m_offsetPerTen = 0;
    private float m_limitValue = 0;
    private int m_NumberSample = 0;
    private BtvChannel m_Channel = null;

    public override void Initialize()
    {
        FileHandle = EegFileService.ReturnFirstValidContainer();
        SamplingFrequency = FileHandle.Frequency.Value;
        m_NumberSample = FileHandle.Channels[m_currentElectrodeID].NumberOfSample;
        NumberOfPoint = SamplingFrequency * PeriodInSeconds;
        m_Channel = FileHandle.Channels[m_currentElectrodeID];

        base.Initialize();
    }

    public void UpdateFileId(int Id)
    {
        FileHandle = EegFileService.ChangeContainerHandle(FileHandle, Id);
        SamplingFrequency = FileHandle.Frequency.Value;
        m_NumberSample = FileHandle.Channels[m_currentElectrodeID].NumberOfSample;
        NumberOfPoint = SamplingFrequency * PeriodInSeconds;
        m_Channel = FileHandle.Channels[m_currentElectrodeID];
    }

    public void UpdateOffset(float offset)
    {
        m_offsetPerTen = offset;
        m_offsetCoefficient = (m_offsetPerTen / 10) * m_Channel.MaxValue;
    }

    public void UpdateOffset()
    {
        m_offsetCoefficient = (m_offsetPerTen / 10) * m_Channel.MaxValue;
    }

    public override void UpdateDraw(int milliSecToLook)
    {
        MostRecentTimeInMilliSecs = milliSecToLook;
        MostRecentSample = (int)(milliSecToLook * ((float)SamplingFrequency / 1000));
        int posInArray = MostRecentSample - NumberOfPoint;
        m_limitValue = (m_parentRectTransform.rect.height - 6.5f) / 2;

        for (int i = 0; i < NumberOfPoint; i++)
        {
            if (i + posInArray >= 0)
            {
                float eegValue = m_Channel.GetSample(i + posInArray, true);
                float value = Gain * eegValue + m_offsetCoefficient;
                if (value >= -m_limitValue && value <= m_limitValue)
                {
                    m_dataArray[i].y = value;
                }
                else
                {
                    if (value >= 0)
                        m_dataArray[i].y = m_limitValue;
                    else
                        m_dataArray[i].y = -m_limitValue;
                }
            }
            else
            {
                m_dataArray[i].y = 0;
            }
        }
        _LineRenderer.SetPositions(m_dataArray);
    }
}
