using UnityEngine;
using System.Collections.Generic;

public class Hemisphere : MonoBehaviour
{
    Material BaseMaterialDepth = null;
    Material BaseMaterialTransparency = null;
    public List<Surface> surfaceData = new List<Surface>();
    List<MeshFilter> MeshFilter = new List<MeshFilter>();
    List<Mesh> Mesh = new List<Mesh>();
    List<MeshRenderer> MeshRenderer = new List<MeshRenderer>();
    List<Renderer> Renderer = new List<Renderer>();
    List<Material[]> materials = new List<Material[]>();
    public List<GameObject> brainMeshes = new List<GameObject>();

    private void OnDestroy()
    {
        for (int i = 0; i < surfaceData.Count; i++)
            surfaceData[i].Dispose();
    }

    public void InitializeData(string triFilePath)
    {
        BaseMaterialDepth = Resources.Load("Materials/Brain-DepthStencil", typeof(Material)) as Material; //Recherche dans Assets\Ressources 
        BaseMaterialTransparency = Resources.Load("Materials/Brain-TransparencyStencil", typeof(Material)) as Material;
        Surface baseSurface = new Surface(triFilePath);

        int nbSurface = ((baseSurface.verticesObj.Count * 3) / 65000) + 1;
        if (nbSurface > 1)
        {
            Surface[] array = baseSurface.split_to_surfaces(nbSurface);
            for (int i = 0; i < array.Length; i++)
                surfaceData.Add(array[i]);
        }
        else
        {
            surfaceData.Add(baseSurface);
        }

        for (int i = 0; i < nbSurface; i++)
        {
            GameObject current = new GameObject();
            //== Get Mesh vertices and tri inside Meshfilter
            MeshFilter.Add(current.AddComponent<MeshFilter>());
            Mesh.Add(MeshFilter[i].mesh);
            Mesh[i].vertices = surfaceData[i].verticesObj.ToArray();
            Mesh[i].triangles = surfaceData[i].idTri;

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
            brainMeshes.Add(current);
        }
    }
}
