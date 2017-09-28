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
        int length = 1024;
        StringBuilder str = new StringBuilder("");
        str.Insert(0, "?", length);
        bool success = getExistingDirectory_QtApp(message, defaultDir, str, length);
        if (success)
            return str.ToString();

        return "";
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

        int bufferSize = 1024;
        StringBuilder str = new StringBuilder("");
        str.Insert(0, "?", bufferSize);

        string filters = "";
        for (int ii = 0; ii < filtersArray.Length - 1; ++ii)
        {
            filters += filtersArray[ii] + "*";

        }
        filters += filtersArray[filtersArray.Length - 1];

        bool success = getOpenFileName_QtApp(message, defaultDir, filters, str, bufferSize);

        if (success)
            return str.ToString();

        return "";
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

        int bufferSize = 10000;
        StringBuilder str = new StringBuilder("");
        str.Insert(0, "?", bufferSize);

        string filters = "";
        for (int ii = 0; ii < filtersArray.Length - 1; ++ii)
        {
            filters += filtersArray[ii] + "*";
        }
        filters += filtersArray[filtersArray.Length - 1];

        bool success = getOpenFilesName_QtApp(message, defaultDir, filters, str, bufferSize);

        if (success)
        {
            string filesDirRes = str.ToString();
            string[] splits = filesDirRes.Split(new char[] { '*' });
            return splits;
        }

        return new string[0];
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

        int bufferSize = 1024;
        StringBuilder str = new StringBuilder("");
        str.Insert(0, "?", bufferSize);

        string filters = "";
        for (int ii = 0; ii < filtersArray.Length - 1; ++ii)
        {
            filters += filtersArray[ii] + "*";
        }
        filters += filtersArray[filtersArray.Length - 1];

        bool success = getSaveFileName_QtApp(message, defaultDir, filters, str, bufferSize);
        if (success)
            return str.ToString();
        return "";
    }

    /// <summary>
    /// Allocate ourselves.
    /// We have a private constructor, so no one else can.
    /// </summary>
    static readonly QtGUI_dll _instance = new QtGUI_dll();

    /// <summary>
    /// Access SiteStructure.Instance to get the singleton object.
    /// Then call methods on that instance.
    /// </summary>
    public static QtGUI_dll Instance
    {
        get { return _instance; }
    }

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
    static private extern bool getExistingDirectory_QtApp(string message, string defaultDir, StringBuilder dirPathRes, int bufferSize);

    [DllImport("GUIExport", EntryPoint = "getOpenFileName_QtApp", CallingConvention = CallingConvention.Cdecl)]
    static private extern bool getOpenFileName_QtApp(string message, string defaultDir, string filters, StringBuilder filePathRes, int bufferSize);

    [DllImport("GUIExport", EntryPoint = "getOpenFilesName_QtApp", CallingConvention = CallingConvention.Cdecl)]
    static private extern bool getOpenFilesName_QtApp(string message, string defaultDir, string filters, StringBuilder filesPathRes, int bufferSize);

    [DllImport("GUIExport", EntryPoint = "getSaveFileName_QtApp", CallingConvention = CallingConvention.Cdecl)]
    static private extern bool getSaveFileName_QtApp(string message, string defaultDir, string filters, StringBuilder savedfilePathRes, int bufferSize);
}
