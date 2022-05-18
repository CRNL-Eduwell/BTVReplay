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

    // Start is called before the first frame update
    private void Awake()
    {
        m_Image = transform.GetComponent<RawImage>(); //surement raw image pour tex
        m_ColorJetMap = DefineColorMap();
    }

    //update
    //tant que l'update tourne c'est qu'il est visible et donc on update la matrice montrer
    //en fonction de l'affichage
    //on doit aussi gerer le passage de la souris sur la matrice de tf
    //qui nous affcihera dans un tooltip le temps pointé, la fréquence ainsi que la puissance
    private void Update()
    {
        if (!m_HasDataToDisplay) return;
    }

    //TODO : Myabe put a lock when doing that in case the tf data is displayed
    public void SetTfData(float[][] data, BTV.Data.BtvEvent btvEvent)
    {
        m_HasDataToDisplay = false;

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
        Texture2D map = EegData2Colors(m_TfData, m_ColorJetMap);
        m_Image.texture = map;

        m_HasDataToDisplay = true;
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
}
