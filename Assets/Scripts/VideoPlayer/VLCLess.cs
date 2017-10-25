using System.Diagnostics; //Requiered for Stopwatch
using UnityEngine;
using UnityEngine.UI;

public class VLCLess : MonoBehaviour, IVideoPlayer
{
    public long currentTime
    {
        get
        {
            return internalTime;
        }
    }
    public long time
    {
        get
        {
            return (long)(currentTime * ((float)eegSampFreq / 1000));
        }
    }
    public long videoTime
    {
        get
        {
            return -1;
        }
    }

    public long totalVideoTime //In MilliSec
    {
        get
        {
            return eegFileDurationInSec * 1000;
        }
    }
    public bool isPlaying
    {
        get
        {
            return playing && !paused;
        }
    }
    public bool isPaused
    {
        get
        {
            return playing && paused;
        }
    }
    public bool isStopped
    {
        get
        {
            return !playing;
        }
    }
    public byte[] textureBytes
    {
        get
        {
            return null;
        }
    }
    //==
    private string videoPath = "";
    private int eegSampFreq = 0;
    private long eegFileDurationInSec = 0;
    //==
    private object objectLock = new object();
    private Stopwatch internalTimer = null;
    private long internalTime = 0;
    private long internalLastTime = 0;
    private bool paused = true, playing = false;

    public void init(string videoPath, int eegSampFreq, int eegFileDurationInSec)
    {
        this.videoPath = videoPath;
        this.eegSampFreq = eegSampFreq;
        this.eegFileDurationInSec = eegFileDurationInSec;
        internalTimer = new Stopwatch();
    }

    public void getVideoReference(RawImage tex, optionsHub hub)
    {

    }

    public void cleanup()
    {

    }

    public void update()
    {
        if (internalTimer != null && internalTimer.IsRunning)
        {
            internalTime = internalTime + (internalTimer.ElapsedMilliseconds - internalLastTime);
            internalLastTime = internalTimer.ElapsedMilliseconds;
            //if (internalTime < totalVideoTime)
            //    sendTimeEvent((int)time);
            //else
            //    stop();
            if (internalTime > totalVideoTime)
                stop();
        }
    }

    public void play()
    {
        //if (paused)
        //{
            if (internalTimer != null)
                internalTimer.Start();
            else
                internalTimer = Stopwatch.StartNew();

            paused = false;
            playing = true;
        //}
    }

    public void pause()
    {
        if (!paused)
        {
            internalTimer.Stop();
            if (playing)
                paused ^= true;
        }
    }

    public void stop()
    {
        internalTimer.Reset();
        internalTime = 0;
        internalLastTime = 0;
        paused = false;
        playing = false;
    }

    public void moveTime(long secondsToAdd)
    {
        internalTime = internalTime + (secondsToAdd * 1000);
    }

    public void setTime(long timeMilliSec)
    {
        internalTime = timeMilliSec;
    }

    public void setVolume(float volume)
    {

    }
}