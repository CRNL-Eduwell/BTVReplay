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
    [SerializeField] BufferingImage _BufferingImage = null;
    [SerializeField] Button _RecordVideo = null;
    #endregion

    #region private members
    private bool m_Initialized = false;
    private Texture2D m_texPlay = null, m_texPause = null, m_texLogo = null;
    private Sprite m_texHandle = null, m_texHandleSlave = null;
    private GameObject m_RecorderPrefab = null;

    private bool m_LoopMode = false;
    private long m_TimeClick = -1;
    private long m_MinTimeClick = 0, m_MaxTimeClick = 0;
    private long m_LoopOffset = 0;
    private long m_LoopLength = 2000;
    private VideoToModulesMessage m_message = new VideoToModulesMessage();


    private bool m_ListenerLock = false;
    private long m_CurrentTime = 0;
    private long m_BeforeVideoTime = 0;
    private bool m_WaitToSync = false;
    private bool m_VideoWasPlaying = false;
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
        if (m_Initialized)
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
        if (m_Initialized)
        {
            if (!VideoInterface.IsStopped)
            {
                VideoInterface.Update();

                if (m_WaitToSync)
                {
                    if (VideoInterface.ClockTime != m_BeforeVideoTime)
                    {
                        m_WaitToSync = false;
                        _BufferingImage.Hide();
                        if (m_VideoWasPlaying)
                        {
                            m_VideoWasPlaying = false;
                            VideoInterface.Play();
                        }
                        m_CurrentTime = VideoInterface.ClockTime;
                    }
                }
                else
                {
                    m_CurrentTime = VideoInterface.ClockTime;
                }

                if (m_LoopMode && VideoInterface.IsPlaying)
                    if (_LoopScrollbar.value >= 1)
                        SetTime(m_MinTimeClick, true);

                if (VideoInterface.ClockTime > VideoInterface.TotalVideoTime)
                    Stop();

                UpdateScrollBarPosition();
            }

            if (m_LoopMode)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.J))
                    _LoopScrollbar.value -= 0.05f;
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.K))
                    _LoopScrollbar.value += 0.05f;
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
        // Tear down a previous player + listeners before re-initialising, otherwise a second
        // LoadVideo stacks another VideoPlayer component and a duplicate set of button listeners
        // (every click would then fire twice).
        if (m_Initialized)
        {
            VideoInterface.Cleanup();
            RemoveListeners();
            if (VideoInterface is MonoBehaviour previousPlayer) Destroy(previousPlayer);
            m_Initialized = false;
        }

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
        long timeMilliseconds = (long)message.TimeMilliseconds;

        if (message.UpdateClickPosition)
            UpdateTimeClick(timeMilliseconds);
        SetTime(timeMilliseconds, true);
    }

    private void AddListeners()
    {
        _Play.onClick.AddListener(TogglePlay);
        _Stop.onClick.AddListener(Stop);
        _GoBkwd10.onClick.AddListener(() => VideoInterface.MoveTime(-10));
        _GoBkwd1.onClick.AddListener(() => VideoInterface.MoveTime(-1));
        _GoFwd10.onClick.AddListener(() => VideoInterface.MoveTime(10));
        _TimeScrollbar.onValueChanged.AddListener(OnValueChangeScrollBar);
        _LoopScrollbar.onValueChanged.AddListener(OnValueChangeLoopScrollBar);
        _VolumeScrollbar.onValueChanged.AddListener((float newVolume) => VideoInterface.SetVolume(newVolume));
        _RecordVideo.onClick.AddListener(InstantiateVideoRecorder);

        EventTrigger scrollBarTrigger = _TimeScrollbar.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry onEndScrollbarEditTriggerEntry = new EventTrigger.Entry();
        onEndScrollbarEditTriggerEntry.eventID = EventTriggerType.PointerUp;
        onEndScrollbarEditTriggerEntry.callback.AddListener((eventData) =>
        {
            VideoInterface.SetTime(m_CurrentTime);
        });
        scrollBarTrigger.triggers.Add(onEndScrollbarEditTriggerEntry);

        m_Initialized = true;
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

        EventTrigger scrollBarTrigger = _TimeScrollbar.gameObject.GetComponent<EventTrigger>();
        for (int i = 0; i < scrollBarTrigger.triggers.Count; i++)
            scrollBarTrigger.triggers[i].callback.RemoveAllListeners();

        Destroy(scrollBarTrigger); //Not done before if it bugs ? 
    }

    private void TogglePlay()
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
        if (m_ListenerLock) return;

        long time = (long)(value * VideoInterface.TotalVideoTime);
        if (m_LoopMode)
        {
            if (time > m_MaxTimeClick)
                SetTime(Math.Min(VideoInterface.TotalVideoTime, m_MaxTimeClick), true);
            else if (time < m_MinTimeClick)
                SetTime(Math.Max(0, m_MinTimeClick), true);
            else
                SetTime(time);
        }
        else
        {
            SetTime(time);
        }
    }

    private void OnValueChangeLoopScrollBar(float value)
    {
        if (m_ListenerLock) return;

        if (VideoInterface.IsPlaying)
            TogglePlay();
        m_LoopOffset = (long)(value * m_LoopLength * 2) - m_LoopLength;
        _TimeScrollbar.value = (float)(m_TimeClick + m_LoopOffset) / VideoInterface.TotalVideoTime;
        VideoInterface.SetTime(m_TimeClick + m_LoopOffset);
    }

    private void InstantiateVideoRecorder()
    {
        ShowWindowMessage message = new ShowWindowMessage
        {
            WindowName = "VideoRecorder"
        };
        Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
    }

    private void UpdateScrollBarPosition()
    {
        m_ListenerLock = true;
        UpdateTimeText(m_CurrentTime);
        _TimeScrollbar.value = (float)m_CurrentTime / VideoInterface.TotalVideoTime;
        if (m_LoopMode)
            _LoopScrollbar.value = Mathf.InverseLerp(m_TimeClick - m_LoopLength, m_TimeClick + m_LoopLength, m_CurrentTime);
        m_ListenerLock = false;
    }

    /// <summary>
    /// Update Display of Current Time and send message to the outside
    /// with the current time in milliseconds
    /// </summary>
    /// <param name="milliseconds">Current time in seconds</param>
    private void UpdateTimeText(long milliseconds)
    {
        _CurrentTime.DisplayToTimeFormat((long)(milliseconds * 0.001f));
        long totalTimeSec = Mathf.RoundToInt(VideoInterface.TotalVideoTime * 0.001f);
        _TotalTime.DisplayToTimeFormat(totalTimeSec);

        //Send Message with timing information to the outside
        m_message.IsStopped = false;
        m_message.TimeMilliseconds = m_CurrentTime;
        Messenger.Default.Send(m_message, MessageContext.VideoToModulesMessage);
    }

    private void UpdateTimeClick(long timeMS)
    {
        m_TimeClick = timeMS;
        m_MinTimeClick = m_TimeClick - m_LoopLength;
        m_MaxTimeClick = m_TimeClick + m_LoopLength;
    }

    private void SetTime(long time, bool updateVideoTime = false)
    {
        if (time == VideoInterface.ClockTime) return;

        m_BeforeVideoTime = VideoInterface.ClockTime;
        m_CurrentTime = time;
        if (updateVideoTime)
        {
            VideoInterface.SetTime(time);
            if (!(VideoInterface is GhostVideoPlayer))
                _BufferingImage.Show();
        }
        m_WaitToSync = true;
        if (!(VideoInterface is GhostVideoPlayer))
            _BufferingImage.Show();
        if (VideoInterface.IsPlaying)
        {
            m_VideoWasPlaying = true;
            VideoInterface.Pause();
        }
    }
    public void ToggleLoopMode()
    {
        m_ListenerLock = true;
        if (!_LoopScrollbar.gameObject.activeSelf)
        {
            VideoInterface.SetVolume(0.0f);
            m_LoopMode = true;
            _LoopScrollbar.gameObject.SetActive(true);
            UpdateTimeClick(VideoInterface.ClockTime);
            _TimeScrollbar.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = m_texHandleSlave;
            _LoopScrollbar.value = 0.5f;
        }
        else
        {
            VideoInterface.SetVolume(_VolumeScrollbar.value);
            m_LoopMode = false;
            m_LoopOffset = 0;
            m_TimeClick = -1;
            _LoopScrollbar.gameObject.SetActive(false);
            _TimeScrollbar.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = m_texHandle;
        }
        m_ListenerLock = false;
    }
}