using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BTV.Data;

public class EventList : Tools.Unity.Lists.SelectableList<BtvEvent>
{
    [SerializeField] Toggle m_checkAll = null;

    private void Start()
    {
        m_checkAll.onValueChanged.AddListener(ToggleAllEvents);
    }

    private void OnDestroy()
    {
        m_checkAll.onValueChanged.RemoveAllListeners();
    }

    private void ToggleAllEvents(bool isChecked)
    {
        if (isChecked)
            SelectAll();
        else
            DeselectAll();
    }

    public void LoadEvents(List<BtvEvent> Events)
    {
        int EventsCount = Events.Count;
        for (int i = 0; i < EventsCount; i++)
        {
            AddEvent(Events[i]);
        }
    }

    public void AddEvent(BtvEvent currentEvent)
    {
        Add(currentEvent);
        m_DisplayedObjects = m_DisplayedObjects.OrderBy(x => x.TimeInMilliSeconds).ToList();
        Refresh();
    }

    public void DeleteEvent(int ID)
    {
        //Delete from Ui List by ref
        Remove(m_DisplayedObjects[ID]);
    }

    public void DeleteAllEvents()
    {
        UnityEngine.Debug.Log("Deleting " + m_DisplayedObjects.Count + " objects");
        for (int i = m_DisplayedObjects.Count - 1; i >= 0; i--)
        {
            Remove(m_DisplayedObjects[i]);
        }
    }

    public void DeleteSelectedEvents()
    {
        var objectsSelected = ObjectsSelected;
        for (int i = objectsSelected.Length - 1; i >= 0; i--)
            Remove(objectsSelected[i]);
    }
}
