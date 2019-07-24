using System;
using UnityEngine;
using UnityEngine.UI;

//Uncomment when deleting optionHub.cs
//
//public delegate void hideMe(bool isHidden);
namespace BTV.UI.Module3D
{
    public class CompPerformanceToolbar : Toolbar
    {
        public event hideMe iAmHiden; //rebrancher au clic bouton de gauche

        [SerializeField]
        private Tools.ComportementalWindow m_ComportementWindow = null;

        public Toggle hideTog
        {
            get
            {
                return hideMeToggle;
            }
        }

        Toggle hideMeToggle = null;
        InputField timePeriodInputField = null;

        public CompPerformanceToolbar(GameObject perfOptionsPanel)
        {
            hideMeToggle = perfOptionsPanel.transform.GetChild(0).GetChild(0).GetComponent<Toggle>();
            timePeriodInputField = perfOptionsPanel.transform.GetChild(0).GetChild(1).GetComponent<InputField>();

        }

        ~CompPerformanceToolbar()
        {
            //hideMeToggle.onValueChanged.RemoveAllListeners();
            timePeriodInputField.onEndEdit.RemoveAllListeners();
        }

        #region Private Methods
        protected override void AddTools()
        {
            m_Tools.Add(m_ComportementWindow);
        }
        #endregion
    }
}
