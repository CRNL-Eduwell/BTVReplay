using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public class BrainReferential : Tool
    {
        public event brainChangeEventHandler needToChangeBrain;
        /// <summary>
        /// </summary>
        public bool ChoicePending
        {
            get;
            set;
        }
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_Mni;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_Patient;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_Electrodes;

        #region Public Methods
        public override void Initialize()
        {
            ChoicePending = false;
            m_Mni.onClick.AddListener(() => { UpdateBrainReferential(0); });
            m_Patient.onClick.AddListener(() => { UpdateBrainReferential(1); });
            m_Electrodes.onClick.AddListener(() => { UpdateBrainReferential(2); });
        }
        #endregion

        //0 : MNI
        //1 : PAT
        //2 : ELEC
        private void UpdateBrainReferential(int VisuID)
        {
            if (ChoicePending)
            {
                needToChangeBrain(VisuID);
                ChoicePending = false;
            }
        }
    }
}
