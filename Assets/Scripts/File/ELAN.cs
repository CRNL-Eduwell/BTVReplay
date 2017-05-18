using System;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class ELAN : CppDLLImportBase
{
    #region members
    public int nbSam = 0;
    public int nbChan = 0;
    public int nbMeas = 0;
    public float sampFreq = 0;
    public string filePath = "";
    public List<string> electList = null;
    public float[] eegData = null;
    #endregion

    #region functions

    public void readElanFile()
    {
        if (eegData == null)
        {
            eegData = new float[nbMeas * nbChan * nbSam];
        }
        readDataAllChannels(_handle, eegData);
        delete_ELAN(_handle); //Release C++ Data since it was just a copy
    }

    void getElectrodeList()
    {
        int size = electrodeListSizeBuilder(_handle);
        StringBuilder sb = new StringBuilder(size);

        getElectrodeList(_handle, sb);

        electList = sb.ToString().Split(new char[] { '!' }, 
                    StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    #endregion

    #region memory_management

    /// <summary>
    /// Constructor with absolute path to TRC File
    /// </summary>
    /// <param name="p_pathfile"></param>
    public ELAN(string pathEEGFile) : base(pathEEGFile)
    {
        sampFreq = samplingFrequency(_handle);
        nbSam = nbSample(_handle);
        nbChan = nbChannels(_handle);
        nbMeas = nbMeasure(_handle);
        getElectrodeList();
    }

    /// <summary>
    /// Allocate DLL memory
    /// </summary>
    protected override void createDLLClass()
    {

    }

    /// <summary>
    /// Allocate DLL memory
    /// </summary>
    protected override void createDLLClass(string str)
    {
        _handle = new HandleRef(this, create_ELAN(str));
    }

    /// <summary>
    /// Clean DLL memory
    /// </summary>
    protected override void deleteDLLClass()
    {
        //UnityEngine.Debug.Log("I'm deleting elan");
        electList.Clear();
        electList = null;
        eegData = null;
        //delete_ELAN(_handle);
    }

    #endregion memory_management

    #region DLLImport
    //memory management
    [DllImport("BTVReplayLibraryC++", EntryPoint = "create_ELAN", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr create_ELAN(string pathEEGFile);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "delete_ELAN", CallingConvention = CallingConvention.Cdecl)]
    static private extern void delete_ELAN(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "electrodeListSizeBuilder", CallingConvention = CallingConvention.Cdecl)]
    static private extern int electrodeListSizeBuilder(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "getElectrodeList", CallingConvention = CallingConvention.Cdecl)]
    static private extern void getElectrodeList(HandleRef handle, StringBuilder sb );

    [DllImport("BTVReplayLibraryC++", EntryPoint = "readDataAllChannels", CallingConvention = CallingConvention.Cdecl)]
    static private extern void readDataAllChannels(HandleRef handle, float[] eegData);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "samplingFrequency", CallingConvention = CallingConvention.Cdecl)]
    static private extern int samplingFrequency(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "nbSample", CallingConvention = CallingConvention.Cdecl)]
    static private extern int nbSample(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "nbChannels", CallingConvention = CallingConvention.Cdecl)]
    static private extern int nbChannels(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "nbMeasure", CallingConvention = CallingConvention.Cdecl)]
    static private extern int nbMeasure(HandleRef handle);

    #endregion
}
