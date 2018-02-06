using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections; //IEnumerator
using UnityEngine;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct MarsAtlas_plot
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string plotName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string nameCSV;
    [MarshalAs(UnmanagedType.I4)]
    public int labelID;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
    public string hemi;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string lobe;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string nameFS;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
    public string nameFull;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
    public string broadman;
}

public class MarsAtlas : CppDLLImportBase
{
    public MarsAtlas_plot[] electrodes_Atlas = null;

    private string m_rootPath = "";
    private string m_atlasIndexPath = "/Config/Data/Atlas/mars_atlas_index.csv";

    public void loadPatientAtlas(string filePath)
    {
        List<string[]> patientAtlasRaw = new List<string[]>();

        string line;
        using (StreamReader reader = new StreamReader(filePath))
        {
            line = reader.ReadLine();
            line = reader.ReadLine();
            line = reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                string[] lineSplit = line.Split(new char[] { '\t' });
                if (lineSplit.Length == 7)
                    patientAtlasRaw.Add(new string[] { lineSplit[0], lineSplit[1] });
            }

            reader.Close();
        }
        
        electrodes_Atlas = new MarsAtlas_plot[patientAtlasRaw.Count];
        for (int i = 0; i < electrodes_Atlas.Length; i++)
        {
            electrodes_Atlas[i].plotName = patientAtlasRaw[i][0];
            electrodes_Atlas[i].labelID = get_label_Atlas(_handle, patientAtlasRaw[i][1]);
        }

        get_electrodes_Atlas(_handle, out electrodes_Atlas, electrodes_Atlas.Length);
    }

    public void findElectrodesWithAtlas(List<Electrode> elecs)
    {
        for (int i = 0; i < elecs.Count; i++)
        {
            for (int j = 0; j < elecs[i].plots.Count; j++)
            {
                List<int> idFound = electrodes_Atlas.Select((item, index) => new { Item = item, Index = index })
                                                 .Where(x => (x.Item.plotName.ToLower() == elecs[i].plots[j].label))
                                                 .Select(x => x.Index)
                                                 .ToList();
                if (idFound.Count > 0)
                {
                    elecs[i].plots[j].atlas.plotName = electrodes_Atlas[idFound[0]].plotName;
                    elecs[i].plots[j].atlas.nameCSV = electrodes_Atlas[idFound[0]].nameCSV;
                    elecs[i].plots[j].atlas.labelID = electrodes_Atlas[idFound[0]].labelID;
                    elecs[i].plots[j].atlas.hemi = electrodes_Atlas[idFound[0]].hemi;
                    elecs[i].plots[j].atlas.lobe = electrodes_Atlas[idFound[0]].lobe;
                    elecs[i].plots[j].atlas.nameFS = electrodes_Atlas[idFound[0]].nameFS;
                    elecs[i].plots[j].atlas.nameFull = electrodes_Atlas[idFound[0]].nameFull;
                    elecs[i].plots[j].atlas.broadman = electrodes_Atlas[idFound[0]].broadman;
                }
            }
        }
    }

    #region memory_management
    public MarsAtlas(string applicationPath) : base()
    {
        m_rootPath = applicationPath;
        m_atlasIndexPath = m_rootPath + m_atlasIndexPath;
        load_Atlas_Index(_handle, m_atlasIndexPath);
    }

    /// <summary>
    /// Allocate DLL memory
    /// </summary>
    protected override void createDLLClass()
    {
        _handle = new HandleRef(this, createAtlas());
    }

    /// <summary>
    /// Allocate DLL memory
    /// </summary>
    protected override void createDLLClass(string str)
    {

    }

    /// <summary>
    /// Clean DLL memory
    /// </summary>
    protected override void deleteDLLClass()
    {
        delete_Atlas(_handle);
    }
    #endregion memory_management


    #region DLLImport
    [DllImport("BTVReplayLibraryC++", EntryPoint = "createAtlas", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr createAtlas();

    [DllImport("BTVReplayLibraryC++", EntryPoint = "delete_Atlas", CallingConvention = CallingConvention.Cdecl)]
    static private extern void delete_Atlas(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "load_Atlas_Index", CallingConvention = CallingConvention.Cdecl)]
    static private extern void load_Atlas_Index(HandleRef handle, string indexFilePath);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "get_label_Atlas", CallingConvention = CallingConvention.Cdecl)]
    static private extern int get_label_Atlas(HandleRef handle, string labelFile);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "get_electrodes_Atlas", CallingConvention = CallingConvention.Cdecl)]
    static private extern void get_electrodes_Atlas(HandleRef handle, out MarsAtlas_plot[] elec, int nbPlot);
    #endregion
}
