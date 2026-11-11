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

    private void OnDestroy()
    {
        if (base.Object != null)
            base.Object.PropertyChanged -= RepositoryInformationUpdated;
    }

    private void SetLabelValue()
    {
        // Same label as the move/copy submenus, so two databases called alike can be told apart.
        m_Label.text = DatabaseService.DisplayName(base.Object);
    }

    private void RepositoryInformationUpdated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ShortName")
        {
            BtvLog.Log("Repository FilePath property updated");
            SetLabelValue();
        }
    }
}