using UnityEngine;
using UnityEngine.UI;

//Uncomment when deleting optionHub.cs
//
//public delegate void brainChangeEventHandler(int idBrain);

namespace BTV.UI.Module3D
{
    public class BrainToolbar : Toolbar
    {
        /// <summary>
        /// </summary>
        [SerializeField]
        private Tools.BrainReferential m_BrainReferentials = null;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Tools.BrainVisualisation m_BrainVisualisation = null;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Tools.BrainGain m_BrainGain = null;

        #region Private Methods
        protected override void AddTools()
        {
            m_Tools.Add(m_BrainReferentials);
            m_Tools.Add(m_BrainVisualisation);
            m_Tools.Add(m_BrainGain);
        }
        #endregion
    }
}