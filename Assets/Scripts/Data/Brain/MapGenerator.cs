

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

class MapGenerator
{
    private int m_width = 0;
    private int m_height = 0;
    private Color[] m_colormap = new Color[512];
    private RawImage m_sphereRawImage = null;
    private Texture2D m_workingTexture = null;

    public MapGenerator(int width, int height, RawImage sphereImage)
    {
        m_width = width;
        m_height = height;
        m_sphereRawImage = sphereImage;

        m_workingTexture = new Texture2D(m_width, m_height, TextureFormat.ARGB32, true, true);
        m_sphereRawImage.texture = m_workingTexture;

        JetColorMap512(ref m_colormap);
    }

    public MapGenerator(int width, int height, Material sphereMaterial)
    {
        m_width = width;
        m_height = height;

        m_workingTexture = new Texture2D(m_width, m_height, TextureFormat.ARGB32, true, true);
        sphereMaterial.mainTexture = m_workingTexture;

        JetColorMap512(ref m_colormap);
    }

    public void CreateMap(float[] eegPhysicalValues)
    {
        List<int>[] colorX = new List<int>[512];
        List<int>[] colorY = new List<int>[512];
        for (int i = 0; i < 512; i++)
        {
            colorX[i] = new List<int>();
            colorY[i] = new List<int>();
        }

        EegData2ColorMap(ref colorX, ref colorY, eegPhysicalValues);

        for (int i = 0; i < 512; i++)
        {
            Color currentCOlor = m_colormap[i];
            for (int j = 0; j < colorX[i].Count; j++)
            {
                m_workingTexture.SetPixel(colorX[i][j], colorY[i][j], currentCOlor);
            }
        }

        //Debug texture generated
        //byte[] d = ImageConversion.EncodeToPNG(m_workingTexture);
        //File.WriteAllBytes("/Users/florian/Desktop/dd.png", d);

        m_workingTexture.Apply();
    }

    /// <summary>
    /// Create Jet Color Map 
    /// </summary>
    /// <param name="colors">output array that will be used to map values to colors</param>
    private void JetColorMap512(ref Color[] colors)
    {
        int compteur = 0;
        for (int i = 0; i < 57; i++)
        {
            float blue = (143.4375f + (i * 1.9649f)) / 255;
            colors[compteur] = new Color(0, 0, blue);
            compteur++;
        }

        compteur = 57;
        for (int i = 0; i < 130; i++)
        {
            float green = (0.4366f + (i * 1.9649f)) / 255;
            float blue = 255 / 255;

            colors[compteur] = new Color(0, green, blue);
            compteur++;
        }

        compteur = 187;
        for (int i = 0; i < 130; i++)
        {
            float red = (0.8733f + (i * 1.9649f)) / 255;
            float green = 255 / 255;
            float blue = (254.1267f - (i * 1.9649f)) / 255;

            colors[compteur] = new Color(red, green, blue);
            compteur++;
        }

        compteur = 317;
        for (int i = 0; i < 130; i++)
        {
            float red = 255 / 255;
            float green = (253.6901f - (i * 1.9649f)) / 255;
            float blue = 0;

            colors[compteur] = new Color(red, green, blue);
            compteur++;
        }

        compteur = 447;
        for (int i = 0; i < 65; i++)
        {
            float red = (253.2534f - (i * 1.9649f)) / 255;
            float green = 0;
            float blue = 0;

            colors[compteur] = new Color(red, green, blue);
            compteur++;
        }
    }

    /// <summary>
    /// Convert Eeg Values to their color counterpart
    /// (0,0) is the top left corner
    /// </summary>
    /// <param name="colorX"></param>
    /// <param name="colorY"></param>
    /// <param name="eegPhysicalValues"></param>
    /// <param name="max"></param>
    /// <param name="min"></param>
    private void EegData2ColorMap(ref List<int>[] colorX, ref List<int>[] colorY, float[] eegPhysicalValues, float max = 0.0f, float min = 0.0f)
    {
        if (max == 0.0f && min == 0.0f)
        {
            max = eegPhysicalValues.Max();
            min = eegPhysicalValues.Min();
        }

        for (int i = 0; i < eegPhysicalValues.Length; i++)
        {
            float r = (eegPhysicalValues[i] - min) / (max - min);

            int col = 0 + (int)(511 * r);

            if (col < 0)
                col = 0;
            else if (col > 511)
                col = 511;

            colorX[col].Add(i % m_width);
            colorY[col].Add(m_height - (i / m_width));
        }
    }
}