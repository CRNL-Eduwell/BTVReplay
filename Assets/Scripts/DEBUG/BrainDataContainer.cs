using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

public class BrainDataContainer
{
    public bool HasAnat
    {
        get
        {
            switch (m_MeshConfiguration)
            {
                case MeshConfiguration.LeftRight:
                    {
                        if (m_LHemisphere != "" && m_RightHemisphere != "" && m_Pts != "")
                        {
                            FileInfo lhemiFileInfo = new FileInfo(m_LHemisphere);
                            bool isLhemiOk = lhemiFileInfo.Exists && (lhemiFileInfo.Extension == ".tri" || lhemiFileInfo.Extension == ".gii");
                            if (!isLhemiOk) return false;
                            FileInfo rhemiFileInfo = new FileInfo(m_RightHemisphere);
                            bool isRhemiOk = rhemiFileInfo.Exists && (rhemiFileInfo.Extension == ".tri" || rhemiFileInfo.Extension == ".gii");
                            if (!isRhemiOk) return false;
                            FileInfo ptsFileInfo = new FileInfo(m_Pts);
                            bool isPtsOk = ptsFileInfo.Exists && ptsFileInfo.Extension == ".pts";
                            return isPtsOk;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case MeshConfiguration.Single:
                    {
                        if (m_LHemisphere != "" && m_Pts != "")
                        {
                            FileInfo lhemiFileInfo = new FileInfo(m_LHemisphere);
                            bool isLhemiOk = lhemiFileInfo.Exists && (lhemiFileInfo.Extension == ".tri" || lhemiFileInfo.Extension == ".gii");
                            if (!isLhemiOk) return false;
                            FileInfo ptsFileInfo = new FileInfo(m_Pts);
                            bool isPtsOk = ptsFileInfo.Exists && ptsFileInfo.Extension == ".pts";
                            return isPtsOk;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case MeshConfiguration.Unknown:
                    return false;
            }

            return false;
        }
    }
    public string LeftHemisphere { get { return m_LHemisphere; } }
    public string RightHemisphere { get { return m_RightHemisphere; } }
    public string Pts { get { return m_Pts; } }
    public string Atlas { get { return m_AtlasCsv; } }
    public MeshConfiguration MeshConfiguration { get { return m_MeshConfiguration; } set { m_MeshConfiguration = value; } }
    public EegTechnology EegTechnology { get { return m_EegTechnology; } set { m_EegTechnology = value; } }

    public BrainDataContainer(string lhemi, string rhemi, string pts, string atlas = "")
    {
        m_LHemisphere = lhemi;
        m_RightHemisphere = rhemi;
        m_Pts = pts;
        m_AtlasCsv = atlas;
    }

    [DataMember(Name = "LHemi")]
    private string m_LHemisphere = "";
    [DataMember(Name = "RHemi")]
    private string m_RightHemisphere = "";
    [DataMember(Name = "Pts")]
    private string m_Pts = "";
    [DataMember(Name = "Atlas")]
    private string m_AtlasCsv = "";
    [DataMember(Name = "Mesh")]
    private MeshConfiguration m_MeshConfiguration = MeshConfiguration.LeftRight;
    [DataMember(Name = "Eeg")]
    private EegTechnology m_EegTechnology = EegTechnology.Intra;
}
