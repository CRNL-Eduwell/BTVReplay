using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    class MontageManager : Tool
    {
        [SerializeField] Button m_AddMontageButton;
        [SerializeField] Dropdown m_SelectMontageDropdown;
        [SerializeField] Button m_RemoveSelectedMontageButton;
        [SerializeField] Button m_EditSelectedMontageButton;

        public override void Initialize()
        {
            Messenger.Default.Register<MontageMessage>(this, OnMontageMessage, MessageContext.MontageMessage);
            m_AddMontageButton.onClick.AddListener(AddNewMontage);
            m_SelectMontageDropdown.onValueChanged.AddListener(UpdateSelectedMontage);
            m_RemoveSelectedMontageButton.onClick.AddListener(RemoveSelectedMontage);
            m_EditSelectedMontageButton.onClick.AddListener(EditSelectedMontage);
        }

        private void OnDestroy()
        {
            Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
        }

        private void OnMontageMessage(MontageMessage message)
        {
            if (message.TaskToExecute == 0)
            {
                m_SelectMontageDropdown.options.Clear();
                foreach (var montage in Services.EegFileService.EegFileService.Montages)
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
                TaskToExecute = 0,
                WindowName = "MontageWindow"
            };
            Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
        }

        private void UpdateSelectedMontage(int value)
        {
            Services.EegFileService.EegFileService.SelectedMontageID = value;
        }

        private void RemoveSelectedMontage()
        {
            if (Services.EegFileService.EegFileService.CurrentMontage.IsCustom)
            {
                Services.EegFileService.EegFileService.RemoveSelectedMontage();
            }
        }

        private void EditSelectedMontage()
        {
            if (Services.EegFileService.EegFileService.CurrentMontage.IsCustom)
            {
                ShowWindowMessage message = new ShowWindowMessage
                {
                    TaskToExecute = 0,
                    WindowName = "MontageWindow"
                };
                Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
                GameObject.Find(message.WindowName).GetComponent<MontageWindow>().SetMontage(Services.EegFileService.EegFileService.CurrentMontage);
            }
        }


    }
}
