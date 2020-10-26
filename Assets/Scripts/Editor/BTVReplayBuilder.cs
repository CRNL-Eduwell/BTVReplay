using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BTVReplayBuilder : MonoBehaviour
{
    private static string m_Data = "Assets/Config/";
    private static string m_DataBuild = "Config/";

    public static void DefaultBuild()
    {
        BuildProjectAndZipIt(@"/Users/florian/Desktop/builds/", false, BuildTarget.StandaloneWindows64);
        BuildProjectAndZipIt(@"/Users/florian/Desktop/builds/", false, BuildTarget.StandaloneLinux64);
        BuildProjectAndZipIt(@"/Users/florian/Desktop/builds/", false, BuildTarget.StandaloneOSX);
    }

    public static void BuildProjectAndZipIt(string buildsDirectory, bool development, BuildTarget target)
    {
        string os = "";
        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                os = "win64";
                break;
            case BuildTarget.StandaloneLinux64:
                os = "linux64";
                break;
            case BuildTarget.StandaloneOSX:
                os = "macos64";
                break;
        }
        string buildName = string.Format("{0}.{1}.{2}", Application.productName, Application.version, os);
        string buildDirectory = buildsDirectory + buildName + "/";
        string dataDirectory = buildDirectory;
        string executableName = "BTVReplay";
        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                executableName += ".exe";
                break;
            case BuildTarget.StandaloneLinux64:
                executableName += ".x86_64";
                break;
            case BuildTarget.StandaloneOSX:
                executableName += ".app";
                dataDirectory += executableName + "/Contents/";
                break;
        }

        BuildOptions buildOptions = development ? BuildOptions.Development : BuildOptions.None;
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            locationPathName = buildDirectory + executableName,
            target = target,
            scenes = new string[] { "Assets/_main.unity" },
            options = buildOptions
        };
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        string projectPath = Application.dataPath;
        projectPath = projectPath.Remove(projectPath.Length - 6);

        DirectoryInfo dataDirectoryInfo = new DirectoryInfo(dataDirectory + m_DataBuild);
        new DirectoryInfo(projectPath + m_Data).CopyFilesRecursively(dataDirectoryInfo);
        foreach (var file in dataDirectoryInfo.GetFiles("*.meta", SearchOption.AllDirectories))
        {
            file.Delete();
        }
        foreach (var file in dataDirectoryInfo.GetFiles("*.obj", SearchOption.AllDirectories))
        {
            file.Delete();
        }

        if (target == BuildTarget.StandaloneLinux64)
        {
            DirectoryInfo pluginsDirectory = new DirectoryInfo(Application.dataPath + "/Plugins/x86_64/Linux");
            DirectoryInfo newPluginsDirectory = new DirectoryInfo(dataDirectory + "BTVReplay_Data/Plugins/x86_64");
            pluginsDirectory.CopyFilesRecursively(newPluginsDirectory);
            foreach (var metaFile in newPluginsDirectory.GetFiles("*.meta"))
            {
                metaFile.Delete();
            }
        }

        //FileInfo readme = new FileInfo(projectPath + "README.md");
        //readme.CopyTo(buildDirectory + readme.Name);

        //FileInfo documentation = new FileInfo(projectPath + "Docs/LaTeX/HiBoP_user_manual.pdf");
        //documentation.CopyTo(buildDirectory + documentation.Name);
    }
}