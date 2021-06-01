using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BrainTV.Tools.TextExtensions;

/// <summary>
/// Represents an instance of a video, either from a real video
/// or a simulated video just to view eeg data
/// </summary>
public class CustomVideoPlayer : MonoBehaviour
{
    public IVideoPlayer VideoInterface { get; private set; }

    #region scene members
    [SerializeField] RawImage _VideoTexture = null;
    [SerializeField] Text _CurrentTime = null;
    [SerializeField] Text _TotalTime = null;
    [SerializeField] Button _Play = null;
    [SerializeField] Button _Stop = null;
    [SerializeField] Button _GoBkwd10 = null;
    [SerializeField] Button _GoBkwd1 = null;
    [SerializeField] Button _GoFwd10 = null;
    [SerializeField] Scrollbar _TimeScrollbar = null;
    [SerializeField] Scrollbar _VolumeScrollbar = null;
    [SerializeField] Scrollbar _LoopScrollbar = null;
    [SerializeField] Button _RecordVideo = null;
    #endregion

    #region private members
    private bool m_scrollbarnotclicked = true, m_initDone = false, m_forceMove = false;
    private EventTrigger m_trigger = null, m_triggerSlaved = null;
    private Texture2D m_texPlay = null, m_texPause = null, m_texLogo = null;
    private Sprite m_texHandle = null, m_texHandleSlave = null;
    private GameObject m_RecorderPrefab = null;

    private bool m_slaved = false, m_keyForceMove = false;
    private long m_timeClick = -1;
    private float m_minTimeClick = 0.0f, m_maxTimeClick = 0.0f;
    private float m_scrollVal = 0.0f, m_scrollMemory = 0.0f;
    private VideoToModulesMessage m_message = new VideoToModulesMessage();

    #endregion

