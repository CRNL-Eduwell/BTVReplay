using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BTV.Data;
using Assets.Scripts.Data.Factory;
using BTV.Services.DatabaseService;

public class DatabaseItem : Tools.Unity.Lists.SelectableItem<SubjectRepository>
{
    public override SubjectRepository Object
    {
        get
        {
            return base.Object;
        }
        set
        {
            if (base.Object != null)
                base.Object.PropertyChanged -= RepositoryInformationUpdated;
            base.Object = value;
            SetLabelValue();
            base.Object.PropertyChanged += RepositoryInformationUpdated;
        }
    }

    [SerializeField] private Text m_Label = null;

    private void SetLabelValue()
    {
        m_Label.text = base.Object.FilePath.Split(new string[] { "\\", "/" }, System.StringSplitOptions.None).Last().Replace(".dbtv", "");
    }

    private void RepositoryInformationUpdated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "FilePath")
        {
            UnityEngine.Debug.Log("Repository FilePath property updated");
            SetLabelValue();
        }
        else
        {
            UnityEngine.Debug.LogError("Repository unknown property updated, please check");
        }
    }
}