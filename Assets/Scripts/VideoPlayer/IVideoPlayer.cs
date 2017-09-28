using UnityEngine.UI;

public interface IVideoPlayer
{
    long currentTime { get; }
    long time { get; }
    long totalVideoTime { get; }
    bool isPlaying { get; }
    bool isPaused { get; }
    bool isStopped { get; }
    byte[] textureBytes { get; }

    void init(string videoPath, int eegSampFreq, int eegFileDurationInSec);
    void getVideoReference(RawImage tex, optionsHub hub);
    void cleanup();
    void update();
    void play();
    void pause();
    void stop();
    void moveTime(long secondsToAdd);
    void setTime(long timeMilliSec);
    void setVolume(float volume);
}
