using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CielaSpike;
using BTV.Services.VideoService;

namespace BTV.UI.Module3D.Tools
{
    public class VideoAudioFilter : Tool
    {
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_FilterAudio = null;

        public override void Initialize()
        {
            m_FilterAudio.onClick.AddListener(CheckIfFilteringOfAudioExist);
        }

        //TODO : Proposer de supprimer les données existantes
        private void CheckIfFilteringOfAudioExist()
        {
            if (VideoService.AudioFileExist && !VideoService.FilteredAudioFileExist)
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