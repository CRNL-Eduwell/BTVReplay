using UnityEngine;
using UnityEngine.UI;

public delegate void brainChangeEventHandler(int idBrain);

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
        protected override void AddListeners()
        {
            base.AddListeners();

            m_BrainReferentials.needToChangeBrain += new brainChangeEventHandler((BrainId) =>
            {
                UnityEngine.Debug.Log("change brain");
                UiToBrainMessage message = new UiToBrainMessage
                {
                    TaskToExecute = 0,
                    ModelId = BrainId
                };
                Messenger.Default.Send(message, MessageContext.UiToBrain);
            });
            m_BrainVisualisation.UpdateBrainMeshes.AddListener((VisuID) =>
             {
                 UnityEngine.Debug.Log("change visu");
                 UiToBrainMessage message = new UiToBrainMessage
                 {
                     TaskToExecute = 1,
                     MeshesToDisplay = VisuID
                 };
                 Messenger.Default.Send(message, MessageContext.UiToBrain);
             });
            m_BrainGain.gainHasChanged += new Tools.BrainGainChangedEventHandler((NewGain) =>
            {
                UnityEngine.Debug.Log("update gain");
                UiToBrainMessage message = new UiToBrainMessage
                {
                    TaskToExecute = 2,
                    Gain = NewGain
                };
                Messenger.Default.Send(message, MessageContext.UiToBrain);
            });
        }
        #endregion
    }
}