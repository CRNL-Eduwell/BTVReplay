using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using BTV.Services.VideoService;
using UnityEngine;

public class AudioSignal : SignalDisp
{
    [SerializeField] CustomVideoPlayer video = null;

    public BtvChannel ChannelHandle { get; private set; } = null;

    public override void init()
    {
        lineRenderer.gameObject.SetActive(false);
        base.init();
    }

    public override void updateDraw(int milliSecToLook)
    {
        //if (lineRenderer.gameObject.activeSelf == false || video.audioWav == null ||
        //    video.audioWav.filterFileExist == false || milliSecToLook == -1)
        //    return;
        if (ChannelHandle == null)
            return;

        int SamplePosition = ChannelHandle.Frequency.ConvertToCeiledNumberOfSamples(milliSecToLook);
        int PositionInArray = SamplePosition - m_numberPoint;
        float limitVal = (m_parentRectTransform.rect.height - 6.5f) / 2;

        for (int i = 0; i < m_dataArray.Length; i++)
        {
            if ((i + PositionInArray >= 0) && (i + PositionInArray < ChannelHandle.NumberOfSample))
            {
                float value = m_gain * ChannelHandle.Data[i + PositionInArray];
                //float value = m_gain * video.audioWav.currentAudio[i + PositionInArray];
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

    public void changeAudioId(int NewId)
    {
        ChannelHandle = VideoService.GetSmoothedAudio(NewId);
        //video.audioWav.idAudioHandle = newId;
    }
}
