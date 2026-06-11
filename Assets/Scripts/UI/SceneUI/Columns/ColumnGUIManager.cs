using UnityEngine;

public class ColumnGUIManager : MonoBehaviour
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
            return Mathf.Abs(RectTransform.rect.width - 25) <= MINIMIZED_THRESHOLD;
        }
    }
    /// <summary>
    /// Column's RectTransform
    /// </summary>
    public RectTransform RectTransform
    {
        get
        {
            if (m_RectTransform == null)
            {
                m_RectTransform = GetComponent<RectTransform>();
            }
            return m_RectTransform;
        }
    }
    /// <summary>
    /// GameObject to hide a minimized column
    /// </summary>
    [SerializeField]
    private GameObject m_MinimizedGameObject = null;

    private RectTransform m_RectTransform = null;
    private const float MINIMIZED_THRESHOLD = 100.0f;
    #endregion

    #region Private Methods
    private void Start()
    {
        m_RectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (m_RectTransform.hasChanged)
        {
            BtvLog.Log("Recttransform changed minimed column");
            m_MinimizedGameObject.SetActive(IsMinimized);
            m_RectTransform.hasChanged = false;
        }
    }
    #endregion
}