using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;
using UnityEngine.UI;

public class NormalizeEventItem : Tools.Unity.Lists.SelectableItem<BtvEvent>
{
    [SerializeField] private Text _ItemText = null;

    public override BtvEvent Object
    {
        get
        {
            return base.Object;
        }
        set
        {
            base.Object = value;
            InitValues();
        }
    }

    private void InitValues()
    {
        int timeInSec = (int)base.Object.TimeInSeconds;

        gameObject.name = "NormalizedItem - " + timeInSec;

        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        _ItemText.text = h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00") + "  " + base.Object.Code.ToString() + " " + base.Object.Comment;
    }
}
