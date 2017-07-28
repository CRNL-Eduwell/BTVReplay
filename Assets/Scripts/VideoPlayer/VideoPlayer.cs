using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.

public delegate void timeVideo(int currentTime);

public class VideoPlayer : MonoBehaviour
{
    public IVideoPlayer videoInterface
    {
        get
        {
            return _Iplayer;
        }
    }
    public event timeVideo sendTime;

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
    #endregion

    private IVideoPlayer _Iplayer = null;
    private bool scrollbarnotclicked = true, initDone = false;
    private EventTrigger trigger = null;
    private Texture2D texPlay = null, texPause = null, texLogo = null;
    private float sampFreq = 0;

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
            playPause.onClick.RemoveAllListeners();
            stopButton.onClick.RemoveAllListeners();
            backTime10.onClick.RemoveAllListeners();
            backTime1.onClick.RemoveAllListeners();
            frontTime10.onClick.RemoveAllListeners();
            volumeScrollBar.onValueChanged.RemoveAllListeners();

            for (int i = 0; i < trigger.triggers.Count; i++)
                trigger.triggers[i].callback.RemoveAllListeners();

            Destroy(trigger); //Not done before if it bugs ? 
        }
    }

    private void Start()
    {
        texPause = Resources.Load("Pictures/playIcone", typeof(Texture2D)) as Texture2D;
        texPlay = Resources.Load("Pictures/pauseIcone", typeof(Texture2D)) as Texture2D;
        texLogo = Resources.Load("Pictures/BTVLogo", typeof(Texture2D)) as Texture2D;
    }

    void Update()
    {
        if (initDone && _Iplayer.isPlaying)
        {
            _Iplayer.update();
            if (_Iplayer.currentTime > _Iplayer.totalVideoTime)
                Stop();

            updateScBarPosAndTimeText();
            sendTime((int)Time);
        }
    }

    #region implement Interface
    void init(string videoPath, int eegSampFreq, int eegFileDurationInSec)
    {
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
        entry.callback.AddListener((eventData) => { scrollbarnotclicked = false; });
        trigger.triggers.Add(entry);

        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerUp;
        entry2.callback.AddListener((eventData) => { setTimeScrollBar(); });
        trigger.triggers.Add(entry2);

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
        long newTimeValue = (long)(scrollBar.value * _Iplayer.totalVideoTime);
        _Iplayer.setTime(newTimeValue);
        scrollbarnotclicked = true;
    }

    long Time
    {
        get
        {
            if (scrollbarnotclicked)
                return _Iplayer.time;
            else
                return (long)(scrollBar.value * _Iplayer.totalVideoTime * (sampFreq / 1000));
        }
    }

    void updateScBarPosAndTimeText()
    {
        if (scrollbarnotclicked)
            scrollBar.value = (float)_Iplayer.currentTime / _Iplayer.totalVideoTime;

        long currentTimeScrollBar = (long)(scrollBar.value * _Iplayer.totalVideoTime);
        long timeSec = Mathf.RoundToInt(currentTimeScrollBar * 0.001f);
        displayTimeGUI(currentTimetext, timeSec);

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
}