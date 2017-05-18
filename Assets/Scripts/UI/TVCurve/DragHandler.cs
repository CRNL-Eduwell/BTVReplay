using UnityEngine;
using UnityEngine.EventSystems;

public delegate void endDragEventHandler();

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public event endDragEventHandler dragHasEnded;
    //===
    public static GameObject itemBeingDragged;
    Vector3 startPosition;
    Transform startParent;

    Vector3[] worldCornerOfBrainPanel = new Vector3[4];
    float xCam2 = 0, yCam2 = 0;

    public void OnBeginDrag(PointerEventData eventData)
    {
        itemBeingDragged = gameObject;
        startPosition = transform.position;
        startParent = transform.parent;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float perCentX = 0, perCentY = 0;
        RectTransform myRectTr = GameObject.Find("Canvas").transform.GetComponent<RectTransform>();
        myRectTr.GetWorldCorners(worldCornerOfBrainPanel);

        transform.SetParent(GameObject.Find("Canvas").transform);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);


        if (worldClick.x > worldCornerOfBrainPanel[1].x && worldClick.x < worldCornerOfBrainPanel[2].x
            && worldClick.y > worldCornerOfBrainPanel[3].y && worldClick.y < worldCornerOfBrainPanel[2].y)
        {
            perCentX = (worldClick.x - myRectTr.position.x) / (worldCornerOfBrainPanel[2].x - worldCornerOfBrainPanel[1].x);
            perCentY = (worldClick.y - myRectTr.position.y) / -(worldCornerOfBrainPanel[3].y - worldCornerOfBrainPanel[2].y);

            xCam2 = (perCentX * Camera.main.pixelRect.width);
            yCam2 = (perCentY * Camera.main.pixelRect.height);

            transform.localPosition = new Vector3(xCam2, yCam2, transform.localPosition.z);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        if (transform.parent == startParent)
        {
            transform.position = startPosition;
        }

        if (transform.name != "DragablePerf")
        {
            dragHasEnded();
        }
    }
}
