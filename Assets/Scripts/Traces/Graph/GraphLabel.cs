using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GraphLabel : MonoBehaviour
{
    public string Electrode
    {
        get
        {
            return ElectrodeLabel.text;
        }
        set
        {
            ElectrodeLabel.text = value;
            m_Collider.size = m_LabelRect.rect.size;
        }
    }
    public string Description
    {
        get
        {
            return DescriptionLabel.text;
        }
        set
        {
            DescriptionLabel.text = value;
        }
    }
    public Color Color
    {
        get
        {
            return ElectrodeColor.color;
        }
        set
        {
            ElectrodeColor.color = value;
        }
    }

    [SerializeField] Text ElectrodeLabel = null;
    [SerializeField] Text DescriptionLabel = null;
    [SerializeField] Image ElectrodeColor = null;
    [SerializeField] public Button ElectrodeButton = null;
    [SerializeField] BoxCollider m_Collider = null;

    private RectTransform m_LabelRect = null;

    public void Initialize(string label, string description)
    {
        m_LabelRect = ElectrodeLabel.GetComponent<RectTransform>();

        Electrode = label;
        Description = description;
        ElectrodeColor.gameObject.SetActive(true);

        //== Ugly way to adapt the size of the collider to the label of the electrode since
        //== we let the possiblity of different sizes of label, 
        float width = LayoutUtility.GetPreferredWidth(m_LabelRect);
        float height = m_LabelRect.rect.height;
        m_Collider.size = new Vector2(width, height);
    }
}
