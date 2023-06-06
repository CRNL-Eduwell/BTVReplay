using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferingImage : MonoBehaviour
{
    #region Properties
    private float m_Timer = 0;
    private float m_RotateTimer = 0.1f;
    #endregion

    #region Private Methods
    private void Update()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer > m_RotateTimer)
        {
            transform.Rotate(0, 0, -45);
            m_Timer = 0;
        }
    }
    #endregion

    #region Public Methods
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    #endregion
}
