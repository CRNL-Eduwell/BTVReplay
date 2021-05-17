using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BTV.Data;
using BTV.Services.EegFileService;

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
            ScalpDataProjector dps = parent.transform.parent.GetChild(2).GetComponent<ScalpDataProjector>();
            for (int i = 0; i < Electrodes.Count; i++)
            {
                EEG_Plot CurrentPlot = (EEG_Plot)Electrodes[i];
                /********************** /!\Axe x de unity inversé /!\ **********************/
                //CurrentPlot.Coordinates.Set(CurrentPlot.Coordinates.z, -CurrentPlot.Coordinates.x, CurrentPlot.Coordinates.y);
                CurrentPlot.Coordinates = new Vector3(-CurrentPlot.Coordinates.x, CurrentPlot.Coordinates.y, CurrentPlot.Coordinates.z);
                /***************************************************************************/

                GameObject NewPlot = GameObject.Instantiate(ElectrodePlot_prefab, CurrentPlot.Coordinates, Quaternion.identity);
                NewPlot.name = CurrentPlot.Label;
                NewPlot.transform.parent = Electrode.transform;
                NewPlot.GetComponent<Site>().Init(CurrentPlot.Label);
                if (dps != null)
                {
                    if (NewPlot.activeSelf)
                        dps.AddSite(NewPlot.GetComponent<Site>());
                }
            }
            dps.InitArrays();
        }

        public void LoadDefaultPearl()
        {
            BtvProgram container = EegFileService.ReturnFirstValidContainer();
            if (container != null)
            {
                Electrodes = new List<object>();
                int count = 0;
                foreach (var channel in container.Channels)
                {
                    string plotName = channel.Label.ToLower();
                    Electrodes.Add(new EEG_Plot(plotName, new Vector3(5 * count, 0, 0)));
                    count++;
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
                    CurrentElectrodeTransform.GetComponent<Site>().UpdatePlot(Electrodes[i]);
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
