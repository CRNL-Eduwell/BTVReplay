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
        // The index comes from EventsService.Events; if the UI list drifted out of sync,
        // skip instead of throwing and aborting the caller's delete flow halfway through.
        if (ID < 0 || ID >= m_DisplayedObjects.Count)
        {
            Debug.LogWarning("EventList: delete index " + ID + " does not fit the displayed list (" + m_DisplayedObjects.Count + " events), nothing removed.");
            return;
        }

        //Delete from Ui List by ref
        Remove(m_DisplayedObjects[ID]);
    }

    public void DeleteAllEvents()
    {
        BtvLog.Log("Deleting " + m_DisplayedObjects.Count + " objects");
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
