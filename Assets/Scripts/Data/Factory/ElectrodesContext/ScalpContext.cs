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
                /********************** /!\Axe x de unity inversé /!\ **********************/
                Vector3 Coordinates = new Vector3(-sites[i].Coordinates.x, sites[i].Coordinates.y, sites[i].Coordinates.z);
                /***************************************************************************/

                GameObject NewPlot = GameObject.Instantiate(ElectrodePlot_prefab, Coordinates, Quaternion.identity, Electrode.transform);
                NewPlot.name = sites[i].Label;
                NewPlot.GetComponent<Site>().Init(sites[i]);
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
