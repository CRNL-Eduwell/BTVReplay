using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CielaSpike;
using BTV.Services.VideoService;
using System.IO;

namespace BTV.UI.Module3D.Tools
{
    public class VideoAudioFilter : Tool
    {
        /// <summary>
        /// Ui Button to filter audio file extracted from the video
        /// </summary>
        [SerializeField]
        private Button m_FilterAudio = null;

        public override void Initialize()
        {
            m_FilterAudio.onClick.AddListener(TryToFilterAudioFromVideo);
        }

        private void TryToFilterAudioFromVideo()
        {
            if (VideoService.AudioFileExist)
            {
                if (!VideoService.FilteredAudioFileExist)
                {
                    StartCoroutine(c_LaunchAudioFiltering("300:100:1300", 64));
                }
                else
                {
                    ApplicationState.displayConfirmation("Filtered File already exists", "Do you want to delete the existing file and filter the audio again ?", DeleteAndFilter, () => { });
                }
            }
            else
            {
                ApplicationState.displayMessage("Audio Filtering", "NOK", "There seem to be no audio file. Please extract the audio from the video before filtering");
            }
        }

        private void DeleteAndFilter()
        {
            try
            {
                File.Delete(VideoService.FilteredAudioPath);
            }
            catch (IOException ioExp)
            {
                UnityEngine.Debug.LogError(ioExp.Message);
            }

            StartCoroutine(c_LaunchAudioFiltering("300:100:1300", 64));
        }

        private IEnumerator c_LaunchAudioFiltering(string FrequencyBands, int FinalFrequency)
        {
            yield return this.StartCoroutineAsync(VideoService.c_LoadRawAudioFromFile(VideoService.AudioFromVideoPath));
            yield return this.StartCoroutineAsync(VideoService.c_FilterAudioFromVideo(FrequencyBands, FinalFrequency), out Task AudioFilteringTask);
            switch (AudioFilteringTask.State)
            {
                case TaskState.Done:
                    yield return Ninja.JumpToUnity;
                    ApplicationState.displayMessage("Audio Filtering", "OK", "Audio has been correctly filtered.");
                    yield return Ninja.JumpBack;
                    break;
                case TaskState.Error:
                    //Display Error Window
                    UnityEngine.Debug.LogError("Error Filtering audio");
                    break;
            }
        }
    }
}