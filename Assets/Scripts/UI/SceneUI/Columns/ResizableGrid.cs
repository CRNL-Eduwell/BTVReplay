using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class ResizableGrid : MonoBehaviour
{
    public bool InitDone { get; set; } = false;
    /// <summary>
    /// ResizableGrid's RectTransform
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
    /// Number of Column in the Grid
    /// </summary>
    public int ColumnCount { get { return m_Columns.Count; } }
    /// <summary>
    /// Number of Vertical Handler in the Grid
    /// </summary>
    public int VerticalHandlerCount { get { return m_VerticalHandlers.Count; } }
    /// <summary>
    /// Vertical Handlers allowing to resize columns
    /// </summary>
    public ReadOnlyCollection<VerticalHandler> VerticalHandlers
    {
        get
        {
            return new ReadOnlyCollection<VerticalHandler>(m_VerticalHandlers);
        }
    }

    [SerializeField] private RectTransform m_RectTransform;
    [SerializeField] private List<VerticalHandler> m_VerticalHandlers = new List<VerticalHandler>();
    [SerializeField] private List<ColumnGUIManager> m_Columns = new List<ColumnGUIManager>();

    private float m_MinimumViewWidth = 35f;

    private void Start()
    {
        foreach (VerticalHandler handler in m_VerticalHandlers)
        {
            handler.OnChangePosition.AddListener(() =>
            {
                SetVerticalHandlersPosition();
                UpdateAnchors();
            });
        }
    }

    public void Init()
    {

        for (int i = 0; i < VerticalHandlerCount; i++)
        {
            m_VerticalHandlers[i].Init();
        }

        UpdateHandlersMinMaxPositions();
        SetVerticalHandlersPosition();
        UpdateAnchors();
    }

    private void OnDestroy()
    {
        foreach (VerticalHandler handler in m_VerticalHandlers)
        {
            handler.OnChangePosition.RemoveAllListeners();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        UnityEngine.Debug.Log("OnRect Resize grid");
        if (InitDone)
        {
            UpdateHandlersMinMaxPositions();
            SetVerticalHandlersPosition();
            UpdateAnchors();
        }
    }

    /// <summary>
    /// Update the position constraints on the handlers depending on the number of columns and views
    /// </summary>
    private void UpdateHandlersMinMaxPositions()
    {
        for (int i = 0; i < VerticalHandlerCount; i++)
        {
            m_VerticalHandlers[i].MinimumPosition = (i + 1) * (m_MinimumViewWidth / m_RectTransform.rect.width);
            m_VerticalHandlers[i].MaximumPosition = 1 - (VerticalHandlerCount - i) * (m_MinimumViewWidth / m_RectTransform.rect.width);
        }
        //Update position with new boundaries(call to setter)
        m_VerticalHandlers.ForEach((h) => h.Position = h.Position);
    }

    public void SetVerticalHandlersPosition(int selectedHandlerID = -1)
    {
        if (selectedHandlerID == -1)
        {
            selectedHandlerID = m_VerticalHandlers.FindIndex((handler) => { return handler.IsClicked; });
            if (selectedHandlerID == -1) return;
        }

        for (int i = 0; i < m_VerticalHandlers.Count; i++)
        {
            float referencePosition = m_VerticalHandlers[selectedHandlerID].Position + (i - selectedHandlerID) * m_MinimumViewWidth / m_RectTransform.rect.width;
            if (i < selectedHandlerID)
            {
                m_VerticalHandlers[i].Position = Mathf.Min(m_VerticalHandlers[i].Position, referencePosition);
            }
            else if (i > selectedHandlerID)
            {
                m_VerticalHandlers[i].Position = Mathf.Max(m_VerticalHandlers[i].Position, referencePosition);
            }
        }
    }

    public void UpdateAnchors()
    {
        for (int i = 0; i < ColumnCount; i++)
        {
            RectTransform column = m_Columns[i].RectTransform;;
            column.anchorMin = new Vector2((i == 0) ? 0 : m_VerticalHandlers[i - 1].Position, column.anchorMin.y);
            column.anchorMax = new Vector2((i == ColumnCount - 1) ? 1 : m_VerticalHandlers[i].Position, column.anchorMax.y);
        }
    }
}
