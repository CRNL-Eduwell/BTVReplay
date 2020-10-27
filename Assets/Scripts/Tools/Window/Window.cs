using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Window : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GameObject itemBeingDragged;
    public Vector2 maxSizeWindow
    {
        get
        {
            return m_maxSizeWindow;
        }

        set
        {
            m_maxSizeWindow = value;
        }
    }
    public Vector2 minSizeWindow
    {
        get
        {
            return m_minSizeWindow;
        }

        set
        {
            m_minSizeWindow = value;
        }
    }
    public Vector2 initialPosition
    {
        get
        {
            return m_initialPanelPosition;
        }
    }
    public Vector2 initialSizeDelta
    {
        get
        {
            return m_initialSizeDelta;
        }
    }
    public Transform initialParent
    {
        get
        {
            return m_initialTransform;
        }
    }
    public int windowId
    {
        get
        {
            return m_idWin;
        }

        set
        {
            m_idWin = value;
        }
    }
    public GridLayout GridLayout
    {
        get;set;
    }
    public bool hasFocus = false;

    //===
    bool dragAllowed = false; //Used to prevent drag in case of right click to move brain
    private RectTransform m_rectTransform = null;

    #region cursor members
    [SerializeField] private Texture2D m_horizontalCursorTexture = null;
    [SerializeField] private Texture2D m_verticalCursorTexture = null;
    [SerializeField] private Texture2D m_diagonalCursorTextureLR = null;
    [SerializeField] private Texture2D m_diagonalCursorTextureRL = null;
    [SerializeField] private Vector2 m_cursorOffsetPosition = new Vector2(11, 11);
    #endregion

    #region Resize Cursor members
    private Vector2 m_initialRectSize;
    private Vector2 m_initialSizeDelta;
    private Vector3 m_initialPanelPosition;
    private Vector3 m_initialMousePosition;
    [SerializeField] private int m_idWin = -1;
    private Vector2 m_minSizeWindow = new Vector2(50, 50); // new Vector2(150, 50);
    private Vector2 m_maxSizeWindow = new Vector2(5000, 5000);  //new Vector2(300, 150);
    #endregion

    #region drag & drop members
    private GameObject m_canvasGameObject = null;
    private RectTransform m_worldRectTransform = null;
    private Vector3[] m_worldCorners = new Vector3[4];
    private Transform m_initialTransform;
    #endregion

    // Use this for initialization
    void Start()
    {
        m_canvasGameObject = GameObject.Find("Canvas");
        m_worldRectTransform = m_canvasGameObject.transform.GetComponent<RectTransform>();
        m_rectTransform = gameObject.GetComponent<RectTransform>();
        m_initialTransform = m_rectTransform.transform.parent;
        m_maxSizeWindow = new Vector2(m_rectTransform.rect.width * 2, m_rectTransform.rect.height);
    }

    #region MouseEnterEvents
    public void OnHorizontalMouseEnter()
    {
        Cursor.SetCursor(m_horizontalCursorTexture, m_cursorOffsetPosition, CursorMode.ForceSoftware);
    }

    public void OnVerticalMouseEnter()
    {
        Cursor.SetCursor(m_verticalCursorTexture, m_cursorOffsetPosition, CursorMode.ForceSoftware);
    }

    //Diagonal LR : cursor goes from top left point to lower right one
    public void OnDiagonalLREnter()
    {
        Cursor.SetCursor(m_diagonalCursorTextureLR, m_cursorOffsetPosition, CursorMode.ForceSoftware);
    }

    //Diagonal RL : cursor goes from lower right point to top left point 
    public void OnDiagonalRLEnter()
    {
        Cursor.SetCursor(m_diagonalCursorTextureRL, m_cursorOffsetPosition, CursorMode.ForceSoftware);
    }

    public void OnMouseExit()
    {
        Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.ForceSoftware);
    }
    #endregion

    #region ResizingEvents
    public void OnBeginDrag()
    {
        m_initialPanelPosition = m_rectTransform.localPosition;
        m_initialSizeDelta = m_rectTransform.sizeDelta;
        m_initialRectSize = m_rectTransform.rect.size;
        m_initialMousePosition = Input.mousePosition;

        itemBeingDragged = gameObject;
    }

    public void OnHorizontalDrag(bool isLeft)
    {
        int side = 0;
        if (isLeft) side = -1; else side = 1;
        float resize = side * (Input.mousePosition.x - m_initialMousePosition.x);

        if (m_initialSizeDelta.x + resize >= m_minSizeWindow.x && m_initialSizeDelta.x + resize < m_maxSizeWindow.x)
        {
            m_rectTransform.sizeDelta = new Vector2(m_initialSizeDelta.x + resize, m_initialSizeDelta.y);
            m_rectTransform.localPosition = m_initialPanelPosition + new Vector3(side * m_rectTransform.pivot.x * resize, 0, 0);
        }
    }

    public void OnVerticalDrag(bool isBottom)
    {
        int side = 0;
        if (isBottom) side = -1; else side = 1;

        float resize = side * (Input.mousePosition.y - m_initialMousePosition.y);

        if (m_initialSizeDelta.y + resize >= m_minSizeWindow.y && m_initialSizeDelta.y + resize < m_maxSizeWindow.y)
        {
            m_rectTransform.sizeDelta = new Vector2(m_initialSizeDelta.x, m_initialSizeDelta.y + resize);
            m_rectTransform.localPosition = m_initialPanelPosition + new Vector3(0, side * m_rectTransform.pivot.y * resize, 0);
        }
    }

    //  Define Resizing according 
    //  to which corner is clicked
    //      3_______2
    //      |       |
    //      0_______1
    public void OnDiagonalDrag(int i)
    {
        int sideRL = 0;
        int sideTB = 0;
        if (i == 0)
        {
            sideRL = -1;
            sideTB = -1;
        }
        else if (i == 1)
        {
            sideRL = 1;
            sideTB = -1;
        }
        else if (i == 2)
        {
            sideRL = 1;
            sideTB = 1;
        }
        else if (i == 3)
        {
            sideRL = -1;
            sideTB = 1;
        }

        Vector2 resize = new Vector2(sideRL * (Input.mousePosition.x - m_initialMousePosition.x), sideTB * (Input.mousePosition.y - m_initialMousePosition.y));

        //if (m_initialSizeDelta.x + resize.x < m_minSizeWindow.x)
        //    resize.x = m_minSizeWindow.x - m_initialRectSize.x;

        //if (m_initialSizeDelta.y + resize.y < m_minSizeWindow.y)
        //    resize.y = m_minSizeWindow.y - m_initialRectSize.y;

        if (m_initialSizeDelta.x + resize.x >= m_minSizeWindow.x && m_initialSizeDelta.x + resize.x < m_maxSizeWindow.x &&
            m_initialSizeDelta.y + resize.y >= m_minSizeWindow.y && m_initialSizeDelta.y + resize.y < m_maxSizeWindow.y)
        {
            m_rectTransform.sizeDelta = m_initialSizeDelta + resize;
            m_rectTransform.localPosition = m_initialPanelPosition + new Vector3(sideRL * (m_rectTransform.anchorMax.x * resize.x), sideTB * (m_rectTransform.anchorMax.y * resize.y));
        }
    }
    #endregion

    #region dragWindow
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            dragAllowed = true;
            m_initialTransform = m_rectTransform.transform.parent;
            m_initialPanelPosition = m_rectTransform.localPosition;

            itemBeingDragged = gameObject;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else
        {
            dragAllowed = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragAllowed)
        {
            m_worldRectTransform.GetWorldCorners(m_worldCorners);
            transform.SetParent(m_canvasGameObject.transform);

            Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (worldClick.x > m_worldCorners[1].x && worldClick.x < m_worldCorners[2].x
                && worldClick.y > m_worldCorners[3].y && worldClick.y < m_worldCorners[2].y)
            {
                transform.position = new Vector3(worldClick.x, worldClick.y, transform.position.z);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragAllowed)
        {
            //if (itemBeingDragged.transform.parent.name != "PanelL" &&
            //itemBeingDragged.transform.parent.name != "PanelR")
            if (itemBeingDragged.transform.parent.name != "Pannel" &&
            itemBeingDragged.transform.parent.name != "RightPannel")
            {
                itemBeingDragged.GetComponent<RectTransform>().SetParent(m_initialTransform);
                itemBeingDragged.GetComponent<RectTransform>().localPosition = m_initialPanelPosition;
            } //maybe else we find which one is closest to prevent few edge cases

            itemBeingDragged = null;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }
    #endregion

    public void setBorderColor(Color newColor)
    {
        Image borderWindowImage = gameObject.GetComponent<Image>();
        borderWindowImage.color = newColor;
    }
}