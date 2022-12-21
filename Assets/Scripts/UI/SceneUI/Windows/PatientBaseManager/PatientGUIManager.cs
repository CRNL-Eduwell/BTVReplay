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

    [SerializeField] private BrainAnatGUIManager _MniGUIManager = null;
    [SerializeField] private BrainAnatGUIManager _PatGUIManager = null;
    [SerializeField] private GenericTabWidget _ExamTabs = null;
    [SerializeField] private ExamGuiManager _ExamGuiManager = null;

    private Subject m_Subject = null; /*!< Reference to the last inputed subject , used to keep track of the name of the subject */
    private string m_ExperimentLabel = "";

    private void Awake()
    {
        _ExamTabs.OnTabClicked.AddListener(OnTabClicked);
    }

    private void OnDestroy()
    {
        _ExamTabs.OnTabClicked.RemoveAllListeners();
    }

    private void OnTabClicked(string label)
    {
        //chercher dans le sujet l'exam correpondant
        m_ExperimentLabel = label;
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

        OnTabClicked(m_Subject.Experiments[0].Label);
        _ExamTabs.SetTabName(m_Subject.Experiments[0].Label, 0);
        _ExamGuiManager.SetEegFiles(m_Subject.Experiments[0].Files);
        _ExamGuiManager.SetVideoFilePath(m_Subject.Experiments[0].Video);
    }

    public Subject GetSubjectsFromGUI()
    {
        Subject myPat = new Subject();
        myPat.PatientName = m_Subject != null ? m_Subject.PatientName : "";
        myPat.AnatomicalSpaces.Add("MNI", _MniGUIManager.GetDataContainer());
        myPat.AnatomicalSpaces.Add("PAT", _PatGUIManager.GetDataContainer());
        if (m_Subject != null)
        {
            for (int i = 0; i < _ExamTabs.TabCount; i++)
            {
                if (m_Subject.Experiments[i] != null && m_Subject.Experiments[i].Label == m_ExperimentLabel)
                {
                    Experiment experiment = new Experiment(m_ExperimentLabel, _ExamGuiManager.GetEegFiles(), _ExamGuiManager.GetVideoFilePath());
                    myPat.Experiments.Add(experiment);
                }
                else
                {
                    myPat.Experiments.Add(m_Subject.Experiments[i]);
                }
            }
        }
        return myPat;
    }

    public string GetCurrentExperimentName()
    {
        return m_ExperimentLabel;
    }
}
