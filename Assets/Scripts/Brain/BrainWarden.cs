using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System;
using BTV.Services.EventsService;

public class BrainWarden : MonoBehaviour, IPointerClickHandler
{
    public bool IsMaxed
    {
        get
        {
            return m_IsMaxed;
        }
        set
        {
            m_IsMaxed = value;
            if (m_IsMaxed)
                BigBrain();
            else
                SmallBrain();
        }
    }

    [SerializeField] Camera brainCam = null;
    private bool m_IsMaxed = false;
    RectTransform m_rectTransform = null;
    Vector2 m_startSize, m_BigSize;
    Vector3[] worldCornerOfBrainPanel = new Vector3[4];
    Window winTrace1 = null;
    Window winTrace2 = null;
    Trace curveTrace1 = null;

    GameObject elecOptionPanel = null;
    GameObject ElecOption = null;

    private void Awake()
    {
        Messenger.Default.Register<VideoToModulesMessage>(this, OnVideoToModulesMessage, MessageContext.VideoToModulesMessage);
    }

    private void Start()
    {
        elecOptionPanel = Resources.Load("Prefabs/Brain-ElecOptions", typeof(GameObject)) as GameObject;

        m_rectTransform = gameObject.GetComponent<RectTransform>();
        m_startSize = m_rectTransform.sizeDelta;
        m_BigSize = m_startSize * 2;

        winTrace1 = GameObject.Find("Trace1Window").GetComponent<Window>();
        winTrace2 = GameObject.Find("Trace2Window").GetComponent<Window>();
        curveTrace1 = winTrace1.gameObject.GetComponent<Trace>();
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.VideoToModulesMessage);
    }

    private void OnGUI()
    {
        if (IsOver(Input.mousePosition))
        {
            brainCam.GetComponent<BrainCamera>().IsMouseOver = true;
            CheckIfPointElectrode();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (eventData.clickCount == 2)
            {
                IsMaxed = !IsMaxed;
            }

            if (eventData.clickCount == 1 && (winTrace1.hasFocus || winTrace2.hasFocus))
                CheckIfhitElectrode();
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

    private void OnVideoToModulesMessage(VideoToModulesMessage message)
    {
        int timeInMilliseconds = (int)message.TimeMilliseconds;
        UpdateEventsOnBrain(timeInMilliseconds);
    }

    private void BigBrain()
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

    private void SmallBrain()
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

    private void CheckIfhitElectrode()
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
            BrainWardenToTraceMessage message = new BrainWardenToTraceMessage
            {
                TaskToExecute = 0,
                ClickedElectrode = GameObject.Find(hits[0].collider.name)
            };
            Messenger.Default.Send(message, MessageContext.BrainWardenToTraceMessage);
        }
        else
        {
            BrainWardenToTraceMessage message = new BrainWardenToTraceMessage
            {
                TaskToExecute = 0,
                ClickedElectrode = null
            };
            Messenger.Default.Send(message, MessageContext.BrainWardenToTraceMessage);
        }
    }

    private bool IsOver(Vector3 mousePos)
    {
        m_rectTransform.GetWorldCorners(worldCornerOfBrainPanel);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (worldClick.x > worldCornerOfBrainPanel[1].x && worldClick.x < worldCornerOfBrainPanel[2].x
            && worldClick.y > worldCornerOfBrainPanel[3].y && worldClick.y < worldCornerOfBrainPanel[2].y)
            return true;
        else
            return false;
    }

    private void CheckIfPointElectrode()
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
            BrainWardenToElectrodePointerMessage message = new BrainWardenToElectrodePointerMessage
            {
                TaskToExecute = 0,
                PointerPosition = new Vector3(worldClick.x, worldClick.y, 0),
                ShowPointer = true,
                ElectrodeLabel = hit.collider.name
            };
            Messenger.Default.Send(message, MessageContext.BrainWardenToElectrodePointerMessage);
        }
        else
        {
            BrainWardenToElectrodePointerMessage message = new BrainWardenToElectrodePointerMessage
            {
                TaskToExecute = 1,
                ShowPointer = false
            };
            Messenger.Default.Send(message, MessageContext.BrainWardenToElectrodePointerMessage);
        }
    }

    private void UpdateEventsOnBrain(int milliSecToLook)
    {
        int EventCount = EventsService.Events.Count;
        if (EventCount > 0)
        {
            int left = milliSecToLook  - (curveTrace1.TraceEeg.PeriodInSeconds * 1000);
            int right = milliSecToLook;

            List<int> idOverFlow = EventsService.GetEventIdsBiggerThanWindow(left, right);
            List<int> idRightEnter = EventsService.GetEventIdsEnteringWindow(left, right);
            //Union joins and delete duplicates
            List<int> indexes = idOverFlow.Union(idRightEnter).ToList();

            ChangeElectrodesColor("", Color.white);
            for (int i = 0; i < indexes.Count; i++)
            {
                if (EventsService.Events[indexes[i]].Correlation2D != null)
                {
                    int id = curveTrace1.TraceEeg.ElectrodeID;
                    for (int j = 0; j < curveTrace1.TraceEeg.FileHandle.NumberOfElectrodes; j++)
                    {
                        string ElectrodeName = curveTrace1.TraceEeg.FileHandle.GetElectrodeNameFromElectrodeID(j);
                        Color NewColor = GetCorrelationColor(EventsService.Events[indexes[i]].Correlation2D[id][j]);
                        ChangeElectrodesColor(ElectrodeName, NewColor);
                    }
                }
                else if (EventsService.Events[indexes[i]].Correlation != null)
                {
                    for (int j = 0; j < curveTrace1.TraceEeg.FileHandle.NumberOfElectrodes; j++)
                    {
                        string ElectrodeName = curveTrace1.TraceEeg.FileHandle.GetElectrodeNameFromElectrodeID(j);
                        Color NewColor = GetCorrelationColor(EventsService.Events[indexes[i]].Correlation[j]);
                        ChangeElectrodesColor(ElectrodeName, NewColor);
                    }
                }
                else
                {
                    string FirstElectrodeName = EventsService.Events[indexes[i]].SiteOfInterest;
                    ChangeElectrodesColor(FirstElectrodeName, Color.red);
                    string SecondElectrodeName = EventsService.Events[indexes[i]].SecondSiteOfInterest;
                    ChangeElectrodesColor(SecondElectrodeName, Color.blue);
                }
            }
        }
        else
        {
            ChangeElectrodesColor("", Color.white);
        }
    }

    private void ChangeElectrodesColor(string Name, Color NewColor)
    {
        Site[] Electrodes = GameObject.Find("Electrodes").gameObject.GetComponentsInChildren<Site>();
        if (Name == "")
        {
            foreach (Site electrode in Electrodes)
            {
                electrode.Color = NewColor;
            }
        }
        else
        {
            Site Electrode = Array.Find(Electrodes, x => x.gameObject.name.ToUpper() == Name);
            if (Electrode != null)
            {
                Electrode.Color = NewColor;
            }
        }
    }

    private Color GetCorrelationColor(float value)
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
