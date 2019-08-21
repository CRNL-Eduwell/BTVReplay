using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EegSignal : SignalDisp
{
    public int IdElectrode
    {
        get
        {
            return idCurrentElec;
        }
        set
        {
            if (value != -1 && value < eHandle.electrodes.Length)
                idCurrentElec = value;
        }
    }
    public string LabelElectrode
    {
        get
        {
            if (eHandle != null)
            {
                if (m_gain >= 0)
                    return eHandle.electrodes[idCurrentElec].name;
                else
                    return " - " + eHandle.electrodes[idCurrentElec].name;
            }
            else
            {
                return "";
            }
        }
    }
    public string nameElectrode
    {
        get
        {
            return eHandle.electrodes[idCurrentElec].name;
        }
    }
    public ELAN fileHandle
    {
        get
        {
            return eHandle;
        }
    }
    public int mostRecentSample
    {
        get
        {
            return m_mostRecentSample;
        }
    }

    [SerializeField] BTVMedia media = null;
    [SerializeField] int idCurrentElec = 0;

    ELAN eHandle = null;
    int m_mostRecentSample = 0;
    float m_offsetCoefficient = 0;
    float m_offsetPerTen = 0;

    public override void init()
    {
        eHandle = ELAN.returnFirstValidHandle(media.elanFiles);
        m_samplingFreq = (int)eHandle.sampFreq;
        m_numberPoint = m_samplingFreq * m_periodSec;

        base.init();
    }

    public void updateFileId(int newId)
    {
        eHandle = ELAN.changeHandle(eHandle, media.elanFiles, newId);
        m_samplingFreq = (int)eHandle.sampFreq;
    }

    public void updateOffset(float newOffset)
    {
        m_offsetPerTen = newOffset;
        m_offsetCoefficient = (m_offsetPerTen / 10) * eHandle.maxValues[idCurrentElec];
    }

    public void updateOffset()
    {
        m_offsetCoefficient = (m_offsetPerTen / 10) * eHandle.maxValues[idCurrentElec];
    }

    public override void updateDraw(int milliSecToLook)
    {
        m_mostRecentSample = (int)(milliSecToLook * ((float)m_samplingFreq / 1000));
        int elecPosOffset = idCurrentElec * eHandle.nbSam;
        int posInArray = m_mostRecentSample - m_numberPoint + elecPosOffset;
        float limitVal = (m_parentRectTransform.rect.height - 6.5f) / 2;

        for (int i = 0; i < m_numberPoint; i++)
        {
            if (i + posInArray >= elecPosOffset)
            {
                float value = m_gain * eHandle.eegData[i + posInArray] + m_offsetCoefficient;
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
