using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class clickableObject : MonoBehaviour
{
    BoxCollider boxCurve = null;
    RectTransform RectTrCurve = null;
    GridLayoutGroup gr = null;
    Rect boxRect;
    float headerWidth = 0, headerHeight = 0;

    public void init()
    {
        boxCurve = gameObject.AddComponent<BoxCollider>();
        RectTrCurve = gameObject.transform.GetComponent<RectTransform>();
        defineboxColiderSize(boxCurve, RectTrCurve);
    }

    public void initCourbe()
    {
        boxCurve = gameObject.AddComponent<BoxCollider>();
        gr = gameObject.transform.parent.transform.GetComponent<GridLayoutGroup>();
        defineboxColiderSize(boxCurve, gr);
    }

    public void Update()
    {
        if ((headerWidth != boxRect.width) || (headerHeight != boxRect.height))
        {
            defineboxColiderSize(boxCurve, RectTrCurve);
        }
    }

    void defineboxColiderSize(BoxCollider p_box, RectTransform rec)
    {
        boxRect = rec.rect;
        headerWidth = boxRect.width;
        headerHeight = boxRect.height;
        p_box.size = new Vector3(headerWidth, headerHeight, 0);
    }

    void defineboxColiderSize(BoxCollider p_box, GridLayoutGroup gr)
    {
        headerWidth = gr.cellSize.x;
        headerHeight = gr.cellSize.y;
        p_box.size = new Vector3(headerWidth, headerHeight, 0);
    }
}
