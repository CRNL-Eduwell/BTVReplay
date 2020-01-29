using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class BaseVideoPlayer : MonoBehaviour
{
    /// <summary>
    /// Exact Time of the video
    /// In MilliSeconds
    /// </summary>
    protected long CurrentTime { get; set; } = 0;

    /// <summary>
    /// Total wanted Time of the video
    /// In MilliSeconds
    /// </summary>
    protected long TotalTime { get; set; } = 0;

    protected bool IsPlaying
    {
        get
        {
            return m_playing && !m_paused;
        }
    }

    protected bool IsPaused
    {
        get
        {
            return m_playing && m_paused;
        }
    }

    protected bool IsStopped
    {
        get
        {
            return !m_playing;
        }
    }

    private Stopwatch m_internalTimer = null;
    private long m_internalLastTime = 0;
    private bool m_paused = true, m_playing = false;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="duration">Duration of video, in seconds</param>
    public void Init(int duration)
    {
        TotalTime = duration * 1000;
        m_internalTimer = new Stopwatch();
    }

    public virtual void Update()
    {
        if (m_internalTimer != null && m_internalTimer.IsRunning)
        {
            CurrentTime = CurrentTime + (m_internalTimer.ElapsedMilliseconds - m_internalLastTime);
            m_internalLastTime = m_internalTimer.ElapsedMilliseconds;
        }
    }

    public void Play()
    {
        if (m_internalTimer != null)
            m_internalTimer.Start();
        else
            m_internalTimer = Stopwatch.StartNew();

        m_paused = false;
        m_playing = true;
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
        CurrentTime = 0;
        m_internalLastTime = 0;
        m_paused = false;
        m_playing = false;
    }

    public void SetTime(long timeMilliSec)
    {
        m_internalTimer.Reset();
        CurrentTime = timeMilliSec;
        m_internalLastTime = 0;
        m_internalTimer.Start();
    }

}
