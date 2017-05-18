using UnityEngine;

public class BrainHemi : MonoBehaviour
{
    Surface surfaceData = null;
    MeshFilter MeshFilter = null;
    Mesh Mesh = null;
    MeshRenderer MeshRenderer = null;
    Renderer Renderer = null;
    Material[] materials = new Material[1];
    Material BaseMaterialDepth = null;

    public void InitializeData(string triFilePath)
    {
        BaseMaterialDepth = Resources.Load("Materials/MaterialDepthStencil", typeof(Material)) as Material; //Recherche dans Assets\Ressources 
        surfaceData = new Surface(triFilePath);

        //== Get Mesh vertices and tri inside Meshfilter
        MeshFilter = gameObject.AddComponent<MeshFilter>();
        Mesh = MeshFilter.mesh;
        Mesh.vertices = surfaceData.verticesObj.ToArray();
        Mesh.triangles = surfaceData.idTri;

        //== Create MeshRenderer : Must Do to see the object
        MeshRenderer = gameObject.AddComponent<MeshRenderer>();
        Renderer = MeshRenderer.GetComponent<Renderer>();
        Renderer.enabled = true;

        //== Put Texture on Renderer
        materials[0] = Instantiate(BaseMaterialDepth);
        Renderer.materials = materials;

        Mesh.RecalculateNormals();
    }
}
