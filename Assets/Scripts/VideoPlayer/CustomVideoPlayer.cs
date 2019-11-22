using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.
using System;
using System.IO;
using System.Diagnostics;
using System.Collections; //IEnumerator
using CielaSpike;
using BTV.Services.EegFileService;
using BTV.Data;
using BTV.Services.VideoService;

public delegate void timeVideo(int currentTime);
public delegate void timeVideoSync(int currentTime);
public delegate void stopVideo();

/// <summary>
/// Represents an instance of a video, either from a real video
/// or a simulated video just to view eeg data
/// </summary>
public class CustomVideoPlayer : MonoBehaviour
{
    public IVideoPlayer videoInterface
    {
        get
        {
            return _Iplayer;
        }
    }

    public event timeVideo sendTime;
    public event timeVideoSync sendTimeVideo;
    public event stopVideo stopTimeVideo;
    //==

    #region scene members
    //[SerializeField] optionsHub hub = null;
    [SerializeField] BTVMedia media = null;
    [SerializeField] RawImage TextureToDraw = null;
    [SerializeField] Text currentTimetext = null;
    [SerializeField] Text totalTimeText = null;
    [SerializeField] Button playPause = null;
    [SerializeField] Button stopButton = null;
    [SerializeField] Button backTime10 = null;
    [SerializeField] Button backTime1 = null;
    [SerializeField] Button frontTime10 = null;
    [SerializeField] Scrollbar scrollBar = null;
    [SerializeField] Scrollbar volumeScrollBar = null;
    [SerializeField] Scrollbar loopScroll = null;
    [SerializeField] Button recordVideo = null;
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
    private long m_currentTimeScrollBar = 0;
    #endregion

    private void Awake()
    {
        media.loadVideo += new initVideo(init);
    }

