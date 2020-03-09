using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class Patient
{
    #region Public Properties & members
    public string PatientName
    {
        get
        {
            for (int i = 0; i < smFiles.Length; i++)
            {
                if (smFiles[i] != "")
                    return getPatientNameFromPath(smFiles[i]);
            }
            return "UNDEFINED_NAME";
        }
    }
    public bool HasMNI
    {
        get { return mni.HasAnat; }
    }
    public bool HasPAT
    {
        get { return pat.HasAnat; }
    }

    public BrainDataContainer mni { get; set; } = new BrainDataContainer();
    public BrainDataContainer pat { get; set; } = new BrainDataContainer();
    public string[] smFiles = new string[6] { "", "", "", "", "", "" };
    public string pos = "";
    public string prov = "";
    public string video = "";
    #endregion

    #region Constructors
    public Patient()
    {
        //mni = new brain_anat();
        //pat = new brain_anat();
    }
    public Patient(Patient thisPat)
    {
        mni = thisPat.mni;
        pat = thisPat.pat;

        for (int i = 0; i < 6; i++)
        {
            smFiles[i] = thisPat.smFiles[i];
        }

        pos = thisPat.pos;
        prov = thisPat.prov;
        video = thisPat.video;
    }
    #endregion

    private string getPatientNameFromPath(string path)
    {
        string[] namesplit = path.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries);
        string file = namesplit[namesplit.Length - 1];
        file = Regex.Replace(file, @"_f(\d+)f(\d+)_ds(\d+)_sm(\d+)", "");
        return file.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0];
    }
}
