using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class SelectableLabel : MonoBehaviour, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public bool HasFocus
    {
        get
        {
            return m_HasFocus;
        }
        set
        {
            m_HasFocus = value;
            NotifyPropertyChanged();
        }
    }
    public Text Text { get { return m_Label; } set { m_Label = value; }  }

    [SerializeField]
    private Text m_Label = null;
    [SerializeField]
    private SelectabeLabelGroup m_LabelGroup = null;

    private bool m_HasFocus = false;

    private void Start()
    {
        m_LabelGroup.Register(this);
    }

    public void OnPointerClick()
    {
        HasFocus = !HasFocus;
        m_Label.color = HasFocus ? Color.red : Color.white;
    }

    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
