using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventMatchItem : Tools.Unity.Lists.SelectableItem<KeyValuePair<int, string>>
{
    [SerializeField] private Text m_code = null;
    [SerializeField] private Text m_comment = null;

    public override KeyValuePair<int, string> Object
    {
        get
        {
            return base.Object;
        }
        set
        {
            base.Object = value;
            initValues();
        }
    }

    private void initValues()
    {
        gameObject.name = "HubEvent - " + base.Object.Key;

        m_code.text = base.Object.Key.ToString();
        m_comment.text = base.Object.Value;
    }
}
