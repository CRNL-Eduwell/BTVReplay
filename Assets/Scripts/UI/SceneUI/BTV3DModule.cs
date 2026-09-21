using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using BTV.Services;

public class BTV3DModule : MonoBehaviour
{
    public Session PatientSession { get; private set; }
    public Trace Window1 { get { return m_Window1; } }
    public Trace Window2 { get { return m_Window2; } }
    public TaskPerformanceTrace TaskPerformanceWindow { get { return m_TaskPerformanceWindow; } }

    //For the moment , only the file paths
    //later , will load the audio clips only once
    //and then distribute a pointer to it
    public List<string> SoundFilePaths { get; set; }

    [SerializeField] private Trace m_Window1 = null;
    [SerializeField] private Trace m_Window2 = null;
    [SerializeField] private TaskPerformanceTrace m_TaskPerformanceWindow = null;

    public void Initialize(Session session)
    {
        PatientSession = session;
        m_TaskPerformanceWindow.Initialize(session);
    }
}
