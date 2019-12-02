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
        public List<object> Electrodes
        {
            get;
            private set;
        }
        private PtsFile m_ptsFile = null;
        private MarsAtlas m_atlas = null;

        public void LoadElectrodes(string pathPts)
        {
            m_ptsFile = new PtsFile(pathPts);
            Electrodes = new List<object>();
            foreach (Tuple<string, Vector3> rawElectrode in m_ptsFile.Electrodes)
            {
                string correctedName = CorrectPlotName(rawElectrode.Item1);
                Tuple<string, int> intraName = GetIntraPlotInformation(correctedName);
                Electrodes.Add(new Intra_Plot(intraName.Item1, intraName.Item2, rawElectrode.Item2));
            }
        }

        public void LoadAtlasData(string pathAtlasCsv)
        {
            if (File.Exists(pathAtlasCsv))
            {
                m_atlas = new MarsAtlas(Application.dataPath);
                m_atlas.loadPatientAtlas(pathAtlasCsv);
                m_atlas.findElectrodesWithAtlas(Electrodes);
            }
        }

        public void LoadElectrodesOnBrain(GameObject parent)
        {
            GameObject ElectrodePlot_prefab = Resources.Load("Prefabs/Brain-ElecPlot", typeof(GameObject)) as GameObject;
            List<Intra_Electrode> electrodes = GetIntraElectrodes();
            for (int i = 0; i < electrodes.Count; i++)
            {
                GameObject Electrode = new GameObject();
                Electrode.name = electrodes[i].Label;
                Electrode.transform.parent = parent.transform;

                for (int j = 0; j < electrodes[i].Plots.Count; j++)
                {
                    /********************** /!\Axe x de unity inversé /!\ **********************/
                    electrodes[i].Plots[j].Coordinates = new Vector3(-electrodes[i].Plots[j].Coordinates.x,
                                                                      electrodes[i].Plots[j].Coordinates.y,
                                                                      electrodes[i].Plots[j].Coordinates.z);
                    /***************************************************************************/

                    GameObject NewPlot = GameObject.Instantiate(ElectrodePlot_prefab, electrodes[i].Plots[j].Coordinates, Quaternion.identity);
                    NewPlot.name = electrodes[i].Plots[j].Label;
                    NewPlot.transform.parent = Electrode.transform;
                    NewPlot.GetComponent<Site>().Init(electrodes[i].Plots[j]);
                }
            }
        }

        public void LoadDefaultPearl()
        {
            BtvProgram container = EegFileService.ReturnFirstValidContainer();
            if (container != null)
            {
                Electrodes = new List<object>();

                string memPlot = "";
                int nbIntraElec = 0, nbIntraPlot = 0;
                
                foreach (var channel in container.Channels)
                {
                    Tuple<string, int> NameAndId = GetIntraPlotInformation(channel.Label);
                    if (memPlot != NameAndId.Item1)
                    {
                        nbIntraElec += 5;
                        nbIntraPlot = -5;
                        memPlot = NameAndId.Item1;
                    }
                    else if (memPlot == NameAndId.Item1)
                    {
                        nbIntraPlot -= 5;
                    }
                    Electrodes.Add(new Intra_Plot(NameAndId.Item1, NameAndId.Item2, new Vector3(nbIntraElec, 0, nbIntraPlot)));
                }
            }
            else
            {
                throw new ArgumentException("Error IntraContext.LoadDefaultPearl : no valid elanFiles have been received");
            }
        }

        public void UpdateElectrodesPosition(GameObject parent)
        {
            for (int i = 0; i < Electrodes.Count; i++)
            {
                Intra_Plot CurrentPlot = (Intra_Plot)Electrodes[i];
                Transform ChildTransform = parent.transform.Find(CurrentPlot.Parent);
                Transform CurrentElectrodeTransform = ChildTransform.Find(CurrentPlot.Label);
                if (CurrentElectrodeTransform != null)
                {
                    CurrentElectrodeTransform.localPosition = new Vector3(-CurrentPlot.Coordinates.x, CurrentPlot.Coordinates.y, CurrentPlot.Coordinates.z);
                    CurrentElectrodeTransform.GetComponent<Site>().UpdatePlot(Electrodes[i]);
                }
            }
        }

        public void UpdateElectrodesPearl(GameObject parent)
        {
            List<Intra_Electrode> electrodesIntra = GetIntraElectrodes();

            int x = 0;
            for (int i = 0; i < electrodesIntra.Count; i++)
            {
                int y = 0, z = 0;

                x += 5;
                Transform ChildTransform = parent.transform.Find(electrodesIntra[i].Label);
                for (int j = 0; j < electrodesIntra[i].Plots.Count; j++)
                {
                    Transform Electrode = ChildTransform.Find(electrodesIntra[i].Plots[j].Label);
                    z -= 5;
                    if (Electrode != null)
                        Electrode.localPosition = new Vector3(x, y, z);
                }
            }
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

        private List<Intra_Electrode> GetIntraElectrodes()
        {
            List<Intra_Electrode> IntraElectrodes = new List<Intra_Electrode>();

            string currentElectrodeName = "";
            for (int i = 0; i < Electrodes.Count; i++)
            {
                Intra_Plot CurrentPlot = (Intra_Plot)Electrodes[i];
                string ElectrodeName = CurrentPlot.Parent;
                if (ElectrodeName == currentElectrodeName)  //This is just a new plot in current Electrode
                {
                    IntraElectrodes[IntraElectrodes.Count - 1].Plots.Add(new Intra_Plot(CurrentPlot));
                }
                else    //This is a new Electrode
                {
                    currentElectrodeName = ElectrodeName;
                    IntraElectrodes.Add(new Intra_Electrode(ElectrodeName));
                    IntraElectrodes[IntraElectrodes.Count - 1].Plots.Add(new Intra_Plot(CurrentPlot));
                }
            }

            return IntraElectrodes;
        }
    }
}
