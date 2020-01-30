using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseVideoPlayer : MonoBehaviour, IVideoPlayer
{
    /// <summary>
    /// Exact Time of the video
    /// In MilliSeconds
    /// </summary>
    public virtual long CurrentTime { get; set; } = 0;
    /// <summary>
    /// Time of the video, there is no possible offset due to user input 
    /// In MilliSeconds
    /// </summary>
    public virtual long Time { get; set; } = -1;
    /// <summary>
    /// Exact Time of the video
    /// In MilliSeconds
    /// </summary>
    public virtual long VideoTime { get; set; } = 0;
    /// <summary>
    /// Total wanted Time of the video
    /// In MilliSeconds
    /// </summary>
    public virtual long TotalVideoTime { get; set; } = 0;

    public virtual bool IsPlaying
    {
        get
        {
            return m_playing && !m_paused;
        }
    }

    public virtual bool IsPaused
    {
        get
        {
            return m_playing && m_paused;
        }
    }

    public virtual bool IsStopped
    {
        get
        {
            return !m_playing;
        }
    }

    public byte[] TextureBytes => throw new System.NotImplementedException();

    #region protected members
    protected string m_VideoFilePath = "";
    protected long m_EegFileDurationInSec = 0;
    protected RawImage m_TextureForVideo = null;
    #endregion
    #region private members
    private Stopwatch m_internalTimer = null;
    private long m_internalLastTime = 0;
    private bool m_paused = true, m_playing = false;
    #endregion

    /// <summary>
    /// Init the internal stopwatch of the player and
    /// keep parameters
    /// </summary>
    /// <param name="path">Video File Path</param>
    /// <param name="duration">Eeg File Duration in Seconds</param>
    /// <param name="texture">Raw Image containing texture to draw the video frame on</param>
    public virtual void Init(string path, int duration, RawImage texture)
    {
        m_VideoFilePath = path;
        m_EegFileDurationInSec = duration;
        m_TextureForVideo = texture;

        TotalVideoTime = duration * 1000;
        m_internalTimer = new Stopwatch();
    }

    public virtual void Cleanup() { }

    public virtual void Update()
    {
        if (m_internalTimer != null && m_internalTimer.IsRunning)
        {
            CurrentTime = CurrentTime + (m_internalTimer.ElapsedMilliseconds - m_internalLastTime);
            m_internalLastTime = m_internalTimer.ElapsedMilliseconds;
        }
    }

    public virtual void Play()
    {
        if (m_internalTimer != null)
            m_internalTimer.Start();
        else
            m_internalTimer = Stopwatch.StartNew();

        m_paused = false;
        m_playing = true;
    }

    public virtual void Pause()
    {
        if (!m_paused)
        {
            m_internalTimer.Stop();
            if (m_playing)
                m_paused ^= true;
        }
    }

    public virtual void Stop()
    {
        m_internalTimer.Reset();
        CurrentTime = 0;
        m_internalLastTime = 0;
        m_paused = false;
        m_playing = false;
    }

    public virtual void MoveTime(long secondsToAdd)
    {
        CurrentTime += (secondsToAdd * 1000);
    }

    public virtual void SetTime(long timeMilliSec)
    {
        CurrentTime = timeMilliSec;
    }

    public virtual void SetVolume(float volume) { }

    public virtual void SetVideoOffset(float newOffset) { }
}