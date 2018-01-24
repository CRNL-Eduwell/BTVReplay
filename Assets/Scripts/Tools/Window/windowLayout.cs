using UnityEngine;
using UnityEngine.EventSystems;

public class windowLayout : MonoBehaviour, IDropHandler
{
    [SerializeField]
    BTVMedia media = null;

    private RectTransform m_rectTransform = null;
    private Rect[] cells2by3 = new Rect[6];
    private Rect[] cells1by3 = new Rect[3];
    private Rect[] cells2by2 = new Rect[4];

    private Rect[] cells2by3Previous = new Rect[6];
    private Rect[] cells1by3Previous = new Rect[3];
    private Rect[] cells2by2Previous = new Rect[4];

    private bool NothingDone = true;
    private bool loaded = false;
    private RectTransform currentRectTransform = null;
    private Window currentWindowManager = null;

    void Awake()
    {
        media.mediaLoaded += new mediaLoadedEventHandler(() =>
        {
            loaded = true;
            defineSizeGrid();
        });
    }

    void OnDestroy()
    {
        media.mediaLoaded -= new mediaLoadedEventHandler(() =>
        {
            loaded = true;
            defineSizeGrid();
        });
    }

    void OnRectTransformDimensionsChange()
    {
        if (loaded)
            forceResize();
        else
            initResize();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (Window.itemBeingDragged != null)
        {
            Window.itemBeingDragged.transform.SetParent(transform);
            currentRectTransform = Window.itemBeingDragged.GetComponent<RectTransform>();
            currentWindowManager = Window.itemBeingDragged.GetComponent<Window>();

            if ((currentRectTransform.rect.width > cells2by3[0].width) || 
                (currentRectTransform.rect.width > cells2by3[0].width && 
                currentRectTransform.rect.height <= cells2by3[0].height))
            {
                // Debug.Log("Look at 1 by 3");
                for (int i = 0; i < cells1by3.Length; i++)
                {
                    Vector3 dd = new Vector3(currentRectTransform.localPosition.x + 0.5f * cells2by3[i].width, 
                                             currentRectTransform.localPosition.y + 0.5f * cells2by3[i].height);
                    if (cells1by3[i].Contains(dd))
                    {
                        //Debug.Log("Droped at" + i);
                        currentRectTransform.localPosition = new Vector3(cells1by3[i].x, cells1by3[i].y, currentRectTransform.localPosition.z);
                        currentRectTransform.sizeDelta = new Vector2(cells1by3[i].width, cells1by3[i].height);
                        currentWindowManager.minSizeWindow = new Vector2(cells2by3[i].width / 2, cells2by3[i].height / 2);
                        currentWindowManager.maxSizeWindow = new Vector2(cells1by3[i].width, 1.5f * cells1by3[i].height);
                        currentWindowManager.windowId = i;
                        NothingDone = false;
                    }
                }

            }
            else if (currentRectTransform.rect.width <= cells2by3[0].width && currentRectTransform.rect.height <= cells2by3[0].height)
            {
                //Debug.Log("Look at 2 by 3");
                for (int i = 0; i < cells2by3.Length; i++)
                {
                    Vector3 dd = new Vector3(currentRectTransform.localPosition.x + 0.5f * cells2by3[i].width, currentRectTransform.localPosition.y + 0.5f * cells2by3[i].height);
                    if (cells2by3[i].Contains(dd))
                    {
                        //Debug.Log("Droped at" + i);
                        currentRectTransform.localPosition = new Vector3(cells2by3[i].x, cells2by3[i].y, currentRectTransform.localPosition.z);
                        currentRectTransform.sizeDelta = new Vector2(cells2by3[i].width, cells2by3[i].height);
                        currentWindowManager.minSizeWindow = new Vector2(cells2by3[i].width / 2, cells2by3[i].height / 2);
                        currentWindowManager.maxSizeWindow = new Vector2(cells2by3[i].width * 2, 1.5f * cells2by3[i].height);
                        currentWindowManager.windowId = i;
                        NothingDone = false;
                    }
                }
            }
            else
            {
                //Debug.Log("Look at 2 by 2");
                for (int i = 0; i < cells2by2.Length; i++)
                {
                    Vector3 dd = new Vector3(currentRectTransform.localPosition.x + 0.5f * cells2by3[i].width, currentRectTransform.localPosition.y + 0.5f * cells2by3[i].height);
                    if (cells2by2[i].Contains(dd))
                    {
                        //Debug.Log("Droped at" + i);
                        currentRectTransform.localPosition = new Vector3(cells2by2[i].x, cells2by2[i].y, currentRectTransform.localPosition.z);
                        currentRectTransform.sizeDelta = new Vector2(cells2by2[i].width, cells2by2[i].height);
                        currentWindowManager.minSizeWindow = new Vector2(cells2by2[i].width / 2, cells2by2[i].height / 2);
                        currentWindowManager.maxSizeWindow = new Vector2(2 * cells2by2[i].width, 2 * cells2by2[i].height);
                        currentWindowManager.windowId = i;
                        NothingDone = false;
                    }
                }
            }

            if (NothingDone)
            {
                replaceAtOrigin();
            }
        }
    }

