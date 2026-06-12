using System;
using UnityEngine;
using UnityEngine.UI;
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
                    LaunchAudioFiltering("300:100:1300", 64);
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

            LaunchAudioFiltering("300:100:1300", 64);
        }

        private async void LaunchAudioFiltering(string FrequencyBands, int FinalFrequency)
        {
            try
            {
                await VideoService.LoadRawAudioFromFileAsync(VideoService.AudioFromVideoPath);
                await VideoService.FilterAudioFromVideoAsync(FrequencyBands, FinalFrequency);
                ApplicationState.displayMessage("Audio Filtering", "OK", "Audio has been correctly filtered.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Error Filtering audio");
                UnityEngine.Debug.LogException(ex);
                ApplicationState.displayMessage("Audio Filtering", "NOK", ex.Message);
            }
        }
    }
}