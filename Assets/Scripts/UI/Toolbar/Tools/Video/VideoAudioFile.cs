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
        /// Ui Button to load the file
        /// </summary>
        [SerializeField]
        private Button m_LoadFilterAudio = null;

        public override void Initialize()
        {
            m_LoadFilterAudio.onClick.AddListener(TryToLoadFilteredFile);
        }

        private void OnDestroy()
        {
            m_LoadFilterAudio.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Load the Audio data that have been previously processed
        /// If there is no processed audio file , display a modal window
        /// </summary>
        private void TryToLoadFilteredFile()
        {
            if (VideoService.FilteredAudioFileExist)
            {
                StartCoroutine(c_StartLoadingFilteredAudio(VideoService.FilteredAudioPath));
            }
            else
            {
                ApplicationState.displayMessage("Audio has not been loaded", "NOK", "Audio File could not be loaded correctly, check for the presence of the file.");
            }
        }

        private IEnumerator c_StartLoadingFilteredAudio(string FilteredAudioFilePath)
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
                    yield return Ninja.JumpToUnity;
                    ApplicationState.displayMessage("Audio has not been loaded", "NOK", "Error during the loading process.");
                    yield return Ninja.JumpBack;
                    break;
            }
        }
    }
}