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

    /// <summary>
    /// Headless entry point that builds the single active build target, selected via Unity's
    /// "-buildTarget" command-line argument (Win64 / Linux64 / OSXUniversal). CI builds each
    /// platform on its own native runner, so it calls this rather than <see cref="DefaultBuild"/>
    /// (which builds all three in one session and would require unsupported cross-compilation).
    /// Exits with a non-zero code on failure so the CI job fails instead of silently uploading
    /// an empty/partial artifact.
    /// </summary>
    public static void Build()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        bool succeeded = BuildProjectAndZipIt(ResolveDefaultBuildsDirectory(), false, target);
        EditorApplication.Exit(succeeded ? 0 : 1);
    }

    public static bool BuildProjectAndZipIt(string buildsDirectory, bool development, BuildTarget target)
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
                // UnityEditor.OSXStandalone ships with the macOS build-support module, so the type
                // only exists when the editor runs on macOS (local dev + the CI macOS runner).
                // Guarded with UNITY_EDITOR_OSX so the Linux/Windows runners -- which compile this
                // editor assembly without that module installed -- don't fail with CS0234. The
                // macOS target is only ever built on a macOS host, which is exactly where the arm64
                // forcing is needed, so nothing is lost.
#if UNITY_EDITOR_OSX
                UnityEditor.OSXStandalone.UserBuildSettings.architecture = UnityEditor.Build.OSArchitecture.ARM64;
#endif
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
            return false;
        }
        BtvLog.Log(string.Format("BTVReplayBuilder: {0} build succeeded -> {1} ({2:0.0} MB)",
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
                    // Belt-and-braces copy of the Linux natives into the build's plugin folder
                    // (the destination layout is Unity's, do not rename it). The source follows
                    // the per-platform Assets/Plugins layout.
                    DirectoryInfo pluginsDirectory = new DirectoryInfo(Application.dataPath + "/Plugins/Linux-x86_64");
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
                    // See the UNITY_EDITOR_OSX note above: the arm64 plugin flatten references
                    // UnityEditor.OSXStandalone, so it must be compiled out on non-macOS runners.
#if UNITY_EDITOR_OSX
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

                    // Unity signs the .app (ad hoc) when it builds it, and the Config copy and the
                    // plugin flatten above then change the sealed contents. A downloaded copy (which
                    // macOS quarantines) used to be refused as "damaged" with no way to open it.
                    // Re-signed, Gatekeeper shows its usual unverified-developer prompt instead.
                    if (!ResignAdHoc(buildDirectory + executableName))
                        return false;
#endif
                }
                break;
        }

        return true;
    }

#if UNITY_EDITOR_OSX
    // Ad-hoc signature ("-"): no Apple Developer ID is involved. Fails the build when signing or
    // the strict verification fails, so a broken bundle never reaches a release.
    private static bool ResignAdHoc(string appPath)
    {
        if (!RunCodesign(appPath, "--force --deep --sign -"))
            return false;
        return RunCodesign(appPath, "--verify --deep --strict");
    }

    private static bool RunCodesign(string appPath, string arguments)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo("/usr/bin/codesign", arguments + " \"" + appPath + "\"")
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        using (var process = System.Diagnostics.Process.Start(startInfo))
        {
            // Read both streams concurrently so a full stderr pipe cannot block codesign.
            var stderr = process.StandardError.ReadToEndAsync();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            output += stderr.Result;
            if (process.ExitCode != 0)
            {
                Debug.LogError(string.Format("BTVReplayBuilder: codesign {0} failed ({1}): {2}", arguments, process.ExitCode, output));
                return false;
            }
        }
        BtvLog.Log("BTVReplayBuilder: codesign " + arguments + " OK for " + appPath);
        return true;
    }
#endif

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
