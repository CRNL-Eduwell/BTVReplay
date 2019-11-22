using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BTV3DModule : MonoBehaviour
{
    public Trace Window1 { get { return m_Window1; } }
    public Trace Window2 { get { return m_Window2; } }
    public TaskPerformanceTrace TaskPerformanceWindow { get { return m_TaskPerformanceWindow; } }

    public Patient Patient { get; set; }
    public TraceEvent MemoryEvent { get; set; }
    //For the moment , only the file paths
    //later , will load the audio clips only once
    //and then distribute a pointer to it
    public List<string> SoundFilePaths { get; set; }

    [SerializeField] private Trace m_Window1 = null;
    [SerializeField] private Trace m_Window2 = null;
    [SerializeField] private TaskPerformanceTrace m_TaskPerformanceWindow = null;
}
