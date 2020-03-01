using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SelectabeLabelGroup : MonoBehaviour
{
    private List<SelectableLabel> m_Labels = new List<SelectableLabel>();

    public void Register(SelectableLabel label)
    {
        label.PropertyChanged += Label_PropertyChanged;
        m_Labels.Add(label);
    }

    private void Label_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "HasFocus")
        {
            SelectableLabel label = sender as SelectableLabel;
            if (label.HasFocus)
            {
                List<SelectableLabel> switchOff = m_Labels.FindAll(x => (x.gameObject.name != label.name) && x.HasFocus);
                for (int i = 0; i < switchOff.Count; i++)
                {
                    switchOff[i].OnPointerClick();
                }
            }
        }
    }
}
