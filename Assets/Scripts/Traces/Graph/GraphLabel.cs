using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphLabel : MonoBehaviour
{
    [SerializeField] Text ElectrodeLabel = null;
    [SerializeField] Image ElectrodeColor = null;
    [SerializeField] public Button ElectrodeButton = null;

    public void init(string label)
    {
        setName(label);
        ElectrodeColor.gameObject.SetActive(true);
    }

    public void setName(string label)
    {
        ElectrodeLabel.text = label;
    }

    public void setColor(Color color)
    {
        ElectrodeColor.color = color;
    }
}