    private void OnDestroy()
    {
        media.loadVideo -= new initVideo(init);
        if (m_initDone)
        {
            _Iplayer.cleanup();

            playPause.onClick.RemoveAllListeners();
            stopButton.onClick.RemoveAllListeners();
            backTime10.onClick.RemoveAllListeners();
            backTime1.onClick.RemoveAllListeners();
            frontTime10.onClick.RemoveAllListeners();
            volumeScrollBar.onValueChanged.RemoveAllListeners();
            recordVideo.onClick.RemoveAllListeners();

            for (int i = 0; i < m_trigger.triggers.Count; i++)
                m_trigger.triggers[i].callback.RemoveAllListeners();

            for (int i = 0; i < m_triggerSlaved.triggers.Count; i++)
                m_triggerSlaved.triggers[i].callback.RemoveAllListeners();

            Destroy(m_trigger); //Not done before if it bugs ? 
            Destroy(m_triggerSlaved); //Not done before if it bugs ? 
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
            if (_Iplayer.isPlaying)
            {
                _Iplayer.update();
                if (_Iplayer.currentTime > _Iplayer.totalVideoTime)
                    Stop();

                if (m_slaved && !m_forceMove)
                {
                    if (_Iplayer.currentTime > m_timeClick + 2000)
                        setTime((int)Math.Max(0, m_timeClick - 2000));
                    if (_Iplayer.currentTime < m_timeClick - 2000)
                        setTime((int)Math.Min(_Iplayer.totalVideoTime, m_timeClick + 2000));
                }
                updateScrollBarPosition();
                updateTimeText();
                sendTime((int)Time);
                sendTimeVideo((int)videoTime);
            }
            else if (_Iplayer.isPaused)
            {
                updateScrollBarPosition();
                updateTimeText();
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

    #region implement Interface
    void init(string videoPath, int eegFileDurationInSec)
    {        
        if (videoPath == "")
            _Iplayer = gameObject.AddComponent<VLCLess>();
        else
            _Iplayer = gameObject.AddComponent<VLCSharp.VLCSharp>();

        TextureToDraw.texture = (Texture2D)Instantiate(m_texLogo);

        _Iplayer.init(videoPath, eegFileDurationInSec, TextureToDraw);
        initListeners();
    }

    void initListeners()
    {
        playPause.onClick.AddListener(Play);
        stopButton.onClick.AddListener(Stop);
        backTime10.onClick.AddListener(() => MoveTime(-10));
        backTime1.onClick.AddListener(() => MoveTime(-1));
        frontTime10.onClick.AddListener(() => MoveTime(10));
        volumeScrollBar.onValueChanged.AddListener((float newVolume) =>
                                                    _Iplayer.setVolume(newVolume));

        recordVideo.onClick.AddListener(()=> 
        {
            if (GameObject.Find("VideoRecorder") == null)
            {
                Instantiate(m_RecorderPrefab, GameObject.Find("Canvas").transform);
            }
            else
            {
                UnityEngine.Debug.Log("There is already a videorecorder isntance ");
            }

        });

        m_trigger = scrollBar.gameObject.AddComponent<EventTrigger>();

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

                setTimeIfValueChanged();
                m_scrollbarnotclicked = true;
            }
        });
        m_trigger.triggers.Add(entry2);

        m_triggerSlaved = loopScroll.gameObject.AddComponent<EventTrigger>();

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

    public void setTime(int timeMilliSec)
    {
        m_scrollbarnotclicked = false;
        _Iplayer.setTime(timeMilliSec);
        m_scrollbarnotclicked = true;
    }

    void Play()
    {
        if (_Iplayer.isPaused || _Iplayer.isStopped)
        {
            _Iplayer.play();
            playPause.GetComponent<RawImage>().texture = m_texPlay;
        }
        else
        {
            _Iplayer.pause();
            playPause.GetComponent<RawImage>().texture = m_texPause;
        }
    }

    void Stop()
    {
        _Iplayer.stop();
        stopTimeVideo();
        TextureToDraw.texture = Instantiate(m_texLogo);
        playPause.GetComponent<RawImage>().texture = m_texPause;
    }

    void MoveTime(long secondsToAdd)
    {
        _Iplayer.setTime(_Iplayer.currentTime + (secondsToAdd * 1000));
    }
    #endregion

    #region timeInterface
    public void setTimeScrollBar()
    {
        _Iplayer.setTime((long)(scrollBar.value * _Iplayer.totalVideoTime));
        m_scrollbarnotclicked = true;
    }

    public void OnValueChangeScrollBar()
    {
        if (_Iplayer.isPlaying && !m_scrollbarnotclicked)
            _Iplayer.setTime((long)(scrollBar.value * _Iplayer.totalVideoTime));
    }

    public void setTimeIfValueChanged()
    {
        if (m_scrollMemory != scrollBar.value)
        {
            if (_Iplayer.isPaused)
                Play();
            m_scrollMemory = scrollBar.value;
            _Iplayer.setTime((long)(scrollBar.value * _Iplayer.totalVideoTime));
        }
        else
        {
            if (_Iplayer.isPlaying)
                Play();
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
                return _Iplayer.time;
            else
                return (long)((scrollBar.value * _Iplayer.totalVideoTime));
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
                return _Iplayer.videoTime;
            else
                return (long)((scrollBar.value * _Iplayer.totalVideoTime));
        }
    }

    void updateScrollBarPosition()
    {
        if (m_scrollbarnotclicked)
        {
            scrollBar.value = (float)(_Iplayer.currentTime) / _Iplayer.totalVideoTime;
            m_currentTimeScrollBar = (long)(_Iplayer.currentTime * 0.001f);
        }
        else if (m_slaved)
        {
            if (m_forceMove)
                setTimeIfValueChanged();

            m_scrollVal = (loopScroll.value * 4000) - 2000;
            scrollBar.value = (float)(m_timeClick + m_scrollVal) / _Iplayer.totalVideoTime;
            m_currentTimeScrollBar = (long)(_Iplayer.currentTime * 0.001f);

            if (m_keyForceMove)
                finishForceMoveLoopScroll();
        }
        else
        {
            if (m_forceMove)
                setTimeIfValueChanged();
            m_currentTimeScrollBar = (long)(scrollBar.value * _Iplayer.totalVideoTime * 0.001f);
        }
    }

    void updateTimeText()
    {
        displayTimeGUI(currentTimetext, m_currentTimeScrollBar);

        long totalTimeSec = Mathf.RoundToInt(_Iplayer.totalVideoTime * 0.001f);
        displayTimeGUI(totalTimeText, totalTimeSec);
    }

    void displayTimeGUI(Text textGUI, long timeInSec)
    {
        long h = timeInSec / 3600;
        long m = (timeInSec / 60) % 60;
        long s = timeInSec % 60;
        timeToString(textGUI, h, m, s);
    }

    /// <summary>
    /// Create The String to display Current Time
    /// </summary>
    /// <param name="textGUI">Object to display Time</param>
    /// <param name="h">Calculated Hour</param>
    /// <param name="m">Calculated Minute</param>
    /// <param name="s">Calculated Second</param>
    void timeToString(Text textGUI, long h, long m, long s)
    {
        if (h > 0)
            textGUI.text = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
        else
            textGUI.text = returnTimeString(m) + ":" + returnTimeString(s);
    }

    /// <summary>
    /// Convert number Value to String representation
    /// parsed with a possible 0 to represent Time
    /// </summary>
    /// <param name="time">Value To Convert</param>
    /// <returns>\a String to display </returns>
    string returnTimeString(long time)
    {
        if (time < 10)
            return "0" + time;
        else
            return time.ToString();
    }
    #endregion

    #region loopMode
    public void slaveMode()
    {
        if (!loopScroll.gameObject.activeSelf)
        {
            _Iplayer.setVolume(0.0f);
            if (_Iplayer.isPlaying)
                Play();
            m_slaved = true;
            loopScroll.gameObject.SetActive(true);
            m_timeClick = _Iplayer.currentTime;
            m_minTimeClick = m_timeClick - (2 * 1000);
            m_maxTimeClick = m_timeClick + (2 * 1000);
            scrollBar.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = m_texHandleSlave;
        }
        else
        {
            _Iplayer.setVolume(volumeScrollBar.value);
            if (_Iplayer.isPaused)
                Play();
            m_slaved = false;
            m_scrollVal = 0;
            m_timeClick = -1;
            loopScroll.value = 0.5f;
            loopScroll.gameObject.SetActive(false);
            scrollBar.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = m_texHandle;
        }
    }

    public void changeTimeClick(long timeMS)
    {
        m_timeClick = timeMS;
        m_minTimeClick = m_timeClick - (2 * 1000);
        m_maxTimeClick = m_timeClick + (2 * 1000);
    }

    void initForceMoveLoopScroll()
    {
        m_scrollbarnotclicked = false;
        if (_Iplayer.isPaused)
        {
            m_forceMove = true;
            Play();
        }
    }

    void finishForceMoveLoopScroll()
    {
        if (!m_scrollbarnotclicked)
        {
            if (_Iplayer.isPlaying)
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
        loopScroll.value += value;
    }
    #endregion
}