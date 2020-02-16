using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.
using BrainTV.Tools.TextExtensions;

public delegate void timeVideo(int currentTime);
public delegate void timeVideoSync(int currentTime);
public delegate void stopVideo();

/// <summary>
/// Represents an instance of a video, either from a real video
/// or a simulated video just to view eeg data
/// </summary>
public class CustomVideoPlayer : MonoBehaviour
{
    public event timeVideo sendTime;
    public event timeVideoSync sendTimeVideo;
    public event stopVideo stopTimeVideo;

    public IVideoPlayer videoInterface
    {
        get
        {
            return _Iplayer;
        }
    }
    /// <summary>
    /// Time of the video, there is a possible offset due to user input
    /// In MilliSeconds
    /// </summary>
    long Time
    {
        get
        {
            if (m_scrollbarnotclicked)
                return _Iplayer.Time;
            else
                return (long)((_TimeScrollbar.value * _Iplayer.TotalVideoTime));
        }
    }
    /// <summary>
    /// Exact Time of the video without a possible offset
    /// In MilliSeconds
    /// </summary>
    long videoTime
    {
        get
        {
            if (m_scrollbarnotclicked)
                return _Iplayer.VideoTime;
            else
                return (long)((_TimeScrollbar.value * _Iplayer.TotalVideoTime));
        }
    }

    #region scene members
    [SerializeField] BTVMedia _Media = null;
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
    private IVideoPlayer _Iplayer = null;
    private bool m_scrollbarnotclicked = true, m_initDone = false, m_forceMove = false;
    private EventTrigger m_trigger = null;
    private Texture2D m_texPlay = null, m_texPause = null, m_texLogo = null;
    private Sprite m_texHandle = null, m_texHandleSlave = null;
    private GameObject m_RecorderPrefab = null;

    private bool m_slaved = false, m_keyForceMove = false;
    private EventTrigger m_triggerSlaved = null;
    private long m_timeClick = -1;
    private float m_minTimeClick = 0.0f, m_maxTimeClick = 0.0f;
    private float m_scrollVal = 0.0f, m_scrollMemory = 0.0f;
    #endregion

    private void Awake()
    {
        _Media.loadVideo += new initVideo(Init);
        Messenger.Default.Register<ModulesToVideoMessage>(this, OnModulesToVideoMessage, MessageContext.ModulesToVideoMessage);
    }

