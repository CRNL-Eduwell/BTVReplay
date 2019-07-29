using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public class BrainReferential : Tool
    {
        public event brainChangeEventHandler needToChangeBrain;

        #region Properties
        [SerializeField]
        private Dropdown m_Dropdown = null;
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            m_Dropdown.onValueChanged.AddListener((value) => { UpdateBrainReferential(value); });
        }
        #endregion

        private void OnDestroy()
        {
            m_Dropdown.onValueChanged.RemoveAllListeners();
        }

        //0 : MNI
        //1 : PAT
        //2 : ELEC
        private void UpdateBrainReferential(int VisuID)
        {
            needToChangeBrain(VisuID);
        }
    }
}