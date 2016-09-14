using UnityEngine;
using System.Collections;

public class selectRing : MonoBehaviour
{
    private GameObject m_ring1 = null;
    private GameObject m_ring2 = null;
    private GameObject m_ring3 = null;

    private float m_speedRotation = 100f;
    private GameObject m_selectedPlot = null;

    /// <summary>
    /// This function is always called before any Start functions and also just after a prefab is instantiated. (If a GameObject is inactive during start up Awake is not called until it is made active.)
    /// </summary>
    void Awake()
    {
        GameObject Ring = Resources.Load("Prefabs/ElecPlot", typeof(GameObject)) as GameObject;

        m_ring1 = Instantiate(Ring);
        m_ring1.name = "ring 1";
        m_ring1.transform.SetParent(transform);
        m_ring1.GetComponent<MeshFilter>().mesh = createTube(1.10f);
        m_ring1.SetActive(false);
        m_ring1.transform.localEulerAngles = new Vector3(90, 0, 0);

        m_ring2 = Instantiate(Ring);
        m_ring2.name = "ring 2";
        m_ring2.transform.SetParent(transform);
        m_ring2.GetComponent<MeshFilter>().mesh = createTube(0.90f);
        m_ring2.SetActive(false);
        m_ring2.transform.localEulerAngles = new Vector3(0, 90, 0);

        m_ring3 = Instantiate(Ring);
        m_ring3.name = "ring 3";
        m_ring3.transform.SetParent(transform);
        m_ring3.GetComponent<MeshFilter>().mesh = createTube(0.70f);
        m_ring3.SetActive(false);
        m_ring3.transform.localEulerAngles = new Vector3(0, 0, 90);

    }

    void Update()
    {
        if (m_selectedPlot != null)
        {
            m_ring1.transform.Rotate(new Vector3(1, 0, 0), m_speedRotation * Time.deltaTime);
            m_ring2.transform.Rotate(new Vector3(1, 0, 0), m_speedRotation * Time.deltaTime);
            m_ring3.transform.Rotate(new Vector3(1, 0, 0), m_speedRotation * Time.deltaTime);
        }
    }

    public void setSelectedPlot(GameObject plot)
    {
        m_selectedPlot = plot;
        bool notNullPlot = m_selectedPlot != null;

        if (notNullPlot)
            m_ring1.transform.position = m_ring2.transform.position = m_ring3.transform.position = m_selectedPlot.transform.position;

        m_ring1.SetActive(notNullPlot);
        m_ring2.SetActive(notNullPlot);
        m_ring3.SetActive(notNullPlot);
    }

    void rotateCircles()
    {
        m_ring1.transform.Rotate(new Vector3(1, 0, 0), m_speedRotation * Time.deltaTime);
        m_ring2.transform.Rotate(new Vector3(1, 0, 0), m_speedRotation * Time.deltaTime);
        m_ring3.transform.Rotate(new Vector3(1, 0, 0), m_speedRotation * Time.deltaTime);
    }

    Mesh createTube(float radius = 1.7f, float inter = 0.15f, float height = 0.1f, int nbSides = 60)
    {
        Mesh mesh = new Mesh();

        // Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2
        float bottomRadius1 = radius; // .5
        float bottomRadius2 = inter; // .15
        float topRadius1 = radius;
        float topRadius2 = inter;

        int nbVerticesCap = nbSides * 2 + 2;
        int nbVerticesSides = nbSides * 2 + 2;

        #region Vertices

        // bottom + top + sides
        Vector3[] vertices = new Vector3[nbVerticesCap * 2 + nbVerticesSides * 2];
        int vert = 0;
        float _2pi = Mathf.PI * 2f;

        // Bottom cap
        int sideCounter = 0;
        while (vert < nbVerticesCap)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            vertices[vert] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0f, sin * (bottomRadius1 - bottomRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0f, sin * (bottomRadius1 + bottomRadius2 * .5f));
            vert += 2;
        }

        // Top cap
        sideCounter = 0;
        while (vert < nbVerticesCap * 2)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
            vert += 2;
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);

            vertices[vert] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0, sin * (bottomRadius1 + bottomRadius2 * .5f));
            vert += 2;
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);

            vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0, sin * (bottomRadius1 - bottomRadius2 * .5f));
            vert += 2;
        }
        #endregion

        #region Normales

        // bottom + top + sides
        Vector3[] normales = new Vector3[vertices.Length];
        vert = 0;

        // Bottom cap
        while (vert < nbVerticesCap)
        {
            normales[vert++] = Vector3.down;
        }

        // Top cap
        while (vert < nbVerticesCap * 2)
        {
            normales[vert++] = Vector3.up;
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;

            normales[vert] = new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1));
            normales[vert + 1] = normales[vert];
            vert += 2;
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;

            normales[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
            normales[vert + 1] = normales[vert];
            vert += 2;
        }
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];

        vert = 0;
        // Bottom cap
        sideCounter = 0;
        while (vert < nbVerticesCap)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(0f, t);
            uvs[vert++] = new Vector2(1f, t);
        }

        // Top cap
        sideCounter = 0;
        while (vert < nbVerticesCap * 2)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(0f, t);
            uvs[vert++] = new Vector2(1f, t);
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(t, 0f);
            uvs[vert++] = new Vector2(t, 1f);
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(t, 0f);
            uvs[vert++] = new Vector2(t, 1f);
        }
        #endregion

        #region Triangles
        int nbFace = nbSides * 4;
        int nbTriangles = nbFace * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        // Bottom cap
        int i = 0;
        sideCounter = 0;
        while (sideCounter < nbSides)
        {
            int current = sideCounter * 2;
            int next = sideCounter * 2 + 2;

            triangles[i++] = next + 1;
            triangles[i++] = next;
            triangles[i++] = current;

            triangles[i++] = current + 1;
            triangles[i++] = next + 1;
            triangles[i++] = current;

            sideCounter++;
        }

        // Top cap
        while (sideCounter < nbSides * 2)
        {
            int current = sideCounter * 2 + 2;
            int next = sideCounter * 2 + 4;

            triangles[i++] = current;
            triangles[i++] = next;
            triangles[i++] = next + 1;

            triangles[i++] = current;
            triangles[i++] = next + 1;
            triangles[i++] = current + 1;

            sideCounter++;
        }

        // Sides (out)
        while (sideCounter < nbSides * 3)
        {
            int current = sideCounter * 2 + 4;
            int next = sideCounter * 2 + 6;

            triangles[i++] = current;
            triangles[i++] = next;
            triangles[i++] = next + 1;

            triangles[i++] = current;
            triangles[i++] = next + 1;
            triangles[i++] = current + 1;

            sideCounter++;
        }


        // Sides (in)
        while (sideCounter < nbSides * 4)
        {
            int current = sideCounter * 2 + 6;
            int next = sideCounter * 2 + 8;

            triangles[i++] = next + 1;
            triangles[i++] = next;
            triangles[i++] = current;

            triangles[i++] = current + 1;
            triangles[i++] = next + 1;
            triangles[i++] = current;

            sideCounter++;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

}
