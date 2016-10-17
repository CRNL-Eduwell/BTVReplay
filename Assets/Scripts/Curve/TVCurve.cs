using UnityEngine;
using System.Collections;
using System.IO;

public class TVCurve : MonoBehaviour
{
    public GameObject lineRendererObject = null;

    LineRenderer lineRendererObj;
    RectTransform lineRendererRectTransObj = null;

    Vector3[] arrayDa;
    float widthOfGameObject = 0;
    float horizontalScale = 0;
    public float Gain = 1;
    public float PreviousGain = 1;
    int numberPoint = 64 * 10;
    int count = 0;
    public void init()
    {
        lineRendererRectTransObj = lineRendererObject.GetComponent<RectTransform>();
        lineRendererObj = lineRendererObject.GetComponent<LineRenderer>();

        //lineRendererRectTransObj = GameObject.Find("LRObject1").GetComponent<RectTransform>();
        //lineRendererObj = GameObject.Find("LRObject1").GetComponent<LineRenderer>();

        lineRendererObj.SetVertexCount(numberPoint);
        lineRendererObj.SetWidth(0.05f, 0.05f);

        arrayDa = new Vector3[numberPoint];
        widthOfGameObject = lineRendererRectTransObj.rect.width;
        horizontalScale = widthOfGameObject / numberPoint;

        for (int i = 0; i < numberPoint; i++)
        {
            arrayDa[i].x = ((-widthOfGameObject / 2) + 1) + i * horizontalScale;
            arrayDa[i].y = Mathf.Sin((2 * (float)3.14 * 1 * i) / numberPoint); ;
            arrayDa[i].z = 0;
        }
        lineRendererObj.SetPositions(arrayDa);
    }

    void Update()
    {
        if (lineRendererRectTransObj != null)
        {
            if (lineRendererRectTransObj.rect.width != widthOfGameObject)
            {
                updateLineRendererHorizontalScale();
            }
        }
    }

    void updateLineRendererHorizontalScale()
    {
        widthOfGameObject = lineRendererRectTransObj.rect.width;
        horizontalScale = widthOfGameObject / numberPoint;

        for (int i = 0; i < numberPoint; i++)
        {
            arrayDa[i].x = ((-widthOfGameObject / 2) + 1) + i * horizontalScale;
        }
        lineRendererObj.SetPositions(arrayDa);
    }

    public void UpdateCurveGain()
    {
        for (int i = 0; i < numberPoint; i++)
        {
            arrayDa[i].y = (arrayDa[i].y / PreviousGain) * Gain;
        }
        lineRendererObj.SetPositions(arrayDa);
    }

    public void updateDraw(double[] data, int offsetArray)
    {
        for (int i = 0; i < numberPoint; i++)
        {
            if (i + offsetArray >= 0)
            {
                arrayDa[i].y = Gain * ((float)data[i + offsetArray] * 100);
            }
            else
            {
                arrayDa[i].y = 0;
            }
        }
        lineRendererObj.SetPositions(arrayDa);
        //outputCSV(@"D:\Users\Florian\Desktop\replayout\test" + lineRendererObject+ " " + count + ".csv");
        //count++;
    }

    public void outputCSV(string outputFilePath)
    {
        StreamWriter sw = new StreamWriter(File.Create(outputFilePath));
        for (int i = 0; i < 64; i++)
        {
            sw.Write(arrayDa[i].y + ";\n");
        }
        sw.Close();
    }
    /**************************************************************/
    /*      Change The Gain of Signal to adapt to the view        */
    /**************************************************************/
    public void GainPlus()
    {
        PreviousGain = Gain;
        Gain += 1;
        UpdateCurveGain();
    }

    public void GainMinus()
    {
        if (Gain - 1 > 0)
        {
            PreviousGain = Gain;
            Gain -= 1;
            UpdateCurveGain();
        }
    }
}
