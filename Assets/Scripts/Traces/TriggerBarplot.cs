using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBarplot : MonoBehaviour
{
    public EegTrigger Trigger
    {
        get;
        set;
    }
    [SerializeField]
    private LineRenderer m_LineRenderer = null;

    public void UpdatePosition(int index, float x, float y, float z)
    {
        m_LineRenderer.SetPosition(index, new Vector3(x, y, z));
    }
}
