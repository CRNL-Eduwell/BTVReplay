using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventList : Tools.Unity.Lists.SelectableList<TraceEvent>
{
    [SerializeField] Toggle m_checkAll = null;

    private void Start()
    {
        m_checkAll.onValueChanged.AddListener(ToggleAllEvents);
        Initialize(); //Init the list class
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

    public void LoadEvents(List<TraceEvent> Events)
    {
        int EventsCount = Events.Count;
        for (int i = 0; i < EventsCount; i++)
        {
            AddEvent(Events[i]);
        }
    }

    public void AddEvent(TraceEvent currentEvent)
    {
        ApplicationState.Module3D.MemoryEvent = new TraceEvent(currentEvent);

        Add(currentEvent);
        m_Objects = m_Objects.OrderBy(x => x.sample).ToList();
        Refresh();
    }

    public void DeleteEvent(int ID)
    {
        //Delete from Ui List by ref
        Remove(Objects[ID]);
    }

    public void DeleteAllEvents()
    {
        UnityEngine.Debug.Log(Objects.Length);
        for (int i = Objects.Length - 1; i >= 0; i--)
        {
            Remove(Objects[i]);
        }
    }

    public void DeleteSelectedEvents()
    {
        for (int i = ObjectsSelected.Length - 1; i >= 0; i--)
            Remove(ObjectsSelected[i]);
    }
}
