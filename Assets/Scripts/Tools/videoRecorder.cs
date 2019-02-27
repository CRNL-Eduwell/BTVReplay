using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void launchRecordVideo(string videoPath, string durationInSec);

public class videoRecorder : MonoBehaviour
{
    public event launchRecordVideo recordVideo;

    [SerializeField] InputField outputPath = null;
    //===
    [SerializeField] InputField beg_hour = null;
    [SerializeField] InputField beg_min = null;
    [SerializeField] InputField beg_sec = null;
    //===
    [SerializeField] InputField end_hour = null;
    [SerializeField] InputField end_min = null;
    [SerializeField] InputField end_sec = null;
    //===
    [SerializeField] Button createVideo = null;
    [SerializeField] VideoPlayer videoPlayer = null;

    private void Awake()
    {
        createVideo.onClick.AddListener(RecordVideo);
    }

    private void OnDestroy()
    {
        createVideo.onClick.RemoveAllListeners();
    }

    void RecordVideo()
    {
        if (outputPath.text == "")
        {
            ApplicationState.displayMessage("Error Output Path", "NOK", "Output Path can not be an empty string");
            gameObject.transform.parent.gameObject.SetActive(false);
            return;
        }
        int totalVideoTimeInSecond = (int)videoPlayer.videoInterface.totalVideoTime / 1000;

        int beg_h = Convert.ToInt32(beg_hour.text);
        int beg_m = Convert.ToInt32(beg_min.text);
        int beg_s = Convert.ToInt32(beg_sec.text);
        int beginTimeInSecond = (beg_h * 3600) + (beg_m * 60) + beg_s;
        bool beginOk = beginTimeInSecond >= 0 && beginTimeInSecond < totalVideoTimeInSecond;

        int end_h = Convert.ToInt32(end_hour.text);
        int end_m = Convert.ToInt32(end_min.text);
        int end_s = Convert.ToInt32(end_sec.text);
        int endTimeInSecond = (end_h * 3600) + (end_m * 60) + end_s;
        bool endOk = endTimeInSecond >= 0 && endTimeInSecond <= totalVideoTimeInSecond;

        int durationInSeconds = endTimeInSecond - beginTimeInSecond;
        if (beginOk && endOk && durationInSeconds > 0)
        {
            videoPlayer.setTime(beginTimeInSecond * 1000);
            recordVideo(outputPath.text, durationInSeconds.ToString());
            gameObject.transform.parent.gameObject.SetActive(false);
        }
    }
}
