using BTV.Services.EegFileService;
using SFB;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    class MontageLoaderSaver : Tool
    {
        [SerializeField] Button m_LoadMontageButton;
        [SerializeField] Button m_SaveSelectedMontageButton;

        protected override void OnInitialize()
        {
            m_LoadMontageButton.onClick.AddListener(Load);
            m_SaveSelectedMontageButton.onClick.AddListener(Save);
        }

        private void Save()
        {
#if UNITY_STANDALONE_OSX
            FileBrowser.GetSavedFileNameAsync((filePath) =>
            {
                if (!string.IsNullOrEmpty(filePath))
                    EegFileService.CurrentMontage.Save(filePath);
            }, new ExtensionFilter[] { new ExtensionFilter("BrainTV montage file", "btvmontage") }, "Save Montage File", "", EegFileService.CurrentMontage.Name);
#else
            string filePath = FileBrowser.GetSavedFileName(new ExtensionFilter[] { new ExtensionFilter("BrainTV Montage File", "btvmontage") }, "Save Montage File", "", EegFileService.CurrentMontage.Name);
            if (!string.IsNullOrEmpty(filePath))
                EegFileService.CurrentMontage.Save(filePath);
#endif
        }

        private void Load()
        {
#if UNITY_STANDALONE_OSX
            FileBrowser.GetExistingFileNameAsync((filePath) =>
            {
                if (!string.IsNullOrEmpty(filePath))
                    EegFileService.LoadMontage(filePath);
            }, new string[] { "btvmontage" }, "Select an Montage File");
#else
            string filePath = FileBrowser.GetExistingFileName(new string[] { "btvmontage" }, "Select a Montage File");
            if (!string.IsNullOrEmpty(filePath))
                EegFileService.LoadMontage(filePath);
#endif
        }
    }
}
