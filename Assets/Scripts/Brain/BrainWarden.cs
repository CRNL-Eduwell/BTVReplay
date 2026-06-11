using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using BTV.Services.EventsService;

public class BrainWarden : MonoBehaviour, IPointerClickHandler
{
    public bool IsMaxed { get; set; } //Delete me later, not used anymore except in workspace manager for workspace file, need to clean that as well

    [SerializeField] Camera brainCam = null;
    private RectTransform m_parentRectTransform = null;
    private RectTransform m_textureRectTransform = null;
    private RawImage m_rawImage = null;
    private Vector3[] m_worldCornerOfBrainPanel = new Vector3[4];
    private Window m_winTrace1 = null;
    private Window m_winTrace2 = null;
    private Trace m_curveTrace1 = null;
    private Trace m_curveTrace2 = null;

    private GameObject m_elecOptionPrefab = null;
    private GameObject m_elecOption = null;

    private int m_lastRtWidth = -1, m_lastRtHeight = -1;
    private RenderTexture m_ownedRt = null;
    private Site[] m_cachedSites = null;
    private Dictionary<string, Site> m_siteByName = null;

    private void Awake()
    {
        m_elecOptionPrefab = Resources.Load("Prefabs/Brain-ElecOptions", typeof(GameObject)) as GameObject;
        m_textureRectTransform = gameObject.GetComponent<RectTransform>();
        m_rawImage = GetComponent<RawImage>();
        m_parentRectTransform = m_textureRectTransform.transform.parent.gameObject.GetComponent<RectTransform>();

        Messenger.Default.Register<VideoToModulesMessage>(this, OnVideoToModulesMessage, MessageContext.VideoToModulesMessage);
    }

