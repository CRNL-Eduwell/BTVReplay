using System.Linq;
using UnityEngine;
using UnityEditor;

public class Builder : MonoBehaviour
{
    public static void DevelopmentBuildShell()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.locationPathName = "D:/Users/Florian/Desktop/BTVReplay/BTVReplay.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.scenes = new string[] { "Assets/_main.unity" };
        BuildOptions buildOptions = BuildOptions.AllowDebugging | BuildOptions.Development;
        buildPlayerOptions.options = buildOptions;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    [MenuItem("Build/Development Build")]
    public static void DevelopmentBuild()
    {
     

    }
}