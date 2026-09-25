using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BTV.Data;

namespace Assets.Scripts.Data.Factory
{
    public class ScalpContext : IElectrodesContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">Root Brain Gameobject</param>
        public void LoadElectrodesOnBrain(GameObject parent, List<AnatomicalSite> sites,
            Action<Site, AnatomicalSite> initializeSite)
        {
            if (sites == null || sites.Count == 0)
            {
                Debug.LogWarning("ScalpContext.LoadElectrodesOnBrain: no electrode sites for this subject; skipping electrode placement.");
                return;
            }

            GameObject ElectrodePlot_prefab = Resources.Load("Prefabs/Brain-ElecPlot", typeof(GameObject)) as GameObject;

            // Local space, as in IntraContext, so a rebuild needs no temporary move of the brain.
            GameObject Electrode = new GameObject("Scalp_Eeg");
            Electrode.transform.SetParent(parent.transform, false);
            // By component: this was GetChild(2) of the brain root, which during a rebuild is a
            // hemisphere still waiting for its deferred Destroy, so the projector came back null
            // and InitArrays below threw.
            ScalpDataProjector dps = parent.transform.parent.GetComponentInChildren<ScalpDataProjector>(true);
            for (int i = 0; i < sites.Count; i++)
            {
                /********************** /!\Axe x de unity inversé /!\ **********************/
                Vector3 Coordinates = new Vector3(-sites[i].Coordinates.x, sites[i].Coordinates.y, sites[i].Coordinates.z);
                /***************************************************************************/

                GameObject NewPlot = GameObject.Instantiate(ElectrodePlot_prefab, Electrode.transform);
                NewPlot.transform.localPosition = Coordinates;
                NewPlot.transform.localRotation = Quaternion.identity;
                NewPlot.name = sites[i].Label;
                initializeSite(NewPlot.GetComponent<Site>(), sites[i]);
                if (dps != null)
                {
                    if (NewPlot.activeSelf)
                        dps.AddSite(NewPlot.GetComponent<Site>());
                }
            }
            if (dps != null) dps.InitArrays();
        }
    }
}
