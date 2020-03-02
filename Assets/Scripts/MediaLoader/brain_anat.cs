public enum mesh_Configuration { leftright, single };
public enum eeg_Technology { intra, scalp };

public class brain_anat
{
    public bool hasAnat
    {
        get
        {
            if (nbMesh == mesh_Configuration.leftright)
                return (lhemi != "" && rhemi != "" && pts != "");
            else if (nbMesh == mesh_Configuration.single)
                return (lhemi != "" && pts != "");
            else
                return false;
        }
    }

    public mesh_Configuration GetMeshNb
    {
        get
        {
            return nbMesh;
        }
    }

    public eeg_Technology GetEegTech
    {
        get
        {
            return eeg;
        }
    }

    public void setNbMesh(string textOpt)
    {
        if (textOpt == "Left/Right Mesh")
            nbMesh = mesh_Configuration.leftright;
        else if (textOpt == "Single Mesh")
            nbMesh = mesh_Configuration.single;
    }

    public string getMeshNb()
    {
        if (nbMesh == mesh_Configuration.leftright)
            return "Left/Right Mesh";
        else if (nbMesh == mesh_Configuration.single)
            return "Single Mesh";
        else
            return "ERROR";
    }

    public void setEegTech(string textOpt)
    {
        if (textOpt == "Intracranial EEG")
            eeg = eeg_Technology.intra;
        else if (textOpt == "Scalp EEG")
            eeg = eeg_Technology.scalp;
    }

    public string getEegTech()
    {
        if (eeg == eeg_Technology.intra)
            return "Intracranial EEG";
        else if (eeg == eeg_Technology.scalp)
            return "Scalp EEG";
        else
            return "ERROR";
    }

    public string lhemi = "";
    public string rhemi = "";
    public string pts = "";
    public string atlasCSV = "";
    mesh_Configuration nbMesh = mesh_Configuration.leftright;
    eeg_Technology eeg = eeg_Technology.intra;
};