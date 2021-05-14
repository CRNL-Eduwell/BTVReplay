using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BTV.Services.CalculationService;
using UnityEngine;
using UnityEngine.UI;

public class DataProjectionService : MonoBehaviour
{
    public RawImage EegMap { get; private set; }
    public List<Site> Electrodes { get; set; } = new List<Site>();

    private float[] m_interpolatedData = null;
    private float[] m_eegPhysicalValues = null;
    private float[] m_coordinates_X = null;
    private float[] m_coordinates_Y = null;
    private float[] m_coordinates_Z = null;
    private MapGenerator m_mapGenerator = null;

    private CustomVideoPlayer m_Video = null;
    //Resolution = 201;
    //InterpolerOrderM = 2;

    private void Awake()
    {
        m_Video = GameObject.FindObjectOfType<CustomVideoPlayer>();
    }

    private void Update()
    {
        if (m_Video == null) return;
        if (Electrodes.Count == 0) return;

        if (m_Video.VideoInterface.IsPlaying)
        {
            if (m_eegPhysicalValues == null || (m_eegPhysicalValues.Length != Electrodes.Count)) m_eegPhysicalValues = new float[Electrodes.Count];

            int count = 0;
            foreach (Site site in Electrodes)
            {
                m_eegPhysicalValues[count] = site.EegValue;
                count++;
            }

            //ProcessSphericalSpline(m_eegPhysicalValues);
            ProcessKnn(m_eegPhysicalValues);
        }
    }

    private void ProcessKnn(float[] eegPhysicalValues)
    {
        //Get3D coordinates of sites
        if (m_coordinates_X == null || (m_coordinates_X.Length != Electrodes.Count)) m_coordinates_X = new float[Electrodes.Count];
        if (m_coordinates_Y == null || (m_coordinates_Y.Length != Electrodes.Count)) m_coordinates_Y = new float[Electrodes.Count];
        if (m_coordinates_Z == null || (m_coordinates_Z.Length != Electrodes.Count)) m_coordinates_Z = new float[Electrodes.Count];

        int count = 0;
        foreach (Site site in Electrodes)
        {
            m_coordinates_X[count] = -site.Coordinates.x;
            m_coordinates_Y[count] = site.Coordinates.y;
            m_coordinates_Z[count] = site.Coordinates.z;

            //Carthesien to spherical
            float r = Mathf.Sqrt((m_coordinates_X[count] * m_coordinates_X[count]) + (m_coordinates_Y[count] * m_coordinates_Y[count]) + (m_coordinates_Z[count] * m_coordinates_Z[count]));

            //Check values if we find the start values (debug)
            //float theta_latt_rad = Mathf.Acos(m_coordinates_Z[count] / r);
            //float phi_long_rad = (float)Mathf.Atan2(m_coordinates_Y[count], m_coordinates_X[count]);
            //float theta_latt_degree = (theta_latt_rad * (180f / Mathf.PI));
            //float phi_long_degree = (phi_long_rad * (180f / Mathf.PI));
            //if (phi_long_degree >= 360) { phi_long_degree -= 360; }
            //else if (phi_long_degree < 0) { phi_long_degree += 360; }

            float theta = (float)Math.Acos(m_coordinates_Z[count] / r);
            if (Math.Abs(theta) < 1.0e-16)
            {
                m_coordinates_Y[count] = 0.0f;
                m_coordinates_X[count] = 0.0f;
            }
            else
            {
                m_coordinates_Y[count] = ((theta / Mathf.Sin(theta)) * (m_coordinates_Y[count]/r));
                m_coordinates_X[count] = ((theta / Mathf.Sin(theta)) * (m_coordinates_X[count]/r));
            }

            float cc = r + (r * (2.0f / Mathf.PI) * m_coordinates_X[count]);
            float rr = r - (r * (2.0f / Mathf.PI) * m_coordinates_Y[count]);

            m_coordinates_X[count] = cc;
            m_coordinates_Y[count] = rr;

            count++;
        }

        //check if data array is allocated and/or if the size is correct
        if (m_interpolatedData == null || (m_interpolatedData.Length != 201*201)) //(hardcoded resolution for now, will add option at a later point)
        {
            m_interpolatedData = new float[201 * 201];
        }

        //Process Interpolation
        CalculationService.Knn(m_interpolatedData, m_coordinates_X, m_coordinates_Y, Electrodes.Count, eegPhysicalValues, 201, 2, 10);

        //Create texture
        if (m_mapGenerator == null)
        {
            GetComponent<MeshFilter>().sharedMesh.uv = GetComponent<HalfSphere>().TextureCoordinates.ToArray();
            m_mapGenerator = new MapGenerator(201, 201, GetComponent<MeshRenderer>().material);
            //m_mapGenerator = new MapGenerator(201, 201, EegMap);
        }

        //assign texture
        m_mapGenerator.CreateMap(m_interpolatedData);
        //UnityEngine.Debug.Log("Text");
    }

    private void ProcessSphericalSpline(float[] eegPhysicalValues)
    {
        //Get3D coordinates of sites
        if (m_coordinates_X == null || (m_coordinates_X.Length != Electrodes.Count)) m_coordinates_X = new float[Electrodes.Count];
        if (m_coordinates_Y == null || (m_coordinates_Y.Length != Electrodes.Count)) m_coordinates_Y = new float[Electrodes.Count];
        if (m_coordinates_Z == null || (m_coordinates_Z.Length != Electrodes.Count)) m_coordinates_Z = new float[Electrodes.Count];

        int count = 0;
        foreach(Site site in Electrodes)
        {
            m_coordinates_X[count] = site.Coordinates.x;
            m_coordinates_Y[count] = site.Coordinates.y;
            m_coordinates_Z[count] = site.Coordinates.z;
            count++;
        }

        //check if data array is allocated and/or if the size is correct
        if (m_interpolatedData == null || (m_interpolatedData.Length != 100*100)) //(hardcoded resolution for now, will add option at a later point)
            m_interpolatedData = new float[100*100];

        //Process Interpolation
        CalculationService.SphericalSpline(m_interpolatedData, m_coordinates_X, m_coordinates_Y, m_coordinates_Z, Electrodes.Count, eegPhysicalValues, 100, 2);

        //Create texture
        //if (m_mapGenerator == null) m_mapGenerator = new MapGenerator(201, 201);

        //assign texture
        //m_mapGenerator.CreateMap(ref eegPhysicalValues);
        //EegMap.texture = m_mapGenerator.CreateMap(ref eegPhysicalValues);

        UnityEngine.Debug.Log("Text");
    }
}
