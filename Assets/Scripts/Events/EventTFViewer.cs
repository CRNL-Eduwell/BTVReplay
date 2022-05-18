using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EventTFViewer : MonoBehaviour
{
    private RawImage m_Image = null;
    private float[][] m_TfData = null;
    private Color[] m_ColorJetMap = null;
    private bool m_HasDataToDisplay = false;
    private BTV.Data.BtvEvent m_BtvEvent = null;
    private float m_begMemory = -1;
    private float m_endMemory = -1;

    // Start is called before the first frame update
    private void Awake()
    {
        m_Image = transform.GetComponent<RawImage>(); //surement raw image pour tex
        m_ColorJetMap = DefineColorMap();
    }

    public void UpdateTfMap(int LeftTimekInMs, int RightTimeInMs)
    {
        if (!m_HasDataToDisplay) return;

        float samplingFreq = TracesService.SamplingFrequency(0); //TODO : pass information of the parent window because if two different files it will bug

        float leftClockInSample = (LeftTimekInMs * samplingFreq) / 1000;
        float rightClockInSample = (RightTimeInMs * samplingFreq) / 1000;
        //
        float begInSample = m_BtvEvent.TimeInSeconds * samplingFreq;
        float endInSample = begInSample + ((m_BtvEvent.Duration * samplingFreq) / 1000);
        //
        float beg = leftClockInSample - begInSample < 0 ? 0 : leftClockInSample - begInSample;
        float end = rightClockInSample - endInSample < 0 ? (rightClockInSample - begInSample) : (endInSample - begInSample);

        if (beg != m_begMemory || end != m_endMemory)
        {
            m_begMemory = beg;
            m_endMemory = end;

            int begI = Mathf.RoundToInt((beg / 64) * (64 / 32));
            int endI = Mathf.RoundToInt((end / 64) * (64 / 32)) - 1;

            bool enterInWindow = (begI == 0 && endI <= 0);
            bool cameOutOfWindow = (begI >= m_TfData.Length) && (endI >= m_TfData.Length);
            bool isInsideWindow = (begI >= 0) && (endI <= m_TfData.Length);
            if (isInsideWindow && !cameOutOfWindow && !enterInWindow)
            {
                m_Image.texture = EegData2Colors(m_TfData, begI, endI, m_ColorJetMap);
            }
        }
    }

    //TODO : Myabe put a lock when doing that in case the tf data is displayed
    public void SetTfData(float[][] data, BTV.Data.BtvEvent btvEvent)
    {
        m_HasDataToDisplay = false;
        UnityEngine.Debug.Log("Dim 0 : " + data.Length);
        UnityEngine.Debug.Log("Dim 1 : " + data[0].Length);

        m_TfData = null;
        m_TfData = new float[data.Length][];
        for (int i = 0; i < data.Length; i++)
        {
            m_TfData[i] = new float[data[i].Length];
            for (int j = 0; j < data[i].Length; j++)
            {
                m_TfData[i][j] = data[i][j];
            }
        }

        m_BtvEvent = new BTV.Data.BtvEvent(btvEvent);
        m_HasDataToDisplay = true;
    }

    public void ResetTfOptions()
    {
        m_Image.texture = null;
        m_begMemory = -1;
        m_endMemory = -1;
        m_HasDataToDisplay = false;
    }

    private Color[] DefineColorMap()
    {
        Color[] colorMap = new Color[512];

        int compteur = 0;
        for (int i = 0; i < 57; i++)
        {
            float r = 0;
            float g = 0;
            float b = 143.4375f + (i * 1.9649f);
            colorMap[i] = new Color(r, g, b);
        }

        compteur = 57;
        for (int i = 0; i < 130; i++)
        {
            float r = 0;
            float g = 0.4366f + (i * 1.9649f);
            float b = 255;
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        compteur = 187;
        for (int i = 0; i < 130; i++)
        {
            float r = 0.8733f + (i * 1.9649f);
            float g = 255;
            float b = 254.1267f - (i * 1.9649f);
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        compteur = 317;
        for (int i = 0; i < 130; i++)
        {
            float r = 255;
            float g = 253.6901f - (i * 1.9649f);
            float b = 0;
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        compteur = 447;
        for (int i = 0; i < 65; i++)
        {
            float r = 253.2534f - (i * 1.9649f);
            float g = 0;
            float b = 0;
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        return colorMap;
    }

    private Texture2D EegData2Colors(float[][] eegData, Color[] colormap)
    {
        float maxValue = 256;
        float minValue = 0;

        Texture2D cursor = new Texture2D(eegData.Length, eegData[0].Length);
        for (int l = 0; l < eegData[0].Length; l++)
        {
            for (int m = 0; m < eegData.Length; m++)
            {
                float r = (eegData[m][l] - minValue) / (maxValue - minValue);

                int col = Mathf.RoundToInt(0 + (511 * r));
                if (col < 0)
                    col = 0;
                else if (col > 511)
                    col = 511;

                cursor.SetPixel(m, l, colormap[col]);
            }
        }
        cursor.Apply();

        ////Debug texture generated
        //byte[] d = ImageConversion.EncodeToPNG(cursor);
        //File.WriteAllBytes("/Users/florian/Desktop/dd.png", d);

        return cursor;
    }

    private Texture2D EegData2Colors(float[][] eegData, int beg, int end, Color[] colormap)
    {
        float maxValue = 256;
        float minValue = 0;

        Texture2D cursor = new Texture2D((end-beg), eegData[0].Length);
        for (int l = 0; l < eegData[0].Length; l++) //x
        {
            int count = 0;
            for (int m = beg; m < end; m++) //y
            {
                float r = (eegData[m][l] - minValue) / (maxValue - minValue);

                int col = Mathf.RoundToInt(0 + (511 * r));
                if (col < 0)
                    col = 0;
                else if (col > 511)
                    col = 511;

                cursor.SetPixel(count, l, colormap[col]);
                count++;
            }
        }
        cursor.Apply();

        //Debug texture generated
        byte[] d = ImageConversion.EncodeToPNG(cursor);
        File.WriteAllBytes("/Users/florian/Desktop/dd.png", d);

        return cursor;
    }
}
