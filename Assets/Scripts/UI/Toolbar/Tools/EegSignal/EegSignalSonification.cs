using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public delegate void toggleSonification(bool isSonifOn);
    public delegate void newSoundSonif(int newIDSound);

    class EegSignalSonification : Tool
    {
        public event toggleSonification sonifToggled;
        public event newSoundSonif soundChanged;

        #region UI members
        /// <summary>
        /// </summary>
        [SerializeField]
        private Toggle m_ToggleSonification = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Dropdown m_SoundChoice = null;
        #endregion

        //audiosource extention file : https://docs.unity3d.com/Manual/AudioFiles.html
        private string[] m_audioFileExtention = { ".mp3", ".ogg", ".wav", ".aiff", ".aif", ".mod", ".it", ".s3m", ".xm" };

        protected override void OnInitialize()
        {
            m_SoundChoice.interactable = false;
            m_ToggleSonification.onValueChanged.AddListener(ToggleSound);
            LoadAudioData();
            m_SoundChoice.itemText.text = m_SoundChoice.options[m_SoundChoice.value].text;
            m_SoundChoice.onValueChanged.AddListener(UpdateSound);
        }

        private void LoadAudioData()
        {
            ApplicationState.Module3D.SoundFilePaths = Directory.GetFiles(Application.dataPath + @"/Config/Sounds/", "*.*")
                .Where(n => m_audioFileExtention.Contains(System.IO.Path.GetExtension(n), StringComparer.OrdinalIgnoreCase))
                .ToList();

            m_SoundChoice.options.Clear();
            for (int i = 0; i < ApplicationState.Module3D.SoundFilePaths.Count(); i++)
            {
                string[] splitPath = ApplicationState.Module3D.SoundFilePaths[i].Split(new char[] { '/', '.' });
                string shortName = splitPath[splitPath.Count() - 2];
                m_SoundChoice.options.Add(new Dropdown.OptionData(shortName));
            }
        }

        private void ToggleSound(bool IsOn)
        {
            m_SoundChoice.interactable = IsOn;
            sonifToggled?.Invoke(IsOn);
        }

        private void UpdateSound(int NewSoundId)
        {
            soundChanged?.Invoke(NewSoundId);
        }
    }
}
