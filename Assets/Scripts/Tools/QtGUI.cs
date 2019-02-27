using System;
using System.Text;
using System.Runtime.InteropServices;

public class QtGUI_dll : CppDLLImportBase
{
    /// <summary>
    /// Open a qt file dialog and return the path of an existing directory.
    /// </summary>
    /// <param name="message"> message to be displayed in top of the file dialog </param>
    /// <param name="defaultDir"> default directory of the file dialog </param>
    /// <returns> return an empty path if no directory has been choosen or if an error occurs </returns>
    public string getExistingDirectory(string message = "Select a directory", string defaultDir = "")
    {
        IntPtr ptrUnmanagedStr = getExistingDirectory_QtApp(message, defaultDir, m_bufferSize);
        return Marshal.PtrToStringAnsi(ptrUnmanagedStr);
    }

    /// <summary>
    /// Open a qt file dialog and return the path of a selected file.
    /// </summary>
    /// <param name="filtersArray"> extension filters of the files of be displayed in the file dialog (ex: filtersArray[0] = "txt", filtersArray[1] = "png" ...) </param>
    /// <param name="message">  message to be displayed in top of the file dialog  </param>
    /// <param name="defaultDir"> default directory of the file dialog </param>
    /// <returns> return an empty path if no file has been choosen or if an error occurs </returns>
    public string getOpenFileName(string[] filtersArray = null, string message = "Select a file", string defaultDir = "")
    {
        filtersArray = filtersArray ?? new string[] { "txt" };

        string filters = "";
        for (int ii = 0; ii < filtersArray.Length - 1; ++ii)
        {
            filters += filtersArray[ii] + "*";
        }
        filters += filtersArray[filtersArray.Length - 1];

        IntPtr ptrUnmanagedStr = getOpenFileName_QtApp(message, defaultDir, filters, m_bufferSize);
        return Marshal.PtrToStringAnsi(ptrUnmanagedStr);
    }

    /// <summary>
    /// Open a qt file dialog and return the list of path of the selected files.
    /// </summary>
    /// <param name="filtersArray">  extension filters of the files of be displayed in the file dialog (ex: filtersArray[0] = "txt", filtersArray[1] = "png" ...) </param>
    /// <param name="message"> message to be displayed in top of the file dialog </param>
    /// <param name="defaultDir"> default directory of the file dialog </param>
    /// <returns> return an empty path if no file has been choosen or if an error occurs </returns>
    public string[] getOpenFilesName(string[] filtersArray = null, string message = "Select files", string defaultDir = "")
    {
        filtersArray = filtersArray ?? new string[] { "txt" };

        string filters = "";
        for (int ii = 0; ii < filtersArray.Length - 1; ++ii)
        {
            filters += filtersArray[ii] + "*";
        }
        filters += filtersArray[filtersArray.Length - 1];

        IntPtr ptrUnmanagedStr = getOpenFilesName_QtApp(message, defaultDir, filters, m_bufferSize);

        string filesDirRes = Marshal.PtrToStringAnsi(ptrUnmanagedStr);
        return filesDirRes.Split(new char[] { '*' });
    }

    /// <summary>
    /// Open a qt file dialog and return the path of a saved file
    /// </summary>
    /// <param name="filtersArray"> extension filters of the files of be displayed in the file dialog (ex: filtersArray[0] = "txt", filtersArray[1] = "png" ...) </param>
    /// <param name="message">  message to be displayed in top of the file dialog  </param>
    /// <param name="defaultDir"> default directory of the file dialog </param>
    /// <returns> return an empty path if no file has been choosen or if an error occurs </returns>
    public string getSaveFileName(string[] filtersArray = null, string message = "Save to", string defaultDir = "")
    {
        filtersArray = filtersArray ?? new string[] { "txt" };

        string filters = "";
        for (int ii = 0; ii < filtersArray.Length - 1; ++ii)
        {
            filters += filtersArray[ii] + "*";
        }
        filters += filtersArray[filtersArray.Length - 1];
        
        IntPtr ptrUnmanagedStr = getSaveFileName_QtApp(message, defaultDir, filters, m_bufferSize);
        return Marshal.PtrToStringAnsi(ptrUnmanagedStr);
    }

    private int m_bufferSize = 1024;
    /// <summary>
    /// Access SiteStructure.Instance to get the singleton object.
    /// Then call methods on that instance.
    /// </summary>
    public static QtGUI_dll Instance { get; } = new QtGUI_dll();

    /// <summary>
    /// Private default constructor of QtGUi, singleton design
    /// </summary>
    private QtGUI_dll() : base() { }

    /// <summary>
    /// Private constructor with pointer of QtGUi, singleton design
    /// </summary>
    /// <param name="ptr"></param>
    private QtGUI_dll(IntPtr ptr) : base(ptr) { }

    /// <summary>
    /// Overrided dispose, this function is useless with singleton design
    /// </summary>
    public override void Dispose() { }
    
    /// <summary>
    /// Allocate DLL memory
    /// </summary>
    protected override void createDLLClass()
    {
        _handle = new HandleRef(this, create_QtApp());
    }

    protected override void createDLLClass(string str)
    {
        
    }

    /// <summary>
    /// Clean DLL memory
    /// </summary>
    protected override void deleteDLLClass()
    {
        delete_QtApp(_handle);
    }

    //memory management
    [DllImport("GUIExport", EntryPoint = "create_QtApp", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr create_QtApp();

    [DllImport("GUIExport", EntryPoint = "delete_QtApp", CallingConvention = CallingConvention.Cdecl)]
    static private extern void delete_QtApp(HandleRef handleSurface);

    // actions
    [DllImport("GUIExport", EntryPoint = "getExistingDirectory_QtApp", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr getExistingDirectory_QtApp(string message, string defaultDir, int bufferSize);

    [DllImport("GUIExport", EntryPoint = "getOpenFileName_QtApp", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr getOpenFileName_QtApp(string message, string defaultDir, string filters, int bufferSize);

    [DllImport("GUIExport", EntryPoint = "getOpenFilesName_QtApp", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr getOpenFilesName_QtApp(string message, string defaultDir, string filters, int bufferSize);

    [DllImport("GUIExport", EntryPoint = "getSaveFileName_QtApp", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr getSaveFileName_QtApp(string message, string defaultDir, string filters, int bufferSize);
}
