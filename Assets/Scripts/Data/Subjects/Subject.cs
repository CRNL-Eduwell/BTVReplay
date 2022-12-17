using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Subject : ViewModelBase
{
    [JsonIgnore]
    public bool IsLoadable
    {
        get
        {
            if (Files.Count == 0) return false;

            int ErrorCount = 0;
            foreach (var file in Files)
            {
                List<ArgumentException> errors = file.Value.ChecKForErrors();
                if (errors != null) ErrorCount += errors.Count;
            }
            return ErrorCount == 0;
        }
    }
    public string PatientName
    {
        get
        {
            return m_PatientName;
        }
        set
        {
            if (m_PatientName != value)
            {
                m_PatientName = value;
                RaisePropertyChanged("PatientName");
            }
        }
    }
    public Dictionary<string, BrainDataContainer> AnatomicalSpaces { get; set; } = new Dictionary<string, BrainDataContainer>();
    public Dictionary<string, IEegFileInfo> Files { get; set; } = new Dictionary<string, IEegFileInfo>();
    public string Video { get; set; } = "";

    [JsonIgnore]
    private string m_PatientName = "";

    public Subject()
    {

    }

    public Subject(string patientName)
    {
        PatientName = patientName;
        BrainDataContainer mni = new BrainDataContainer
        {
            LeftHemisphere = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri",
            RightHemisphere = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri",
            Transformation = Application.dataPath + "/Config/Data/MNI/transfo_mni.trm"
        };
        BrainDataContainer pat = new BrainDataContainer
        {
            LeftHemisphere = "",
            RightHemisphere = "",
            Transformation = ""
        };
        AnatomicalSpaces.Add("MNI", mni);
        AnatomicalSpaces.Add("PAT", pat);
    }

    public Subject(Subject subjectToCopy)
    {
        PatientName = subjectToCopy.PatientName;
        AnatomicalSpaces = subjectToCopy.AnatomicalSpaces.ToDictionary(entry => entry.Key, entry => new BrainDataContainer(entry.Value));
        Files = subjectToCopy.Files.ToDictionary(entry => entry.Key, entry => (IEegFileInfo)entry.Value.Clone());
        Video = subjectToCopy.Video;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">"MNI" or "PAT" for now</param>
    /// <returns></returns>
    public bool HasAllInformationForSpace(string key)
    {
        if (AnatomicalSpaces.ContainsKey(key))
        {
            bool retrieved = AnatomicalSpaces.TryGetValue(key, out BrainDataContainer value);
            return retrieved ? value.HasAnat : false;
        }
        return false;
    }

    public void Display()
    {
        UnityEngine.Debug.Log("");
        UnityEngine.Debug.Log("Patient : " + PatientName);
        UnityEngine.Debug.Log("MNI Referential");
        if (HasAllInformationForSpace("MNI"))
        {
            if (AnatomicalSpaces.TryGetValue("MNI", out BrainDataContainer value))
            {
                value.Display();
            }
        }
        UnityEngine.Debug.Log("Patient Referential");
        if (HasAllInformationForSpace("PAT"))
        {
            if (AnatomicalSpaces.TryGetValue("PAT", out BrainDataContainer value))
            {
                value.Display();
            }
        }
        foreach (var item in Files)
        {
            UnityEngine.Debug.Log("Key : " + item.Key + " - Path : " + item.Value.Files[0]);
        }
        UnityEngine.Debug.Log("Video : " + Video);
        UnityEngine.Debug.Log("");
    }
    #region operators
    public override bool Equals(object obj)
    {
        if (obj is Subject subject)
        {
            bool sameName = PatientName == subject.PatientName;
            bool sameAnat = AnatomicalSpaces.All(k => subject.AnatomicalSpaces.Contains(k)) && AnatomicalSpaces.Count == subject.AnatomicalSpaces.Count;
            bool sameEeg = Files.All(k => subject.Files.Contains(k)) && Files.Count == subject.Files.Count;
            bool sameVideo = Video == subject.Video;
            return sameName && sameAnat && sameEeg && sameVideo;
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(Subject a, Subject b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        return a.Equals(b);
    }
    public static bool operator !=(Subject a, Subject b)
    {
        return !(a == b);
    }
    #endregion
}
