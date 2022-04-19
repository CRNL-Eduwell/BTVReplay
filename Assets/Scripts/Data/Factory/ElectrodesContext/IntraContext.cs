using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using BTV.Services.EegFileService;
using BTV.Data;

namespace Assets.Scripts.Data.Factory
{
    public class IntraContext : IElectrodesContext
    {
        public void LoadElectrodesOnBrain(GameObject parent, List<AnatomicalSite> sites)
        {
            int counter = 0;
            GameObject ElectrodePlot_prefab = Resources.Load("Prefabs/Brain-ElecPlot", typeof(GameObject)) as GameObject;
            List<KeyValuePair<string, List<AnatomicalSite>>> electrodes = GetIntraElectrodes(sites);
            for (int i = 0; i < electrodes.Count; i++)
            {
                GameObject Electrode = new GameObject();
                Electrode.name = electrodes[i].Key;
                Electrode.transform.parent = parent.transform;

                for (int j = 0; j < electrodes[i].Value.Count; j++)
                {
                    /********************** /!\Axe x de unity inversé /!\ **********************/
                    electrodes[i].Value[j].Coordinates = new Vector3(-electrodes[i].Value[j].Coordinates.x,
                                                                      electrodes[i].Value[j].Coordinates.y,
                                                                      electrodes[i].Value[j].Coordinates.z);
                    /***************************************************************************/

                    GameObject NewPlot = GameObject.Instantiate(ElectrodePlot_prefab, electrodes[i].Value[j].Coordinates, Quaternion.identity);
                    NewPlot.name = electrodes[i].Value[j].Label;
                    NewPlot.transform.parent = Electrode.transform;
                    NewPlot.GetComponent<Site>().Init(sites[counter]);
                    counter++;
                }
            }
        }

        private List<KeyValuePair<string, List<AnatomicalSite>>> GetIntraElectrodes(List<AnatomicalSite> sites)
        {
            List<KeyValuePair<string, List<AnatomicalSite>>> IntraElectrodes = new List<KeyValuePair<string, List<AnatomicalSite>>>();
            string currentElectrodeName = "";
            for (int i = 0; i < sites.Count; i++)
            {
                string correctedName = CorrectPlotName(sites[i].Label);
                Tuple<string, int> intraName = GetIntraPlotInformation(correctedName);

                string ElectrodeName = intraName.Item1;
                if (ElectrodeName == currentElectrodeName)  //This is just a new plot in current Electrode
                {
                    IntraElectrodes[IntraElectrodes.Count - 1].Value.Add(new AnatomicalSite(correctedName, sites[i].Coordinates, sites[i].Broadmann, sites[i].MarsAtlas));
                }
                else    //This is a new Electrode
                {
                    currentElectrodeName = ElectrodeName;
                    IntraElectrodes.Add(new KeyValuePair<string, List<AnatomicalSite>>(ElectrodeName, new List<AnatomicalSite>()));
                    IntraElectrodes[IntraElectrodes.Count - 1].Value.Add(new AnatomicalSite(correctedName, sites[i].Coordinates, sites[i].Broadmann, sites[i].MarsAtlas));
                }
            }
            return IntraElectrodes;
        }

        private string CorrectPlotName(string plot)
        {
            //== Correct if elec is named Pp1 (P'1)
            List<int> nbP = plot.ToLower().Select((v, ii) => new { v, ii })
            .Where(c => c.v.Equals('p'))
            .Select(c => c.ii).ToList();

            if (nbP.Count > 1)
            {
                var stringBuilder = new StringBuilder(plot);
                stringBuilder[nbP.Count - 1] = '\'';
                plot = stringBuilder.ToString();
            }
            //=========
            if (plot[0] == 'p' || plot[0] == 'P') //if electrode is p then just to lower case else change p for ' and to lower
                plot = plot.ToLower();
            else
                plot = plot.ToLower().Replace('p', '\'');

            string[] tempPlot = plot.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", tempPlot);
        }

        private Tuple<string, int> GetIntraPlotInformation(string rawName)
        {
            Regex ReLeft = new Regex(@"([a-zA-Z]+)(\d+)");
            Regex ReRight = new Regex(@"([a-zA-Z]+)(\')(\d+)");

            Match resultLeft = ReLeft.Match(rawName);
            Match resultRight = ReRight.Match(rawName);

            string plotName = "";
            int plotID = -1;

            if (resultLeft.Groups[1].Length == 1)
            {
                plotName = resultLeft.Groups[1].Value;
                plotID = int.Parse(resultLeft.Groups[2].Value.ToString());
            }
            else if (resultRight.Groups[1].Length == 1)
            {
                plotName = resultRight.Groups[1].Value + resultRight.Groups[2].Value;
                plotID = int.Parse(resultRight.Groups[3].Value.ToString());
            }

            return new Tuple<string, int>(plotName, plotID);
        }
    }
}
