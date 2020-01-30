using UnityEngine.UI;

public interface IVideoPlayer
{
    /// <summary>
    /// Exact Time of the video
    /// In MilliSeconds
    /// </summary>
    long CurrentTime { get; }
    long Time { get; }
    long VideoTime { get; }
    /// <summary>
    /// Total Duration of the Video
    /// In MilliSeconds
    /// </summary>
    long TotalVideoTime { get; }
    bool IsPlaying { get; }
    bool IsPaused { get; }
    bool IsStopped { get; }
    byte[] TextureBytes { get; }

    void Init(string path, int duration, RawImage texture);
    void Cleanup();
    void Update();
    void Play();
    void Pause();
    void Stop();
    void MoveTime(long secondsToAdd);
    void SetTime(long timeMilliSec);
    void SetVolume(float volume);
    void SetVideoOffset(float newOffset);
}