// Adapted from HiBoP (https://github.com/hbp-HiBoP/HiBoP), BSD-3-Clause. Licence and copyright: THIRD-PARTY-NOTICES.md.
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class VerticalHandler : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Is this handler the last clicked handler ?
    /// </summary>
    public bool IsClicked { get; set; } = false;
    /// <summary>
    /// Minimum position of the handler
    /// </summary>
    public float MinimumPosition { get; set; } = 0;
    /// <summary>
    /// Maximum position of the handler
    /// </summary>
    public float MaximumPosition { get; set; } = 0;
    /// <summary>
    /// Threshold to decide when to attract the handler
    /// </summary>
    public float MagneticThreshold { get; set; } = 0.020f;
    /// <summary>
    /// Position near which the handler is attracted
    /// </summary>
    public float MagneticPosition { get; set; }
    /// <summary>
    /// Getter/Setter for the position of the Handler
    /// </summary>
    public float Position
    {
        get
        {
            return m_Position;
        }
        set
        {
            m_Position = Mathf.Clamp(value, MinimumPosition, MaximumPosition);
            if (Mathf.Abs(m_Position - MagneticPosition) < MagneticThreshold && IsClicked)
            {
                m_Position = MagneticPosition;
            }
            //This round up make a change to the value, see with Benjamin why it was done in HiBoP
            //m_Position = RoundAtPrecision(m_Position, 2 / m_ResizableGrid.RectTransform.rect.width);
            RectTransform handler = GetComponent<RectTransform>();
            handler.anchorMin = new Vector2(m_Position, handler.anchorMin.y);
            handler.anchorMax = new Vector2(m_Position, handler.anchorMax.y);
        }
    }
    /// <summary>
    /// Event emited when the position is changed
    /// </summary>
    public UnityEvent OnChangePosition = new UnityEvent();

    [SerializeField] private Texture2D m_verticalCursorTexture = null;
    [SerializeField] private Vector2 m_cursorOffsetPosition = new Vector2(11, 11);
    [SerializeField] private ResizableGrid m_ResizableGrid = null;

    private float m_Position = 0;
    private const float MAGNETIC_THRESHOLD = 0.020f;

    public void Init()
    {
        Vector2 position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        m_Position = (position.x) / m_ResizableGrid.RectTransform.rect.width;
    }

    public void OnDrag(PointerEventData eventData)
    {
        ///!|                           CAUTION
        //The camera parameter must be null in case of Screenspace - Overlay for the camera
        //At the moment we use                         Screenspace - Camera 
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ResizableGrid.RectTransform, eventData.position, Camera.main, out Vector2 localPosition);
        Position = localPosition.x / m_ResizableGrid.RectTransform.rect.width;
        OnChangePosition.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(m_verticalCursorTexture, m_cursorOffsetPosition, CursorMode.ForceSoftware);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.ForceSoftware);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsClicked = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsClicked = false;
    }

    private float RoundAtPrecision(float number, float precision)
    {
        if (precision > 1.0f || precision <= 0.0f) return number;
        return precision * Mathf.Round(number / precision);
    }
}
