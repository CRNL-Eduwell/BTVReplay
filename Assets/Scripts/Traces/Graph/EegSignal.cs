using BTV.Data.DataContainer;
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
            if (value > -1 && value < FileHandle.NumberOfElectrode)
            {
                m_currentElectrodeID = value;
                m_Data = FileHandle.GetEegDataFromElectrodeID(m_currentElectrodeID);
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
    public DataContainer FileHandle{ get; private set; } = null;
    public int MostRecentSample { get; private set; } = 0;
    public float MostRecentValue
    {
        get
        {
            return m_Data[MostRecentSample];
        }
    }

    [SerializeField] int m_currentElectrodeID = 0;

    private float m_offsetCoefficient = 0;
    private float m_offsetPerTen = 0;
    private int m_NumberSample = 0;
    private float[] m_Data = null;

    public override void init()
    {
        FileHandle = EegFileService.ReturnFirstValidContainer();
        m_samplingFreq = FileHandle.Frequency.Value;
        m_NumberSample = FileHandle.NumberOfSample;
        m_numberPoint = m_samplingFreq * m_periodSec;
        m_Data = FileHandle.GetEegDataFromElectrodeID(m_currentElectrodeID);

        base.init();
    }

    public void updateFileId(int newId)
    {
        FileHandle = EegFileService.ChangeContainerHandle(FileHandle, newId);
        m_samplingFreq = FileHandle.Frequency.Value;
        m_NumberSample = FileHandle.NumberOfSample;
        m_numberPoint = m_samplingFreq * m_periodSec;
        m_Data = FileHandle.GetEegDataFromElectrodeID(m_currentElectrodeID);
    }

    public void updateOffset(float newOffset)
    {
        m_offsetPerTen = newOffset;
        //m_offsetCoefficient = (m_offsetPerTen / 10) * eHandle.maxValues[idCurrentElec];
    }

    public void updateOffset()
    {
        //m_offsetCoefficient = (m_offsetPerTen / 10) * eHandle.maxValues[idCurrentElec];
    }

    public override void updateDraw(int milliSecToLook)
    {
        MostRecentSample = (int)(milliSecToLook * ((float)m_samplingFreq / 1000));
        int posInArray = MostRecentSample - m_numberPoint;
        float limitVal = (m_parentRectTransform.rect.height - 6.5f) / 2;

        for (int i = 0; i < m_numberPoint; i++)
        {
            if (i + posInArray >= 0)
            {
                float value = m_gain * m_Data[i + posInArray] + m_offsetCoefficient;
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
