using UnityEngine;
using UnityEngine.UI;

public class courbeClick : MonoBehaviour
{
    public GameObject panelcourbe = null;
    public GameObject panelTelecommande = null;
    public GameObject panelOtherCourbe = null;
    public GameObject panelOtherTelecommande = null;
    public GameObject panelBrain = null;
    public GameObject ringGameObject = null;
    public MainScript2 main = null;
    public Camera camBrain = null;
    public Text elecLabel = null;

    RectTransform RectTrBrain;
    BoxCollider box = null;
    GridLayoutGroup gr = null;
    float headerWidth = 0, headerHeight = 0;
    DragHandler dr = null;

    selectRing ringScript;
    RemoteManager rm;
    bool imselected = false;
    Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    Color blue = new Color(0.6117f, 0.7058f, 0.7960f);

    // Use this for initialization
    public void init ()
    {
        box = gameObject.transform.parent.transform.gameObject.AddComponent<BoxCollider>();
        gr = gameObject.transform.parent.transform.parent.transform.GetComponent<GridLayoutGroup>();
        defineboxColiderSize(box, gr);

        RectTrBrain = panelBrain.GetComponent<RectTransform>();
        ringScript = ringGameObject.GetComponent<selectRing>();
        rm = panelTelecommande.GetComponent<RemoteManager>();
        rm.idElecHasChanged += new idElecChangedEventHandler(changeNameElectrode);

        dr = gameObject.transform.parent.transform.GetComponent<DragHandler>();
        dr.dragHasEnded += new endDragEventHandler(checkChangeGrid);
    }

    void OnDestroy()
    {
        rm.idElecHasChanged -= new idElecChangedEventHandler(changeNameElectrode);
        dr.dragHasEnded -= new endDragEventHandler(checkChangeGrid);
    }

    // Update is called once per frame
    public void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            processClick();
        }
    }

    void processClick()
    {
        GameObject plot = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        if (hits.Length > 0)
        {
            if (hits[0].collider.name == gameObject.transform.parent.name) //TV
            {
                imselected = !imselected;
                if (imselected)
                {
                    unselectOtherTV();
                    panelcourbe.GetComponent<Image>().color = orange;
                    panelTelecommande.SetActive(true);
                    string elecTosShow = main.elecList[rm.currentIdElec];
                    plot = GameObject.Find(elecTosShow.ToLower().Replace('\'', 'p'));
                    ringScript.setSelectedPlot(plot);
                }
                else
                {
                    panelTelecommande.SetActive(false);
                    panelcourbe.GetComponent<Image>().color = blue;
                    plot = null;
                    ringScript.setSelectedPlot(plot);
                }
            }
            else if (hits[0].collider.name == panelBrain.name) //Brain
            {
                if (imselected)
                {
                    plot = checkSelected();
                    if (plot != null)
                    {
                        bool canChange = plot.transform.GetComponent<MeshRenderer>().materials[0].color == Color.white;
                        if (canChange)
                        {
                            string newElec = plot.name; //.Replace('p', '\'');
                            int elecID = main.elecList.FindIndex(x => x.ToLower().Equals(newElec));
                            if (elecID != -1)
                            {
                                rm.changeIdElectrode(elecID);
                                rm.changeColorElec();
                                changeNameElectrode();
                                ringScript.setSelectedPlot(plot);
                            }
                        }
                    }
                }
            }
        }
    }

    void unselectOtherTV()
    {
        panelOtherCourbe.transform.GetChild(0).GetComponent<courbeClick>().imselected = false;
        if (panelOtherTelecommande.activeSelf == true)
        {
            panelOtherTelecommande.SetActive(false);
            panelOtherCourbe.GetComponent<Image>().color = blue;
        }
        ringScript.setSelectedPlot(null);
    }

    //Check if click on canvas brain, then change referentiel to hit colider on 3D object in worldspace
    GameObject checkSelected()
    {
        GameObject plot = null;
        Vector3[] worldCornerOfBrainPanel = new Vector3[4];

        RectTrBrain.GetWorldCorners(worldCornerOfBrainPanel);

        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * 10000, Color.yellow, 5);

        if (worldClick.x > worldCornerOfBrainPanel[1].x && worldClick.x < worldCornerOfBrainPanel[2].x
            && worldClick.y > worldCornerOfBrainPanel[3].y && worldClick.y < worldCornerOfBrainPanel[2].y)
        {
            float perCentX = (worldClick.x - RectTrBrain.position.x) / (worldCornerOfBrainPanel[2].x - worldCornerOfBrainPanel[1].x);
            float perCentY = (worldClick.y - RectTrBrain.position.y) / -(worldCornerOfBrainPanel[3].y - worldCornerOfBrainPanel[2].y);

            float xCam2 = (camBrain.pixelRect.center.x + (perCentX * camBrain.pixelRect.width));
            float yCam2 = (camBrain.pixelRect.center.y + (perCentY * camBrain.pixelRect.height));

            //Debug ray click
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Ray ray2 = camBrain.ScreenPointToRay(new Vector3(xCam2, yCam2, 0));
            //Debug.DrawRay(ray2.origin, ray2.direction * 1000, Color.red, 5);

            RaycastHit[] hits = Physics.RaycastAll(ray2);
            //RaycastHit hit;
            //if (Physics.Raycast(ray2, out hit))
            //{
            //    plot = GameObject.Find(hit.collider.name);
            //}
            if (hits.Length > 0)
            {
                plot = GameObject.Find(hits[0].collider.name);
            }
        }

        return plot;
    }

    public void changeNameElectrode()
    {
        elecLabel.text = main.elecList[rm.currentIdElec];
    }

    void defineboxColiderSize(BoxCollider p_box, GridLayoutGroup gr)
    {
        headerWidth = gr.cellSize.x;
        headerHeight = gr.cellSize.y;
        p_box.size = new Vector3(headerWidth, headerHeight, 0);
    }

    void checkChangeGrid()
    {
        GridLayoutGroup g = gameObject.transform.parent.transform.parent.transform.GetComponent<GridLayoutGroup>();
        if (gr != g)
        {
            gr = g;
            defineboxColiderSize(box, gr);
        }
    }
}
