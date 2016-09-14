using System.IO;                    //Stream/BinaryReader
using UnityEngine;
using System.Collections.Generic;   //List<T>
using System.Text.RegularExpressions;

public class ElectrodePlot
{
    public int id;
    public Vector3 position3D;

    public ElectrodePlot(int p_id, Vector3 p_position)
    {
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

    public int loadPtsFile(string p_pathPtsFile)
    {
        string line = "", currentElectrodeName = "";
        int numberPlot = 0;

        using (StreamReader sr = new StreamReader(p_pathPtsFile))
        {
            line = sr.ReadLine();
            if (!line.StartsWith("ptsfile"))
            {
                //Debug.LogError("Error ElectrodeList.LoadPtsFile -> format file incorrect : " + p_pathPtsFile);
                return -1;
            }

            line = sr.ReadLine();
            line = sr.ReadLine();
            numberPlot = int.Parse(line);

            if (numberPlot <= 0)
            {
                //Debug.LogError("Error ElectrodeList.LoadPtsFile -> format file incorrect : " + p_pathPtsFile);
                return -1;
            }

            patientName = getPatientName(p_pathPtsFile);
            for (int i = 0; i < numberPlot; i++)
            {
                line = sr.ReadLine();
                string[] split = line.Split(new string[] { "\t" }, System.StringSplitOptions.RemoveEmptyEntries);

                if (split.Length == 9)
                {
                    string plot = split.GetValue(0).ToString().ToLower();
                    Regex re = new Regex(@"([a-zA-Z]+)(\d+)");
                    Match result = re.Match(plot);
                    string plotName = result.Groups[1].Value;
                    int plotID = int.Parse(result.Groups[2].Value.ToString());

                    float x = float.Parse(split.GetValue(1).ToString());
                    float y = float.Parse(split.GetValue(2).ToString());
                    float z = float.Parse(split.GetValue(3).ToString());

                    if (plotName == currentElectrodeName) //This is just a new plot in current Electrode
                    {
                        electrodes[electrodes.Count - 1].plots.Add(new ElectrodePlot(plotID, new Vector3(x, y, z)));
                        electrodes[electrodes.Count - 1].mask.Add(true);
                    }
                    else //This is a new Electrode
                    {
                        currentElectrodeName = plotName;
                        electrodes.Add(new Electrode(plotName));
                        mask.Add(true);
                        electrodes[electrodes.Count - 1].plots.Add(new ElectrodePlot(plotID, new Vector3(x, y, z)));
                        electrodes[electrodes.Count - 1].mask.Add(true);
                    }
                }
                else
                {
                    //Debug.LogError("Error Reading Pts : Each Line Must have 9 elements");
                    return -1;
                }
            }

            //displayElecrodesList();
        }
        return 0;
    }

    public void loadElecOnBrain()
    {
        GameObject ElecPlot = Resources.Load("Prefabs/ElecPlot", typeof(GameObject)) as GameObject;
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
                currentElecPlot.name = electrodes[i].name + electrodes[i].plots[j].id;
                currentElecPlot.transform.parent = currentElec.transform;

                if (j > 0)
                {
                    SphereSize sphereSizeScript = currentElecPlot.AddComponent<SphereSize>();
                    sphereSizeScript.InitializeData(currentElecPlot.name);
                }
            }
        }
    }

    string getPatientName(string p_pathPtsFile)
    {
        string[] splitFileName = p_pathPtsFile.Split(new char[] { '\\', '_', '.' });
        string patientName = splitFileName.GetValue(splitFileName.Length - 2).ToString();

        return patientName;
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
