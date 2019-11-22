using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene3DUI : MonoBehaviour
{
    #region Properties
    /// <summary>
    /// Is the scene minimzed ?
    /// </summary>
    public bool IsMinimized
    {
        get
        {
            //return Mathf.Abs(m_RectTransform.rect.width - m_ResizableGrid.MinimumViewWidth) <= MINIMIZED_THRESHOLD;
            return Mathf.Abs(m_RectTransform.rect.width) <= MINIMIZED_THRESHOLD;
        }
    }

    /// <summary>
    /// GameObject to hide a minimized column
    /// </summary>
    [SerializeField]
    private GameObject m_MinimizedGameObject;

    private RectTransform m_RectTransform = null;
    private const float MINIMIZED_THRESHOLD = 250.0f;
    #endregion

    #region Private Methods
    private void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (m_RectTransform.hasChanged)
        {
            m_MinimizedGameObject.SetActive(IsMinimized);
            m_RectTransform.hasChanged = false;
        }
    }
    #endregion
}