using System.Collections;
using BTV.Services.VideoService;
using CielaSpike;
using UnityEngine;
using UnityEngine.UI;

public class RecordVideoWindow : MonoBehaviour
{
    [SerializeField] private Button m_Close = null;
    [SerializeField] private InputField m_OutputVideoPath = null;
    [SerializeField] private TimeUI m_RecordBeginTime = null;
    [SerializeField] private TimeUI m_RecordEndTime = null;
    [SerializeField] private Button m_CreateVideo = null;

    private CustomVideoPlayer m_VideoPlayer = null;
    private VideoRecorder m_VideoRecorder = null;

    private void Awake()
    {
        m_Close.onClick.AddListener(CloseWindow);
        m_CreateVideo.onClick.AddListener(RecordVideo);

        m_VideoPlayer = FindObjectOfType<CustomVideoPlayer>();
        m_VideoRecorder = FindObjectOfType<VideoRecorder>();
    }

    private void OnDestroy()
    {
        m_Close.onClick.RemoveAllListeners();
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

        int totalVideoTimeInSecond = (int)m_VideoPlayer.VideoInterface.TotalVideoTime / 1000;
        int beginTimeInSecond = m_RecordBeginTime.TimeInSeconds;
        int endTimeInSecond = m_RecordEndTime.TimeInSeconds;
        int durationInSeconds = endTimeInSecond - beginTimeInSecond;
        bool IsBeginTimeValid = beginTimeInSecond >= 0 && beginTimeInSecond < totalVideoTimeInSecond;
        bool IsEndTimeValid = endTimeInSecond >= 0 && endTimeInSecond <= totalVideoTimeInSecond;

        if (IsBeginTimeValid && IsEndTimeValid && durationInSeconds > 0)
        {
            ModulesToVideoMessage messageToVideo = new ModulesToVideoMessage
            {
                UpdateClickPosition = false,
                TimeMilliseconds = beginTimeInSecond * 1000
            };
            Messenger.Default.Send(messageToVideo, MessageContext.ModulesToVideoMessage);

            StartCoroutine(m_VideoRecorder.c_LaunchVideoRecording(m_OutputVideoPath.text, durationInSeconds.ToString()));
            CloseWindow();
        }
    }
}