    void replaceAtOrigin()
    {
        Window.itemBeingDragged.transform.SetParent(currentWindowManager.initialParent);
        currentRectTransform.localPosition = new Vector3(currentWindowManager.initialPosition.x, currentWindowManager.initialPosition.y);
        currentRectTransform.sizeDelta = new Vector2(currentWindowManager.initialSizeDelta.x, currentWindowManager.initialSizeDelta.y);
    }

    void defineSizeGrid()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                cells2by3Previous[j + (i * 2)] = new Rect(cells2by3[j + (i * 2)]);

                cells2by3[j + (i * 2)].width = m_rectTransform.rect.width / 2;
                cells2by3[j + (i * 2)].height = m_rectTransform.rect.height / 3;
                cells2by3[j + (i * 2)].x = m_rectTransform.rect.x + (0.5f * cells2by3[j + (i * 2)].width) + (j * cells2by3[j + (i * 2)].width);
                cells2by3[j + (i * 2)].y = m_rectTransform.rect.y + (0.5f * cells2by3[j + (i * 2)].height) + (i * cells2by3[j + (i * 2)].height);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                cells2by2Previous[j + (i * 2)] = new Rect(cells2by2[j + (i * 2)]);

                cells2by2[j + (i * 2)].width = m_rectTransform.rect.width / 2;
                cells2by2[j + (i * 2)].height = m_rectTransform.rect.height / 2;
                cells2by2[j + (i * 2)].x = m_rectTransform.rect.x + (0.5f * cells2by2[j + (i * 2)].width) + (j * cells2by2[j + (i * 2)].width);
                cells2by2[j + (i * 2)].y = m_rectTransform.rect.y + (0.5f * cells2by2[j + (i * 2)].height) + (i * cells2by2[j + (i * 2)].height);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                cells1by3Previous[j + i] = new Rect(cells1by3[j + i]);

                cells1by3[j + i].width = m_rectTransform.rect.width / 1;
                cells1by3[j + i].height = m_rectTransform.rect.height / 3;
                cells1by3[j + i].x = m_rectTransform.rect.x + (0.5f * cells1by3[j + i].width) + (j * cells1by3[j + i].width);
                cells1by3[j + i].y = m_rectTransform.rect.y + (0.5f * cells1by3[j + i].height) + (i * cells1by3[j + i].height);
            }
        }
    }

    public void forceResize()
    {
        if (m_rectTransform == null)
            m_rectTransform = gameObject.GetComponent<RectTransform>();

        defineSizeGrid();

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject currentChildObject = gameObject.transform.GetChild(i).gameObject;

            RectTransform r = currentChildObject.GetComponent<RectTransform>();
            Window w = currentChildObject.GetComponent<Window>();


            if (r != null && w != null)
            {
                //Debug.Log("[" + r.name + "]");

                if (r.sizeDelta.x > cells2by3Previous[0].width && r.sizeDelta.y <= cells2by3Previous[0].height)
                {
                    //Debug.Log("Look at 1 by 3");
                    r.sizeDelta = new Vector2(cells1by3[w.windowId].width, cells1by3[w.windowId].height);
                    r.localPosition = new Vector3(cells1by3[w.windowId].x, cells1by3[w.windowId].y, r.localPosition.z);
                }
                else if (r.sizeDelta.x <= cells2by3Previous[0].width && r.sizeDelta.y <= cells2by3Previous[0].height)
                {
                    //Debug.Log("Look at 2 by 3");
                    r.sizeDelta = new Vector2(cells2by3[w.windowId].width, cells2by3[w.windowId].height);
                    r.localPosition = new Vector3(cells2by3[w.windowId].x, cells2by3[w.windowId].y, r.localPosition.z);
                }
                else
                {
                    //Debug.Log("Look at 2 by 2");
                    r.sizeDelta = new Vector2(cells2by2[w.windowId].width, cells2by2[w.windowId].height);
                    r.localPosition = new Vector3(cells2by2[w.windowId].x, cells2by2[w.windowId].y, r.localPosition.z);
                }
            }
        }
    }

    public void initResize()
    {
        if (m_rectTransform == null)
            m_rectTransform = gameObject.GetComponent<RectTransform>();

        defineSizeGrid();

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject currentChildObject = gameObject.transform.GetChild(i).gameObject;

            RectTransform r = currentChildObject.GetComponent<RectTransform>();
            Window w = currentChildObject.GetComponent<Window>();

            if (r != null && w != null)
            {
                r.sizeDelta = new Vector2(cells2by2[w.windowId].width, cells2by2[w.windowId].height);
                r.localPosition = new Vector3(cells2by2[w.windowId].x, cells2by2[w.windowId].y, r.localPosition.z);
            }
        }
    }
}
