using System;
using UnityEngine.UI;

public interface IVideoPlayer
{
    /// <summary>
    /// Raised when a SetTime/MoveTime seek has landed (possibly synchronously for players
    /// that seek instantly). The UI uses it to know when the displayed frame matches the
    /// requested time, instead of guessing from clock movement.
    /// </summary>
    event Action SeekCompleted;

    bool IsPrepared { get; }
    /// <summary>
    /// Current playback time, in milliseconds: the one time the rest of the app reads. (Two more
    /// getters, Time and VideoTime, used to sit here with no callers.)
    /// </summary>
    long ClockTime { get; }
    /// <summary>
    /// Total Duration of the Video
    /// In MilliSeconds
    /// </summary>
    long TotalVideoTime { get; }
    bool IsPlaying { get; }
    bool IsPaused { get; }
    bool IsStopped { get; }
    /// <summary>
    /// True when a seek lands immediately (the ghost clock), false when the player must decode
    /// to the new frame first (real video), in which case the UI shows a buffering spinner.
    /// </summary>
    bool SeeksInstantly { get; }

    void Init(string path, int duration, RawImage texture);
    void Cleanup();
    /// <summary>
    /// Advances the player by one frame. Called by CustomVideoPlayer only; deliberately not
    /// named Update, so Unity does not also call it as a MonoBehaviour message.
    /// </summary>
    void Tick();
    void Play();
    void Pause();
    void Stop();
    void MoveTime(long secondsToAdd);
    void SetTime(long timeMilliSec);
    void SetVolume(float volume);
}