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
    // Mesh and material instances created here. Unity does not free them with their
    // GameObjects, so every brain-model switch used to leak one mesh and two materials per
    // sub-surface.
    private readonly List<Object> m_OwnedInstances = new List<Object>();
    private Material BaseMaterialTransparency = null;

    public void InitializeData(string triFilePath, string trmFilePath = "")
    {
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
            m_OwnedInstances.Add(Mesh[i]);
            Mesh[i].vertices = SurfacesGameObjects[i].Vertices.ToArray();
            Mesh[i].triangles = SurfacesGameObjects[i].TriangleIds;

            //== Create MeshRenderer : Must Do to see the object
            MeshRenderer.Add(current.AddComponent<MeshRenderer>());
            Renderer.Add(MeshRenderer[i].GetComponent<Renderer>());
            Renderer[i].enabled = true;

            //== Put Texture on Renderer. A depth-stencil material used to be instantiated here too,
            // then immediately replaced by this assignment: the brain has only ever rendered with
            // the transparency material, so the dead instance is gone and nothing looks different.
            Material transparency = Instantiate(BaseMaterialTransparency);
            m_OwnedInstances.Add(transparency);
            Renderer[i].material = transparency;

            Mesh[i].RecalculateNormals();
            current.name = "Brain " + i;
            MeshesGameObjects.Add(current);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < SurfacesGameObjects.Count; i++)
            SurfacesGameObjects[i].Dispose();
        foreach (Object instance in m_OwnedInstances)
            if (instance != null) Destroy(instance);
    }
}
