using System.IO;                    //Stream/BinaryReader
using UnityEngine;
using System.Collections.Generic;   //List<T>
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

public class IntraElectrodes : MonoBehaviour, IElectrodes
{
    public List<object> electrodes
    {
        get;
        private set;
    }

    MarsAtlas atlas = null;

    void Awake()
    {
        electrodes = new List<object>();
        atlas = new MarsAtlas(Application.dataPath);
    }

    void OnDestroy()
    {
        atlas.Dispose();
    }

    public int loadPtsFile(string pathPts)
    {
        if (electrodes.Count > 0)
            electrodes = new List<object>();

        string line = "";
        int numberPlot = 0;

        using (StreamReader sr = new StreamReader(pathPts))
        {
            line = sr.ReadLine();
            if (!line.StartsWith("ptsfile"))
            {
                Debug.LogError("Error ElectrodeList.LoadPtsFile -> format file incorrect : " + pathPts);
                return -1;
            }

            line = sr.ReadLine();
            line = sr.ReadLine();
            numberPlot = int.Parse(line);

            if (numberPlot <= 0)
            {
                Debug.LogError("Error ElectrodeList.LoadPtsFile -> format file incorrect : " + pathPts);
                return -1;
            }

            for (int i = 0; i < numberPlot; i++)
            {
                line = sr.ReadLine();
                string[] split = line.Split(new string[] { "\t" }, System.StringSplitOptions.RemoveEmptyEntries);

                //if (split.Length != 9)
                //    Debug.LogError("Error Reading Pts : Each Line Must have 9 elements");

                if (split.Length >= 3)
                {
                    string plot = split.GetValue(0).ToString();

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
                    plot = string.Join(" ", tempPlot);

                    Regex ReLeft = new Regex(@"([a-zA-Z]+)(\d+)");
                    Regex ReRight = new Regex(@"([a-zA-Z]+)(\')(\d+)");

                    Match resultLeft = ReLeft.Match(plot);
                    Match resultRight = ReRight.Match(plot);

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

                    float x = float.Parse(split.GetValue(1).ToString());
                    float y = float.Parse(split.GetValue(2).ToString());
                    float z = float.Parse(split.GetValue(3).ToString());

                    electrodes.Add(new Intra_Plot(plotName, plotID, new Vector3(x, y, z)));
                }
                else
                {
                    Debug.LogError("Error Reading Pts : Each Line must have at least 4 elements (label + xyz coordinates) ");
                }
            }
        }
        return 0;
    }

    public void loadElectrodesOnBrain()
    {
        GameObject ElecPlot = Resources.Load("Prefabs/Brain-ElecPlot", typeof(GameObject)) as GameObject;
        List<IntraElec> electrodes = getIntraElectrodes();
        for (int i = 0; i < electrodes.Count; i++)
        {
            GameObject currentElec = new GameObject();
            currentElec.name = electrodes[i].Label;
            currentElec.transform.parent = gameObject.transform;

            for (int j = 0; j < electrodes[i].Plots.Count; j++)
            {
                /********************** /!\Axe x de unity inversé /!\ **********************/
                electrodes[i].Plots[j].Coordinates = new Vector3(-electrodes[i].Plots[j].Coordinates.x,
                                                                  electrodes[i].Plots[j].Coordinates.y,
                                                                  electrodes[i].Plots[j].Coordinates.z);
                /***************************************************************************/

                GameObject currentElecPlot = Instantiate(ElecPlot, electrodes[i].Plots[j].Coordinates, Quaternion.identity);
                currentElecPlot.name = electrodes[i].Plots[j].Label;
                currentElecPlot.transform.parent = currentElec.transform;

                ElecPlotSize sphereSizeScript = currentElecPlot.AddComponent<ElecPlotSize>();
                sphereSizeScript.init(currentElecPlot.name, electrodes[i].Plots[j]);
            }
        }
    }

