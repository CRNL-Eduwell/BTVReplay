using System.IO;                    //Stream/BinaryReader
using System.Linq;
using System.Collections.Generic;   //List<T>
using UnityEngine;
using BrainTV.Tools.NumberExtensions;

public class ScalpElectrodes : MonoBehaviour, IElectrodes
{
    public List<object> electrodes
    {
        get;
        private set;
    }

    void Awake()
    {
        electrodes = new List<object>();
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

                if (split.Length >= 3)
                {
                    string plot = split.GetValue(0).ToString().ToLower();
                    split.GetValue(1).ToString().TryParseFloat(out float x);
                    split.GetValue(2).ToString().TryParseFloat(out float y);
                    split.GetValue(3).ToString().TryParseFloat(out float z);

                    electrodes.Add(new EEG_Plot(plot, new Vector3(x, y, z)));
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

        GameObject currentElec = new GameObject();
        currentElec.name = "Scalp_Eeg";
        currentElec.transform.parent = gameObject.transform;
        for (int i = 0; i < electrodes.Count; i++)
        {
            EEG_Plot currentPlot = (EEG_Plot)electrodes[i];
            /********************** /!\Axe x de unity inversé /!\ **********************/
            currentPlot.Coordinates.Set(-currentPlot.Coordinates.x, currentPlot.Coordinates.y, currentPlot.Coordinates.z);
            /***************************************************************************/

            GameObject currentElecPlot = Instantiate(ElecPlot, currentPlot.Coordinates, Quaternion.identity);
            currentElecPlot.name = currentPlot.Label;
            currentElecPlot.transform.parent = currentElec.transform;

            //ElecPlotSize sphereSizeScript = currentElecPlot.AddComponent<ElecPlotSize>();
            //sphereSizeScript.init(currentElecPlot.name, electrodes[i]);
        }
    }

    public void updateElectrodesPosition()
    {
        for (int i = 0; i < electrodes.Count; i++)
        {
            EEG_Plot currentPlot = (EEG_Plot)electrodes[i];
            Transform childTransform = gameObject.transform.Find("Scalp_Eeg");
            Transform currentElecTransform = childTransform.Find(currentPlot.Label);
            if (currentElecTransform != null)
            {
                currentElecTransform.localPosition = new Vector3(-currentPlot.Coordinates.x, currentPlot.Coordinates.y, currentPlot.Coordinates.z);
                //currentElecTransform.GetComponent<ElecPlotSize>().setPlot(electrodes[i]);
            }
        }
    }

    public void loadAtlasData(string pathAtlasCsv)
    {

    }

    public void loadDefaultPearl(ELAN[] elanFiles)
    {
        if (electrodes.Count > 0)
            electrodes = new List<object>();

        int idHandle = ELAN.returnFirstValidHandleId(elanFiles);
        for (int i = 0; i < elanFiles[idHandle].electrodes.Length; i++)
        {
            string plotName = elanFiles[idHandle].electrodes[i].name.ToLower();
            electrodes.Add(new EEG_Plot(plotName, new Vector3(5 * i, 0, 0)));
        }
    }

    public void updateElectrodesPearl()
    {
        Transform childTransform = gameObject.transform.Find("Scalp_Eeg");
        Transform[] activeElec = childTransform.GetComponentsInChildren<Transform>().Where(xx => xx.gameObject.activeSelf).ToArray();

        for (int i = 0; i < activeElec.Length; i++)
            activeElec[i].localPosition = new Vector3(i * 5, 0, 0);
    }
}
