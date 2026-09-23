using System;
using BTV.Services.VideoService;
using UnityEngine;

public class VideoRecorder : MonoBehaviour
{
    public async void LaunchVideoRecording(string VideoFilePath, string DurationInSecond)
    {
        if (!VideoService.VlcFileExist)
        {
            ApplicationState.displayMessage("Video Record", "NOK", VideoService.VlcMissingMessage);
            return;
        }

        try
        {
            await VideoService.RecordVideoSnippetAsync(VideoFilePath, DurationInSecond);
            ApplicationState.displayMessage("Video Record", "OK", "Video as been correctly recorded. \n Please Check the output path you have provided.");
        }
        catch (Exception ex)
        {
            BtvLog.Handled("Error recording video", ex);
            ApplicationState.displayMessage("Video Record", "NOK", ex.Message);
        }
    }
}
