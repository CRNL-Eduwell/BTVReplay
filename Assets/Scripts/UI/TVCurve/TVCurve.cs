using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class TVCurve : MonoBehaviour
{
    public GameObject lineRendererObject = null;
    public GameObject zeroLinerObject = null;
    public Text elecLabel = null;
    public int idCurve = 0;
    public ELAN eHandle = null;

    public RectTransform dragableRectTransform = null;
    public GridLayoutGroup mainGrid = null;
    LineRenderer lineRenderer;
    RectTransform lineRendererRectTransform = null;
    LineRenderer lineRendererZero;
    RectTransform zeroLineRectTransform = null;

    //===Curve stuff
    Vector3[] arrayDa;
    public float widthOfGameObject = 0;
    float horizontalScale = 0;
    public float Gain = 1;
    public float PreviousGain = 1;
    int numberPoint = 64 * 10;

    //===Zero line stuff
    Vector3[] arrayDaZero;

    //===Objects
    private RemoteManager rm = null;
    private BTVMedia_New btvMedia = null;
    DragHandler dr = null;
    private MainScript2 main = null;

    void Awake()
    {
        main = GameObject.Find("Launch Func GameObject").GetComponent<MainScript2>();
        dragableRectTransform = gameObject.transform.parent.transform.GetComponent<RectTransform>();
        mainGrid = dragableRectTransform.transform.parent.transform.GetComponent<GridLayoutGroup>();

        btvMedia = GameObject.Find("Canvas").transform.GetChild(8).GetComponent<BTVMedia_New>();
        rm = GameObject.Find("Canvas").transform.GetChild(1 + idCurve).GetComponent<RemoteManager>();
        dr = gameObject.transform.parent.transform.GetComponent<DragHandler>();

        rm.gainHasChanged += new gainChangedEventHandler(UpdateCurveGain);
        rm.idFileHasChanged += new idFileChangedEventHandler(changeHandleIfExists);
        //dr.dragHasEnded += new endDragEventHandler(UpdateHScale);


        zeroLineRectTransform = zeroLinerObject.transform.GetComponent<RectTransform>();
        lineRendererZero = zeroLineRectTransform.GetComponent<LineRenderer>();
        lineRendererZero.numPositions = 2;
        lineRendererZero.startWidth = 0.04f;
        lineRendererZero.endWidth = 0.04f;
        arrayDaZero = new Vector3[2];
        arrayDaZero[0] = new Vector3(-((mainGrid.cellSize.x - 8) / 2), 0, 0);
        arrayDaZero[1] = new Vector3((mainGrid.cellSize.x - 8) / 2, 0, 0);
        lineRendererZero.SetPositions(arrayDaZero);

        lineRendererRectTransform = lineRendererObject.GetComponent<RectTransform>();
        lineRenderer = lineRendererObject.GetComponent<LineRenderer>();
        lineRenderer.numPositions = numberPoint; //Deprecated : lineRendererObj.SetVertexCount(numberPoint);
        lineRenderer.startWidth = 0.04f; //Deprecated : lineRendererObj.setWidth(0.04f, 0.04f);
        lineRenderer.endWidth = 0.04f;
        arrayDa = new Vector3[numberPoint];
        widthOfGameObject = lineRendererRectTransform.rect.width;
        horizontalScale = widthOfGameObject / numberPoint;
        for (int i = 0; i < numberPoint; i++)
        {
            arrayDa[i].x = ((-widthOfGameObject / 2) + 1) + i * horizontalScale;
            arrayDa[i].y = Mathf.Sin((2 * (float)3.14 * 1 * i) / numberPoint); ;
            arrayDa[i].z = 0;
        }
        lineRenderer.SetPositions(arrayDa);
    }

    void OnDestroy()
    {
        rm.gainHasChanged -= new gainChangedEventHandler(UpdateCurveGain);
        rm.idFileHasChanged -= new idFileChangedEventHandler(changeHandleIfExists);
        //dr.dragHasEnded -= new endDragEventHandler(UpdateHScale);
    }

    public void init()
    {
        eHandle = returnFirstHandle();
        rm.loadElectrodeInPanel(eHandle.electList);
        elecLabel.text = eHandle.electList[rm.currentIdElec];
    }

    void OnRectTransformDimensionsChange()
    {
        if (dragableRectTransform != null)
        {
            UpdateHScale();
        }
    }

    void UpdateHScale()
    {
        if (mainGrid.cellSize.x != widthOfGameObject)
        {
            updateLineRendererHorizontalScale();
        }
    }

    void updateLineRendererHorizontalScale()
    {
        zeroLineRectTransform.sizeDelta = new Vector2(mainGrid.cellSize.x, mainGrid.cellSize.y / 2);
        arrayDaZero[0] = new Vector3(-((mainGrid.cellSize.x - 8) / 2), 0, 0);
        arrayDaZero[1] = new Vector3((mainGrid.cellSize.x - 8) / 2, 0, 0);
        lineRendererZero.SetPositions(arrayDaZero);

        widthOfGameObject = mainGrid.cellSize.x;
        horizontalScale = widthOfGameObject / numberPoint;
        for (int i = 0; i < numberPoint; i++)
        {
            arrayDa[i].x = ((-widthOfGameObject / 2) + 1) + i * horizontalScale;
        }
        lineRenderer.SetPositions(arrayDa);
    }

    void UpdateCurveGain(int oldGain, int newGain)
    {
        for (int i = 0; i < numberPoint; i++)
        {
            arrayDa[i].y = (arrayDa[i].y / oldGain) * newGain;
        }
        lineRenderer.SetPositions(arrayDa);
    }

    public void updateDraw(int sampleToLook)
    {
        int offset = (rm.currentIdElec * eHandle.nbSam);
        int posInArray = sampleToLook + offset;

        for (int i = 0; i < numberPoint; i++)
        {
            if (i + posInArray >= offset)
            {
                arrayDa[i].y = rm.Gain * eHandle.eegData[i + posInArray];
            }
            else
            {
                arrayDa[i].y = 0;
            }
        }
        lineRenderer.SetPositions(arrayDa);
    }

    void changeHandleIfExists(int newId)
    {
        eHandle = changeHandle(eHandle, newId);
    }

    ELAN changeHandle(ELAN e, int newID)
    {
        switch (newID)
        {
            case 0:
                if (btvMedia.e0 != null)
                    return btvMedia.e0;
                else
                    return e;
            case 1:
                if (btvMedia.e250 != null)
                    return btvMedia.e250;
                else
                    return e;
            case 2:
                if (btvMedia.e500 != null)
                    return btvMedia.e500;
                else
                    return e;
            case 3:
                if (btvMedia.e1000 != null)
                    return btvMedia.e1000;
                else
                    return e;
            case 4:
                if (btvMedia.e2500 != null)
                    return btvMedia.e2500;
                else
                    return e;
            case 5:
                if (btvMedia.e5000 != null)
                    return btvMedia.e5000;
                else
                    return e;
            default:
                Debug.Log("Problem when choosing elecfile handle");
                return e;
        }
    }

    ELAN returnFirstHandle()
    {
        if (btvMedia.e0 != null)
        {
            return btvMedia.e0;
        }
        else if (btvMedia.e250 != null)
        {
            return btvMedia.e250;
        }
        else if (btvMedia.e500 != null)
        {
            return btvMedia.e500;
        }
        else if (btvMedia.e1000 != null)
        {
            return btvMedia.e1000;
        }
        else if (btvMedia.e2500 != null)
        {
            return btvMedia.e2500;
        }
        else if (btvMedia.e5000 != null)
        {
            return btvMedia.e5000;
        }
        else
        {
            return null;
        }
    }

}
