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
            _ExamGuiManager.IsInteractable = value;
        }
    }

    [SerializeField] BrainAnatGUIManager _MniGUIManager = null;
    [SerializeField] BrainAnatGUIManager _PatGUIManager = null;
    [SerializeField] ExamGuiManager _ExamGuiManager = null;

    private Subject m_Subject = null; /*!< Reference to the last inputed subject , used to keep track of the name of the subject */

    private void Awake()
    {
        _ExamGuiManager.UpdateData.AddListener((string str) =>
        {
            //chercher dans le sujet l'exam correpondant
        });
    }

    private void OnDestroy()
    {
        _ExamGuiManager.UpdateData.RemoveAllListeners();
    }

    public void SetToDefault()
    {
        LastSubject = GetSubjectsFromGUI();
        m_Subject = null;

        _MniGUIManager.SetDataConainerInUI(new BrainDataContainer());
        _PatGUIManager.SetDataConainerInUI(new BrainDataContainer());

        _ExamGuiManager.SetEegFiles(new Dictionary<string, IEegFileInfo>());
        _ExamGuiManager.SetVideoFilePath("");
    }

    public void SetSubjectToGUI(Subject subject)
    {
        LastSubject = GetSubjectsFromGUI();
        m_Subject = new Subject(subject);

        bool mniFound = m_Subject.AnatomicalSpaces.TryGetValue("MNI", out BrainDataContainer mniContainer);
        if (mniFound) _MniGUIManager.SetDataConainerInUI(mniContainer);

        bool patFound = m_Subject.AnatomicalSpaces.TryGetValue("PAT", out BrainDataContainer patContainer);
        if (patFound) _PatGUIManager.SetDataConainerInUI(patContainer);

        _ExamGuiManager.SetEegFiles(m_Subject.Files);
        _ExamGuiManager.SetVideoFilePath(m_Subject.Video);
    }

    public Subject GetSubjectsFromGUI()
    {
        Subject myPat = new Subject();
        myPat.PatientName = m_Subject != null ? m_Subject.PatientName : "";
        myPat.AnatomicalSpaces.Add("MNI", _MniGUIManager.GetDataContainer());
        myPat.AnatomicalSpaces.Add("PAT", _PatGUIManager.GetDataContainer());
        myPat.Files = _ExamGuiManager.GetEegFiles();
        myPat.Video = _ExamGuiManager.GetVideoFilePath();
        return myPat;
    }
}
