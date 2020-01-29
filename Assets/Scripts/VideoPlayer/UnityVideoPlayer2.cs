using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UnityVideoPlayer2 : BaseVideoPlayer, IVideoPlayer
{
    /// <summary>
    /// Same as time, see if both are usefull ????
    /// </summary>
    public new long CurrentTime { get { return base.CurrentTime + m_OffsetVideoMilliSec; } }

    public long Time { get { return CurrentTime; } }

    public long VideoTime { get { return CurrentTime - m_OffsetVideoMilliSec; } }

    public long TotalVideoTime { get { return (long)(m_VideoPlayer.length * 1000); } }

    public new bool IsPlaying { get { return base.IsPlaying; } }

    public new bool IsPaused { get { return base.IsPaused; } }

    public new bool IsStopped { get { return base.IsStopped; } }

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
        base.Init(duration);
        m_VideoPlayer = gameObject.AddComponent<VideoPlayer>();
    }

    public void Cleanup()
    {

    }

    public new void Update()
    {
        base.Update();
        UnityEngine.Debug.Log(CurrentTime);
    }

    public new void Play()
    {
        if (!m_VideoPlayer.isPrepared)
        {
            StartCoroutine(PrepareAndPlay());
            return;
        }

        if (m_VideoPlayer.isPaused || m_VideoPlayer.isPrepared)
        {
            base.Play();
            m_VideoPlayer.Play();
        }
    }

    public new void Pause()
    {
        if (m_VideoPlayer.isPlaying)
        {
            base.Pause();
            m_VideoPlayer.Pause();
        }
    }

    public new void Stop()
    {
        base.Stop();
        m_VideoPlayer.Stop();
        //Delete old texture or BTVLogo 
        Texture oldTexture = m_TextureForVideo.texture;
        Destroy(oldTexture);
    }

    public void MoveTime(long secondsToAdd)
    {
        base.CurrentTime += (secondsToAdd * 1000);
        m_VideoPlayer.time = base.CurrentTime;
    }

    public new void SetTime(long timeMilliSec)
    {
        base.CurrentTime = timeMilliSec;
        m_VideoPlayer.time = base.CurrentTime;
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
