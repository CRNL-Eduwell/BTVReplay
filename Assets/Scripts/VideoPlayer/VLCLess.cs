using System.Diagnostics; //Requiered for Stopwatch
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an instance of a "fake" Video Reader.
/// This allow us to visualise EEG Data the same way we would with the video
/// Of an experiment.
/// This is an extrapolation generated with a timer To simulate a real video
/// according to the length of an EEG file
/// </summary>
public class VLCLess : MonoBehaviour, IVideoPlayer
{
    /// <summary>
    /// Exact Time of the video
    /// In MilliSeconds
    /// </summary>
    public long currentTime
    {
        get
        {
            return m_internalTime;
        }
    }

    /// <summary>
    /// Time of the video, there is no possible offset due to user input since 
    /// In MilliSeconds
    /// </summary>
    public long time
    {
        get
        {
            return currentTime;
        }
    }

    /// <summary>
    /// Exact Time of the video
    /// In MilliSeconds
    /// </summary>
    public long videoTime
    {
        get
        {
            return -1;
        }
    }

    /// <summary>
    /// Total Duration of the Video
    /// In MilliSeconds
    /// </summary>
    public long totalVideoTime
    {
        get
        {
            return m_eegFileDurationInSec * 1000;
        }
    }
    public bool isPlaying
    {
        get
        {
            return m_playing && !m_paused;
        }
    }
    public bool isPaused
    {
        get
        {
            return m_playing && m_paused;
        }
    }
    public bool isStopped
    {
        get
        {
            return !m_playing;
        }
    }
    public byte[] textureBytes
    {
        get
        {
            return null;
        }
    }

    #region private members
    private string m_videoPath = "";
    private long m_eegFileDurationInSec = 0;
    private Stopwatch m_internalTimer = null;
    private long m_internalTime = 0, m_internalLastTime = 0;
    private bool m_paused = true, m_playing = false;
    #endregion

    public void init(string videoPath, int eegFileDurationInSec)
    {
        m_videoPath = videoPath;
        m_eegFileDurationInSec = eegFileDurationInSec;
        m_internalTimer = new Stopwatch();
    }

    public void getVideoReference(RawImage tex, optionsHub hub)
    {

    }

    public void cleanup()
    {

    }

    public void update()
    {
        if (m_internalTimer != null && m_internalTimer.IsRunning)
        {
            m_internalTime = m_internalTime + (m_internalTimer.ElapsedMilliseconds - m_internalLastTime);
            m_internalLastTime = m_internalTimer.ElapsedMilliseconds;
            //if (internalTime < totalVideoTime)
            //    sendTimeEvent((int)time);
            //else
            //    stop();
            if (m_internalTime > totalVideoTime)
                stop();
        }
    }

    public void play()
    {
        //if (paused)
        //{
            if (m_internalTimer != null)
                m_internalTimer.Start();
            else
                m_internalTimer = Stopwatch.StartNew();

            m_paused = false;
            m_playing = true;
        //}
    }

    public void pause()
    {
        if (!m_paused)
        {
            m_internalTimer.Stop();
            if (m_playing)
                m_paused ^= true;
        }
    }

    public void stop()
    {
        m_internalTimer.Reset();
        m_internalTime = 0;
        m_internalLastTime = 0;
        m_paused = false;
        m_playing = false;
    }

    public void moveTime(long secondsToAdd)
    {
        m_internalTime = m_internalTime + (secondsToAdd * 1000);
    }

    public void setTime(long timeMilliSec)
    {
        m_internalTime = timeMilliSec;
    }

    public void setVolume(float volume)
    {

    }
}