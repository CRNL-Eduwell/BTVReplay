using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UnityVideoPlayer : MonoBehaviour, IVideoPlayer
{
    /// <summary>
    /// Same as time, see if both are usefull ????
    /// </summary>
    public long CurrentTime { get { return (long)(m_VideoPlayer.time * 1000) + m_OffsetVideoMilliSec; } }

    public long Time { get { return CurrentTime; } }

    public long VideoTime { get { return CurrentTime - m_OffsetVideoMilliSec; } }

    public long TotalVideoTime { get { return (long)(m_VideoPlayer.length * 1000); } }

    public bool IsPlaying { get { return m_VideoPlayer.isPlaying; } }

    public bool IsPaused { get { return m_VideoPlayer.isPaused; } }

    public bool IsStopped { get { return !m_VideoPlayer.isPlaying && !m_VideoPlayer.isPaused; } }

    public byte[] TextureBytes => throw new System.NotImplementedException();

    #region private members
    private string m_VideoFilePath = "";
    private long m_EegFileDurationInSec = 0;
    private RawImage m_TextureForVideo = null;
    private VideoPlayer m_VideoPlayer = null;
    private int m_OffsetVideoMilliSec = 0;
    #endregion

    public void Init(string path, int duration, RawImage texture)
    {
        m_VideoFilePath = path;
        m_EegFileDurationInSec = duration;
        m_TextureForVideo = texture;

        m_VideoPlayer = gameObject.AddComponent<VideoPlayer>();
    }

    public void Cleanup()
    {

    }

    public void Update()
    {
        //UnityEngine.Debug.Log("Current Time : " + m_VideoPlayer.time.ToString() + " s");
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
        double time = (double)timeMilliSec / 1000;
        if (time < 0 || time > m_VideoPlayer.length)
            time = 0;

        m_VideoPlayer.time = time;
    }

    public void SetVolume(float volume)
    {
        m_VideoPlayer.SetDirectAudioVolume(0, volume);
    }

    public void SetVideoOffset(float newOffset)
    {
        throw new System.NotImplementedException();
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
        m_TextureForVideo.texture = new RenderTexture((int)m_VideoPlayer.width, (int)m_VideoPlayer.height, 0, RenderTextureFormat.ARGB32);
        m_VideoPlayer.targetTexture = (RenderTexture)(m_TextureForVideo.texture);

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

        m_VideoPlayer.Prepare();
    }
}