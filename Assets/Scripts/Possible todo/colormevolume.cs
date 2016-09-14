using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class colormevolume : MonoBehaviour {

//    List<Vector3> verticesObj = new List<Vector3>();          //List of X-Y-Z Coordinates : each element is one vetice (point)
//    int[] idTri = new int[3];
//    MeshFilter Meshfilter = null;
//    Mesh mesh = null;
//    MeshRenderer meshrenderer = null;
//    Renderer render = null;
//    Material[] m = new Material[1];
//    bool a = false;
//    Material c = null;

//    // Use this for initialization
//    void Start ()
//    {
//        ////== Get Mesh vertices and tri inside Meshfilter
//        //MeshFilter = gameObject.AddComponent<MeshFilter>();
//        //Mesh = MeshFilter.mesh;
//        //Mesh.vertices = surfaceData.verticesObj.ToArray();
//        //Mesh.triangles = surfaceData.idTri;

//        ////== Create MeshRenderer : Must Do to see the object
//        //MeshRenderer = gameObject.AddComponent<MeshRenderer>();
//        //Renderer = MeshRenderer.GetComponent<Renderer>();
//        //Renderer.enabled = true;

//        ////== Put Texture on Renderer
//        //materials[0] = Instantiate(BaseMaterialDepth);
//        //Renderer.materials = materials;

//        //Mesh.RecalculateNormals();
//        c = Resources.Load("Materials/SpriteGradient", typeof(Material)) as Material; //Recherche dans Assets\Ressources 

//    }

//    // Update is called once per frame
//    void Update () {
//        if (a == false)
//        {

//            //verticesObj.Add(new Vector3(0, 0, 0));
//            //verticesObj.Add(new Vector3(100, 0, 0));
//            //verticesObj.Add(new Vector3(100, 50, 0));


//            //idTri[0] = 0;
//            //idTri[1] = 1;
//            //idTri[2] = 2;

//            for (int y = 0; y <= 5; y++)
//            {
//                for (int x = 0; x <= 10 - ((10/5) * y); x++)
//                {
//                    verticesObj.Add(new Vector3(x, y));
//                }
//            }


//            int[] triangles = new int[10 * 5 * 6];
//            for (int ti = 0, vi = 0, y = 0; y < 5; y++, vi++)
//            {
//                for (int x = 0; x < 10; x++, ti += 6, vi++)
//                {
//                    triangles[ti] = vi;
//                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
//                    triangles[ti + 4] = triangles[ti + 1] = vi + 10 + 1;
//                    triangles[ti + 5] = vi + 10 + 2;
//                }
//            }

//            Meshfilter = gameObject.AddComponent<MeshFilter>();
//            mesh = Meshfilter.mesh;
//            mesh.vertices = verticesObj.ToArray();
//            //mesh.triangles = idTri;
//            mesh.triangles = triangles;

//            meshrenderer = gameObject.AddComponent<MeshRenderer>();
//            render = meshrenderer.GetComponent<Renderer>();
//            render.enabled = true;

//            m[0] = Instantiate(c);   
//            render.materials = m;

//            mesh.RecalculateNormals();
//            a = true;
//        }
//	}
}
