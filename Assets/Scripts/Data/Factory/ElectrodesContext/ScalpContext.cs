using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.Factory
{
    public class ScalpContext : IElectrodesContext
    {
        public List<object> Electrodes
        {
            get;
            private set;
        }
        private PtsFile m_ptsFile = null;

        public void LoadElectrodes(string pathPts)
        {
            m_ptsFile = new PtsFile(pathPts);
            Electrodes = new List<object>();
            foreach (Tuple<string, Vector3> rawElectrode in m_ptsFile.Electrodes)
            {
                Electrodes.Add(new EEG_Plot(rawElectrode.Item1, rawElectrode.Item2));
            }
        }

        public void LoadAtlasData(string pathAtlasCsv)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">Root Brain Gameobject</param>
        public void LoadElectrodesOnBrain(GameObject parent)
        {
            GameObject ElectrodePlot_prefab = Resources.Load("Prefabs/Brain-ElecPlot", typeof(GameObject)) as GameObject;

            GameObject Electrode = new GameObject();
            Electrode.name = "Scalp_Eeg";
            Electrode.transform.parent = parent.transform;
            for (int i = 0; i < Electrodes.Count; i++)
            {
                EEG_Plot CurrentPlot = (EEG_Plot)Electrodes[i];
                /********************** /!\Axe x de unity inversé /!\ **********************/
                CurrentPlot.Coordinates.Set(-CurrentPlot.Coordinates.x, CurrentPlot.Coordinates.y, CurrentPlot.Coordinates.z);
                /***************************************************************************/

                GameObject NewPlot = GameObject.Instantiate(ElectrodePlot_prefab, CurrentPlot.Coordinates, Quaternion.identity);
                NewPlot.name = CurrentPlot.Label;
                NewPlot.transform.parent = Electrode.transform;

                //ElecPlotSize sphereSizeScript = currentElecPlot.AddComponent<ElecPlotSize>();
                //sphereSizeScript.init(currentElecPlot.name, electrodes[i]);
            }
        }

        public void LoadDefaultPearl(ELAN[] elanFiles)
        {
            int idHandle = ELAN.returnFirstValidHandleId(elanFiles);
            if (idHandle != -1)
            {
                Electrodes = new List<object>();
                for (int i = 0; i < elanFiles[idHandle].electrodes.Length; i++)
                {
                    string plotName = elanFiles[idHandle].electrodes[i].name.ToLower();
                    Electrodes.Add(new EEG_Plot(plotName, new Vector3(5 * i, 0, 0)));
                }
            }
            else
            {
                throw new ArgumentException("Error ScalpContext.LoadDefaultPearl : no valid elanFiles have been received");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">Root Electrode object</param>
        public void UpdateElectrodesPosition(GameObject parent)
        {
            for (int i = 0; i < Electrodes.Count; i++)
            {
                EEG_Plot CurrentPlot = (EEG_Plot)Electrodes[i];
                Transform ChildTransform = parent.transform.Find("Scalp_Eeg");
                Transform CurrentElectrodeTransform = ChildTransform.Find(CurrentPlot.Label);
                if (CurrentElectrodeTransform != null)
                {
                    CurrentElectrodeTransform.localPosition = new Vector3(-CurrentPlot.Coordinates.x, CurrentPlot.Coordinates.y, CurrentPlot.Coordinates.z);
                    //currentElecTransform.GetComponent<ElecPlotSize>().setPlot(electrodes[i]);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">Root Electrode object</param>
        public void UpdateElectrodesPearl(GameObject parent)
        {
            Transform ChildTransform = parent.transform.Find("Scalp_Eeg");
            Transform[] ActiveElectrodes = ChildTransform.GetComponentsInChildren<Transform>().Where(xx => xx.gameObject.activeSelf).ToArray();

            for (int i = 0; i < ActiveElectrodes.Length; i++)
                ActiveElectrodes[i].localPosition = new Vector3(i * 5, 0, 0);
        }
    }
}
