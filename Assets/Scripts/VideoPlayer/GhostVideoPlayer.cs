using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics; //Requiered for Stopwatch

/// <summary>
/// Represents an instance of a fake Video Player.
/// This allow us to visualise EEG Data the same way we would with the video
/// Of an experiment.
/// This is an extrapolation generated with a timer to simulate a real video
/// according to the length of an EEG file
/// </summary>
public class GhostVideoPlayer : BaseVideoPlayer, IVideoPlayer
{
    public new long CurrentTime { get { return base.CurrentTime; } }

    public long Time { get { return CurrentTime; } }

    public long VideoTime { get { return -1; } }

    public long TotalVideoTime { get { return base.TotalTime; } }

    public new bool IsPlaying { get { return base.IsPlaying; } }

    public new bool IsPaused { get { return base.IsPaused; } }

    public new bool IsStopped { get { return base.IsStopped; } }

    public byte[] TextureBytes { get { return null; } }

    #region private members
    private string m_VideoFilePath = "";
    private long m_EegFileDurationInSec = 0;
    #endregion

    /// <summary>
    /// Init the internal stopwatch of the player and
    /// keep parameters
    /// </summary>
    /// <param name="path">Video File Path</param>
    /// <param name="duration">Eeg File Duration in Seconds</param>
    /// <param name="texture">Raw Image containing texture to draw the video frame on</param>
    public void Init(string path, int duration, RawImage texture)
    {
        m_VideoFilePath = path;
        m_EegFileDurationInSec = duration;
        base.Init(duration);
    }

    public void Cleanup()
    {

    }

    public new void Update()
    {
        if (IsPlaying)
        {
            base.Update();
            if (CurrentTime > TotalVideoTime)
                Stop();
        }
    }

    public void MoveTime(long secondsToAdd)
    {
        base.CurrentTime += (secondsToAdd * 1000);
    }

    public new void SetTime(long timeMilliSec)
    {
        base.CurrentTime = timeMilliSec;
    }

    public void SetVolume(float volume)
    {

    }

    public void SetVideoOffset(float newOffset)
    {

    }
}