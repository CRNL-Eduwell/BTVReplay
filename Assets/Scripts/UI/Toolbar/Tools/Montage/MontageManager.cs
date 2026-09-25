using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using BTV.Services;
using BTV.Services.EegFileService;

namespace BTV.UI.Module3D.Tools
{
    class MontageManager : Tool
    {
        [SerializeField] Button m_AddMontageButton;
        [SerializeField] Dropdown m_SelectMontageDropdown;
        [SerializeField] Button m_RemoveSelectedMontageButton;
        [SerializeField] Button m_EditSelectedMontageButton;

        protected override void OnInitialize()
        {
            Messenger.Default.Register<MontageMessage>(this, OnMontageMessage, MessageContext.MontageMessage);
            m_AddMontageButton.onClick.AddListener(AddNewMontage);
            m_SelectMontageDropdown.onValueChanged.AddListener(UpdateSelectedMontage);
            m_RemoveSelectedMontageButton.onClick.AddListener(RemoveSelectedMontage);
            m_EditSelectedMontageButton.onClick.AddListener(EditSelectedMontage);
        }

        private void OnDestroy()
        {
            // Must match the context registered in Initialize (was LoaderMessage by copy-paste).
            Messenger.Default.Unregister(this, MessageContext.MontageMessage);
        }

        private void OnMontageMessage(MontageMessage message)
        {
            if (!Session.IsCurrent(PatientSession)) return;
            if (message.TaskToExecute == MontageMessage.Task.UpdateMontageList)
            {
                m_SelectMontageDropdown.options.Clear();
                foreach (var montage in EegFileService.GetMontages(PatientSession))
                {
                    m_SelectMontageDropdown.options.Add(new Dropdown.OptionData(montage.Name));
                }
                m_SelectMontageDropdown.value = message.SelectedMontageID;
            }
        }

        private void AddNewMontage()
        {
            ShowWindowMessage message = new ShowWindowMessage
            {
                WindowName = "MontageWindow"
            };
            Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
            GameObject.Find(message.WindowName).GetComponent<MontageWindow>().Initialize(PatientSession);
        }

        private void UpdateSelectedMontage(int value)
        {
            EegFileService.SetSelectedMontage(PatientSession, value);
        }

        private void RemoveSelectedMontage()
        {
            if (EegFileService.GetCurrentMontage(PatientSession).IsCustom)
            {
                EegFileService.RemoveSelectedMontage(PatientSession);
            }
        }

        private void EditSelectedMontage()
        {
            if (EegFileService.GetCurrentMontage(PatientSession).IsCustom)
            {
                ShowWindowMessage message = new ShowWindowMessage
                {
                    WindowName = "MontageWindow"
                };
                Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
                MontageWindow window = GameObject.Find(message.WindowName).GetComponent<MontageWindow>();
                window.Initialize(PatientSession);
                window.SetMontage(EegFileService.GetCurrentMontage(PatientSession));
            }
        }


    }
}
