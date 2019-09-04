using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class EventList : Tools.Unity.Lists.SelectableList<TraceEvent>
{
    [SerializeField] Toggle m_checkAll = null;

    private void Start()
    {
        m_checkAll.onValueChanged.AddListener((bool value) => { if (value) SelectAll(); else DeselectAll(); });
    }

    private void OnDestroy()
    {
        m_checkAll.onValueChanged.RemoveAllListeners();
    }

    public List<int> sampleValues
    {
        get
        {
            List<int> sample = new List<int>(m_Items.Count);
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
}
