using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public delegate void newPlotClicked(GameObject plot);
public delegate void changeColorPlotEvent(string namePlot, Color color);

public class BrainWarden : MonoBehaviour, IPointerClickHandler
{
    public event newPlotClicked plotWasClicked;
    public event changeColorPlotEvent changeColorEvent;

    [SerializeField] optionsHub hub = null;
    [SerializeField] Camera brainCam = null;
    [SerializeField] BTVMedia media = null;
    [SerializeField] VideoPlayer video = null;

    bool isMaxed = false;
    bool initDone = false;
    RectTransform m_rectTransform = null;
    Vector2 m_startSize, m_BigSize;
    Vector3[] worldCornerOfBrainPanel = new Vector3[4];
    Window winTrace1 = null;
    Window winTrace2 = null;
    TraceCurve curveTrace1 = null;
    GameObject plot = null;
    GameObject elecPointer = null;
    GameObject elecPointerPic = null;
    Text elecPointerText = null;

    GameObject elecOptionPanel = null;
    GameObject ElecOption = null;

    void Start ()
    {
        elecOptionPanel = Resources.Load("Prefabs/Brain-ElecOptions", typeof(GameObject)) as GameObject;

        media.mediaLoaded += new mediaLoadedEventHandler(() => initDone = true);
        video.sendTime += new timeVideo2(updateEventsOnBrain);

        m_rectTransform = gameObject.GetComponent<RectTransform>();
        m_startSize = m_rectTransform.sizeDelta;
        m_BigSize = m_startSize * 2;

        winTrace1 = GameObject.Find("Trace1Window").GetComponent<Window>();
        winTrace2 = GameObject.Find("Trace2Window").GetComponent<Window>();
        curveTrace1 = GameObject.Find("Trace1Window").GetComponent<TraceCurve>();

        elecPointer = GameObject.Find("Canvas").transform.GetChild(4).gameObject;
        elecPointerPic = elecPointer.transform.GetChild(0).gameObject;
        elecPointerText = elecPointerPic.transform.GetChild(0).GetComponent<Text>();
    }

    private void OnDestroy()
    {
        media.mediaLoaded -= new mediaLoadedEventHandler(() => initDone = true);
        video.sendTime -= new timeVideo2(updateEventsOnBrain);
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

            if (eventData.clickCount == 1 && (winTrace1.hasFocus || winTrace2.hasFocus))
                checkIfhitElectrode();
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            m_rectTransform.GetWorldCorners(worldCornerOfBrainPanel);

            Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float perCentX = (worldClick.x - m_rectTransform.position.x) / (worldCornerOfBrainPanel[2].x - worldCornerOfBrainPanel[1].x);
            float perCentY = (worldClick.y - m_rectTransform.position.y) / -(worldCornerOfBrainPanel[3].y - worldCornerOfBrainPanel[2].y);

            float xCam2 = (brainCam.pixelRect.center.x + (perCentX * brainCam.pixelRect.width));
            float yCam2 = (brainCam.pixelRect.center.y + (perCentY * brainCam.pixelRect.height));

            Ray ray2 = brainCam.ScreenPointToRay(new Vector3(xCam2, yCam2, 0));
            //Debug.DrawRay(ray2.origin, ray2.direction * 1000, Color.red, 5);

            RaycastHit[] hits = Physics.RaycastAll(ray2);
            if (hits.Length > 0 && ElecOption == null)
            {
                ElecOption = Instantiate(elecOptionPanel);
                ElecOption.transform.SetParent(transform);
                ElecOption.transform.localScale = new Vector3(1, 1, 1);
                ElecOption.transform.localPosition = new Vector3(0, 0, 0);

                GameObject plotClick = GameObject.Find(hits[0].collider.name);
                ElecOption.GetComponent<ElecOptions>().init(plotClick);
            }
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

    void updateEventsOnBrain(int sampleToLook)
    {
        if (hub.eventRemote.userEvents.Count > 0)
        {
            int left = sampleToLook - curveTrace1.numberOfPoint;
            int right = sampleToLook;

            var keys = new List<int>(hub.eventRemote.userEvents.Keys);
            var values = new List<eventEeg>(hub.eventRemote.userEvents.Values);
            List<int> currentIndex = keys.Select((item, index) => new { Item = item, Index = index })
                                                         .Where(x => x.Item > left && x.Item < right)
                                                         .Select(x => x.Index)
                                                         .ToList();

            if (currentIndex.Count > 0)
            {
                changeColorEvent("", Color.white);
                for (int i = 0; i < currentIndex.Count; i++)
                {
                    if(keys[currentIndex[i]] < right && right < keys[currentIndex[i]] + values[currentIndex[i]].duration * ((float)curveTrace1.samplingFrequency / 1000))
                    {
                        changeColorEvent(values[currentIndex[i]].elecOfInterest, Color.red);
                        changeColorEvent(values[currentIndex[i]].secondElecOfInterest, Color.blue);
                    }
                }
            }
            else
            {
                changeColorEvent("", Color.white);
            }
        }
    }
}
