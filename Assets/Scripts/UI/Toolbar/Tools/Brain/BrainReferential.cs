using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public class BrainReferential : Tool
    {
        public GenericEvent<int> needToChangeBrain = new GenericEvent<int>();

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
            switch (VisuID)
            {
                case 0:
                    ChangeReferentialSafely(ApplicationState.Module3D.Patient.HasMNI, VisuID);
                    break;
                case 1:
                    ChangeReferentialSafely(ApplicationState.Module3D.Patient.HasPAT, VisuID);
                    break;
                case 2:
                    needToChangeBrain.Invoke(VisuID);
                    break;
                default:
                    Debug.LogError("BrainReferential.cs => Id of Brain Referential does not exist");
                    break;
            }
        }

        private void ChangeReferentialSafely(bool HasData, int VisuID)
        {
            if (HasData)
            {
                needToChangeBrain.Invoke(VisuID);
            }
            else
            {
                m_Dropdown.value = VisuID + 1;
                m_Dropdown.RefreshShownValue();
            }
        }
    }
}