using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using BTV.Services.EventsService;
using UnityEngine;

public class NormalizeEventList : Tools.Unity.Lists.SelectableList<BtvEvent>
{
    private void Awake()
    {
        int EventCount = EventsService.Events.Count;
        for (int i = 0; i < EventCount; i++)
        {
            AddEvent(EventsService.Events[i]);
        }
    }

    private void OnDestroy()
    {
        ((ISelectionCountable)this).OnSelectionChanged.RemoveAllListeners();
        for (int i = m_DisplayedObjects.Count - 1; i >= 0; i--)
        {
            Remove(m_DisplayedObjects[i]);
        }
    }

    public void AddEvent(BtvEvent currentEvent)
    {
        Add(currentEvent);
        Refresh();
    }

    public void DeleteEvent(BtvEvent currentEvent)
    {
        Remove(currentEvent);
        Refresh();
    }
}