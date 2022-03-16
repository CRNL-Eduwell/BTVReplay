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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">Root Brain Gameobject</param>
        public void LoadElectrodesOnBrain(GameObject parent, List<AnatomicalSite> sites)
        {
            GameObject ElectrodePlot_prefab = Resources.Load("Prefabs/Brain-ElecPlot", typeof(GameObject)) as GameObject;

            GameObject Electrode = new GameObject();
            Electrode.name = "Scalp_Eeg";
            Electrode.transform.parent = parent.transform;
            ScalpDataProjector dps = parent.transform.parent.GetChild(2).GetComponent<ScalpDataProjector>();
            for (int i = 0; i < sites.Count; i++)
            {
                //EEG_Plot CurrentPlot = (EEG_Plot)Electrodes[i];
                EEG_Plot CurrentPlot = new EEG_Plot(sites[i].Label, sites[i].Coordinates);
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
    }
}
