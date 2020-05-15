using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;

public class SubjectList : Tools.Unity.Lists.SelectableList<Subject>
{
    private void Start()
    {
        Initialize(); //Init the list class
    }

    private void OnDestroy()
    {
        OnSelectionChanged.RemoveAllListeners();
    }

    public void AddElements(List<Subject> subjects)
    {
        foreach (var subject in subjects)
        {
            AddElement(subject);
        }
        Refresh();
    }

    public void AddElement(Subject subject)
    {
        Add(subject);
        Refresh();
    }

    public void RemoveAllElements()
    {
        for (int i = Objects.Length - 1; i >= 0; i--)
        {
            Remove(Objects[i]);
        }
        Refresh();
    }

    public void RemoveElement(Subject subject)
    {
        Remove(subject);
        Refresh();        
    }
    
    public void ReplaceElement(Subject oldSubject, Subject newSubject)
    {
        int index = Array.IndexOf(Objects, oldSubject);
        if (index != -1)
        {
            m_SelectedStateByObject.Remove(oldSubject);
            m_SelectedStateByObject.Add(newSubject, false);
            m_Objects[index] = newSubject;
            OnSelectionChangeCallBack();
            Refresh();
        }
    }
}