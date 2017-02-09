using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class pointElectrode : MonoBehaviour
{
    public RectTransform panelBrain = null;
    public Camera camBrain = null;

    Vector3[] panelBrainWorldCorner = new Vector3[4];
    GameObject plot = null;

    void Start ()
    {
	
	}
	
	void Update ()
    {
        if (checkSelected())
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            gameObject.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            gameObject.transform.GetChild(0).GetComponentInChildren<Text>().text = plot.name.ToUpper();
        }
        else
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    bool checkSelected()
    {
        float perCentX = 0, perCentY = 0;

        panelBrain.GetWorldCorners(panelBrainWorldCorner);

        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (worldClick.x > panelBrainWorldCorner[1].x && worldClick.x < panelBrainWorldCorner[2].x
            && worldClick.y > panelBrainWorldCorner[3].y && worldClick.y < panelBrainWorldCorner[2].y)
        {
            perCentX = (worldClick.x - panelBrain.position.x) / (panelBrainWorldCorner[2].x - panelBrainWorldCorner[1].x);
            perCentY = (worldClick.y - panelBrain.position.y) / -(panelBrainWorldCorner[3].y - panelBrainWorldCorner[2].y);

            float xCam2 = (camBrain.pixelRect.center.x + (perCentX * camBrain.pixelRect.width));
            float yCam2 = (camBrain.pixelRect.center.y + (perCentY * camBrain.pixelRect.height));


            //Debug ray click
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawRay(ray.origin, ray.direction* 1000, Color.yellow,5);
            Ray ray2 = camBrain.ScreenPointToRay(new Vector3(xCam2, yCam2, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray2, out hit))
            {
                plot = GameObject.Find(hit.collider.name);
            }
            else
            {
                plot = null;
            }
        }

        return plot != null;
    }
}
