using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BTVReplayBuilder : MonoBehaviour
{
    private static string m_Data = "Assets/Config/";
    private static string m_DataBuild = "";

    /// <summary>
    /// Headless entry point (Unity -batchmode -executeMethod BTVReplayBuilder.DefaultBuild).
    /// Output directory comes from "-buildOutput &lt;path&gt;" on the command line, the
    /// BTV_BUILD_OUTPUT environment variable, or defaults to &lt;project&gt;/Builds/.
    /// </summary>
    public static void DefaultBuild()
    {
        string buildsDirectory = ResolveDefaultBuildsDirectory();
        BuildProjectAndZipIt(buildsDirectory, false, BuildTarget.StandaloneWindows64);
        BuildProjectAndZipIt(buildsDirectory, false, BuildTarget.StandaloneLinux64);
        BuildProjectAndZipIt(buildsDirectory, false, BuildTarget.StandaloneOSX);
    }

    public static void BuildProjectAndZipIt(string buildsDirectory, bool development, BuildTarget target)
    {
        string os = "";
        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                os = "win64";
                m_DataBuild = "BTVReplay_Data/Config";
                break;
            case BuildTarget.StandaloneLinux64:
                os = "linux64";
                m_DataBuild = "BTVReplay_Data/Config";
                break;
            case BuildTarget.StandaloneOSX:
                UnityEditor.OSXStandalone.UserBuildSettings.architecture = UnityEditor.Build.OSArchitecture.ARM64;
                os = "macos64";
                m_DataBuild = "Contents/Config";
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
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        // Abort the post-build steps (copying Config, plugins) if the player build itself
        // failed - otherwise we copy data into a missing/partial directory and the real
        // failure is buried under confusing IO errors.
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError(string.Format("BTVReplayBuilder: {0} build FAILED ({1}) with {2} error(s); skipping data/plugin copy.",
                target, report.summary.result, report.summary.totalErrors));
            return;
        }
        Debug.Log(string.Format("BTVReplayBuilder: {0} build succeeded -> {1} ({2:0.0} MB)",
            target, buildDirectory, report.summary.totalSize / (1024f * 1024f)));

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
                    if (UnityEditor.OSXStandalone.UserBuildSettings.architecture == UnityEditor.Build.OSArchitecture.ARM64)
                    {
                        string pluginsPath = Path.Join(dataDirectory, "Contents", "PlugIns");
                        DirectoryInfo pluginsDirectory = new DirectoryInfo(pluginsPath);
                        DirectoryInfo arm64PluginsDirectory = new DirectoryInfo(Path.Join(pluginsPath, "ARM64"));
                        // Older Unity nested arm64 native plugins under PlugIns/ARM64; flatten them
                        // up one level. Guarded in case a future Unity places them directly.
                        if (arm64PluginsDirectory.Exists)
                        {
                            arm64PluginsDirectory.CopyFilesRecursively(pluginsDirectory);
                            arm64PluginsDirectory.Delete(true);
                        }
                        else
                        {
                            Debug.LogWarning("BTVReplayBuilder: Contents/PlugIns/ARM64 not found; assuming arm64 plugins are already placed correctly. Verify the .app loads native libraries.");
                        }
                    }
                }
                break;
        }
    }

    private static string ResolveDefaultBuildsDirectory()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-buildOutput") return EnsureTrailingSlash(args[i + 1]);
        }
        string env = Environment.GetEnvironmentVariable("BTV_BUILD_OUTPUT");
        if (!string.IsNullOrEmpty(env)) return EnsureTrailingSlash(env);

        // <project>/Builds/ (Application.dataPath is <project>/Assets; this is git-ignored)
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return EnsureTrailingSlash(Path.Combine(projectRoot, "Builds"));
    }

    private static string EnsureTrailingSlash(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        char last = path[path.Length - 1];
        return (last == '/' || last == '\\') ? path : path + "/";
    }
}
