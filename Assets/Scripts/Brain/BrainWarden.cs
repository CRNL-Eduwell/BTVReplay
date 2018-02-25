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
    Trace curveTrace1 = null;
    GameObject plot = null;
    GameObject elecPointer = null;
    GameObject elecPointerPic = null;
    Text elecPointerText = null;

    ElecPointer el = null;
    GameObject elecOptionPanel = null;
    GameObject ElecOption = null;

    void Start ()
    {
        elecOptionPanel = Resources.Load("Prefabs/Brain-ElecOptions", typeof(GameObject)) as GameObject;

        media.loadTrace += new initTrace(() =>
        {
            initDone = true;
            video.sendTime += new timeVideo(updateEventsOnBrain);
        });

        m_rectTransform = gameObject.GetComponent<RectTransform>();
        m_startSize = m_rectTransform.sizeDelta;
        m_BigSize = m_startSize * 2;

        winTrace1 = GameObject.Find("Trace1Window").GetComponent<Window>();
        winTrace2 = GameObject.Find("Trace2Window").GetComponent<Window>();
        curveTrace1 = GameObject.Find("Trace1Window").GetComponent<Trace>();

        el = GameObject.Find("Canvas").transform.GetChild(5).GetChild(0).GetComponent<ElecPointer>();
        elecPointer = GameObject.Find("Canvas").transform.GetChild(5).gameObject;
        elecPointerPic = elecPointer.transform.GetChild(0).gameObject;
        elecPointerText = elecPointerPic.transform.GetChild(0).GetComponent<Text>();
    }

    private void OnDestroy()
    {
        media.loadTrace -= new initTrace(() =>
        {
            initDone = true;
            video.sendTime += new timeVideo(updateEventsOnBrain);
        });

        if (initDone)
            video.sendTime -= new timeVideo(updateEventsOnBrain);
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

    public bool isOver(Vector3 mousePos)
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
            ElecPlotSize hitPlot = GameObject.Find(hit.collider.name).GetComponent<ElecPlotSize>();
            el.moveTo(worldClick.x, worldClick.y, 0);
            el.show(true);
            el.setElecLabel(hit.collider.name.ToUpper());
            el.setMarsAtlasLabel(hitPlot.MarsAtlasName);
            el.setBroadmanLabel(hitPlot.BroadmanName);
            el.setCorrdinatesLabel(hitPlot.Coordinates);
        }
        else
        {
            el.show(false);
        }
    }

    void updateEventsOnBrain(int milliSecToLook)
    {
        if (hub.eventRemote.userEvents.Length > 0)
        {
            int left =(int)(milliSecToLook * ((float)curveTrace1.TraceEeg.SamplingFrequency / 1000)) - curveTrace1.TraceEeg.numberOfPoint;
            int right = (int)(milliSecToLook * ((float)curveTrace1.TraceEeg.SamplingFrequency / 1000));

            //var keys = new List<int>(hub.eventRemote.userEvents.Keys);
            //var values = new List<TraceEvent>(hub.eventRemote.userEvents.Values);

            float factor = ((float)curveTrace1.TraceEeg.SamplingFrequency / 1000);
            List<int> idOverFlow = hub.eventRemote.userEvents.Select((item, index) => new { Item = item, Index = index })
                                         .Where(x => (x.Item.sample <= left && (x.Item.sample + (x.Item.duration * factor) >= right)))
                                         .Select(x => x.Index)
                                         .ToList();

            List<int> idRightEnter = hub.eventRemote.userEvents.Select((item, index) => new { Item = item, Index = index })
                                           .Where(x => (x.Item.sample < right && x.Item.sample > left && (x.Item.sample + (x.Item.duration * factor) >= right)))
                                           .Select(x => x.Index)
                                           .ToList();

            //Union joins and delete duplicates
            List<int> indexes = idOverFlow.Union(idRightEnter).ToList();

            changeColorEvent("", Color.white);
            //for (int i = 0; i < indexes.Count; i++)
            //{
            //    if (hub.eventRemote.userEvents[indexes[i]].correlationArray != null)
            //    {
            //        for (int j = 0; j < curveTrace1.TraceEeg.fileHandle.electrodes.Length; j++)
            //        {
            //            changeColorEvent(curveTrace1.TraceEeg.fileHandle.electrodes[j].name, correlationColor(hub.eventRemote.userEvents[indexes[i]].correlationArray[j]));
            //        }
            //    }
            //    else
            //    {
            //        changeColorEvent(hub.eventRemote.userEvents[indexes[i]].elecOfInterest, Color.red);
            //        changeColorEvent(hub.eventRemote.userEvents[indexes[i]].secondElecOfInterest, Color.blue);
            //    }
            //}
            for (int i = 0; i < indexes.Count; i++)
            {
                if (hub.eventRemote.userEvents[indexes[i]].correlation2DArray != null)
                {
                    int id = curveTrace1.TraceEeg.IdElectrode;
                    for (int j = 0; j < curveTrace1.TraceEeg.fileHandle.electrodes.Length; j++)
                    {
                        changeColorEvent(curveTrace1.TraceEeg.fileHandle.electrodes[j].name, correlationColor(hub.eventRemote.userEvents[indexes[i]].correlation2DArray[id][j]));
                    }
                }
                else if (hub.eventRemote.userEvents[indexes[i]].correlationArray != null)
                {
                    for (int j = 0; j < curveTrace1.TraceEeg.fileHandle.electrodes.Length; j++)
                    {
                        changeColorEvent(curveTrace1.TraceEeg.fileHandle.electrodes[j].name, correlationColor(hub.eventRemote.userEvents[indexes[i]].correlationArray[j]));
                    }
                }
                else
                {
                    changeColorEvent(hub.eventRemote.userEvents[indexes[i]].elecOfInterest, Color.red);
                    changeColorEvent(hub.eventRemote.userEvents[indexes[i]].secondElecOfInterest, Color.blue);
                }
            }
        }
        else
        {
            changeColorEvent("", Color.white);
        }
    }

    Color correlationColor(float value)
    {
        if (value > 0)
        {
            float r = Color.white.r * (1 - value) + Color.red.r * value;
            float g = Color.white.g * (1 - value) + Color.red.g * value;
            float b = Color.white.b * (1 - value) + Color.red.b * value;
            return new Color(r, g, b, 1);
        }
        else if (value < 0)
        {
            float absVal = Mathf.Abs(value);
            float r = Color.white.r * (1 - absVal) + Color.blue.r * absVal;
            float g = Color.white.g * (1 - absVal) + Color.blue.g * absVal;
            float b = Color.white.b * (1 - absVal) + Color.blue.b * absVal;
            return new Color(r, g, b, 1);
        }
        else
        {
            return Color.green;
        }
    }
}
