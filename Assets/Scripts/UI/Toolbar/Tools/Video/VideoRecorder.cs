using System.Collections;
using BTV.Services.VideoService;
using CielaSpike;
using UnityEngine;
using UnityEngine.UI;

public class VideoRecorder : MonoBehaviour
{
    [SerializeField]
    private InputField m_OutputVideoPath = null;
    [SerializeField]
    private GameObject UiPanel = null;
    [SerializeField]
    private TimeUI m_RecordBeginTime = null;
    [SerializeField]
    private TimeUI m_RecordEndTime = null;
    [SerializeField]
    private Button m_CreateVideo = null;

    private CustomVideoPlayer m_VideoPlayer = null;

    private void Awake()
    {
        m_CreateVideo.onClick.AddListener(RecordVideo);
        m_VideoPlayer = GameObject.Find("View").transform.GetChild(1).GetComponent<CustomVideoPlayer>();
    }

    private void OnDestroy()
    {
        m_CreateVideo.onClick.RemoveAllListeners();
    }

    private void CloseWindow()
    {
        Destroy(gameObject);
    }

    private void RecordVideo()
    {
        if (m_OutputVideoPath.text == "")
        {
            ApplicationState.displayMessage("Error Output Path", "NOK", "Output Path can not be an empty string");
            CloseWindow();
            return;
        }

        int totalVideoTimeInSecond = (int)m_VideoPlayer.videoInterface.TotalVideoTime / 1000;
        int beginTimeInSecond = m_RecordBeginTime.TimeInSeconds;
        int endTimeInSecond = m_RecordEndTime.TimeInSeconds;
        int durationInSeconds = endTimeInSecond - beginTimeInSecond;
        bool IsBeginTimeValid = beginTimeInSecond >= 0 && beginTimeInSecond < totalVideoTimeInSecond;
        bool IsEndTimeValid = endTimeInSecond >= 0 && endTimeInSecond <= totalVideoTimeInSecond;

        if (IsBeginTimeValid && IsEndTimeValid && durationInSeconds > 0)
        {
            m_VideoPlayer.setTime(beginTimeInSecond * 1000);
            StartCoroutine(c_LaunchVideoRecording(m_OutputVideoPath.text, durationInSeconds.ToString()));
            UiPanel.gameObject.SetActive(false);
        }
    }

    IEnumerator c_LaunchVideoRecording(string VideoFilePath, string DurationInSecond)
    {
        yield return this.StartCoroutineAsync(VideoService.c_RecordVideoSnippet(VideoFilePath, DurationInSecond), out Task videoRecordingTask);
        switch (videoRecordingTask.State)
        {
            case TaskState.Done:
                yield return Ninja.JumpToUnity;
                ApplicationState.displayMessage("Video Record", "OK", "Video as been correctly recorded. \n Please Check the output path you have provided.");
                yield return Ninja.JumpBack;
                break;
            case TaskState.Error:
                //Display Error Window
                UnityEngine.Debug.LogError("Error recording video");
                break;
        }
        CloseWindow();
    }
}
