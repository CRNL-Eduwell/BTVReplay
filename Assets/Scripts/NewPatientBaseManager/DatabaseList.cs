using Assets.Scripts.Data.Factory;
using BTV.Services.DatabaseService;
using System.Collections.Generic;

public class DatabaseList : Tools.Unity.Lists.SelectableList<SubjectRepository>
{
    private void Start()
    {
        Initialize(); //Init the list class
    }

    private void OnDestroy()
    {
        OnSelectionChanged.RemoveAllListeners();
    }

    public void AddElement(SubjectRepository element)
    {
        Add(element);
        Refresh();
    }

    public void RemoveElement(SubjectRepository element)
    {
        Remove(element);
        Refresh();
    }
}