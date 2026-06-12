using System;
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
        float beginTimeInSecond = m_RecordBeginTime.TimeInSeconds;
        float endTimeInSecond = m_RecordEndTime.TimeInSeconds;
        float durationInSeconds = endTimeInSecond - beginTimeInSecond;
        bool IsBeginTimeValid = beginTimeInSecond >= 0 && beginTimeInSecond < totalVideoTimeInSecond;
        bool IsEndTimeValid = endTimeInSecond >= 0 && endTimeInSecond <= totalVideoTimeInSecond;

        if (IsBeginTimeValid && IsEndTimeValid && Convert.ToInt32(durationInSeconds) > 0)
        {
            ModulesToVideoMessage messageToVideo = new ModulesToVideoMessage
            {
                UpdateClickPosition = false,
                TimeMilliseconds = beginTimeInSecond * 1000
            };
            Messenger.Default.Send(messageToVideo, MessageContext.ModulesToVideoMessage);

            // The recording runs on the (persistent) VideoRecorder, so closing this window no
            // longer kills the completion dialog - the old StartCoroutine died with the window.
            m_VideoRecorder.LaunchVideoRecording(m_OutputVideoPath.text, durationInSeconds.ToString());
            CloseWindow();
        }
    }
}
