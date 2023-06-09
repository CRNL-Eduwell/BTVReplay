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
        }
    }
}
