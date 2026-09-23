using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UnityVideoPlayer : MonoBehaviour, IVideoPlayer
{
    public event Action SeekCompleted;

    public bool IsPrepared { get { return m_VideoPlayer != null ? m_VideoPlayer.isPrepared : false; } }
    /// <summary>
    /// Same as time, see if both are usefull ????
    /// </summary>
    public long ClockTime { get { return (long)((m_VideoPlayer.clockTime * 1000) + m_OffsetVideoMilliSec); } }

    public long Time { get { return (long)((m_VideoPlayer.time * 1000) + m_OffsetVideoMilliSec); } }

    public long VideoTime { get { return ClockTime - m_OffsetVideoMilliSec; } }

    // Deliberately cached at prepareCompleted instead of reading m_VideoPlayer.length live:
    // length could read 0 mid-seek while dragging the scrollbar (historical Unity issue), and
    // every module divides by this value.
    public long TotalVideoTime { get; set; }

    public bool IsPlaying { get { return m_VideoPlayer.isPlaying && !m_VideoPlayer.isPaused; } }

    public bool IsPaused { get { return m_VideoPlayer.isPaused; } }

    public bool IsStopped { get { return !m_VideoPlayer.isPrepared; } }

    public byte[] TextureBytes => throw new System.NotImplementedException();

    #region private members
    private string m_VideoFilePath = "";
    private long m_EegFileDurationInSec = 0;
    private RawImage m_TextureForVideo = null;
    private VideoPlayer m_VideoPlayer = null;
    private int m_OffsetVideoMilliSec = 0;
    private RectTransform m_parentRectTransform = null;
    private float WidthToHeightRatio = 0.0f, HeightToWidthRatio = 0.0f;
    #endregion

    /// <summary>
    /// Init the internal stopwatch of the player and
    /// init a Unity Vide Player component that will handle
    /// all the rendering of the video
    /// </summary>
    /// <param name="path">Video File Path</param>
    /// <param name="duration">Eeg File Duration in Milliseconds</param>
    /// <param name="texture">Raw Image containing texture to draw the video frame on</param>
    public void Init(string path, int duration, RawImage texture)
    {
        m_VideoFilePath = path;
        m_EegFileDurationInSec = duration;
        m_TextureForVideo = texture;

        m_VideoPlayer = gameObject.AddComponent<VideoPlayer>();
        m_VideoPlayer.prepareCompleted += M_VideoPlayer_prepareCompleted;
        m_VideoPlayer.seekCompleted += M_VideoPlayer_seekCompleted;
        m_parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    private void M_VideoPlayer_seekCompleted(VideoPlayer source)
    {
        SeekCompleted?.Invoke();
    }

    private void M_VideoPlayer_prepareCompleted(VideoPlayer source)
    {
        //Delete old texture or BTVLogo 
        Texture oldTexture = m_TextureForVideo.texture;
        Destroy(oldTexture);

        //Create New Render Texture and assign it to the videoplayer
        m_TextureForVideo.texture = new RenderTexture(source.texture.width, source.texture.height, 0, RenderTextureFormat.ARGB32);
        source.targetTexture = (RenderTexture)(m_TextureForVideo.texture);
        WidthToHeightRatio = (float)m_TextureForVideo.texture.width / m_TextureForVideo.texture.height;
        HeightToWidthRatio = (float)m_TextureForVideo.texture.height / m_TextureForVideo.texture.width;
        ResizeTexture();

        Play();

        TotalVideoTime = (long)(m_VideoPlayer.length * 1000);
    }

    public void Tick()
    {

    }

    private void OnRectTransformDimensionsChange()
    {
        ResizeTexture();
    }

    public void Play()
    {
        if (!m_VideoPlayer.isPrepared)
        {
            StartCoroutine(PrepareVideoPlayerRessources());
            return;
        }

        if (m_VideoPlayer.isPaused || m_VideoPlayer.isPrepared)
        {
            m_VideoPlayer.Play();
        }
    }

    public void Pause()
    {
        if (m_VideoPlayer.isPlaying)
        {
            m_VideoPlayer.Pause();
        }
    }

    public void Stop()
    {
        m_VideoPlayer.Stop();
        //Delete old texture or BTVLogo 
        Texture oldTexture = m_TextureForVideo.texture;
        Destroy(oldTexture);
    }

    public void MoveTime(long secondsToAdd)
    {
        SetTime((long)(m_VideoPlayer.time * 1000) + (secondsToAdd * 1000));
    }

    public void SetTime(long timeMilliSec)
    {
        double seconds = Math.Max(0, (double)timeMilliSec / 1000);
        // Seek by frame, not by time: setting .time lands on a codec/keyframe-dependent
        // position, while setting .frame is exact - the displayed frame then matches the
        // requested time deterministically.
        if (m_VideoPlayer.isPrepared && m_VideoPlayer.frameRate > 0)
            m_VideoPlayer.frame = (long)Math.Round(seconds * m_VideoPlayer.frameRate);
        else
            m_VideoPlayer.time = seconds;
    }

    public void SetVolume(float volume)
    {
        m_VideoPlayer.SetDirectAudioVolume(0, volume);
    }

    private IEnumerator PrepareVideoPlayerRessources()
    {
        // Play on awake defaults to true.
        m_VideoPlayer.playOnAwake = false;
        m_VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        m_VideoPlayer.url = m_VideoFilePath;
        m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        // Skip the first 100 frames.
        m_VideoPlayer.frame = 0;
        m_VideoPlayer.EnableAudioTrack(0, true);
        // Do NOT loop: CustomVideoPlayer stops playback once ClockTime passes TotalVideoTime.
        // With looping on, the clock silently wraps to 0 and that stop never fires, desyncing
        // the EEG/video. Let the clock reach the end so the stop logic runs.
        m_VideoPlayer.isLooping = false;
        m_VideoPlayer.waitForFirstFrame = true;

        m_VideoPlayer.Prepare();

        while (!m_VideoPlayer.isPrepared)
            yield return null;
    }

    public void Cleanup()
    {
        if (m_VideoPlayer != null)
        {
            m_VideoPlayer.prepareCompleted -= M_VideoPlayer_prepareCompleted;
            m_VideoPlayer.seekCompleted -= M_VideoPlayer_seekCompleted;
            // Destroying this wrapper does not destroy the engine component it added, so a
            // video re-load would stack VideoPlayer components on the GameObject.
            Destroy(m_VideoPlayer);
        }
    }

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
