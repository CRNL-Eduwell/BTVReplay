using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UnityVideoPlayer : BaseVideoPlayer
{
    /// <summary>
    /// Same as time, see if both are usefull ????
    /// </summary>
    public override long CurrentTime { get { return base.CurrentTime + m_OffsetVideoMilliSec; } }

    public override long Time { get { return CurrentTime; } }

    public override long VideoTime { get { return CurrentTime - m_OffsetVideoMilliSec; } }

    public override long TotalVideoTime { get { return (long)(m_VideoPlayer.length * 1000); } }

    #region private members
    private VideoPlayer m_VideoPlayer = null;
    private int m_OffsetVideoMilliSec = 0;
    #endregion

    /// <summary>
    /// Init the internal stopwatch of the player and
    /// init a Unity Vide Player component that will handle
    /// all the rendering of the video
    /// </summary>
    /// <param name="path">Video File Path</param>
    /// <param name="duration">Eeg File Duration in Seconds</param>
    /// <param name="texture">Raw Image containing texture to draw the video frame on</param>
    public override void Init(string path, int duration, RawImage texture)
    {
        base.Init(path, duration,texture);
        m_VideoPlayer = gameObject.AddComponent<VideoPlayer>();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Play()
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

    public override void Pause()
    {
        if (m_VideoPlayer.isPlaying)
        {
            base.Pause();
            m_VideoPlayer.Pause();
        }
    }

    public override void Stop()
    {
        base.Stop();
        m_VideoPlayer.Stop();
        //Delete old texture or BTVLogo 
        Texture oldTexture = m_TextureForVideo.texture;
        Destroy(oldTexture);
    }

    public override void MoveTime(long secondsToAdd)
    {
        base.MoveTime(secondsToAdd);
        m_VideoPlayer.time += secondsToAdd;
    }

    public override void SetTime(long timeMilliSec)
    {
        base.SetTime(timeMilliSec);
        m_VideoPlayer.time = (long)((double)base.CurrentTime/1000);
    }

    public override void SetVolume(float volume)
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
