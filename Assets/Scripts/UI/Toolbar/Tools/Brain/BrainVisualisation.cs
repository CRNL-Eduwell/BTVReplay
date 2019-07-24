using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public class BrainVisualisation : Tool
    {
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
        private Button m_FullBrain;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_LeftBrain;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_RightBrain;

        #region Public Methods
        public override void Initialize()
        {
            ChoicePending = false;
            m_FullBrain.onClick.AddListener(() => { UpdateBrainVisualisation(0); });
            m_LeftBrain.onClick.AddListener(() => { UpdateBrainVisualisation(-1); });
            m_RightBrain.onClick.AddListener(() => { UpdateBrainVisualisation(1); });
        }
        #endregion

        //-1 : Left Hemisphere
        // 0 : Both Hemisphere
        // 1 : Right Hemisphere
        private void UpdateBrainVisualisation(int VisuID)
        {
            if (ChoicePending)
            {
                Brain.changeVisuBrain(VisuID);
                ChoicePending = false;
            }
        }
    }
}
