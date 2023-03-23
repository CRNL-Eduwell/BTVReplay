using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubjectWidget : MonoBehaviour
{
    //Interactable ?
    public Subject MemorySubject { get { return m_MemorySubject; } }
    public Subject Subject { get { return m_Subject; } }

    [SerializeField] private AnatomicalDataWidget _AnatomicalDataWidget = null;
    [SerializeField] private ExperimentDataWidget _ExperimentDataWidget = null;

    private Subject m_MemorySubject = null;
    private Subject m_Subject = null;

    public void SetDefault()
    {
        m_MemorySubject = null;
        m_Subject = null;
    }

    public void SetSubject(Subject subject)
    {
        m_MemorySubject = new Subject(subject);
        m_Subject = new Subject(subject);

        _AnatomicalDataWidget.SetSubject(m_Subject);
        //_ExperimentDataWidget.SetSubject(m_Subject);
    }
}