    public void updateElectrodesPosition()
    {
        for (int i = 0; i < electrodes.Count; i++)
        {
            Intra_Plot currentPlot = (Intra_Plot)electrodes[i];
            Transform childTransform = gameObject.transform.Find(currentPlot.Parent);
            Transform currentElecTransform = childTransform.Find(currentPlot.Label);
            if (currentElecTransform != null)
            {
                currentElecTransform.localPosition = new Vector3(currentPlot.Coordinates.x, currentPlot.Coordinates.y, currentPlot.Coordinates.z);
                currentElecTransform.GetComponent<ElecPlotSize>().setPlot(electrodes[i]);
            }
        }
    }

    public void loadAtlasData(string pathAtlasCsv)
    {
        atlas.loadPatientAtlas(pathAtlasCsv);
        atlas.findElectrodesWithAtlas(electrodes);
    }

    public void loadDefaultPearl(ELAN[] elanFiles)
    {
        if (electrodes.Count > 0)
            electrodes = new List<object>();

        int idHandle = ELAN.returnFirstValidHandleId(elanFiles);
        int nbIntraElec = 0, nbIntraPlot = 0;
        string memPlot = ""; 
        for (int i = 0; i < elanFiles[idHandle].electrodes.Length; i++)
        {
            Regex ReLeft = new Regex(@"([a-zA-Z]+)(\d+)");
            Regex ReRight = new Regex(@"([a-zA-Z]+)(\')(\d+)");

            Match resultLeft = ReLeft.Match(elanFiles[idHandle].electrodes[i].name);
            Match resultRight = ReRight.Match(elanFiles[idHandle].electrodes[i].name);

            string plotName = "";
            int plotID = -1;

            if (resultLeft.Groups[1].Length == 1)
            {
                plotName = (resultLeft.Groups[1].Value).ToLower();
                plotID = int.Parse(resultLeft.Groups[2].Value.ToString());
            }
            else if (resultRight.Groups[1].Length == 1)
            {
                plotName = (resultRight.Groups[1].Value + resultRight.Groups[2].Value).ToLower();
                plotID = int.Parse(resultRight.Groups[3].Value.ToString());
            }

            if (memPlot != plotName)
            {
                nbIntraElec += 5;
                nbIntraPlot = -5;
                memPlot = plotName;
            }
            else if (memPlot == plotName)
            {
                nbIntraPlot -= 5;
            }

            electrodes.Add(new Intra_Plot(plotName, plotID, new Vector3(nbIntraElec, 0, nbIntraPlot)));
        }
    }

    public void updateElectrodesPearl()
    {
        List<IntraElec> electrodesIntra = getIntraElectrodes();

        int x = 0;
        for (int i = 0; i < electrodesIntra.Count; i++)
        {
            int y = 0, z = 0;

            x += 5;
            Transform childTransform = gameObject.transform.Find(electrodesIntra[i].Label);
            for (int j = 0; j < electrodesIntra[i].Plots.Count; j++)
            {
                Transform currentElec = childTransform.Find(electrodesIntra[i].Plots[j].Label);
                z -= 5;
                if (currentElec != null)
                    currentElec.localPosition = new Vector3(x, y, z);
            }
        }
    }

    public List<IntraElec> getIntraElectrodes()
    {
        List<IntraElec> intraElectrodes = new List<IntraElec>();

        string currentElectrodeName = "";
        for (int i = 0; i < electrodes.Count; i++)
        {
            Intra_Plot currentPlot = (Intra_Plot)electrodes[i];
            string electrodeName = currentPlot.Parent;
            if (electrodeName == currentElectrodeName)  //This is just a new plot in current Electrode
            {
                intraElectrodes[intraElectrodes.Count - 1].Plots.Add(new Intra_Plot(currentPlot));
            }
            else    //This is a new Electrode
            {
                currentElectrodeName = electrodeName;
                intraElectrodes.Add(new IntraElec(electrodeName));
                intraElectrodes[intraElectrodes.Count - 1].Plots.Add(new Intra_Plot(currentPlot));
            }
        }

        return intraElectrodes;
    }
}
