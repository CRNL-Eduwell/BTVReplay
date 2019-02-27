using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSignal : SignalDisp
{
    [SerializeField] VideoPlayer video = null;

    public override void init()
    {
        lineRenderer.gameObject.SetActive(false);
        base.init();
    }

    public override void updateDraw(int milliSecToLook)
    {
        if (lineRenderer.gameObject.activeSelf == false || video.audioWav == null ||
            video.audioWav.filterFileExist == false || milliSecToLook == -1)
            return;

        int posInArray = (int)(milliSecToLook * ((float)m_samplingFreq / 1000)) - m_numberPoint;
        float limitVal = (m_parentRectTransform.rect.height - 6.5f) / 2;

        for (int i = 0; i < m_dataArray.Length; i++)
        {
            if ((i + posInArray >= 0) && (i + posInArray < video.audioWav.currentAudio.Length))
            {
                float value = m_gain * video.audioWav.currentAudio[i + posInArray];
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

    public void Show(bool show)
    {
        lineRenderer.gameObject.SetActive(show);
    }

    public void changeAudioId(int newId)
    {
        video.audioWav.idAudioHandle = newId;
    }
}
