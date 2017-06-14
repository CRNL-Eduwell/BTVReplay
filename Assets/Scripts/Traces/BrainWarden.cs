using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void newPlotClicked(GameObject plot);

public class BrainWarden : MonoBehaviour, IPointerClickHandler
{
    public event newPlotClicked plotWasClicked;

    [SerializeField] Camera brainCam = null;
    [SerializeField] BTVMedia media = null;

    bool isMaxed = false;
    bool initDone = false;
    RectTransform m_rectTransform = null;
    Vector2 m_startSize, m_BigSize;
    Vector3[] worldCornerOfBrainPanel = new Vector3[4];
    Window trace1 = null;
    Window trace2 = null;
    GameObject plot = null;
    GameObject elecPointer = null;
    GameObject elecPointerPic = null;
    Text elecPointerText = null;

    void Start ()
    {
        media.mediaLoaded += new mediaLoadedEventHandler(() => initDone = true);

        m_rectTransform = gameObject.GetComponent<RectTransform>();
        m_startSize = m_rectTransform.sizeDelta;
        m_BigSize = m_startSize * 2;

        trace1 = GameObject.Find("Trace1Window").GetComponent<Window>();
        trace2 = GameObject.Find("Trace2Window").GetComponent<Window>();
        elecPointer = GameObject.Find("Canvas").transform.GetChild(4).gameObject;
        elecPointerPic = elecPointer.transform.GetChild(0).gameObject;
        elecPointerText = elecPointerPic.transform.GetChild(0).GetComponent<Text>();
    }

    private void OnDestroy()
    {
        media.mediaLoaded -= new mediaLoadedEventHandler(() => initDone = true);
    }

    void Update()
    {
        if (initDone && isOver(Input.mousePosition))
            checkIfPointElectrode();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (eventData.clickCount == 2)
            {
                if (!isMaxed)
                    bigBrain();
                else
                    smallBrain();

                isMaxed = !isMaxed;
            }

            if (eventData.clickCount == 1)
                checkIfhitElectrode();
        }
    }

    void bigBrain()
    {
        m_rectTransform.anchorMin = new Vector2(0, 0);
        m_rectTransform.anchorMax = new Vector2(1, 1);
        m_rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // [ left - bottom ]
        m_rectTransform.offsetMin = new Vector2(0f, 0f);
        // [ right - top ]
        m_rectTransform.offsetMax = new Vector2(0f, 0f);

        m_rectTransform.sizeDelta = m_BigSize;
    }

    void smallBrain()
    {
        m_rectTransform.anchorMin = new Vector2(0f, 0.5f);
        m_rectTransform.anchorMax = new Vector2(0.5f, 1.0f);
        m_rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // [ left - bottom ]
        m_rectTransform.offsetMin = new Vector2(0f, 0f);
        // [ right - top ]
        m_rectTransform.offsetMax = new Vector2(0f, 0f);

        m_rectTransform.sizeDelta = m_startSize;
    }

    void checkIfhitElectrode()
    {
        m_rectTransform.GetWorldCorners(worldCornerOfBrainPanel);

        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (trace1.hasFocus || trace2.hasFocus)
        {
            float perCentX = (worldClick.x - m_rectTransform.position.x) / (worldCornerOfBrainPanel[2].x - worldCornerOfBrainPanel[1].x);
            float perCentY = (worldClick.y - m_rectTransform.position.y) / -(worldCornerOfBrainPanel[3].y - worldCornerOfBrainPanel[2].y);

            float xCam2 = (brainCam.pixelRect.center.x + (perCentX * brainCam.pixelRect.width));
            float yCam2 = (brainCam.pixelRect.center.y + (perCentY * brainCam.pixelRect.height));

            Ray ray2 = brainCam.ScreenPointToRay(new Vector3(xCam2, yCam2, 0));
            //Debug.DrawRay(ray2.origin, ray2.direction * 1000, Color.red, 5);

            RaycastHit[] hits = Physics.RaycastAll(ray2);
            if (hits.Length > 0)
            {
                plot = GameObject.Find(hits[0].collider.name);
                plotWasClicked(plot);
            }
            else
            {
                plotWasClicked(null);
            }
        }
    }

    bool isOver(Vector3 mousePos)
    {
        m_rectTransform.GetWorldCorners(worldCornerOfBrainPanel);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (worldClick.x > worldCornerOfBrainPanel[1].x && worldClick.x < worldCornerOfBrainPanel[2].x
            && worldClick.y > worldCornerOfBrainPanel[3].y && worldClick.y < worldCornerOfBrainPanel[2].y)
            return true;
        else
            return false;
    }

    void checkIfPointElectrode()
    {
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float perCentX = (worldClick.x - m_rectTransform.position.x) / (worldCornerOfBrainPanel[2].x - worldCornerOfBrainPanel[1].x);
        float perCentY = (worldClick.y - m_rectTransform.position.y) / -(worldCornerOfBrainPanel[3].y - worldCornerOfBrainPanel[2].y);

        float xCam2 = (brainCam.pixelRect.center.x + (perCentX * brainCam.pixelRect.width));
        float yCam2 = (brainCam.pixelRect.center.y + (perCentY * brainCam.pixelRect.height));

        Ray ray2 = brainCam.ScreenPointToRay(new Vector3(xCam2, yCam2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray2, out hit))
        {
            elecPointer.transform.position = new Vector3(worldClick.x, worldClick.y, 0);
            elecPointerPic.SetActive(true);
            elecPointerText.text = hit.collider.name.ToUpper();
        }
        else
        {
            elecPointerPic.SetActive(false);
        }
    }
}
