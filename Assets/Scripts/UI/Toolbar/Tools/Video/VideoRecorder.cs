using System.Collections;
using BTV.Services.VideoService;
using CielaSpike;
using UnityEngine;
using UnityEngine.UI;

public class VideoRecorder : MonoBehaviour
{
    public IEnumerator c_LaunchVideoRecording(string VideoFilePath, string DurationInSecond)
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
    }
}
