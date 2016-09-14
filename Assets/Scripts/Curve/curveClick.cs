using UnityEngine;
using UnityEngine.UI;

public class curveClick : MonoBehaviour
{
    public GameObject panelcourbe = null;
    public GameObject panelTelecommande = null;
    public GameObject DropDown = null;
    public GameObject panelOtherCourbe = null;
    public GameObject panelOtherTelecommande = null;
    public GameObject ringGameObject = null;
    public GameObject panelBrain = null;
    public Camera camBrain = null;

    public bool imselected = false;

    //===
    BoxCollider boxCurve;
    RectTransform RectTrCurve;
    RectTransform RectTrBrain;
    selectRing ringScript;
    Dropdown dropDownScript;

    //===
    Rect boxRect;
    float headerWidth, headerHeight;
    Vector3[] worldCornerOfRectTransform = new Vector3[4];
    Vector3[] worldCornerOfBrainPanel = new Vector3[4];

    // Use this for initialization
    void Start ()
    {
        boxCurve = panelcourbe.AddComponent<BoxCollider>();
        RectTrCurve = panelcourbe.GetComponent<RectTransform>();
        RectTrBrain = panelBrain.GetComponent<RectTransform>();
        ringScript = ringGameObject.GetComponent<selectRing>();
        dropDownScript = DropDown.GetComponent<Dropdown>();

        defineboxColiderSize(boxCurve);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if ((headerWidth != boxRect.width) || (headerHeight != boxRect.height))
        {
            defineboxColiderSize(boxCurve);
        }

        if (Input.GetMouseButtonUp(0))
        {
            processClick();
        }
    }

    void defineboxColiderSize(BoxCollider p_box)
    {
        boxRect = RectTrCurve.rect;
        headerWidth = boxRect.width;
        headerHeight = boxRect.height;
        p_box.size = new Vector3(headerWidth, headerHeight, 0);
    }

    void processClick()
    {
        GameObject plot = null;

        if (isMouseOverObject(RectTrCurve, Input.mousePosition)) //Click on TV
        {
            imselected = !imselected;
            if (imselected)
            {
                unselectOtherTV();
                panelTelecommande.SetActive(true);
                string elecTosShow = dropDownScript.options[dropDownScript.value].text;
                plot = GameObject.Find(elecTosShow.ToLower().Replace('\'', 'p'));
                ringScript.setSelectedPlot(plot);
            }
            else
            {
                panelTelecommande.SetActive(false);
                plot = null;
                ringScript.setSelectedPlot(plot);
            }
        }
        else if (isMouseOverObject(RectTrBrain, Input.mousePosition)) //Click on Brain
        {
            if (imselected)
            {
                if ((plot = checkSelected()) != null)
                {
                    int elecID = dropDownScript.options.FindIndex(x => x.text.ToLower().Contains(plot.name.Replace('p', '\'')));
                    if (elecID != -1)
                    {
                        dropDownScript.value = elecID;
                    }
                }
                ringScript.setSelectedPlot(plot);
            }
        }
    }

    void unselectOtherTV()
    {
        panelOtherCourbe.transform.GetChild(0).GetComponent<curveClick>().imselected = false;
        if (panelOtherTelecommande.activeSelf == true)
        {
            panelOtherTelecommande.SetActive(false);
        }
        ringScript.setSelectedPlot(null);
    }

    bool isMouseOverObject(RectTransform transformObject, Vector3 mousePosition)
    {
        transformObject.GetWorldCorners(worldCornerOfRectTransform);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(mousePosition);

        if (worldClick.x > worldCornerOfRectTransform[1].x && worldClick.x < worldCornerOfRectTransform[2].x
        && worldClick.y > worldCornerOfRectTransform[3].y && worldClick.y < worldCornerOfRectTransform[2].y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Check if click on canvas brain, then change referentiel to hit colider on 3D object in worldspace
    GameObject checkSelected()
    {
        GameObject plot = null;
        float perCentX = 0, perCentY = 0;

        RectTrBrain.GetWorldCorners(worldCornerOfBrainPanel);

        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * 10000, Color.yellow, 5);

        if (worldClick.x > worldCornerOfBrainPanel[1].x && worldClick.x < worldCornerOfBrainPanel[2].x
            && worldClick.y > worldCornerOfBrainPanel[3].y && worldClick.y < worldCornerOfBrainPanel[2].y)
        {

            perCentX = (worldClick.x - RectTrBrain.position.x) / (worldCornerOfBrainPanel[2].x - worldCornerOfBrainPanel[1].x);
            perCentY = (worldClick.y - RectTrBrain.position.y) / -(worldCornerOfBrainPanel[3].y - worldCornerOfBrainPanel[2].y);

            float xCam2 = (camBrain.pixelRect.center.x + (perCentX * camBrain.pixelRect.width));
            float yCam2 = (camBrain.pixelRect.center.y + (perCentY * camBrain.pixelRect.height));

            //Debug ray click
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Ray ray2 = camBrain.ScreenPointToRay(new Vector3(xCam2, yCam2, 0));
            //Debug.DrawRay(ray2.origin, ray2.direction * 1000, Color.red, 5);

            RaycastHit hit;

            if (Physics.Raycast(ray2, out hit))
            {
                plot = GameObject.Find(hit.collider.name);
            }
        }

        return plot;
    }

    public void showPanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
        imselected = !imselected;
    }
}
