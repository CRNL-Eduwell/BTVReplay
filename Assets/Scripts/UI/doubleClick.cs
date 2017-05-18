using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class doubleClick : MonoBehaviour
{
    public RectTransform rectToSize;
    public Slot slot;
    public GameObject perf;
    public GameObject courbe1;
    public GameObject courbe2;

    double doubleClickStart;
    Vector3[] worldCornerOfRectTransform = new Vector3[4];
    bool isMaxed = false;

    void Update()
    {
        if (isMouseOverObject(rectToSize, Input.mousePosition)) 
        {
            if (Input.GetMouseButtonUp(0))
            {
                CheckDoubleClick();
                if (doubleClickStart == -1)
                {
                    if (isMaxed == false)
                    {
                        //Debug.Log("Maxing");
                        slot.putBigBrain();
                        HideIfOnBrain(false);
                        isMaxed = true;
                    }
                    else
                    {
                        //Debug.Log("Demaxing");
                        slot.putSmallBrain();
                        HideIfOnBrain(true);
                        isMaxed = false;
                    }
                }
            }
        }
    }

    void HideIfOnBrain(bool showMe)
    {
        if (perf.transform.childCount > 0)
        {
            perf.SetActive(showMe);
        }
        if (courbe1.transform.childCount > 0)
        {
            courbe1.SetActive(showMe);
        }
        if (courbe2.transform.childCount > 0)
        {
            courbe2.SetActive(showMe);
        }
    }

    void CheckDoubleClick()
    {
        if ((Time.time - doubleClickStart) < 0.3f)
        {
            doubleClickStart = -1; //double click
        }
        else
        {
            doubleClickStart = Time.time;
        }
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
}