    private void Start()
    {
        m_winTrace1 = GameObject.Find("Trace1Window").GetComponent<Window>();
        m_winTrace2 = GameObject.Find("Trace2Window").GetComponent<Window>();
        m_curveTrace1 = m_winTrace1.gameObject.GetComponent<Trace>();
        m_curveTrace2 = m_winTrace2.gameObject.GetComponent<Trace>();
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.VideoToModulesMessage);
    }

    private void Update()
    {
        if (m_textureRectTransform.hasChanged)
        {
            int w = (int)m_textureRectTransform.rect.width;
            int h = (int)m_textureRectTransform.rect.height;
            // Only rebuild when the size actually changed (hasChanged also fires on moves), and
            // destroy the previous RenderTexture - it was only Released before, so the objects
            // accumulated while dragging the window.
            if (w > 0 && h > 0 && (w != m_lastRtWidth || h != m_lastRtHeight))
            {
                RenderTexture renderTexture = new RenderTexture(w, h, 24);
                renderTexture.antiAliasing = 1;

                brainCam.targetTexture = renderTexture;
                brainCam.aspect = (float)w / h;
                m_rawImage.texture = renderTexture;

                // Only free RenderTextures we created here. The camera's initial targetTexture
                // is a scene asset, and Destroy() on an asset throws "Destroying assets is not
                // permitted" - so we track and release only our own runtime instances.
                if (m_ownedRt != null) { m_ownedRt.Release(); Destroy(m_ownedRt); }
                m_ownedRt = renderTexture;

                m_lastRtWidth = w;
                m_lastRtHeight = h;
            }
            m_textureRectTransform.hasChanged = false;
        }
    }

    private void OnGUI()
    {
        if (IsOver(Input.mousePosition) && !(m_curveTrace1.IsMouseOver || m_curveTrace2.IsMouseOver))
        {
            brainCam.GetComponent<BrainCamera>().IsMouseOver = true;
            CheckIfPointElectrode();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (eventData.clickCount == 1 && (m_winTrace1.hasFocus || m_winTrace2.hasFocus))
                CheckIfhitElectrode();
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Ray ray2 = RaycastOnBrainPannel();
            RaycastHit[] hits = Physics.RaycastAll(ray2);
            if (hits.Length > 0 && m_elecOption == null)
            {
                m_elecOption = Instantiate(m_elecOptionPrefab);
                m_elecOption.transform.SetParent(transform);
                m_elecOption.transform.localScale = new Vector3(1, 1, 1);

                Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                m_elecOption.transform.position = new Vector3(worldClick.x, worldClick.y, 0);

                GameObject plotClick = GameObject.Find(hits[0].collider.name);
                m_elecOption.GetComponent<ElecOptions>().init(plotClick);
            }
        }
    }

    private void OnVideoToModulesMessage(VideoToModulesMessage message)
    {
        int timeInMilliseconds = (int)message.TimeMilliseconds;
        UpdateEventsOnBrain(timeInMilliseconds);
    }

    private void CheckIfhitElectrode()
    {
        Ray ray2 = RaycastOnBrainPannel();
        RaycastHit[] hits = Physics.RaycastAll(ray2);
        GameObject hitObject = hits.Length > 0 ? GameObject.Find(hits[0].collider.name) : null;
        BrainWardenToTraceMessage message = new BrainWardenToTraceMessage
        {
            TaskToExecute = 0,
            ClickedElectrode = hitObject
        };
        Messenger.Default.Send(message, MessageContext.BrainWardenToTraceMessage);
    }

    /// <summary>
    /// Send a ray from the camera to the clicked Point on the pannel displaying the brain view to the 3D objet
    /// </summary>
    /// <returns>The created Ray</returns>
    private Ray RaycastOnBrainPannel()
    {
        m_textureRectTransform.GetWorldCorners(m_worldCornerOfBrainPanel);

        //Debug.DrawRay(ray2.origin, ray2.direction * 1000, Color.red, 5);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float perCentX = (worldClick.x - m_textureRectTransform.position.x) / (m_worldCornerOfBrainPanel[2].x - m_worldCornerOfBrainPanel[1].x);
        float perCentY = (worldClick.y - m_textureRectTransform.position.y) / -(m_worldCornerOfBrainPanel[3].y - m_worldCornerOfBrainPanel[2].y);

        float xCam2 = (brainCam.pixelRect.center.x + (perCentX * brainCam.pixelRect.width));
        float yCam2 = (brainCam.pixelRect.center.y + (perCentY * brainCam.pixelRect.height));

        return brainCam.ScreenPointToRay(new Vector3(xCam2, yCam2, 0));
    }

    private bool IsOver(Vector3 mousePos)
    {
        m_textureRectTransform.GetWorldCorners(m_worldCornerOfBrainPanel);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(mousePos);

        if (worldClick.x > m_worldCornerOfBrainPanel[1].x && worldClick.x < m_worldCornerOfBrainPanel[2].x
            && worldClick.y > m_worldCornerOfBrainPanel[3].y && worldClick.y < m_worldCornerOfBrainPanel[2].y)
            return true;
        else
            return false;
    }

    private void CheckIfPointElectrode()
    {
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ray ray2 = RaycastOnBrainPannel();
        if (Physics.Raycast(ray2, out RaycastHit hit))
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
            TraceOption opt = TracesService.GetOptionsFor(0);
            int left = milliSecToLook  - (opt.WindowInSeconds * 1000);
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
                    int id = opt.ElectrodeID;
                    for (int j = 0; j < opt.FileHandle.NumberOfElectrodes; j++)
                    {
                        string ElectrodeName = opt.FileHandle.GetElectrodeNameFromElectrodeID(j);
                        Color NewColor = GetCorrelationColor(EventsService.Events[indexes[i]].Correlation2D[id][j]);
                        ChangeElectrodesColor(ElectrodeName, NewColor);
                    }
                }
                else if (EventsService.Events[indexes[i]].Correlation != null)
                {
                    for (int j = 0; j < opt.FileHandle.NumberOfElectrodes; j++)
                    {
                        string ElectrodeName = opt.FileHandle.GetElectrodeNameFromElectrodeID(j);
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

    /// <summary>
    /// Caches the electrode Site list + a name lookup. Built lazily on first use (electrodes are
    /// loaded asynchronously after Awake, and playback - which drives recolouring - only starts
    /// once a subject is loaded). The scene is reloaded per subject, so the cache lives one
    /// session and does not need invalidation.
    /// </summary>
    private void EnsureSiteCache()
    {
        if (m_cachedSites != null) return;
        GameObject electrodes = GameObject.Find("Electrodes");
        if (electrodes == null) return;
        m_cachedSites = electrodes.GetComponentsInChildren<Site>();
        m_siteByName = new Dictionary<string, Site>();
        foreach (Site s in m_cachedSites)
        {
            string key = s.gameObject.name.ToUpper();
            if (!m_siteByName.ContainsKey(key)) m_siteByName[key] = s;
        }
    }

    private void ChangeElectrodesColor(string Name, Color NewColor)
    {
        EnsureSiteCache();
        if (m_cachedSites == null) return;

        if (Name == "")
        {
            foreach (Site electrode in m_cachedSites)
                electrode.Color = NewColor;
        }
        else if (m_siteByName.TryGetValue(Name.ToUpper(), out Site electrode) && electrode != null)
        {
            electrode.Color = NewColor;
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
