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
    public long CurrentTime
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
    public long Time
    {
        get
        {
            return CurrentTime;
        }
    }

    /// <summary>
    /// Exact Time of the video
    /// In MilliSeconds
    /// </summary>
    public long VideoTime
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
    public long TotalVideoTime
    {
        get
        {
            return m_eegFileDurationInSec * 1000;
        }
    }
    public bool IsPlaying
    {
        get
        {
            return m_playing && !m_paused;
        }
    }
    public bool IsPaused
    {
        get
        {
            return m_playing && m_paused;
        }
    }
    public bool IsStopped
    {
        get
        {
            return !m_playing;
        }
    }
    public byte[] TextureBytes
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

    public void Init(string videoPath, int eegFileDurationInSec, RawImage tex)
    {
        m_videoPath = videoPath;
        m_eegFileDurationInSec = eegFileDurationInSec;
        m_internalTimer = new Stopwatch();
    }

    public void SetVideoOffset(float newOffset)
    {

    }

    public void Cleanup()
    {

    }

    public void Update()
    {
        if (m_internalTimer != null && m_internalTimer.IsRunning)
        {
            m_internalTime = m_internalTime + (m_internalTimer.ElapsedMilliseconds - m_internalLastTime);
            m_internalLastTime = m_internalTimer.ElapsedMilliseconds;
            //if (internalTime < totalVideoTime)
            //    sendTimeEvent((int)time);
            //else
            //    stop();
            if (m_internalTime > TotalVideoTime)
                Stop();
        }
    }

    public void Play()
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

    public void Pause()
    {
        if (!m_paused)
        {
            m_internalTimer.Stop();
            if (m_playing)
                m_paused ^= true;
        }
    }

    public void Stop()
    {
        m_internalTimer.Reset();
        m_internalTime = 0;
        m_internalLastTime = 0;
        m_paused = false;
        m_playing = false;
    }

    public void MoveTime(long secondsToAdd)
    {
        m_internalTime = m_internalTime + (secondsToAdd * 1000);
    }

    public void SetTime(long timeMilliSec)
    {
        m_internalTime = timeMilliSec;
    }

    public void SetVolume(float volume)
    {

    }
}