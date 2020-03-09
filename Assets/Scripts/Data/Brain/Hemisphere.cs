using UnityEngine;
using System.Collections.Generic;

public class Hemisphere : MonoBehaviour
{
    public List<GameObject> MeshesGameObjects = new List<GameObject>();
    public List<Surface> SurfacesGameObjects = new List<Surface>();
    //===
    private List<MeshFilter> MeshFilter = new List<MeshFilter>();
    private List<Mesh> Mesh = new List<Mesh>();
    private List<MeshRenderer> MeshRenderer = new List<MeshRenderer>();
    private List<Renderer> Renderer = new List<Renderer>();
    private List<Material[]> materials = new List<Material[]>();
    private Material BaseMaterialDepth = null;
    private Material BaseMaterialTransparency = null;

    public void InitializeData(string triFilePath, string trmFilePath = "")
    {
        BaseMaterialDepth = Resources.Load("Materials/Brain-DepthStencil", typeof(Material)) as Material; //Recherche dans Assets\Ressources 
        BaseMaterialTransparency = Resources.Load("Materials/Brain-TransparencyStencil", typeof(Material)) as Material;
        Surface baseSurface = new Surface(triFilePath, trmFilePath);

        int nbSurface = ((baseSurface.Vertices.Count * 3) / 65000) + 1;
        if (nbSurface > 1)
        {
            Surface[] array = baseSurface.SplitToSurfaces(nbSurface);
            for (int i = 0; i < array.Length; i++)
                SurfacesGameObjects.Add(array[i]);
        }
        else
        {
            SurfacesGameObjects.Add(baseSurface);
        }

        for (int i = 0; i < nbSurface; i++)
        {
            GameObject current = new GameObject();
            //== Get Mesh vertices and tri inside Meshfilter
            MeshFilter.Add(current.AddComponent<MeshFilter>());
            Mesh.Add(MeshFilter[i].mesh);
            Mesh[i].vertices = SurfacesGameObjects[i].Vertices.ToArray();
            Mesh[i].triangles = SurfacesGameObjects[i].TriangleIds;

            //== Create MeshRenderer : Must Do to see the object
            MeshRenderer.Add(current.AddComponent<MeshRenderer>());
            Renderer.Add(MeshRenderer[i].GetComponent<Renderer>());
            Renderer[i].enabled = true;

            //== Put Texture on Renderer
            materials.Add(new Material[] { Instantiate(BaseMaterialDepth) });
            Renderer[i].materials = materials[i];
            Renderer[i].material = Instantiate(BaseMaterialTransparency);

            Mesh[i].RecalculateNormals();
            current.name = "Brain " + i;
            MeshesGameObjects.Add(current);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < SurfacesGameObjects.Count; i++)
            SurfacesGameObjects[i].Dispose();
    }
}
