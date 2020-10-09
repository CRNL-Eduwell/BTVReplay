using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PatientGUIManager : MonoBehaviour
{
    public Subject LastSubject { get; private set; } = null;
    public bool IsInteractable
    {
        get
        {
            return _MniGUIManager.IsInteractable;
        }
        set
        {
            _MniGUIManager.IsInteractable = value;
            _PatGUIManager.IsInteractable = value;
            foreach (var eeg in _EegFiles)
                eeg.IsInteractable = value;
            _Video.IsInteractable = value;
        }
    }

    [SerializeField] BrainAnatGUIManager _MniGUIManager = null;
    [SerializeField] BrainAnatGUIManager _PatGUIManager = null;
    [SerializeField] EegInfoGUIManager[] _EegFiles = new EegInfoGUIManager[6] { null, null, null, null, null, null, };
    [SerializeField] BrowseWidget _Video = null;

    private Subject m_Subject = null; /*!< Reference to the last inputed subject , used to keep track of the name of the subject */

    private void Awake()
    {
        foreach (var eeg in _EegFiles)
        {
            eeg.onEndEditKey.AddListener((str)=> { IsKeyOk(str, eeg); });
        }
    }

    private void OnDestroy()
    {
        foreach (var eeg in _EegFiles)
        {
            eeg.onEndEditKey.RemoveAllListeners();
        }
    }

    public void SetToDefault()
    {
        LastSubject = GetSubjectsFromGUI();
        m_Subject = null;

        _MniGUIManager.SetDataConainerInUI(new BrainDataContainer());
        _PatGUIManager.SetDataConainerInUI(new BrainDataContainer());

        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
            _EegFiles[i].SetEegFileInfoToGUI(default);

        _Video.TextWithoutPopUp = "";
    }

    public void SetSubjectToGUI(Subject subject)
    {
        LastSubject = GetSubjectsFromGUI();
        m_Subject = subject;

        bool mniFound = subject.AnatomicalSpaces.TryGetValue("MNI", out BrainDataContainer mniContainer);
        if (mniFound) _MniGUIManager.SetDataConainerInUI(mniContainer);

        bool patFound = subject.AnatomicalSpaces.TryGetValue("PAT", out BrainDataContainer patContainer);
        if (patFound) _PatGUIManager.SetDataConainerInUI(patContainer);

        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
            _EegFiles[i].SetEegFileInfoToGUI(subject.Files.ElementAtOrDefault(i));

        _Video.TextWithoutPopUp = subject.Video;
    }

    private void IsKeyOk(string str, EegInfoGUIManager eeg)
    {
        if (string.IsNullOrEmpty(str))
        {
            ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to define a key for the eeg file that is not a null/empty string");
            eeg.RevertKeyField();
            return;
        }

        List<string> keys = new List<string>(); 
        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
        {
            if (_EegFiles[i] == eeg) continue; //if this is the one modified , we don't want to take it into account
            KeyValuePair<string, IEegFileInfo> kvp = _EegFiles[i].GetEegFileInfoFromGUI();
            keys.Add(kvp.Key);
        }

        if (keys.Contains(str))
        {
            ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to have a different key for each eeg file");
            eeg.RevertKeyField();
            return;
        }
    }

    public Subject GetSubjectsFromGUI()
    {
        Subject myPat = new Subject();
        myPat.PatientName = m_Subject != null ? m_Subject.PatientName : "";

        myPat.AnatomicalSpaces.Add("MNI", _MniGUIManager.GetDataContainer());
        myPat.AnatomicalSpaces.Add("PAT", _PatGUIManager.GetDataContainer());
        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
        {
            KeyValuePair<string, IEegFileInfo> kvp = _EegFiles[i].GetEegFileInfoFromGUI();
            if (!kvp.Equals(default(KeyValuePair<string, IEegFileInfo>)))
            {
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to define a key for the eeg file that is not a null/empty string");
                    continue;
                }
                else if (myPat.Files.ContainsKey(kvp.Key))
                {
                    ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to have a different key for each eeg file");
                    continue;
                }
                else
                {
                    myPat.Files.Add(kvp.Key, kvp.Value);
                }
            }
        }
        myPat.Video = _Video.Text;

        return myPat;
    }
}
