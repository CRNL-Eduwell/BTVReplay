using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UnityVideoPlayer : MonoBehaviour, IVideoPlayer
{
    public bool IsPrepared { get { return m_VideoPlayer != null ? m_VideoPlayer.isPrepared : false; } }
    /// <summary>
    /// Same as time, see if both are usefull ????
    /// </summary>
    public long CurrentTime { get { return (long)((m_VideoPlayer.clockTime * 1000) + m_OffsetVideoMilliSec); } }

    public long Time { get { return CurrentTime; } }

    public long VideoTime { get { return CurrentTime - m_OffsetVideoMilliSec; } }

    public long TotalVideoTime { get { return (long)(m_VideoPlayer.length * 1000); } }

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

        m_parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    public void Update()
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
            StartCoroutine(PrepareAndPlay());
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
        m_VideoPlayer.time += secondsToAdd;
    }

    public void SetTime(long timeMilliSec)
    {
        m_VideoPlayer.time = ((double)timeMilliSec / 1000);
    }

    public void SetVolume(float volume)
    {
        m_VideoPlayer.SetDirectAudioVolume(0, volume);
    }

    private IEnumerator PrepareAndPlay()
    {
        PrepareVideoPlayerRessources();

        while (!m_VideoPlayer.isPrepared)
            yield return null;

        //Delete old texture or BTVLogo 
        Texture oldTexture = m_TextureForVideo.texture;
        Destroy(oldTexture);

        //Create New Render Texture and assign it to the videoplayer
        m_TextureForVideo.texture = new RenderTexture(m_VideoPlayer.texture.width, m_VideoPlayer.texture.height, 0, RenderTextureFormat.ARGB32);
        m_VideoPlayer.targetTexture = (RenderTexture)(m_TextureForVideo.texture);
        WidthToHeightRatio = (float)m_TextureForVideo.texture.width / m_TextureForVideo.texture.height;
        HeightToWidthRatio = (float)m_TextureForVideo.texture.height / m_TextureForVideo.texture.width;
        ResizeTexture();

        Play();
    }

    private void PrepareVideoPlayerRessources()
    {
        // Play on awake defaults to true.
        m_VideoPlayer.playOnAwake = false;
        m_VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        m_VideoPlayer.url = m_VideoFilePath;
        m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        // Skip the first 100 frames.
        m_VideoPlayer.frame = 0;
        m_VideoPlayer.EnableAudioTrack(0, true);
        // Restart from beginning when done.
        m_VideoPlayer.isLooping = true;
        m_VideoPlayer.waitForFirstFrame = true;

        m_VideoPlayer.Prepare();
    }

    public void Cleanup() { }

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
