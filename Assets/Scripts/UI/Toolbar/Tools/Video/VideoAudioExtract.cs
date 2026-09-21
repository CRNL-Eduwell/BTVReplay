using System;
using System.IO;
using BTV.Services.VideoService;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
	public class VideoAudioExtract : Tool
	{
        //Event that shows that you have now some audio

        /// <summary>
        /// Ui Button to extract audio from the video
        /// </summary>
		[SerializeField]
		private Button m_ExtractAudio = null;

        protected override void OnInitialize()
        {
            m_ExtractAudio.onClick.AddListener(TryToExtractAudioFromVideo);
        }

        private void TryToExtractAudioFromVideo()
        {
            // Checked before anything else so a missing VLC is reported as a plain dialog (no
            // console error) and never deletes an existing audio file it could not re-create.
            if (!VideoService.VlcFileExist)
            {
                ApplicationState.displayMessage("Audio Extraction", "NOK", VideoService.VlcMissingMessage);
                return;
            }

            if (VideoService.VideoFileExist && !VideoService.AudioFileExist)
            {
                LaunchAudioExtraction(VideoService.AudioFromVideoPath, VideoService.OriginalVideoPath);
            }
            else if (VideoService.VideoFileExist && VideoService.AudioFileExist)
            {
                ApplicationState.displayConfirmation("Audio File already exists", "Do you want to delete the existing file and extract the audio again ?", DeleteAndExtract, () => { });
            }
            else
            {
                ApplicationState.displayMessage("Audio Extraction", "NOK", "Error, there seems to be no video file for this patient.");
            }
        }

        private void DeleteAndExtract()
        {
            try
            {
                File.Delete(VideoService.AudioFromVideoPath);
            }
            catch (IOException ioExp)
            {
                UnityEngine.Debug.LogError(ioExp.Message);
            }

            LaunchAudioExtraction(VideoService.AudioFromVideoPath, VideoService.OriginalVideoPath);
        }

        private async void LaunchAudioExtraction(string AudioPath, string VideoPath)
        {
            try
            {
                await VideoService.ExtractAudioAsync(AudioPath, VideoPath);
                ApplicationState.displayMessage("Audio Extraction", "OK", "Audio as been correctly extracted from video file.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Error extracting audio");
                UnityEngine.Debug.LogException(ex);
                ApplicationState.displayMessage("Audio Extraction", "NOK", ex.Message);
            }
        }
    }
}
