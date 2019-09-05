using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.
using System;
using System.IO;
using System.Diagnostics;
using System.Collections; //IEnumerator
using CielaSpike;
using BTV.Services.EegFileService;
using BTV.Data.DataContainer;

public delegate void timeVideo(int currentTime);
public delegate void timeVideoSync(int currentTime);
public delegate void stopVideo();

/// <summary>
/// Represents an instance of a video, either from a real video
/// or a simulated video just to view eeg data
/// </summary>
public class CustomVideoPlayer : MonoBehaviour
{
    public WavReader audioWav
    {
        get
        {
            return _wavReader;
        }
        set
        {
            _wavReader = value;
        }
    }
    public IVideoPlayer videoInterface
    {
        get
        {
            return _Iplayer;
        }
    }
    public string VideoPath
    {
        get
        {
            return m_videoPath;
        }
        set
        {
            m_videoPath = value;
        }
    }
    public string AudioPath
    {
        get
        {
            string[] videoPathSplit = VideoPath.Split('.');
            if (videoPathSplit.Length > 0)
                return VideoPath.Replace("." + videoPathSplit[videoPathSplit.Length - 1], ".wav");
            else
                return "";
        }
    }
    public string AudioFilteredPath
    {
        get
        {
            string[] audioPathSplit = AudioPath.Split('.');
            if (audioPathSplit.Length > 0)
                return AudioPath.Replace("." + audioPathSplit[audioPathSplit.Length - 1], "_audio.csv");
            else
                return "";
        }
    }
    public bool needAudioProcess
    {
        get
        {
            if (File.Exists(AudioPath) && File.Exists(AudioFilteredPath))
                return false;
            else
                return true;
        }
    }
    //==
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
    [SerializeField] videoRecorder VideoRecorder = null;
    [SerializeField] Button recordVideo = null;
    #endregion

    #region private members
    private WavReader _wavReader = null;
    private IVideoPlayer _Iplayer = null;
    private bool m_scrollbarnotclicked = true, m_initDone = false, m_forceMove = false;
    private EventTrigger m_trigger = null;
    private Texture2D m_texPlay = null, m_texPause = null, m_texLogo = null;
    private Sprite m_texHandle = null, m_texHandleSlave = null;

    private bool m_slaved = false, m_keyForceMove = false;
    private EventTrigger m_triggerSlaved = null;
    private long m_timeClick = -1;
    private float m_minTimeClick = 0.0f, m_maxTimeClick = 0.0f;
    private float m_scrollVal = 0.0f, m_scrollMemory = 0.0f;
    private long m_currentTimeScrollBar = 0;
    private string m_videoPath = "";
    #endregion

    private void Awake()
    {
        media.loadVideo += new initVideo(init);
        VideoRecorder.recordVideo += new launchRecordVideo(record);
    }

    private void OnDestroy()
    {
        media.loadVideo -= new initVideo(init);
        VideoRecorder.recordVideo -= new launchRecordVideo(record);
        if (m_initDone)
        {
            _Iplayer.cleanup();

            if (_wavReader != null)
                _wavReader.Dispose();

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
        VideoPath = videoPath;
        
        //TODO : rebrancher proprement 
        //if (needAudioProcess)
        //    hub.videoRemote.setButtonsInteractable(true);
        //else
        //    hub.videoRemote.setButtonsInteractable(false);

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
            GameObject recordPanel = GameObject.Find("Canvas").transform.GetChild(6).gameObject;
            recordPanel.SetActive(!recordPanel.activeSelf);
            //StartCoroutine(record());
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

    #region WawReader
    public IEnumerator c_filterAudio()
    {
        yield return Ninja.JumpBack;
        yield return StartCoroutine(c_loadAudio());
        DataContainer container = EegFileService.ReturnFirstValidContainer();
        yield return filterAudio(_wavReader, container.Frequency.Value);
        yield return Ninja.JumpToUnity;
        yield return null;
    }

    public IEnumerator c_loadAudio()
    {
        string audioPath = AudioPath;

        if (new FileInfo(audioPath).Exists == false)
        {
            yield return Ninja.JumpBack;
            yield return PrepareAudio(audioPath, VideoPath);
            yield return Ninja.JumpToUnity;
        }
        yield return Ninja.JumpBack;
        yield return loadAudio(audioPath, r => _wavReader = r);
        yield return Ninja.JumpToUnity; //recomm si jamais
        yield return null;
    }

    YieldInstruction PrepareAudio(string audioPath, string videoPath)
    {
        // I give my callback to the process
        // Async needed for another thread and not freezing/laging UI
        return this.StartCoroutineAsync(WavReader.c_extractAudio(audioPath, videoPath));
    }

    YieldInstruction loadAudio(string audioPath, Action<WavReader> resWav)
    {
        // I give my callback to the process
        // Async needed for another thread and not freezing/laging UI
        //return this.StartCoroutineAsync(WavReader.c_loadAudioFile(audioPath, resWav));
        return this.StartCoroutineAsync(WavReader.c_loadAudioFile(audioPath, resWav));

    }

    YieldInstruction filterAudio(WavReader wav, int samplingFreq)
    {
        // I give my callback to the process
        // Async needed for another thread and not freezing/laging UI
        //return this.StartCoroutineAsync(wav.ToHilbert("300:100:1300", samplingFreq));
        return this.StartCoroutineAsync(wav.c_ToHilbert("300:100:1300", samplingFreq));
    }
    #endregion

    public void record(string outVideoPath, string durationInSeconds)
    {
        StartCoroutine(record2(outVideoPath, durationInSeconds));
    }

    public IEnumerator record2(string outVideoPath, string durationInSeconds)
    {
        yield return Ninja.JumpBack;
        yield return recordVideo2(outVideoPath, durationInSeconds);
        yield return Ninja.JumpToUnity;
    }

    YieldInstruction recordVideo2(string outVideoPath, string durationInSeconds)
    {
        return this.StartCoroutineAsync(c_startRecording(outVideoPath, durationInSeconds));
    }

    IEnumerator c_startRecording(string outVideoPath, string durationInSeconds)
    {
        Process m_recordProcess = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";

        string cmd = "VLC -I dummy-quiet screen:// --screen-fps 25 --sout ^\"#transcode{vcodec=h264,venc=x264, vb=1500,acodec=none,scale=1.0}:std{access=file,mux=mp4,dst=" + outVideoPath + "}\" --stop-time " + durationInSeconds.ToString() + " vlc://quit";

        startInfo.Arguments = "/c " + cmd;
        m_recordProcess.StartInfo = startInfo;
        m_recordProcess.Start();
        m_recordProcess.WaitForExit();

        yield return Ninja.JumpToUnity;
        ApplicationState.displayMessage("Video Record", "OK", "Video as been correctly recorded. \n Please Check the output path you have provided.");
        yield return Ninja.JumpBack;

        yield return null;
    }
}