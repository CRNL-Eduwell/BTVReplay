using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;   //List<T>
using System.Collections; //IEnumerator

using UnityEngine;

public class Surface : CppDLLImportBase
{
    public List<Vector3> Vertices = new List<Vector3>();          //List of X-Y-Z Coordinates : each element is one vetice (point)
    public int[] TriangleIds;                                     //List of Index number : three vertices used to make one triangle
    //===
    private string m_filePath = "";

    public Surface[] SplitToSurfaces(int nbSubSurfaces)
    {
        HandleRef pSubSurfaces = new HandleRef(this, split_to_surfaces_Surface(_handle, nbSubSurfaces));
        int nbMultiSurface = nb_multiSurface(pSubSurfaces);
        Surface[] splits = new Surface[nbMultiSurface];
        for (int ii = 0; ii < nbMultiSurface; ++ii)
        {
            splits[ii] = new Surface(move_MultiSurface(pSubSurfaces, ii));
            CopyVerticesAndTriangles(splits[ii]);
        }
        delete_MultiSurface(pSubSurfaces);
        return splits;
    }

    void CopyVerticesAndTriangles(Surface surface)
    {
        float[] verticesArray = new float[3 * get_number_vertices(surface._handle)];
        copy_vertices(surface._handle, verticesArray);
        for (int i = 0; i < verticesArray.Length / 3; i++)
            surface.Vertices.Add(new Vector3(verticesArray[3 * i], verticesArray[(3 * i) + 1], verticesArray[(3 * i) + 2]));

        surface.TriangleIds = new int[3 * get_number_triangles(surface._handle)];
        copy_triangles(surface._handle, surface.TriangleIds);
    }

    #region memory_management

    public Surface(string pathSurfaceFile) : base(pathSurfaceFile)
    {
        CopyVerticesAndTriangles(this);
    }

    public Surface(IntPtr surfaceHandle) : base(surfaceHandle) { }

    protected override void createDLLClass()
    {

    }

    protected override void createDLLClass(string str)
    {
        m_filePath = str;
        //==
        string[] pathSurfaceFileSplit = m_filePath.Split(new char[] { '.' });
        string fileExtention = pathSurfaceFileSplit[pathSurfaceFileSplit.Length - 1].ToUpper();
        //==
        string workingdir = Path.GetDirectoryName(m_filePath);

        string[] files;
        if (fileExtention == "TRI")
            files = System.IO.Directory.GetFiles(workingdir, "transfo_mni.trm");
        else if (fileExtention == "GII")
            files = System.IO.Directory.GetFiles(workingdir, "*_Scanner_Based.trm");
        else
            files = new string[] { "" };

        if (files.Length > 0)
        {
            files[0].Replace('\\', '/');
            _handle = new HandleRef(this, read_file_to_surface(m_filePath, files[0], fileExtention));
        }
        else
        {
            _handle = new HandleRef(this, read_file_to_surface(m_filePath, "", fileExtention));
        }
        //==
    }

    protected override void deleteDLLClass()
    {
        delete_Surface(_handle);
        Vertices.Clear();
        Vertices = null;
        TriangleIds = null;
    }

    #endregion memory_management

    #region DLLImport
    [DllImport("BTVReplayLibraryC++", EntryPoint = "read_file_to_surface", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr read_file_to_surface(string pathSurfaceFile, string pathTransformationFile, string fileExtention);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "get_number_vertices", CallingConvention = CallingConvention.Cdecl)]
    static private extern int get_number_vertices(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "get_number_triangles", CallingConvention = CallingConvention.Cdecl)]
    static private extern int get_number_triangles(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "copy_vertices", CallingConvention = CallingConvention.Cdecl)]
    static private extern void copy_vertices(HandleRef handle, float[] verticesArray);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "copy_triangles", CallingConvention = CallingConvention.Cdecl)]
    static private extern void copy_triangles(HandleRef handle, int[] trianglesArray);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "delete_Surface", CallingConvention = CallingConvention.Cdecl)]
    static private extern void delete_Surface(HandleRef handle);

    //====

    [DllImport("BTVReplayLibraryC++", EntryPoint = "split_to_surfaces_Surface", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr split_to_surfaces_Surface(HandleRef handleSurface, int nbSubSurfaces);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "nb_multiSurface", CallingConvention = CallingConvention.Cdecl)]
    static private extern int nb_multiSurface(HandleRef handleSurface);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "move_MultiSurface", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr move_MultiSurface(HandleRef handleSurface, int numSurface);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "delete_MultiSurface", CallingConvention = CallingConvention.Cdecl)]
    static private extern void delete_MultiSurface(HandleRef handleSurface);

    #endregion
}
