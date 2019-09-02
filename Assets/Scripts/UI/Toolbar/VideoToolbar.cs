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

        //====
        CustomVideoPlayer m_videoPlayer = null;
        CoroutineManager m_coroutineManager = null;
        //====
        Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
        Color softBlue = new Color(0.6117f, 0.7058f, 0.7960f, 0.627450f);

        protected override void AddTools()
        {
            m_videoPlayer = GameObject.Find("Workable Part").transform.GetChild(0).GetChild(1).GetComponent<CustomVideoPlayer>();
            m_coroutineManager = GameObject.Find("ringSelect").GetComponent<CoroutineManager>();

            m_Tools.Add(m_Gain);
            m_Tools.Add(m_Offset);
            m_Tools.Add(m_AudioTrace);
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            m_Gain.gainAudioHasChanged += UpdateAudioTraceGain;
            m_Offset.offsetVideoHasChanged += UpdateAudioTraceOffset;
            m_AudioTrace.audioToggled += ToggleAudioTrace;
            m_AudioTrace.StartAudioFilter += StartAudioFiltering;
            m_AudioTrace.StartAudioLoad += LoadAudioTrace;
            m_AudioTrace.smAudioHasChanged += UpdateAudioTraceFile;
        }

        private void UpdateAudioTraceGain(float NewGain)
        {
            UnityEngine.Debug.Log("Update audio trace gain");
            UiToVideoMessage message = new UiToVideoMessage
            {
                TaskToExecute = 0,
                Gain = NewGain
            };
            Messenger.Default.Send(message, MessageContext.UiToVideo);
        }

        private void UpdateAudioTraceOffset(float NewOffset)
        {
            UnityEngine.Debug.Log("Update audio trace offset");
            UiToVideoMessage message = new UiToVideoMessage
            {
                TaskToExecute = 1,
                Offset = NewOffset
            };
            Messenger.Default.Send(message, MessageContext.UiToVideo);
        }

        private void ToggleAudioTrace(bool IsOn)
        {
            UnityEngine.Debug.Log("Toggle Audio Trace");
            UiToVideoMessage message = new UiToVideoMessage
            {
                TaskToExecute = 2,
                IsTraceOn = IsOn
            };
            Messenger.Default.Send(message, MessageContext.UiToVideo);
        }

        private void StartAudioFiltering()
        {
            m_coroutineManager.StartCoroutine(m_videoPlayer.c_filterAudio());
        }

        private void LoadAudioTrace()
        {
            m_coroutineManager.StartCoroutine(m_videoPlayer.c_loadAudio());
            ToggleAudioTrace(true);
        }

        private void UpdateAudioTraceFile(int NewIdSm)
        {
            UnityEngine.Debug.Log("Update Audio Trace File");
            UiToVideoMessage message = new UiToVideoMessage
            {
                TaskToExecute = 3,
                TraceID = NewIdSm
            };
            Messenger.Default.Send(message, MessageContext.UiToVideo);
        }
    }
}