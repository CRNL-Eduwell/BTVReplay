using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfSphere : MonoBehaviour
{
    public List<Vector3> Positions { get; set; } = new List<Vector3>();
    public List<Vector2> TextureCoordinates { get; set; } = new List<Vector2>();
    public List<int> TriangleIndexes { get; set; } = new List<int>();

    public void InitSphere()
    {
        CreateSphere(100, 0, 0, 0, 80, 120);

        MeshFilter mf = gameObject.AddComponent<MeshFilter>();

        mf.mesh.vertices = Positions.ToArray();
        mf.mesh.triangles = TriangleIndexes.ToArray();

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.enabled = true;

        //== Put Texture on Renderer
        Material BaseMaterialTransparency = Resources.Load("Materials/BrainRenderMaterial", typeof(Material)) as Material;
        Material[] mats = new Material[] { Instantiate(BaseMaterialTransparency) };
        mr.materials = mats;

        mf.mesh.RecalculateNormals();
    }

    private void CreateSphere(double radius, double cx, double cy, double cz, int num_phi, int num_theta)
    {
        double dphi = Math.PI / num_phi;
        double dtheta = 2 * Math.PI / num_theta;

        // Remember the first point.
        int pt0 = Positions.Count;

        // Make the points.
        double phi1 = Math.PI / 2;
        for (int p = 0; p <= num_phi; p++)
        {
            double r1 = radius * Math.Cos(phi1);
            double y1 = radius * Math.Sin(phi1);

            double theta = 0;
            for (int t = 0; t <= num_theta / 2; t++)
            {
                float xOnSphere = (float)(cx + r1 * Math.Cos(theta));
                float yOnSphere = (float)(cy + y1);
                float zOnSphere = (float)(cz + -r1 * Math.Sin(theta));

                Positions.Add(new Vector3(-xOnSphere, yOnSphere, -zOnSphere)); //put sphere in the correct direction according to unity referential

                // Derived Latitude from z
                double lat = Math.Abs(Math.Asin(Math.Sin(phi1) * Math.Sin(theta)));
                // Express this as distance from pole along surface
                double dist = 1 - lat / Math.PI * 2;
                // Where this projects on to x axis (for y = 0):
                double xProj = Math.Cos(lat) < 0.00001 ? 0 : Math.Cos(lat);
                // Ratio of dist to xProj gives a multiplier to compensate
                // for texture getting stretched near equator
                float ratio = (xProj == 0) ? 1 : (float)(dist / xProj);

                double xOnBitMap = ((Math.Cos(phi1) * Math.Cos(theta) * ratio) + 1) / 2;
                double yOnBitMap = ((Math.Sin(phi1) * ratio) + 1) / 2;

                TextureCoordinates.Add(new Vector2((float)xOnBitMap, (float)yOnBitMap));

                theta += dtheta;
            }
            phi1 -= dphi;
        }

        // Make the triangles.
        int i1, i2, i3, i4;
        for (int p = 0; p <= num_phi - 1; p++)
        {
            i1 = p * ((num_theta / 2) + 1);
            i2 = i1 + ((num_theta / 2) + 1);
            for (int t = 0; t <= (num_theta / 2) - 1; t++)
            {
                i3 = i1 + 1;
                i4 = i2 + 1;
                TriangleIndexes.Add(pt0 + i1);
                TriangleIndexes.Add(pt0 + i2);
                TriangleIndexes.Add(pt0 + i4);

                TriangleIndexes.Add(pt0 + i1);
                TriangleIndexes.Add(pt0 + i4);
                TriangleIndexes.Add(pt0 + i3);
                i1 += 1;
                i2 += 1;
            }
        }
    }

}
