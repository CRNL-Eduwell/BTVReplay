using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.
using System;
using System.IO;
using System.Collections; //IEnumerator
using CielaSpike;

public delegate void timeVideo(int currentTime);
public delegate void timeVideoSync(int currentTime);
public delegate void stopVideo();

public class VideoPlayer : MonoBehaviour
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
    public float SamplingFrequency
    {
        get
        {
            return sampFreq;
        }
    }
    public string VideoPath
    {
        get
        {
            return m_vidPath;
        }
        set
        {
            m_vidPath = value;
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

    #region scene members
    [SerializeField] optionsHub hub = null;
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
    #endregion

    private WavReader _wavReader = null;
    private IVideoPlayer _Iplayer = null;
    private bool scrollbarnotclicked = true, initDone = false, forceMove = false;
    private EventTrigger trigger = null;
    private Texture2D texPlay = null, texPause = null, texLogo = null;
    private Sprite texHandle = null, texHandleSlave = null;
    private float sampFreq = 0;

    private bool slaved = false, keyForceMove = false;
    private EventTrigger triggerSlaved = null;
    private long timeClick = -1;
    private float minTC = 0.0f, maxTC = 0.0f;
    private float scrollVal = 0.0f, memSc = 0.0f;
    private long currentTimeScrollBar = 0;

    private string m_vidPath = "";

    private void Awake()
    {
        media.loadVideo += new initVideo(init);
    }

    private void OnDestroy()
    {
        media.loadVideo -= new initVideo(init);
        if (initDone)
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

            for (int i = 0; i < trigger.triggers.Count; i++)
                trigger.triggers[i].callback.RemoveAllListeners();

            for (int i = 0; i < triggerSlaved.triggers.Count; i++)
                triggerSlaved.triggers[i].callback.RemoveAllListeners();

            Destroy(trigger); //Not done before if it bugs ? 
            Destroy(triggerSlaved); //Not done before if it bugs ? 
        }
    }

    private void Start()
    {
        texPause = Resources.Load("Pictures/playIcone", typeof(Texture2D)) as Texture2D;
        texPlay = Resources.Load("Pictures/pauseIcone", typeof(Texture2D)) as Texture2D;
        texLogo = Resources.Load("Pictures/BTVLogo", typeof(Texture2D)) as Texture2D;
        texHandle = Resources.Load("Pictures/handleScroll", typeof(Sprite)) as Sprite;
        texHandleSlave = Resources.Load("Pictures/handleSlave", typeof(Sprite)) as Sprite;
    }

    private void Update()
    {
        if (initDone)
        {
            if (_Iplayer.isPlaying)
            {
                _Iplayer.update();
                if (_Iplayer.currentTime > _Iplayer.totalVideoTime)
                    Stop();

                if (slaved && !forceMove)
                {
                    if (_Iplayer.currentTime > timeClick + 2000)
                        setTime((int)timeClick - 2000);
                    if (_Iplayer.currentTime < timeClick - 2000)
                        setTime((int)timeClick + 2000);
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

            if (slaved)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.J))
                    forceMoveLoopScroll(-0.05f);

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.K))
                    forceMoveLoopScroll(0.05f);
            }

        }
    }

    #region implement Interface
    void init(string videoPath, int eegSampFreq, int eegFileDurationInSec)
    {
        VideoPath = videoPath;
        if (needAudioProcess)
            hub.videoRemote.setButtonsInteractable(true);
        else
            hub.videoRemote.setButtonsInteractable(false);

        sampFreq = eegSampFreq;

        if (videoPath == "")
            _Iplayer = gameObject.AddComponent<VLCLess>();
        else
            _Iplayer = gameObject.AddComponent<VLCSharp.VLCSharp>();

        TextureToDraw.texture = (Texture2D)Instantiate(texLogo);

        _Iplayer.getVideoReference(TextureToDraw, hub);
        _Iplayer.init(videoPath, eegSampFreq, eegFileDurationInSec);
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

        trigger = scrollBar.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((eventData) =>
        {
            if (!slaved)
                initForceMoveLoopScroll();
        });
        trigger.triggers.Add(entry);

        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerUp;
        entry2.callback.AddListener((eventData) => 
        {
            if (!scrollbarnotclicked)
            {
                if (forceMove)
                    forceMove = false;
                
                setTimeIfValueChanged();
                scrollbarnotclicked = true;
            }
        });
        trigger.triggers.Add(entry2);

        triggerSlaved = loopScroll.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry3 = new EventTrigger.Entry();
        entry3.eventID = EventTriggerType.BeginDrag; 
        entry3.callback.AddListener((eventData) => 
        {
            initForceMoveLoopScroll();
        });
        triggerSlaved.triggers.Add(entry3);

        EventTrigger.Entry entry4 = new EventTrigger.Entry();
        entry4.eventID = EventTriggerType.EndDrag;
        entry4.callback.AddListener((eventData) =>
        {
            finishForceMoveLoopScroll();
        });
        triggerSlaved.triggers.Add(entry4);

        initDone = true;
    }

    public void setTime(int timeMilliSec)
    {
        scrollbarnotclicked = false;
        _Iplayer.setTime(timeMilliSec);
        scrollbarnotclicked = true;
    }

    void Play()
    {
        if (_Iplayer.isPaused || _Iplayer.isStopped)
        {
            _Iplayer.play();
            playPause.GetComponent<RawImage>().texture = texPlay;
        }
        else
        {
            _Iplayer.pause();
            playPause.GetComponent<RawImage>().texture = texPause;
        }
    }

    void Stop()
    {
        _Iplayer.stop();
        stopTimeVideo();
        TextureToDraw.texture = Instantiate(texLogo);
        playPause.GetComponent<RawImage>().texture = texPause;
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
        scrollbarnotclicked = true;
    }

    public void OnValueChangeScrollBar()
    {
        if (_Iplayer.isPlaying && !scrollbarnotclicked)
            _Iplayer.setTime((long)(scrollBar.value * _Iplayer.totalVideoTime));
    }

    public void setTimeIfValueChanged()
    {
        if (memSc != scrollBar.value)
        {
            if (_Iplayer.isPaused)
                Play();
            memSc = scrollBar.value;
            _Iplayer.setTime((long)(scrollBar.value * _Iplayer.totalVideoTime));
        }
        else
        {
            if (_Iplayer.isPlaying)
                Play();
        }
    }

    long Time
    {
        get
        {
            if (scrollbarnotclicked)
                return _Iplayer.time;
            else
                return (long)((scrollBar.value * _Iplayer.totalVideoTime) * (sampFreq / 1000));
        }
    }

    long videoTime
    {
        get
        {
            if (scrollbarnotclicked)
                return _Iplayer.videoTime;
            else
                return (long)((scrollBar.value * _Iplayer.totalVideoTime) * (sampFreq / 1000));
        }
    }

    void updateScrollBarPosition()
    {
        if (scrollbarnotclicked)
        {
            scrollBar.value = (float)(_Iplayer.currentTime) / _Iplayer.totalVideoTime;
            currentTimeScrollBar = (long)(_Iplayer.currentTime * 0.001f);
        }
        else if (slaved)
        {
            if (forceMove)
                setTimeIfValueChanged();

            scrollVal = (loopScroll.value * 4000) - 2000;
            scrollBar.value = (float)(timeClick + scrollVal) / _Iplayer.totalVideoTime;
            currentTimeScrollBar = (long)(_Iplayer.currentTime * 0.001f);

            if (keyForceMove)
                finishForceMoveLoopScroll();
        }
        else
        {
            if (forceMove)
                setTimeIfValueChanged();
            currentTimeScrollBar = (long)(scrollBar.value * _Iplayer.totalVideoTime * 0.001f);
        }
    }

    void updateTimeText()
    {
        displayTimeGUI(currentTimetext, currentTimeScrollBar);

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

    void timeToString(Text textGUI, long h, long m, long s)
    {
        if (h > 0)
            textGUI.text = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
        else
            textGUI.text = returnTimeString(m) + ":" + returnTimeString(s);
    }

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
            slaved = true;
            loopScroll.gameObject.SetActive(true);
            timeClick = _Iplayer.currentTime;
            minTC = timeClick - (2 * 1000);
            maxTC = timeClick + (2 * 1000);
            scrollBar.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = texHandleSlave;
        }
        else
        {
            _Iplayer.setVolume(volumeScrollBar.value);
            if (_Iplayer.isPaused)
                Play();
            slaved = false;
            scrollVal = 0;
            timeClick = -1;
            loopScroll.value = 0.5f;
            loopScroll.gameObject.SetActive(false);
            scrollBar.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = texHandle;
        }
    }

    public void changeTimeClick(long timeMS)
    {
        timeClick = timeMS;
        minTC = timeClick - (2 * 1000);
        maxTC = timeClick + (2 * 1000);
    }

    void initForceMoveLoopScroll()
    {
        scrollbarnotclicked = false;
        if (_Iplayer.isPaused)
        {
            forceMove = true;
            Play();
        }
    }

    void finishForceMoveLoopScroll()
    {
        if (!scrollbarnotclicked)
        {          
            if (_Iplayer.isPlaying)
                Play();

            forceMove = false;
            scrollbarnotclicked = true;
            keyForceMove = false;
        }
    }

    void forceMoveLoopScroll(float value)
    {
        keyForceMove = true;
        initForceMoveLoopScroll();
        loopScroll.value += value;
    }
    #endregion

    #region WawReader
    public IEnumerator c_filterAudio()
    {
        yield return Ninja.JumpBack;
        yield return StartCoroutine(c_loadAudio());
        float sampFreq = ELAN.getSamplingFreq(media.elanFiles);
        yield return filterAudio(_wavReader, (int)sampFreq);
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
}