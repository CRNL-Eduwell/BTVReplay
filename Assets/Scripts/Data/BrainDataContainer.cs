using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

public class BrainDataContainer
{
    public bool HasAnat
    {
        get
        {
            switch (MeshConfiguration)
            {
                case MeshConfiguration.LeftRight:
                    {
                        if (LeftHemisphere != "" && RightHemisphere != "" && Pts != "")
                        {
                            FileInfo lhemiFileInfo = new FileInfo(LeftHemisphere);
                            bool isLhemiOk = lhemiFileInfo.Exists && (lhemiFileInfo.Extension == ".tri" || lhemiFileInfo.Extension == ".gii");
                            if (!isLhemiOk) return false;
                            FileInfo rhemiFileInfo = new FileInfo(RightHemisphere);
                            bool isRhemiOk = rhemiFileInfo.Exists && (rhemiFileInfo.Extension == ".tri" || rhemiFileInfo.Extension == ".gii");
                            if (!isRhemiOk) return false;
                            FileInfo ptsFileInfo = new FileInfo(Pts);
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
                        if (LeftHemisphere != "" && Pts != "")
                        {
                            FileInfo lhemiFileInfo = new FileInfo(LeftHemisphere);
                            bool isLhemiOk = lhemiFileInfo.Exists && (lhemiFileInfo.Extension == ".tri" || lhemiFileInfo.Extension == ".gii");
                            if (!isLhemiOk) return false;
                            FileInfo ptsFileInfo = new FileInfo(Pts);
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
    public string LeftHemisphere { get; set; } = "";
    public string RightHemisphere { get; set; } = "";
    public string Transformation { get; set; } = "";
    public string Pts { get; set; } = "";
    public string Atlas { get; set; } = "";
    public MeshConfiguration MeshConfiguration { get; set; } = MeshConfiguration.LeftRight;
    public EegTechnology EegTechnology { get; set; } = EegTechnology.Intra;

    public BrainDataContainer() { }

    public BrainDataContainer(string lhemi, string rhemi, string trm, string pts, string atlas = "", string meshConfiguration = "", string eegTechnology = "")
    {
        LeftHemisphere = lhemi;
        RightHemisphere = rhemi;
        Transformation = trm;
        Pts = pts;
        Atlas = atlas;
        MeshConfiguration = EnumExtensions.GetValueFromDescription<MeshConfiguration>(meshConfiguration);
        EegTechnology = EnumExtensions.GetValueFromDescription<EegTechnology>(eegTechnology);
    }

    public BrainDataContainer(BrainDataContainer containerToCopy)
    {
        LeftHemisphere = containerToCopy.LeftHemisphere;
        RightHemisphere = containerToCopy.RightHemisphere;
        Transformation = containerToCopy.Transformation;
        Pts = containerToCopy.Pts;
        Atlas = containerToCopy.Atlas;
        MeshConfiguration = containerToCopy.MeshConfiguration;
        EegTechnology = containerToCopy.EegTechnology;
    }

    public void SetMeshConfigurationFromString(string str)
    {
        MeshConfiguration = EnumExtensions.GetValueFromDescription<MeshConfiguration>(str);
    }

    public void SetEegTechnologyFromString(string str)
    {
        EegTechnology = EnumExtensions.GetValueFromDescription<EegTechnology>(str);
    }
}
