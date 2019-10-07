using System.Collections;
using BTV.Services.VideoService;
using CielaSpike;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public class VideoAudioFile : Tool
    {
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_FilterAudio = null;

        public override void Initialize()
        {
            m_FilterAudio.onClick.AddListener(CheckIfFilteredFileExist);
        }

        //TODO : Proposer de supprimer les données existantes
        private void CheckIfFilteredFileExist()
        {
            if (VideoService.FilteredAudioFileExist)
                StartCoroutine(c_LaunchFilteredAudioLoading(VideoService.FilteredAudioPath));
        }

        private IEnumerator c_LaunchFilteredAudioLoading(string FilteredAudioFilePath)
        {
            yield return this.StartCoroutineAsync(VideoService.c_LoadFilteredAudioFromFile(FilteredAudioFilePath), out Task LoadFilteredAudioTask);
            switch (LoadFilteredAudioTask.State)
            {
                case TaskState.Done:
                    yield return Ninja.JumpToUnity;
                    ApplicationState.displayMessage("Audio Loaded", "OK", "Audio has been correctly loaded.");
                    yield return Ninja.JumpBack;
                    break;
                case TaskState.Error:
                    //Display Error Window
                    UnityEngine.Debug.LogError("Error loading audio Data");
                    break;
            }
        }
    }
}