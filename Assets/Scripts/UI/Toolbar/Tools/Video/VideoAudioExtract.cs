using System.Collections;
using System.IO;
using BTV.Services.VideoService;
using CielaSpike;
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

        public override void Initialize()
        {
            m_ExtractAudio.onClick.AddListener(TryToExtractAudioFromVideo);
        }

        private void TryToExtractAudioFromVideo()
        {
            if (VideoService.VideoFileExist && !VideoService.AudioFileExist)
            {
                StartCoroutine(c_LaunchAudioExtraction(VideoService.AudioFromVideoPath, VideoService.OriginalVideoPath));
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

            StartCoroutine(c_LaunchAudioExtraction(VideoService.AudioFromVideoPath, VideoService.OriginalVideoPath));
        }

        private IEnumerator c_LaunchAudioExtraction(string AudioPath, string VideoPath)
        {
            yield return this.StartCoroutineAsync(VideoService.c_ExtractAudio(AudioPath, VideoPath), out Task AudioExtractionTask);
            switch (AudioExtractionTask.State)
            {
                case TaskState.Done:
                    yield return Ninja.JumpToUnity;
                    ApplicationState.displayMessage("Audio Extraction", "OK", "Audio as been correctly extracted from video file.");
                    yield return Ninja.JumpBack;
                    break;
                case TaskState.Error:
                    //Display Error Window
                    UnityEngine.Debug.LogError("Error extracting audio");
                    break;
            }
        }
    }
}