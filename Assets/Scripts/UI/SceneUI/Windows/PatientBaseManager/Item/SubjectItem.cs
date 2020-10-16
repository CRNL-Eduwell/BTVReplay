using UnityEngine;
using UnityEngine.UI;

public class SubjectItem : Tools.Unity.Lists.SelectableItem<Subject>
{
    public override Subject Object
    {
        get
        {
            return base.Object;
        }
        set
        {
            if (base.Object != null)
                base.Object.PropertyChanged -= SubjectInformationUpdated;
            base.Object = value;
            SetLabelValue();
            base.Object.PropertyChanged += SubjectInformationUpdated;
        }
    }

    [SerializeField] private Text m_Label = null;

    private void OnDestroy()
    {
        if (base.Object != null)
            base.Object.PropertyChanged -= SubjectInformationUpdated;
    }

    private void SetLabelValue()
    {
        m_Label.text = base.Object.PatientName;
    }

    private void SubjectInformationUpdated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "PatientName")
        {
            UnityEngine.Debug.Log("Subject Name updated");
            SetLabelValue();
        }
        else
        {
            UnityEngine.Debug.LogError("Subject unknown property updated, please check");
        }
    }
}