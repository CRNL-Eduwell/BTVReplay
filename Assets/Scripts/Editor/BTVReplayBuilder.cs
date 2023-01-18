using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BTVReplayBuilder : MonoBehaviour
{
    private static string m_Data = "Assets/Config/";
    private static string m_DataBuild = "Contents/Config/";

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
                UnityEditor.OSXStandalone.UserBuildSettings.architecture = UnityEditor.OSXStandalone.MacOSArchitecture.ARM64;
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
                dataDirectory += executableName + "/";
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

        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                {

                }
                break;
            case BuildTarget.StandaloneLinux64:
                {
                    DirectoryInfo pluginsDirectory = new DirectoryInfo(Application.dataPath + "/Plugins/x86_64/Linux");
                    DirectoryInfo newPluginsDirectory = new DirectoryInfo(dataDirectory + "BTVReplay_Data/Plugins/x86_64");
                    pluginsDirectory.CopyFilesRecursively(newPluginsDirectory);
                    foreach (var metaFile in newPluginsDirectory.GetFiles("*.meta"))
                    {
                        metaFile.Delete();
                    }
                }
                break;
            case BuildTarget.StandaloneOSX:
                {
                    if (UnityEditor.OSXStandalone.UserBuildSettings.architecture == UnityEditor.OSXStandalone.MacOSArchitecture.ARM64)
                    {
                        string pluginsPath = Path.Join(dataDirectory, "Contents", "PlugIns");
                        DirectoryInfo pluginsDirectory = new DirectoryInfo(pluginsPath);
                        DirectoryInfo arm64PluginsDirectory = new DirectoryInfo(Path.Join(pluginsPath, "ARM64"));
                        arm64PluginsDirectory.CopyFilesRecursively(pluginsDirectory);
                        arm64PluginsDirectory.Delete(true);
                    }
                }
                break;
        }
    }
}