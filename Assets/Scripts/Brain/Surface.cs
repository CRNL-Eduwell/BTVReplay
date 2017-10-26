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
    public List<Vector3> verticesObj = new List<Vector3>();          //List of X-Y-Z Coordinates : each element is one vetice (point)
    public int[] idTri;                                              //List of Index number : three vertices used to make one triangle
    private string surfaceFilePath = "";

    #region memory_management

    public Surface(string pathSurfaceFile) : base(pathSurfaceFile)
    {
        float[] verticesArray = new float[3 * get_number_vertices(_handle)];
        copy_vertices(_handle, verticesArray);
        //verticesObj = new List<Vector3>(verticesArray.Length / 3);
        for (int i = 0; i < verticesArray.Length / 3; i++)
            verticesObj.Add(new Vector3(verticesArray[3 * i], verticesArray[(3 * i) + 1], verticesArray[(3 * i) + 2]));

        idTri = new int[3 * get_number_triangles(_handle)];
        copy_triangles(_handle, idTri);
    }

    protected override void createDLLClass()
    {

    }

    protected override void createDLLClass(string str)
    {
        surfaceFilePath = str;
        //==
        string[] pathSurfaceFileSplit = surfaceFilePath.Split(new char[] { '.' });
        string fileExtention = pathSurfaceFileSplit[pathSurfaceFileSplit.Length - 1].ToUpper();
        //==
        string workingdir = Path.GetDirectoryName(surfaceFilePath);

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
            _handle = new HandleRef(this, read_file_to_surface(surfaceFilePath, files[0], fileExtention));
        }
        else
        {
            _handle = new HandleRef(this, read_file_to_surface(surfaceFilePath, "", fileExtention));
        }
        //==
    }

    protected override void deleteDLLClass()
    {
        delete_Surface(_handle);
        verticesObj.Clear();
        verticesObj = null;
        idTri = null;
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
    #endregion
}
