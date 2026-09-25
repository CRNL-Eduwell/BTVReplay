using System;
using BTV.Services.VideoService;
using BTV.Services;
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

        protected override void OnInitialize()
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
            if (VideoService.FilteredAudioFileExists(PatientSession))
            {
                StartLoadingFilteredAudio(VideoService.GetFilteredAudioPath(PatientSession));
            }
            else
            {
                ApplicationState.displayMessage("Audio has not been loaded", "NOK", "Audio File could not be loaded correctly, check for the presence of the file.");
            }
        }

        private async void StartLoadingFilteredAudio(string FilteredAudioFilePath)
        {
            try
            {
                await VideoService.LoadFilteredAudioFromFileAsync(PatientSession, FilteredAudioFilePath);
                if (this == null || !Session.IsCurrent(PatientSession)) return;
                ApplicationState.displayMessage("Audio Loaded", "OK", "Audio has been correctly loaded.");
            }
            catch (Exception ex)
            {
                BtvLog.Handled("Error loading filtered audio", ex);
                ApplicationState.displayMessage("Audio has not been loaded", "NOK", "Error during the loading process.");
            }
        }
    }
}
