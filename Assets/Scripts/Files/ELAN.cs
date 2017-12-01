using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections; //IEnumerator
using UnityEngine;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct elecFile
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string name;
    [MarshalAs(UnmanagedType.I4)]
    public int index;
}

public class ELAN : CppDLLImportBase
{
    public string fileFolder
    {
        get
        {
            return Path.GetDirectoryName(filePath);
        }
    }
    public string filePath
    {
        get;
        set;
    }

    #region members
    public int nbSam = 0;
    public int nbChan = 0;
    public int nbMeas = 0;
    public float sampFreq = 0;
    public elecFile[] electrodes = null;
    public float[] maxValues = null;
    public float[] eegData = null;
    public int idFileHandle
    {
        get; set;
    }
    #endregion

    #region functions
    public static ELAN changeHandle(ELAN currentFile, ELAN[] elanFiles, int newID)
    {
        if (elanFiles[newID] != null)
        {
            elanFiles[newID].idFileHandle = newID;
            return elanFiles[newID];
        }
        else
        {
            return currentFile;
        }
    }

    public static bool checkHandle(ELAN[] elanFiles, int newID)
    {
        if (elanFiles[newID] != null)
            return true;
        else
            return false;
    }

    public static ELAN returnFirstValidHandle(ELAN[] elanFiles)
    {
        for (int i = 0; i < elanFiles.Count(); i++)
        {
            if (elanFiles[i] != null)
                return elanFiles[i];
        }

        return null;
    }

    public static int returnFirstValidHandleId(ELAN[] elanFiles)
    {
        for (int i = 0; i < elanFiles.Count(); i++)
        {
            if (elanFiles[i] != null)
                return i;
        }

        return -1;
    }

    public static IEnumerator c_loadIfExist(string filePath, Action<ELAN> resultCB)
    { 
        if (filePath != "") //remplacer par check file exist
        {
            ELAN elan = new ELAN(filePath);
            elan.readElanFile();
            elan.getMaxValueChanels();
            elan.releaseCppHandle();
            resultCB(elan);
            yield return null;
        }
        else
        {
            resultCB(null);
        }
    }

    public static float getSamplingFreq(ELAN[] elanFiles)
    {
        for(int i = 0; i < elanFiles.Count(); i++)
        {
            if (elanFiles[i] != null)
                return elanFiles[i].sampFreq;
        }

        return -1;
    }

    public static long getTotalFileDuration(ELAN valid)
    {
        return (long)(valid.nbSam / valid.sampFreq);
    }

    public void getMaxValueChanels(int idMeas = 1)
    {
        if (maxValues == null)
            maxValues = new float[nbChan];

        getMaxValueChanel(_handle, maxValues, idMeas - 1);
    }

    /// <summary> 
    /// Copy EEG data from C++ 
    /// then release the C++ handle
    /// </summary>
    public void readElanFile()
    {
        if (eegData == null)
            eegData = new float[nbMeas * nbChan * nbSam];

        readDataAllChannels(_handle, eegData);
    }
    #endregion

    #region memory_management

    /// <summary>
    /// Constructor with absolute path to ELAN File
    /// </summary>
    /// <param name="p_pathfile"></param>
    public ELAN(string pathEEGFile) : base(pathEEGFile)
    {
        filePath = pathEEGFile;
        sampFreq = samplingFrequency(_handle);
        nbSam = nbSample(_handle);
        nbChan = nbChannels(_handle);
        nbMeas = nbMeasure(_handle);
        getElectrodes();
    }

    void releaseCppHandle()
    {
        delete_ELAN(_handle);
    }

    void getElectrodes()
    {
        electrodes = new elecFile[nbChan];
        for (int i = 0; i < nbChan; i++)
            electrodes[i] = new elecFile();

        getElectrodes(_handle, out electrodes);
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
        //electList.Clear();
        //electList = null;
        electrodes = null;
        eegData = null;
        //delete_ELAN(_handle);
    }

    #endregion memory_management

    #region DLLImport
    [DllImport("BTVReplayLibraryC++", EntryPoint = "create_ELAN", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr create_ELAN(string pathEEGFile);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "delete_ELAN", CallingConvention = CallingConvention.Cdecl)]
    static private extern void delete_ELAN(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "getElectrodes", CallingConvention = CallingConvention.Cdecl)]
    static private extern void getElectrodes(HandleRef handle, out elecFile[] elec);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "readDataAllChannels", CallingConvention = CallingConvention.Cdecl)]
    static private extern void readDataAllChannels(HandleRef handle, float[] eegData);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "getMaxValueChanel", CallingConvention = CallingConvention.Cdecl)]
    static private extern void getMaxValueChanel(HandleRef handle, float[] maxValues, int idMeasure);

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