    private void OnDestroy()
    {
        _Media.loadVideo -= new initVideo(Init);
        Messenger.Default.Unregister(this, MessageContext.ModulesToVideoMessage);
        if (m_initDone)
        {
            _Iplayer.Cleanup();
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
            if (_Iplayer.IsPlaying)
            {
                _Iplayer.Update();
                if (_Iplayer.CurrentTime > _Iplayer.TotalVideoTime)
                    Stop();

                if (m_slaved && !m_forceMove)
                {
                    if (_Iplayer.CurrentTime > m_timeClick + 2000)
                        UpdateTime((int)Math.Max(0, m_timeClick - 2000));
                    if (_Iplayer.CurrentTime < m_timeClick - 2000)
                        UpdateTime((int)Math.Min(_Iplayer.TotalVideoTime, m_timeClick + 2000));
                }
                UpdateScrollBarPosition();
                sendTime((int)Time);
                sendTimeVideo((int)videoTime);
            }
            else if (_Iplayer.IsPaused)
            {
                UpdateScrollBarPosition();
                sendTime((int)Time);
                sendTimeVideo((int)videoTime);
            }

            if (m_slaved)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.J))
                    forceMoveLoopScroll(-0.05f);

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.K))
                    forceMoveLoopScroll(0.05f);
            }

        }
    }

    private void Init(string videoPath, int eegFileDurationInSec)
    {
        if (videoPath == "")
            _Iplayer = gameObject.AddComponent<GhostVideoPlayer>();
        else
            _Iplayer = gameObject.AddComponent<UnityVideoPlayer>();

        _VideoTexture.texture = (Texture2D)Instantiate(m_texLogo);

        _Iplayer.Init(videoPath, eegFileDurationInSec, _VideoTexture);
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
        _GoBkwd10.onClick.AddListener(() => _Iplayer.MoveTime(-10));
        _GoBkwd1.onClick.AddListener(() => _Iplayer.MoveTime(-1));
        _GoFwd10.onClick.AddListener(() => _Iplayer.MoveTime(10));
        _TimeScrollbar.onValueChanged.AddListener(OnValueChangeScrollBar);
        _VolumeScrollbar.onValueChanged.AddListener((float newVolume) => _Iplayer.SetVolume(newVolume));
        _RecordVideo.onClick.AddListener(InstantiateVideoRecorder);

        m_trigger = _TimeScrollbar.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((eventData) =>
        {
            if (!m_slaved)
                initForceMoveLoopScroll();
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
            initForceMoveLoopScroll();
        });
        m_triggerSlaved.triggers.Add(entry3);

        EventTrigger.Entry entry4 = new EventTrigger.Entry();
        entry4.eventID = EventTriggerType.EndDrag;
        entry4.callback.AddListener((eventData) =>
        {
            finishForceMoveLoopScroll();
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
        if (_Iplayer.IsPaused || _Iplayer.IsStopped)
        {
            _Iplayer.Play();
            _Play.GetComponent<RawImage>().texture = m_texPlay;
        }
        else
        {
            _Iplayer.Pause();
            _Play.GetComponent<RawImage>().texture = m_texPause;
        }
    }

    private void Stop()
    {
        _Iplayer.Stop();
        stopTimeVideo();
        _VideoTexture.texture = Instantiate(m_texLogo);
        _Play.GetComponent<RawImage>().texture = m_texPause;
    }

    private void OnValueChangeScrollBar(float value)
    {
        if (_Iplayer.IsPlaying && !m_scrollbarnotclicked)
            _Iplayer.SetTime((long)(value * _Iplayer.TotalVideoTime));
    }

    private void InstantiateVideoRecorder()
    {
        if (GameObject.Find("VideoRecorder") == null)
        {
            Instantiate(m_RecorderPrefab, GameObject.Find("Canvas").transform);
        }
        else
        {
            UnityEngine.Debug.Log("There is already a videorecorder isntance ");
        }
    }

    private void SetTimeIfValueChanged()
    {
        if (m_scrollMemory != _TimeScrollbar.value)
        {
            if (_Iplayer.IsPaused)
                Play();
            m_scrollMemory = _TimeScrollbar.value;
        }
        else
        {
            if (_Iplayer.IsPlaying)
                Play();
        }
    }

    private void UpdateScrollBarPosition()
    {
        if (m_scrollbarnotclicked)
        {
            _TimeScrollbar.value = (float)(_Iplayer.CurrentTime) / _Iplayer.TotalVideoTime;
            UpdateTimeText((long)(_Iplayer.CurrentTime * 0.001f));
        }
        else if (m_slaved)
        {
            if (m_forceMove)
                SetTimeIfValueChanged();

            m_scrollVal = (_LoopScrollbar.value * 4000) - 2000;
            _TimeScrollbar.value = (float)(m_timeClick + m_scrollVal) / _Iplayer.TotalVideoTime;
            UpdateTimeText((long)(_Iplayer.CurrentTime * 0.001f));

            if (m_keyForceMove)
                finishForceMoveLoopScroll();
        }
        else
        {
            if (m_forceMove)
                SetTimeIfValueChanged();
            UpdateTimeText((long)(_TimeScrollbar.value * _Iplayer.TotalVideoTime * 0.001f));
        }
    }

    /// <summary>
    /// TODO : Faire que cela set une propriété et que en faisant cela , cela execute
    /// cette fonction en plus d'envoyer les events de temps
    /// </summary>
    /// <param name="seconds"></param>
    private void UpdateTimeText(long seconds)
    {
        _CurrentTime.DisplayToTimeFormat(seconds);
        long totalTimeSec = Mathf.RoundToInt(_Iplayer.TotalVideoTime * 0.001f);
        _TotalTime.DisplayToTimeFormat(totalTimeSec);
    }

    private void UpdateTime(long timeMilliSec)
    {
        m_scrollbarnotclicked = false;
        _Iplayer.SetTime(timeMilliSec);
        m_scrollbarnotclicked = true;
    }

    private void UpdateTimeClick(long timeMS)
    {
        m_timeClick = timeMS;
        m_minTimeClick = m_timeClick - (2 * 1000);
        m_maxTimeClick = m_timeClick + (2 * 1000);
    }

    #region loopMode
    public void slaveMode()
    {
        if (!_LoopScrollbar.gameObject.activeSelf)
        {
            _Iplayer.SetVolume(0.0f);
            if (_Iplayer.IsPlaying)
                Play();
            m_slaved = true;
            _LoopScrollbar.gameObject.SetActive(true);
            m_timeClick = _Iplayer.CurrentTime;
            m_minTimeClick = m_timeClick - (2 * 1000);
            m_maxTimeClick = m_timeClick + (2 * 1000);
            _TimeScrollbar.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = m_texHandleSlave;
        }
        else
        {
            _Iplayer.SetVolume(_VolumeScrollbar.value);
            if (_Iplayer.IsPaused)
                Play();
            m_slaved = false;
            m_scrollVal = 0;
            m_timeClick = -1;
            _LoopScrollbar.value = 0.5f;
            _LoopScrollbar.gameObject.SetActive(false);
            _TimeScrollbar.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = m_texHandle;
        }
    }

    void initForceMoveLoopScroll()
    {
        m_scrollbarnotclicked = false;
        if (_Iplayer.IsPaused)
        {
            m_forceMove = true;
            Play();
        }
    }

    void finishForceMoveLoopScroll()
    {
        if (!m_scrollbarnotclicked)
        {
            if (_Iplayer.IsPlaying)
                Play();

            m_forceMove = false;
            m_scrollbarnotclicked = true;
            m_keyForceMove = false;
        }
    }

    void forceMoveLoopScroll(float value)
    {
        m_keyForceMove = true;
        initForceMoveLoopScroll();
        _LoopScrollbar.value += value;
    }
    #endregion
}