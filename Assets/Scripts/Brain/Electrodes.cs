using System.IO;                    //Stream/BinaryReader
using UnityEngine;
using System.Collections.Generic;   //List<T>
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

public class ElectrodePlot
{
    public string label
    {
        get
        {
            return plotName + Id;
        }
    }

    public string Id
    {
        get
        {
            if (id == -1)
                return "";
            else if (id < 10)
                return "0" + id;
            else
                return id.ToString();
        }
    }

    public int id;
    public Vector3 position3D;
    public MarsAtlas_plot atlas;
    string plotName = "";

    public ElectrodePlot(string p_plotName, int p_id, Vector3 p_position)
    {
        plotName = p_plotName;
        id = p_id;
        position3D = p_position;
    }

    void display()
    {
        //Debug.Log("3D Position of Electrode : " + id + " is " + position3D);
    }
}

public class Electrode
{
    public string name;
    public List<bool> mask;
    public List<ElectrodePlot> plots;

    public Electrode(string p_elecName)
    {
        name = p_elecName;
        plots = new List<ElectrodePlot>();
        mask = new List<bool>();
    }
}

public class Electrodes : MonoBehaviour
{
    string patientName = "";
    List<bool> mask = new List<bool>();
    List<Electrode> electrodes = new List<Electrode>();
    MarsAtlas atlas = null;

    void Awake()
    {
        atlas = new MarsAtlas(Application.dataPath);
    }

    void OnDestroy()
    {
        atlas.Dispose();
    }

    public int loadPtsFile(string p_pathPtsFile)
    {
        bool isIntra = !(p_pathPtsFile.Contains("MNI_EEG"));
        if (isIntra)
            loadIntraElec(p_pathPtsFile);
        else
            loadScalpElec(p_pathPtsFile);

        return 0;
    }

