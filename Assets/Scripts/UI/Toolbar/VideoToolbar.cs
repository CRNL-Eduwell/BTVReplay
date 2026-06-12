using UnityEngine;

namespace BTV.UI.Module3D
{
    public class VideoToolbar : Toolbar
    {
        [SerializeField]
        private Tools.VideoGain m_Gain = null;

        [SerializeField]
        private Tools.VideoOffset m_Offset = null;

        [SerializeField]
        private Tools.VideoAudiotrace m_AudioTrace = null;

        [SerializeField]
        private Tools.VideoAudioExtract m_AudioExtract = null;

        [SerializeField]
        private Tools.VideoAudioFilter m_AudioFilter = null;

        [SerializeField]
        private Tools.VideoAudioFile m_AudioFile = null;

        //====
        Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
        Color softBlue = new Color(0.6117f, 0.7058f, 0.7960f, 0.627450f);

        protected override void AddTools()
        {
            m_Tools.Add(m_Gain);
            m_Tools.Add(m_Offset);
            m_Tools.Add(m_AudioTrace);
            m_Tools.Add(m_AudioExtract);
            m_Tools.Add(m_AudioFilter);
            m_Tools.Add(m_AudioFile);
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            m_Gain.gainHasChanged += UpdateAudioTraceGain;
            m_Offset.offsetVideoHasChanged += UpdateAudioTraceOffset;
            m_AudioTrace.ToggleTraceAudio += ToggleAudioTrace;
            m_AudioTrace.UpdateAudioFileID += UpdateAudioTraceFile;
        }

        private void UpdateAudioTraceGain(float NewGain)
        {
            BtvLog.Log("Update audio trace gain");
            UiToVideoMessage message = new UiToVideoMessage
            {
                TaskToExecute = 0,
                Gain = NewGain
            };
            Messenger.Default.Send(message, MessageContext.UiToVideo);
        }

        private void UpdateAudioTraceOffset(float NewOffset)
        {
            BtvLog.Log("Update audio trace offset");
            UiToVideoMessage message = new UiToVideoMessage
            {
                TaskToExecute = 1,
                Offset = NewOffset
            };
            Messenger.Default.Send(message, MessageContext.UiToVideo);
        }

        private void ToggleAudioTrace(bool IsOn)
        {
            BtvLog.Log("Toggle Audio Trace");
            UiToVideoMessage message = new UiToVideoMessage
            {
                TaskToExecute = 2,
                IsTraceOn = IsOn
            };
            Messenger.Default.Send(message, MessageContext.UiToVideo);
        }

        private void UpdateAudioTraceFile(int NewIdSm)
        {
            BtvLog.Log("Update Audio Trace File");
            UiToVideoMessage message = new UiToVideoMessage
            {
                TaskToExecute = 3,
                TraceID = NewIdSm
            };
            Messenger.Default.Send(message, MessageContext.UiToVideo);
        }
    }
}