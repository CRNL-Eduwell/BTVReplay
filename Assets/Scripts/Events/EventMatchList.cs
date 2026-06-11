using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BTV.Data;

public class EventMatchList : Tools.Unity.Lists.SelectableList<KeyValuePair<int,string>>
{
    public void LoadEvents(List<KeyValuePair<int, string>> Events)
    {
        int EventsCount = Events.Count;
        for (int i = 0; i < EventsCount; i++)
        {
            AddEvent(Events[i]);
        }
    }
    public void AddEvent(KeyValuePair<int, string> currentEvent)
    {
        Add(currentEvent);
        Refresh();
    }
    public void DeleteAllEvents()
    {
        BtvLog.Log("Deleting " + m_DisplayedObjects.Count + " objects");
        for (int i = m_DisplayedObjects.Count - 1; i >= 0; i--)
        {
            Remove(m_DisplayedObjects[i]);
        }
    }
}
