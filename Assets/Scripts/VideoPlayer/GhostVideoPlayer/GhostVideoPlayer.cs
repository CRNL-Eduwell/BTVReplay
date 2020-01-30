using UnityEngine.UI;

/// <summary>
/// Represents an instance of a fake Video Player.
/// This allow us to visualise EEG Data the same way we would with the video
/// Of an experiment.
/// This is an extrapolation generated with a timer to simulate a real video
/// according to the length of an EEG file
/// </summary>
public class GhostVideoPlayer : BaseVideoPlayer
{
    /// <summary>
    /// Time of the video, there is no possible offset due to user input 
    /// In MilliSeconds
    /// </summary>
    public override long Time { get { return CurrentTime; } }

    public override void Update()
    {
        if (IsPlaying)
        {
            base.Update();
            if (CurrentTime > TotalVideoTime)
                Stop();
        }
    }
}