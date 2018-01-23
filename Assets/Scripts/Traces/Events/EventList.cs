using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class EventList : Tools.List<TraceEvent>
{
    public List<int> sampleValues
    {
        get
        {
            List<int> sample = new List<int>(m_ItemByObject.Count);
            foreach (TraceEvent a in m_Objects)
                sample.Add(a.sample);

            return sample;
        }
    }

    public void sortBySample()
    {
        m_Objects = m_Objects.OrderBy(x => x.sample).ToList();
        Refresh();
    }

    public void RemoveDD(TraceEvent dd)
    {
        UnityEngine.Debug.Log(m_Objects.Count);
        m_Objects.Remove(dd);
        UnityEngine.Debug.Log(m_Objects.Count);
        Refresh();
        UnityEngine.Debug.Log(m_Objects.Count);
    }

}
