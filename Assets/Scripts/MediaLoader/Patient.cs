using System;
using UnityEngine;

public class Patient
{
    #region Public Properties & members
    public bool hasMNI
    {
        get { return mni.hasAnat; }
    }
    public bool hasPAT
    {
        get { return pat.hasAnat; }
    }
    public brain_anat mni;
    public brain_anat pat;
    public string[] smFiles = new string[6] { "", "", "", "", "", "" };
    public string pos = "";
    public string prov = "";
    public string video = "";
    public string patientName = "";
    #endregion

    #region Constructors
    public Patient()
    {
        mni = new brain_anat();
        pat = new brain_anat();
    }
    public Patient(Patient thisPat)
    {
        mni = thisPat.mni;
        pat = thisPat.pat;

        for (int i = 0; i < 6; i++)
        {
            smFiles[i] = thisPat.smFiles[i];
            if (smFiles[i] != "" && patientName == "")
                patientName = getPatientNameFromPath(smFiles[i]);
        }

        pos = thisPat.pos;
        prov = thisPat.prov;
        video = thisPat.video;
    }
    #endregion

    //public static string getPatientNameFromPath(string path)
    //{
    //    string[] namesplit = path.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries);
    //    return namesplit[namesplit.Length - 2];
    //}

    public static string getPatientNameFromPath(string path)
    {
        string[] namesplit = path.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries);
        string file = namesplit[namesplit.Length - 1];
        return file.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0];
    }
}
