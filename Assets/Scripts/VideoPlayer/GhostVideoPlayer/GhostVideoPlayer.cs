using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an instance of a fake Video Player.
/// This allow us to visualise EEG Data the same way we would with the video
/// Of an experiment.
/// This is an extrapolation generated with a timer to simulate a real video
/// according to the length of an EEG file
/// </summary>
public class GhostVideoPlayer : MonoBehaviour, IVideoPlayer
{
    public bool IsPrepared { get; private set; } = false;
    /// <summary>
    /// Exact Time of the video
    /// In MilliSeconds
    /// </summary>
    public long CurrentTime { get; set; } = 0;
    /// <summary>
    /// Time of the video, there is no possible offset due to user input 
    /// In MilliSeconds
    /// </summary>
    public long Time { get; set; } = -1;
    /// <summary>
    /// Exact Time of the video
    /// In MilliSeconds
    /// </summary>
    public long VideoTime { get; set; } = 0;
    /// <summary>
    /// Total wanted Time of the video
    /// In MilliSeconds
    /// </summary>
    public long TotalVideoTime { get; set; } = 0;

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

    public byte[] TextureBytes => throw new System.NotImplementedException();

    #region private members
    private string m_VideoFilePath = "";
    private RawImage m_TextureForVideo = null;
    private Stopwatch m_internalTimer = null;
    private long m_internalLastTime = 0;
    private bool m_paused = true, m_playing = false;
    private RectTransform m_parentRectTransform = null;
    private float WidthToHeightRatio = 0.0f, HeightToWidthRatio = 0.0f;
    #endregion

    /// <summary>
    /// Init the internal stopwatch of the player and
    /// keep parameters
    /// </summary>
    /// <param name="path">Video File Path</param>
    /// <param name="duration">Eeg File Duration in Milliseconds</param>
    /// <param name="texture">Raw Image containing texture to draw the video frame on</param>
    public void Init(string path, int duration, RawImage texture)
    {
        m_VideoFilePath = path;
        m_TextureForVideo = texture;
        TotalVideoTime = duration;
        m_internalTimer = new Stopwatch();

        m_parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    public void Cleanup() { }

    public void Update()
    {
        if (m_internalTimer != null && m_internalTimer.IsRunning)
        {
            CurrentTime = CurrentTime + (m_internalTimer.ElapsedMilliseconds - m_internalLastTime);
            m_internalLastTime = m_internalTimer.ElapsedMilliseconds;
            if (CurrentTime > TotalVideoTime)
                SetTime(0);
        }
    }

    public void Play()
    {
        if (!m_paused && !m_playing)
        {
            WidthToHeightRatio = (float)m_TextureForVideo.texture.width / m_TextureForVideo.texture.height;
            HeightToWidthRatio = (float)m_TextureForVideo.texture.height / m_TextureForVideo.texture.width;
            ResizeTexture();
            IsPrepared = true;
        }

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
        IsPrepared = false;
    }

    public void MoveTime(long secondsToAdd)
    {
        CurrentTime += (secondsToAdd * 1000);
    }

    public void SetTime(long timeMilliSec)
    {
        CurrentTime = timeMilliSec;
    }

    public void SetVolume(float volume) { }

    public void SetVideoOffset(float newOffset) { }

    private void ResizeTexture()
    {
        if (m_parentRectTransform == null) return;

        float resizingWidth = m_parentRectTransform.rect.height * WidthToHeightRatio;
        float resizingHeight = m_parentRectTransform.rect.width * HeightToWidthRatio;

        float width = (resizingHeight >= m_parentRectTransform.rect.height) ? resizingWidth : m_parentRectTransform.rect.width;
        float height = (resizingHeight >= m_parentRectTransform.rect.height) ? m_parentRectTransform.rect.height : resizingHeight;
        m_TextureForVideo.rectTransform.sizeDelta = new Vector2(width, height);
    }
}