    private void Awake()
    {
        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
        Messenger.Default.Register<ModulesToVideoMessage>(this, OnModulesToVideoMessage, MessageContext.ModulesToVideoMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
        Messenger.Default.Unregister(this, MessageContext.ModulesToVideoMessage);
        if (m_initDone)
        {
            VideoInterface.Cleanup();
            RemoveListeners();
        }
    }

    private void Start()
    {
        m_RecorderPrefab = Resources.Load("Prefabs/VideoRecorder", typeof(GameObject)) as GameObject;
        m_texPause = Resources.Load("Pictures/playIcone", typeof(Texture2D)) as Texture2D;
        m_texPlay = Resources.Load("Pictures/pauseIcone", typeof(Texture2D)) as Texture2D;
        m_texLogo = Resources.Load("Pictures/BTVLogo", typeof(Texture2D)) as Texture2D;
        m_texHandle = Resources.Load("Pictures/handleScroll", typeof(Sprite)) as Sprite;
        m_texHandleSlave = Resources.Load("Pictures/handleSlave", typeof(Sprite)) as Sprite;
    }

    private void Update()
    {
        if (m_initDone)
        {
            if(!VideoInterface.IsStopped)
            {
                VideoInterface.Update();
                if (VideoInterface.CurrentTime > VideoInterface.TotalVideoTime)
                    Stop();

                if (m_slaved && !m_forceMove)
                {
                    if (VideoInterface.CurrentTime > m_timeClick + 2000)
                        UpdateTime((int)Math.Max(0, m_timeClick - 2000));
                    if (VideoInterface.CurrentTime < m_timeClick - 2000)
                        UpdateTime((int)Math.Min(VideoInterface.TotalVideoTime, m_timeClick + 2000));
                }
                UpdateScrollBarPosition();
            }

            if (m_slaved)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.J))
                    ForceMoveLoopScroll(-0.05f);

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.K))
                    ForceMoveLoopScroll(0.05f);
            }

        }
    }

    private void OnLoaderMessage(LoaderMessage message)
    {
        if (message.Task == LoaderMessage.LoaderTask.LoadVideo)
        {
            Init(message.VideoPath, message.totalFileDuration);
        }
    }

    private void Init(string videoPath, int eegFileDurationInMillisec)
    {
        if (videoPath == "")
            VideoInterface = gameObject.AddComponent<GhostVideoPlayer>();
        else
            VideoInterface = gameObject.AddComponent<UnityVideoPlayer>();

        _VideoTexture.texture = (Texture2D)Instantiate(m_texLogo);

        VideoInterface.Init(videoPath, eegFileDurationInMillisec, _VideoTexture);
        AddListeners();
    }

    private void OnModulesToVideoMessage(ModulesToVideoMessage message)
    {
        long TimeMilliseconds = (long)message.TimeMilliseconds;

        if (message.UpdateClickPosition)
            UpdateTimeClick(TimeMilliseconds);
        UpdateTime(TimeMilliseconds);
    }

    private void AddListeners()
    {
        _Play.onClick.AddListener(Play);
        _Stop.onClick.AddListener(Stop);
        _GoBkwd10.onClick.AddListener(() => VideoInterface.MoveTime(-10));
        _GoBkwd1.onClick.AddListener(() => VideoInterface.MoveTime(-1));
        _GoFwd10.onClick.AddListener(() => VideoInterface.MoveTime(10));
        _TimeScrollbar.onValueChanged.AddListener(OnValueChangeScrollBar);
        _VolumeScrollbar.onValueChanged.AddListener((float newVolume) => VideoInterface.SetVolume(newVolume));
        _RecordVideo.onClick.AddListener(InstantiateVideoRecorder);

        m_trigger = _TimeScrollbar.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((eventData) =>
        {
            if (!m_slaved)
                InitForceMoveLoopScroll();
        });
        m_trigger.triggers.Add(entry);

        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerUp;
        entry2.callback.AddListener((eventData) =>
        {
            if (!m_scrollbarnotclicked)
            {
                if (m_forceMove)
                    m_forceMove = false;

                SetTimeIfValueChanged();
                m_scrollbarnotclicked = true;
            }
        });
        m_trigger.triggers.Add(entry2);

        m_triggerSlaved = _LoopScrollbar.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry3 = new EventTrigger.Entry();
        entry3.eventID = EventTriggerType.BeginDrag;
        entry3.callback.AddListener((eventData) =>
        {
            InitForceMoveLoopScroll();
        });
        m_triggerSlaved.triggers.Add(entry3);

        EventTrigger.Entry entry4 = new EventTrigger.Entry();
        entry4.eventID = EventTriggerType.EndDrag;
        entry4.callback.AddListener((eventData) =>
        {
            FinishForceMoveLoopScroll();
        });
        m_triggerSlaved.triggers.Add(entry4);

        m_initDone = true;
    }

    private void RemoveListeners()
    {
        _Play.onClick.RemoveAllListeners();
        _Stop.onClick.RemoveAllListeners();
        _GoBkwd10.onClick.RemoveAllListeners();
        _GoBkwd1.onClick.RemoveAllListeners();
        _GoFwd10.onClick.RemoveAllListeners();
        _TimeScrollbar.onValueChanged.RemoveAllListeners();
        _VolumeScrollbar.onValueChanged.RemoveAllListeners();
        _RecordVideo.onClick.RemoveAllListeners();

        for (int i = 0; i < m_trigger.triggers.Count; i++)
            m_trigger.triggers[i].callback.RemoveAllListeners();

        for (int i = 0; i < m_triggerSlaved.triggers.Count; i++)
            m_triggerSlaved.triggers[i].callback.RemoveAllListeners();

        Destroy(m_trigger); //Not done before if it bugs ? 
        Destroy(m_triggerSlaved); //Not done before if it bugs ? 
    }

    private void Play()
    {
        if (VideoInterface.IsPaused || VideoInterface.IsStopped)
        {
            VideoInterface.Play();
            _Play.GetComponent<RawImage>().texture = m_texPlay;
        }
        else
        {
            VideoInterface.Pause();
            _Play.GetComponent<RawImage>().texture = m_texPause;
        }
    }

    private void Stop()
    {
        VideoInterface.Stop();
        _TimeScrollbar.value = 0;
        UpdateTimeText(0);

        m_message.IsStopped = true;
        m_message.TimeMilliseconds = 0;
        Messenger.Default.Send(m_message, MessageContext.VideoToModulesMessage);

        _VideoTexture.texture = Instantiate(m_texLogo);
        _Play.GetComponent<RawImage>().texture = m_texPause;
    }

    private void OnValueChangeScrollBar(float value)
    {
        if (VideoInterface.IsPlaying && !m_scrollbarnotclicked)
            VideoInterface.SetTime((long)(value * VideoInterface.TotalVideoTime));
    }

    private void InstantiateVideoRecorder()
    {
        ShowWindowMessage message = new ShowWindowMessage
        {
            TaskToExecute = 0,
            WindowName = "VideoRecorder"
        };
        Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
    }

    private void SetTimeIfValueChanged()
    {
        if (m_scrollMemory != _TimeScrollbar.value)
        {
            if (VideoInterface.IsPaused)
                Play();
            m_scrollMemory = _TimeScrollbar.value;
        }
        else
        {
            if (VideoInterface.IsPlaying)
                Play();
        }
    }

    private void UpdateScrollBarPosition()
    {
        if (m_scrollbarnotclicked)
        {
            _TimeScrollbar.value = ((float)VideoInterface.CurrentTime) / VideoInterface.TotalVideoTime;
            UpdateTimeText((long)(VideoInterface.CurrentTime * 0.001f));
        }
        else if (m_slaved)
        {
            if (m_forceMove)
                SetTimeIfValueChanged();

            m_scrollVal = (_LoopScrollbar.value * 4000) - 2000;
            _TimeScrollbar.value = (float)(m_timeClick + m_scrollVal) / VideoInterface.TotalVideoTime;
            UpdateTimeText((long)(VideoInterface.CurrentTime * 0.001f));

            if (m_keyForceMove)
                FinishForceMoveLoopScroll();
        }
        else
        {
            if (m_forceMove)
                SetTimeIfValueChanged();
            UpdateTimeText((long)(_TimeScrollbar.value * VideoInterface.TotalVideoTime * 0.001f));
        }
    }

    /// <summary>
    /// Update Display of Current Time and send message to the outside
    /// with the current time in milliseconds
    /// </summary>
    /// <param name="seconds">Current time in seconds</param>
    private void UpdateTimeText(long seconds)
    {
        _CurrentTime.DisplayToTimeFormat(seconds);
        long totalTimeSec = Mathf.RoundToInt(VideoInterface.TotalVideoTime * 0.001f);
        _TotalTime.DisplayToTimeFormat(totalTimeSec);

        //Send Message with timing information to the outside
        m_message.IsStopped = false;
        m_message.TimeMilliseconds = m_scrollbarnotclicked ? VideoInterface.CurrentTime : _TimeScrollbar.value * VideoInterface.TotalVideoTime;
        Messenger.Default.Send(m_message, MessageContext.VideoToModulesMessage);
    }

    private void UpdateTime(long timeMilliSec)
    {
        m_scrollbarnotclicked = false;
        VideoInterface.SetTime(timeMilliSec);
        m_scrollbarnotclicked = true;
    }

    private void UpdateTimeClick(long timeMS)
    {
        m_timeClick = timeMS;
        m_minTimeClick = m_timeClick - (2 * 1000);
        m_maxTimeClick = m_timeClick + (2 * 1000);
    }

    #region loopMode
    public void SlaveMode()
    {
        if (!_LoopScrollbar.gameObject.activeSelf)
        {
            VideoInterface.SetVolume(0.0f);
            if (VideoInterface.IsPlaying)
                Play();
            m_slaved = true;
            _LoopScrollbar.gameObject.SetActive(true);
            m_timeClick = VideoInterface.CurrentTime;
            m_minTimeClick = m_timeClick - (2 * 1000);
            m_maxTimeClick = m_timeClick + (2 * 1000);
            _TimeScrollbar.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = m_texHandleSlave;
        }
        else
        {
            VideoInterface.SetVolume(_VolumeScrollbar.value);
            if (VideoInterface.IsPaused)
                Play();
            m_slaved = false;
            m_scrollVal = 0;
            m_timeClick = -1;
            _LoopScrollbar.value = 0.5f;
            _LoopScrollbar.gameObject.SetActive(false);
            _TimeScrollbar.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = m_texHandle;
        }
    }

    private void InitForceMoveLoopScroll()
    {
        m_scrollbarnotclicked = false;
        if (VideoInterface.IsPaused)
        {
            m_forceMove = true;
            Play();
        }
    }

    private void FinishForceMoveLoopScroll()
    {
        if (!m_scrollbarnotclicked)
        {
            if (VideoInterface.IsPlaying)
                Play();

            m_forceMove = false;
            m_scrollbarnotclicked = true;
            m_keyForceMove = false;
        }
    }

    private void ForceMoveLoopScroll(float value)
    {
        m_keyForceMove = true;
        InitForceMoveLoopScroll();
        _LoopScrollbar.value += value;
    }
    #endregion
}