using System.IO;                    //Stream/BinaryReader
using UnityEngine;
using System.Collections.Generic;   //List<T>
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

public class EEG_Plot
{
    public string Label
    {
        get;
        set;
    }
    public Vector3 Coordinates
    {
        get;
        set;
    }

    public EEG_Plot(string label, Vector3 coordinates)
    {
        Label = label;
        Coordinates = coordinates;
    }

    public void display()
    {
        Debug.Log("Name : " + Label);
        Debug.Log("Coordinates : " + Coordinates.ToString());
    }
}

public class Intra_Plot : EEG_Plot
{
    /// <summary>
    /// Label of the intracerebral plot according to the EEG File
    /// </summary>
    public new string Label
    {
        get;
        set;
    }
    /// <summary>
    /// Label of the intracerebral plot according to the Atlas File
    /// They use a 0 in front of numbers smaller than 10
    /// </summary>
    public string AtlasLabel
    {
        get
        {
            return Parent + IdToStr(Id);
        }
    }
    public string Parent
    {
        get;
        set;
    }
    public int Id
    {
        get;
        set;
    }
    public MarsAtlas_plot Atlas
    {
        get;
        set;
    }

    public Intra_Plot(Intra_Plot plotToCopy) : base(plotToCopy.Label, plotToCopy.Coordinates)
    {
        Label = plotToCopy.Label;
        Parent = plotToCopy.Parent;
        Id = plotToCopy.Id;
        Atlas = plotToCopy.Atlas;
    }

    public Intra_Plot(string label, int id, Vector3 coordinates) : base("", coordinates)
    {
        Parent = label;
        Id = id;
        Label = label + id;
        base.Label = Label;
    }

    string IdToStr(int id)
    {
        if (id == -1)
            return "";
        else if (id < 10)
            return "0" + id;
        else
            return id.ToString();
    }
}

public class IntraElec
{
    public string Label
    {
        get;
        set;
    }

    public List<Intra_Plot> Plots
    {
        get;
        set;
    }

    public IntraElec(string label)
    {
        Label = label;
        Plots = new List<Intra_Plot>();
    }
}

public class Electrodes : MonoBehaviour
{
    List<object> electrodes = new List<object>();
    IElectrodes myInterface = null;

    public void loadPtsFile(string pathPts, eeg_Technology eeg)
    {
        if (eeg == eeg_Technology.intra)
            myInterface = gameObject.AddComponent<IntraElectrodes>();
        else if (eeg == eeg_Technology.scalp)
            myInterface = gameObject.AddComponent<ScalpElectrodes>();

        if(myInterface != null)
            myInterface.loadPtsFile(pathPts);
    }

    public void loadElecOnBrain()
    {
        myInterface.loadElectrodesOnBrain();
    }

    public void updateElecPosition()
    {
        myInterface.updateElectrodesPosition();
    }

    public void loadAtlasData(string pathAtlasCsv)
    {
        myInterface.loadAtlasData(pathAtlasCsv);
    }

    //=== Default Data
    public int loadDefaultPearl(ELAN[] elanFiles, eeg_Technology eeg)
    {
        if (myInterface == null && eeg == eeg_Technology.intra)
            myInterface = gameObject.AddComponent<IntraElectrodes>();
        else if (myInterface == null && eeg == eeg_Technology.scalp)
            myInterface = gameObject.AddComponent<ScalpElectrodes>();

        myInterface.loadDefaultPearl(elanFiles);
        return 0;
    }

    public void updateElecPearl()
    {
        myInterface.updateElectrodesPearl();
    }
}
