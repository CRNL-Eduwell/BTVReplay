using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;

public class SubjectList : Tools.Unity.Lists.SelectableList<Subject>
{
    private void OnDestroy()
    {
        ((ISelectionCountable)this).OnSelectionChanged.RemoveAllListeners();
    }

    public void AddElements(List<Subject> objects)
    {
        foreach (var subject in objects)
        {
            AddElement(subject);
        }
        Refresh();
    }

    public void AddElement(Subject obj)
    {
        Add(obj);
        Refresh();
    }

    public void RemoveAllElements()
    {
        for (int i = m_Objects.Count - 1; i >= 0; i--)
        {
            Remove(m_Objects[i]);
        }
        Refresh();
    }

    public void RemoveElement(Subject obj)
    {
        Remove(obj);
        Refresh();        
    }
    
    public void ReplaceElement(Subject old, Subject obj)
    {
        Replace(obj, old);
        Refresh();
    }
}