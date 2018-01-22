using UnityEngine;

public class WindowOpt : MonoBehaviour
{
    #region cursor members
    [SerializeField] private Texture2D m_verticalCursorTexture = null;
    [SerializeField] private Vector2 m_cursorOffsetPosition = new Vector2(11, 11);
    #endregion

    private Transform m_rectTransformView = null;
    private RectTransform m_rectTransform = null;
    private RectTransform m_rectTransformL = null;
    private RectTransform m_rectTransformR = null;
    private float m_initialWidth = 0;
    private bool m_optionVisible = false;
    private Vector3 m_initialMousePosition;
    private Vector2 m_minSizeWindow = new Vector2(50, 50); // new Vector2(150, 50);
    private Vector2 m_maxSizeWindow = new Vector2(5000, 5000);  //new Vector2(300, 150);

    void Start()
    {
        m_rectTransformView = GameObject.Find("View").transform;
        m_initialWidth = m_rectTransformView.gameObject.GetComponent<RectTransform>().rect.width;
        m_rectTransformL = m_rectTransformView.GetChild(0).GetComponent<RectTransform>();
        m_rectTransformR = m_rectTransformView.GetChild(1).GetComponent<RectTransform>();
        m_rectTransform = gameObject.GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    #region MouseEnterEvents
    public void OnVerticalMouseEnter()
    {
        Cursor.SetCursor(m_verticalCursorTexture, m_cursorOffsetPosition, CursorMode.ForceSoftware);
    }

    public void OnMouseExit()
    {
        Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.ForceSoftware);
    }
    #endregion

    #region ResizingEvents
    public void OnBeginDrag()
    {
        m_initialMousePosition = Input.mousePosition;
    }

    public void OnHorizontalDrag()
    {
        float delta = Input.mousePosition.x - m_initialMousePosition.x;
        float newSize = ((m_rectTransformL.rect.width + m_rectTransformR.rect.width + delta) / m_initialWidth);

        if ((1 - newSize) < 0.3f && (1 - newSize) > 0.1f)
        {
            m_rectTransformL.anchorMin = new Vector2(0, 0);
            m_rectTransformL.anchorMax = new Vector2(newSize / 2, 1);
            m_rectTransformL.offsetMin = new Vector2(0, 0);
            m_rectTransformL.offsetMax = new Vector2(0, 0);

            m_rectTransformR.anchorMin = new Vector2(newSize / 2, 0);
            m_rectTransformR.anchorMax = new Vector2(newSize, 1);
            m_rectTransformR.offsetMin = new Vector2(0, 0);
            m_rectTransformR.offsetMax = new Vector2(0, 0);

            m_rectTransform.anchorMin = new Vector2(newSize, 0);
            m_rectTransform.anchorMax = new Vector2(1, 1);
            m_rectTransform.offsetMin = new Vector2(0, 0);
            m_rectTransform.offsetMax = new Vector2(0, 0);

            m_initialMousePosition = Input.mousePosition;
        }
    }
    #endregion

    public void HideOptionPanel(bool showMe)
    {
        m_optionVisible = showMe;
        m_rectTransform.gameObject.SetActive(m_optionVisible);

        if (m_optionVisible == false)
        {
            m_rectTransformL.anchorMin = new Vector2(0, 0);
            m_rectTransformL.anchorMax = new Vector2(0.5f, 1);
            m_rectTransformL.offsetMin = new Vector2(0, 0);
            m_rectTransformL.offsetMax = new Vector2(0, 0);

            m_rectTransformR.anchorMin = new Vector2(0.5f, 0);
            m_rectTransformR.anchorMax = new Vector2(1, 1);
            m_rectTransformR.offsetMin = new Vector2(0, 0);
            m_rectTransformR.offsetMax = new Vector2(0, 0);
        }
        else
        {
            float percent = m_rectTransform.rect.width / m_initialWidth;

            m_rectTransformL.anchorMin = new Vector2(0, 0);
            m_rectTransformL.anchorMax = new Vector2((1 - percent) / 2, 1);
            m_rectTransformL.offsetMin = new Vector2(0, 0);
            m_rectTransformL.offsetMax = new Vector2(0, 0);

            m_rectTransformR.anchorMin = new Vector2((1 - percent) / 2, 0);
            m_rectTransformR.anchorMax = new Vector2(1 - percent, 1);
            m_rectTransformR.offsetMin = new Vector2(0, 0);
            m_rectTransformR.offsetMax = new Vector2(0, 0);

            m_rectTransform.anchorMin = new Vector2(1 - percent, 0);
            m_rectTransform.anchorMax = new Vector2(1, 1);
            m_rectTransform.offsetMin = new Vector2(0, 0);
            m_rectTransform.offsetMax = new Vector2(0, 0);
        }
    }
}
