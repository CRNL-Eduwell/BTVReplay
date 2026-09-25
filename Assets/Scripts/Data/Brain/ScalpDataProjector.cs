using System;
using System.Collections;
using System.Collections.Generic;
using BTV.Services.CalculationService;
using UnityEngine;

public class ScalpDataProjector : MonoBehaviour
{
    private HalfSphere m_HalfSphere = null;
    private MeshFilter m_MeshFilter = null;
    private MeshRenderer m_MeshRenderer = null;

    private List<Site> m_Electrodes = null;
    private float[] m_EegPhysicalValues = null;
    private float[] m_Coordinates_X = null;
    private float[] m_Coordinates_Y = null;
    private float[] m_Coordinates_Z = null;
    private float[] m_InterpolatedData = null;

    private MapGenerator m_MapGenerator = null;

    private CustomVideoPlayer m_Video = null;

    private void Awake()
    {
        m_HalfSphere = GetComponent<HalfSphere>();
        m_MeshFilter = GetComponent<MeshFilter>();
        m_MeshRenderer = GetComponent<MeshRenderer>();
        m_Video = FindAnyObjectByType<CustomVideoPlayer>();

        //check if data array is allocated and/or if the size is correct
        if (m_InterpolatedData == null || (m_InterpolatedData.Length != 201 * 201)) //(hardcoded resolution for now, will add option at a later point)
        {
            m_InterpolatedData = new float[201 * 201];
        }

        //Create texture
        if (m_MapGenerator == null)
        {
            m_MeshFilter.sharedMesh.uv = m_HalfSphere.TextureCoordinates.ToArray();
            m_MapGenerator = new MapGenerator(201, 201, m_MeshRenderer.material);
        }

        m_Electrodes = new List<Site>();
    }

    private void Update()
    {
        if (m_Video == null) return;
        if (m_Electrodes.Count == 0) return;

        if (m_Video.VideoInterface.IsPlaying)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Update Site Values");
            UpdateSiteValues();
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("Process Knn");
            CalculationService.Knn(m_InterpolatedData, m_Coordinates_X, m_Coordinates_Y, m_Electrodes.Count, m_EegPhysicalValues, 201, 2, 10);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("Generate map");
            m_MapGenerator.CreateMap(m_InterpolatedData);
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public void AddSite(Site site)
    {
        m_Electrodes.Add(site);
    }

    public void InitArrays()
    {
        if (m_EegPhysicalValues == null || (m_EegPhysicalValues.Length != m_Electrodes.Count)) m_EegPhysicalValues = new float[m_Electrodes.Count];
        if (m_Coordinates_X == null || (m_Coordinates_X.Length != m_Electrodes.Count)) m_Coordinates_X = new float[m_Electrodes.Count];
        if (m_Coordinates_Y == null || (m_Coordinates_Y.Length != m_Electrodes.Count)) m_Coordinates_Y = new float[m_Electrodes.Count];
        if (m_Coordinates_Z == null || (m_Coordinates_Z.Length != m_Electrodes.Count)) m_Coordinates_Z = new float[m_Electrodes.Count];

        int count = 0;
        foreach (Site site in m_Electrodes)
        {
            Vector3 coord = site.Texture2dCoordinates_Normal;
            m_Coordinates_X[count] = coord.x;
            m_Coordinates_Y[count] = coord.y;
            count++;
        }
    }

    private void UpdateSiteValues()
    {
        if (m_EegPhysicalValues == null || (m_EegPhysicalValues.Length != m_Electrodes.Count))
        {
            m_EegPhysicalValues = new float[m_Electrodes.Count];
        }

        //Debug values for John_Brown test case , scalp eeg file
        //m_EegPhysicalValues = new float[] { 112.61f, 26.1f, - 7.3f, - 3.67f, 62.22f, 331.75f, 19.79f, 0.42f, - 6.19f, - 6.83f, 42f, 269.2f, 11f, - 48.55f, 18.52f, 50.13f, 3.01f, 193.3f, 22.71f, - 11.57f, -11.73f };

        int count = 0;
        foreach (Site site in m_Electrodes)
        {
            m_EegPhysicalValues[count] = site.EegValue;
            count++;
        }
    }
}
