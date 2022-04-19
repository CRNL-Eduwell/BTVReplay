using BTV.Data;
using BTV.Services.VideoService;

public class AudioSignal3 : SignalDisp
{
    public BtvChannel ChannelHandle { get; private set; } = null;
    public float OffsetInMilliseconds { get; set; } = 0;

    public override void Initialize()
    {
        _LineRenderer.gameObject.SetActive(false);
        base.Initialize();
    }

    public override void UpdateDraw(int milliSecToLook)
    {
        if (ChannelHandle == null)
            return;

        int SamplePosition = ChannelHandle.Frequency.ConvertToCeiledNumberOfSamples((int)(milliSecToLook + OffsetInMilliseconds));
        int PositionInArray = SamplePosition - NumberOfPoint;
        float limitVal = (m_parentRectTransform.rect.height - 6.5f) / 2;

        for (int i = 0; i < m_dataArray.Length; i++)
        {
            if ((i + PositionInArray >= 0) && (i + PositionInArray < ChannelHandle.NumberOfSample))
            {
                float value = Gain * ChannelHandle.GetSample(i + PositionInArray);
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
        _LineRenderer.SetPositions(m_dataArray);
    }

    public void Show(bool show)
    {
        _LineRenderer.gameObject.SetActive(show);
    }

    public void UpdateAudioID(int NewId)
    {
        ChannelHandle = VideoService.GetSmoothedAudio(NewId);
    }
}
