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
		/// </summary>
		[SerializeField]
		private Button m_ExtractAudio = null;

        public override void Initialize()
        {
            m_ExtractAudio.onClick.AddListener(CheckIfAudioOfVideoExist);
        }

        //TODO : Proposer de supprimer les données existantes
        private void CheckIfAudioOfVideoExist()
        {
            if (!VideoService.AudioFileExist)
            {
                UnityEngine.Debug.Log("Go extract");
                StartCoroutine(c_LaunchAudioExtraction(VideoService.AudioFromVideoPath, VideoService.OriginalVideoPath));
            }
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