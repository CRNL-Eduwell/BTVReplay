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

    [SerializeField] BrainAnatGUIManager _MniGUIManager = null;
    [SerializeField] BrainAnatGUIManager _PatGUIManager = null;
    [SerializeField] EegInfoGUIManager[] _EegFiles = new EegInfoGUIManager[6] { null, null, null, null, null, null, };
    [SerializeField] BrowseWidget _Video = null;

    public Subject GetSubjectsFromGUI()
    {
        Subject myPat = new Subject();

        //myPat.PatientName = GetHeaderText();

        myPat.AnatomicalSpaces.Add("MNI", _MniGUIManager.GetDataContainer());
        myPat.AnatomicalSpaces.Add("PAT", _PatGUIManager.GetDataContainer());
        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
        {
            KeyValuePair<string, IEegFileInfo> kvp = _EegFiles[i].GetEegFileInfoFromGUI();
            if(!kvp.Equals(default(KeyValuePair<string, IEegFileInfo>)))
            {
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to define a key for the eeg file that is not a null/empty string");
                    return null;
                }
                else if (myPat.Files.ContainsKey(kvp.Key))
                {
                    ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to have a different key for each eeg file");
                    return null;
                }
                else
                {
                    myPat.Files.Add(kvp.Key, kvp.Value);
                }
            }
        }
        myPat.Video = _Video._InputField.text;

        return myPat;
    }

    public void SetToDefault()
    {
        LastSubject = GetSubjectsFromGUI();

        _MniGUIManager.SetDataConainerInUI(new BrainDataContainer());
        _PatGUIManager.SetDataConainerInUI(new BrainDataContainer());

        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
            _EegFiles[i].SetEegFileInfoToGUI(default);

        _Video._InputField.text = "";
    }

    public void SetSubjectToGUI(Subject subject)
    {
        LastSubject = GetSubjectsFromGUI();

        //SetHeadertext(subject.PatientName);

        bool mniFound = subject.AnatomicalSpaces.TryGetValue("MNI", out BrainDataContainer mniContainer);
        if(mniFound) _MniGUIManager.SetDataConainerInUI(mniContainer);

        bool patFound = subject.AnatomicalSpaces.TryGetValue("PAT", out BrainDataContainer patContainer);
        if (patFound) _PatGUIManager.SetDataConainerInUI(patContainer);

        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
            _EegFiles[i].SetEegFileInfoToGUI(subject.Files.ElementAtOrDefault(i));

        _Video._InputField.text = subject.Video;
    }
}
