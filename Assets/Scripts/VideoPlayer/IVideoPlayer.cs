using UnityEngine.UI;

public interface IVideoPlayer
{
    long currentTime { get; }
    long time { get; }
    long videoTime { get; }
    long totalVideoTime { get; }
    bool isPlaying { get; }
    bool isPaused { get; }
    bool isStopped { get; }
    byte[] textureBytes { get; }

    void init(string videoPath, int eegFileDurationInSec, RawImage tex);
    void UpdateVideoOffset(float newOffset);
    void cleanup();
    void update();
    void play();
    void pause();
    void stop();
    void moveTime(long secondsToAdd);
    void setTime(long timeMilliSec);
    void setVolume(float volume);
}
