

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
    private Color[] m_TextureColors = null;
    private Texture2D m_workingTexture = null;

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
        if (m_TextureColors == null || m_TextureColors.Length != eegPhysicalValues.Length)
        {
            m_TextureColors = new Color[eegPhysicalValues.Length];
        }

        UnityEngine.Profiling.Profiler.BeginSample("MapColors");
        EegData2ColorMap(eegPhysicalValues);
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("MapColors set");
        m_workingTexture.SetPixels(m_TextureColors);
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("MapColors apply");
        m_workingTexture.Apply();
        UnityEngine.Profiling.Profiler.EndSample();

        //Debug texture generated
        //byte[] d = ImageConversion.EncodeToPNG(m_workingTexture);
        //File.WriteAllBytes("/Users/florian/Desktop/dd.png", d);
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
    /// Convert Eeg Values to their color counterpart.
    /// The colors array is a flattened 2D array, where
    /// pixels are laid out left to right, bottom to top (i.e. row after row). 
    /// (0,0) is the bootom left corner
    /// </summary>
    /// <param name="eegPhysicalValues"></param>
    /// <param name="max"></param>
    /// <param name="min"></param>
    private void EegData2ColorMap(float[] eegPhysicalValues, float max = 0.0f, float min = 0.0f)
    {
        UnityEngine.Profiling.Profiler.BeginSample("Data2ColorMap 1");
        if (max == 0.0f && min == 0.0f)
        {
            max = eegPhysicalValues.Max();
            min = eegPhysicalValues.Min();
        }
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("Data2ColorMap 2");
        int valueCount = eegPhysicalValues.Length;
        for (int i = 0; i < valueCount; ++i)
        {
            float r = (eegPhysicalValues[i] - min) / (max - min);

            int col = 0 + (int)(511 * r);

            if (col < 0)
                col = 0;
            else if (col > 511)
                col = 511;

            m_TextureColors[valueCount - 1 - i] = m_colormap[col];
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }
}