    int loadIntraElec(string p_pathPtsFile)
    {
        if (mask.Count > 0)
            mask = new List<bool>();

        if (electrodes.Count > 0)
            electrodes = new List<Electrode>();

        string line = "", currentElectrodeName = "";
        int numberPlot = 0;

        using (StreamReader sr = new StreamReader(p_pathPtsFile))
        {
            line = sr.ReadLine();
            if (!line.StartsWith("ptsfile"))
            {
                Debug.LogError("Error ElectrodeList.LoadPtsFile -> format file incorrect : " + p_pathPtsFile);
                return -1;
            }

            line = sr.ReadLine();
            line = sr.ReadLine();
            numberPlot = int.Parse(line);

            if (numberPlot <= 0)
            {
                Debug.LogError("Error ElectrodeList.LoadPtsFile -> format file incorrect : " + p_pathPtsFile);
                return -1;
            }

            patientName = getPatientName(p_pathPtsFile);
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
                    if (plot[0] != 'p') //if electrode is p then just to lower case else change p for ' and to lower
                        plot = plot.ToLower().Replace('p', '\'');
                    else
                        plot = plot.ToLower();

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

                    if (plotName == currentElectrodeName) //This is just a new plot in current Electrode
                    {
                        electrodes[electrodes.Count - 1].plots.Add(new ElectrodePlot(plotName, plotID, new Vector3(x, y, z)));
                        electrodes[electrodes.Count - 1].mask.Add(true);
                    }
                    else //This is a new Electrode
                    {
                        currentElectrodeName = plotName;
                        electrodes.Add(new Electrode(plotName));
                        mask.Add(true);
                        electrodes[electrodes.Count - 1].plots.Add(new ElectrodePlot(plotName, plotID, new Vector3(x, y, z)));
                        electrodes[electrodes.Count - 1].mask.Add(true);
                    }
                }
                else
                {
                    Debug.LogError("Error Reading Pts : Each Line must have at least 4 elements (label + xyz coordinates) ");
                }
            }
        }
        return 0;
    }

    int loadScalpElec(string p_pathPtsFile)
    {
        if (mask.Count > 0)
            mask = new List<bool>();

        if (electrodes.Count > 0)
            electrodes = new List<Electrode>();

        string line = "";
        int numberPlot = 0;

        using (StreamReader sr = new StreamReader(p_pathPtsFile))
        {
            line = sr.ReadLine();
            if (!line.StartsWith("ptsfile"))
            {
                Debug.LogError("Error ElectrodeList.LoadPtsFile -> format file incorrect : " + p_pathPtsFile);
                return -1;
            }

            line = sr.ReadLine();
            line = sr.ReadLine();
            numberPlot = int.Parse(line);

            if (numberPlot <= 0)
            {
                Debug.LogError("Error ElectrodeList.LoadPtsFile -> format file incorrect : " + p_pathPtsFile);
                return -1;
            }

            electrodes.Add(new Electrode("Scalp_Eeg"));
            mask.Add(true);
            patientName = getPatientName(p_pathPtsFile);
            for (int i = 0; i < numberPlot; i++)
            {
                line = sr.ReadLine();
                string[] split = line.Split(new string[] { "\t" }, System.StringSplitOptions.RemoveEmptyEntries);

                if (split.Length >= 3)
                {
                    string plot = split.GetValue(0).ToString().ToLower();
                    float x = float.Parse(split.GetValue(1).ToString());
                    float y = float.Parse(split.GetValue(2).ToString());
                    float z = float.Parse(split.GetValue(3).ToString());

                    electrodes[electrodes.Count - 1].plots.Add(new ElectrodePlot(plot, -1, new Vector3(x, y, z)));
                    electrodes[electrodes.Count - 1].mask.Add(true);
                }
                else
                {
                    Debug.LogError("Error Reading Pts : Each Line must have at least 4 elements (label + xyz coordinates) ");
                }
            }
        }
        return 0;
    }

    public int loadDefaultPearl(ELAN[] elanFiles)
    {
        if (mask.Count > 0)
            mask = new List<bool>();

        if (electrodes.Count > 0)
            electrodes = new List<Electrode>();

        string currentElectrodeName = "";
        int idHandle = ELAN.returnFirstValidHandleId(elanFiles);
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

            if (plotName == currentElectrodeName) //This is just a new plot in current Electrode
            {
                electrodes[electrodes.Count - 1].plots.Add(new ElectrodePlot(plotName, plotID, new Vector3(-5 + ((electrodes.Count - 1) * -5), 0, -5 + (electrodes[electrodes.Count - 1].plots.Count) * -5)));
                electrodes[electrodes.Count - 1].mask.Add(true);
            }
            else //This is a new Electrode
            {
                currentElectrodeName = plotName;
                electrodes.Add(new Electrode(plotName));
                mask.Add(true);
                electrodes[electrodes.Count - 1].plots.Add(new ElectrodePlot(plotName, plotID, new Vector3(-5 + ((electrodes.Count - 1) * -5), 0, -5 + (electrodes[electrodes.Count - 1].plots.Count) * -5)));
                electrodes[electrodes.Count - 1].mask.Add(true);
            }
        }
        return 0;
    }

    public void loadAtlasData(string p_pathAtlasCSV)
    {
        atlas.loadPatientAtlas(p_pathAtlasCSV);
        atlas.findElectrodesWithAtlas(electrodes);
    }

    public void loadElecOnBrain()
    {
        GameObject ElecPlot = Resources.Load("Prefabs/Brain-ElecPlot", typeof(GameObject)) as GameObject;
        for (int i = 0; i < electrodes.Count; i++)
        {
            GameObject currentElec = new GameObject();
            currentElec.name = electrodes[i].name;
            currentElec.transform.parent = gameObject.transform;

            for (int j = 0; j < electrodes[i].plots.Count; j++)
            {
                /********************** /!\Axe x de unity inversé /!\ **********************/
                electrodes[i].plots[j].position3D.x = -electrodes[i].plots[j].position3D.x;
                /***************************************************************************/

                GameObject currentElecPlot = (GameObject)Instantiate(ElecPlot, electrodes[i].plots[j].position3D, Quaternion.identity);
                currentElecPlot.name = electrodes[i].plots[j].label; // electrodes[i].name + electrodes[i].plots[j].id;
                currentElecPlot.transform.parent = currentElec.transform;

                ElecPlotSize sphereSizeScript = currentElecPlot.AddComponent<ElecPlotSize>();
                sphereSizeScript.init(currentElecPlot.name, electrodes[i].plots[j]);
            }
        }
    }

    public void updateElecPosition()
    {
        for (int i = 0; i < electrodes.Count; i++)
        {
            Transform childTransform = gameObject.transform.Find(electrodes[i].name);
            for (int j = 0; j < electrodes[i].plots.Count; j++)
            {
                Transform currentElec = childTransform.Find(electrodes[i].name + electrodes[i].plots[j].id.ToString());
                if (currentElec != null)
                {
                    currentElec.localPosition = new Vector3(-electrodes[i].plots[j].position3D.x,
                                                            electrodes[i].plots[j].position3D.y,
                                                            electrodes[i].plots[j].position3D.z);
                    currentElec.GetComponent<ElecPlotSize>().setPlot(electrodes[i].plots[j]);
                    //Debug.Log("Update : " + currentElec.name);
                }
            }
        }
    }

    public void updateElecPearl()
    {
        int x = 0;
        for (int i = 0; i < electrodes.Count; i++)
        {
            int y = 0, z = 0;

            x += 5;
            Transform childTransform = gameObject.transform.Find(electrodes[i].name);
            for (int j = 0; j < electrodes[i].plots.Count; j++)
            {
                Transform currentElec = childTransform.Find(electrodes[i].name + electrodes[i].plots[j].id.ToString());
                z -= 5;
                if (currentElec != null)
                    currentElec.localPosition = new Vector3(x, y, z);
            }
        }

    }

    string getPatientName(string p_pathPtsFile)
    {
        string[] splitFileName = p_pathPtsFile.Split(new char[] { '\\', '_', '.' });
        return splitFileName.GetValue(splitFileName.Length - 2).ToString();
    }

    void displayElecrodesList()
    {
        //Debug.Log("Electrode List for patient : " + patientName);
        //Debug.Log("There is " + electrodes.Count + " Electrodes ");

        for (int i = 0; i < electrodes.Count; i++)
        {
            //Debug.Log(" => This is Electrode " + electrodes[i].name);
            for (int j = 0; j < electrodes[i].plots.Count; j++)
            {
                //Debug.Log("    -" + electrodes[i].name + electrodes[i].plots[j].id +
                // " => Position : " + electrodes[i].plots[j].position3D);
            }
        }
    }
}
