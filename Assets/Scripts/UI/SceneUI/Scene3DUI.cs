using System.Collections;
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
            return Mathf.Abs(RectTransform.rect.width) <= MINIMIZED_THRESHOLD;
        }
    }

    /// <summary>
    /// Column's RectTransform (lazily fetched so it is valid even when Unity raises
    /// OnRectTransformDimensionsChange before Awake).
    /// </summary>
    private RectTransform RectTransform
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
    private bool m_RefreshPending = false;
    private const float MINIMIZED_THRESHOLD = 250.0f;
    #endregion

    #region Private Methods
    private void Start()
    {
        RefreshMinimizedState();
    }

    // Unity raises this whenever this RectTransform's dimensions change, replacing the
    // per-frame rect.hasChanged poll that used to live in Update(). The callback fires
    // *inside* the UGUI layout rebuild loop, where toggling a UI GameObject active is
    // illegal ("remove from rebuild list while inside a rebuild loop"), so the SetActive
    // is coalesced and deferred to the next frame, outside the rebuild.
    private void OnRectTransformDimensionsChange()
    {
        if (!m_RefreshPending && isActiveAndEnabled)
        {
            m_RefreshPending = true;
            StartCoroutine(RefreshAfterRebuild());
        }
    }

    private IEnumerator RefreshAfterRebuild()
    {
        yield return null;
        m_RefreshPending = false;
        RefreshMinimizedState();
    }

    private void RefreshMinimizedState()
    {
        if (m_MinimizedGameObject == null)
            return;
        bool minimized = IsMinimized;
        if (m_MinimizedGameObject.activeSelf != minimized)
            m_MinimizedGameObject.SetActive(minimized);
    }
    #endregion
}
