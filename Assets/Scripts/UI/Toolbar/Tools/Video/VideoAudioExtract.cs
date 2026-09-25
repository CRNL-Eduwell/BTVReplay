using System;
using System.IO;
using BTV.Services.VideoService;
using BTV.Services;
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

            if (VideoService.VideoFileExists(PatientSession) && !VideoService.AudioFileExists(PatientSession))
            {
                LaunchAudioExtraction(VideoService.GetAudioFromVideoPath(PatientSession), VideoService.GetOriginalVideoPath(PatientSession));
            }
            else if (VideoService.VideoFileExists(PatientSession) && VideoService.AudioFileExists(PatientSession))
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
                File.Delete(VideoService.GetAudioFromVideoPath(PatientSession));
            }
            catch (IOException ioExp)
            {
                UnityEngine.Debug.LogError(ioExp.Message);
            }

            LaunchAudioExtraction(VideoService.GetAudioFromVideoPath(PatientSession), VideoService.GetOriginalVideoPath(PatientSession));
        }

        private async void LaunchAudioExtraction(string AudioPath, string VideoPath)
        {
            try
            {
                await VideoService.ExtractAudioAsync(AudioPath, VideoPath);
                if (this == null || !Session.IsCurrent(PatientSession)) return;
                ApplicationState.displayMessage("Audio Extraction", "OK", "Audio as been correctly extracted from video file.");
            }
            catch (Exception ex)
            {
                BtvLog.Handled("Error extracting audio", ex);
                ApplicationState.displayMessage("Audio Extraction", "NOK", ex.Message);
            }
        }
    }
}
