using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBarplot : MonoBehaviour
{
    public float Position { get { return m_LineRenderer.GetPosition(0).x; } }
    public EegTrigger Trigger
    {
        get;
        set;
    }
    [SerializeField]
    private LineRenderer m_LineRenderer = null;

    public void Show(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }

    public void UpdatePosition(int index, float x, float y, float z)
    {
        m_LineRenderer.SetPosition(index, new Vector3(x, y, z));
    }

    public void SetColor(Color color)
    {
        m_LineRenderer.startColor = color;
        m_LineRenderer.endColor = color;
    }
}
