using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public GameObject brainPanel = null;
    public GameObject perfPanel = null;
    public GameObject courbe1Panel = null;
    public GameObject courbe2Panel = null;
    public GameObject videoGridPanel = null;

    //=== Brain + Courbe Side
    RectTransform brainRect = null, panelRect = null, gameobjRect = null;
    GridLayoutGroup gridGroup = null;
    Vector2 startSize, bigSize, memorySize;
    Vector3 startPosition;

    //=== Video Side
    RectTransform videoRect = null;
    GridLayoutGroup videoGridGroup = null;
    Vector2 memorySizeVideo;

    void Awake()
    {
        gameobjRect = gameObject.GetComponent<RectTransform>();
        panelRect = brainPanel.transform.parent.GetComponent<RectTransform>();
        brainRect = brainPanel.transform.GetComponent<RectTransform>();
        gridGroup = gameObject.GetComponent<GridLayoutGroup>();

    }


    void Start()
    {
        startSize = brainRect.sizeDelta;
        startPosition = brainRect.position;
        bigSize = panelRect.sizeDelta;
        gridGroup.cellSize = gameobjRect.rect.size;
        memorySize = gameobjRect.rect.size;

        if(gameObject.name == "Panel11")
        {
            videoRect = videoGridPanel.GetComponent<RectTransform>();
            videoGridGroup = videoGridPanel.GetComponent<GridLayoutGroup>();
            videoGridGroup.cellSize = new Vector2((videoRect.rect.size.x / 2),(videoRect.rect.size.y / 3));
            memorySizeVideo = videoRect.rect.size;
        }
    }

    void Update()
    {
        if (memorySize != gameobjRect.rect.size)
        {
            startSize = brainRect.sizeDelta;
            startPosition = brainRect.position;
            bigSize = panelRect.sizeDelta;
            gridGroup.cellSize = gameobjRect.rect.size;
            memorySize = gameobjRect.rect.size;
        }

        if (gameObject.name == "Panel11")
        {
            if (memorySizeVideo != videoRect.rect.size)
            {
                videoGridGroup.cellSize = new Vector2((videoRect.rect.size.x / 2), (videoRect.rect.size.y / 3));
                memorySizeVideo = videoRect.rect.size;
            }
        }
    }

    public GameObject item
    {
        get {
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject;
            }
            return null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!item)
        {
            DragHandler.itemBeingDragged.transform.SetParent(transform);
        }
        checkChild();
    }

    void checkChild()
    {
        int perf = perfPanel.transform.childCount;
        int courbe1 = courbe1Panel.transform.childCount;
        int courbe2 = courbe2Panel.transform.childCount;

        if (perf == 0 && courbe1 == 0 && courbe2 == 0)
        {
            brainPanel.transform.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            brainPanel.transform.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            brainPanel.transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            // [ left - bottom ]
            brainPanel.transform.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
            // [ right - top ]
            brainPanel.transform.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);

            brainPanel.transform.GetComponent<RectTransform>().sizeDelta = bigSize;
        }
        if (perf == 1 || courbe1 == 1 || courbe2 == 1)
        {
            brainPanel.transform.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.5f);
            brainPanel.transform.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1.0f);
            brainPanel.transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            // [ left - bottom ]
            brainPanel.transform.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
            // [ right - top ]
            brainPanel.transform.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);

            brainPanel.transform.GetComponent<RectTransform>().position = startPosition;
            brainPanel.transform.GetComponent<RectTransform>().sizeDelta = startSize;
        }
    }

    public void putBigBrain()
    {
        brainPanel.transform.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        brainPanel.transform.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        brainPanel.transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        // [ left - bottom ]
        brainPanel.transform.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
        // [ right - top ]
        brainPanel.transform.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);

        brainPanel.transform.GetComponent<RectTransform>().sizeDelta = bigSize;
    }

    public void putSmallBrain()
    {
        brainPanel.transform.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.5f);
        brainPanel.transform.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1.0f);
        brainPanel.transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        // [ left - bottom ]
        brainPanel.transform.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
        // [ right - top ]
        brainPanel.transform.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);

        brainPanel.transform.GetComponent<RectTransform>().position = startPosition;
        brainPanel.transform.GetComponent<RectTransform>().sizeDelta = startSize;
    }
}
