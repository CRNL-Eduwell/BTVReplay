using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BTVReplayBuilderWindow : EditorWindow
{
    private const string k_BuildDirPref = "BTVReplay.BuildDirectory";

    private string m_BuildDirectory = "";
    private bool m_DevelopmentBuild = false;
    private bool m_Windows = true;
    private bool m_Linux = true;
    private bool m_MacOSX = true;

    [MenuItem("Tools/Build BTVReplay")]
    public static void OpenBuildWindow()
    {
        BTVReplayBuilderWindow window = (BTVReplayBuilderWindow)GetWindow(typeof(BTVReplayBuilderWindow));
        window.Show();
    }

    private void OnEnable()
    {
        // Remember the last folder; default to <project>/Builds/ (git-ignored).
        string fallback = System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, "Builds") + "/";
        m_BuildDirectory = EditorPrefs.GetString(k_BuildDirPref, fallback);
    }

    void OnGUI()
    {
        GUILayout.Label("BTVReplay Builder", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        m_BuildDirectory = EditorGUILayout.TextField("Builds Directory", m_BuildDirectory);
        if (GUILayout.Button("Select"))
        {
            m_BuildDirectory = EditorUtility.OpenFolderPanel("Select the builds folder", m_BuildDirectory, "");
        }
        GUILayout.EndHorizontal();
        m_DevelopmentBuild = GUILayout.Toggle(m_DevelopmentBuild, "Development Build");
        m_Windows = GUILayout.Toggle(m_Windows, "Windows");
        m_Linux = GUILayout.Toggle(m_Linux, "Linux");
        m_MacOSX = GUILayout.Toggle(m_MacOSX, "MacOSX");
        if (GUILayout.Button("Build!"))
        {
            if (string.IsNullOrEmpty(m_BuildDirectory))
            {
                EditorUtility.DisplayDialog("BTVReplay Builder", "Please choose a builds directory.", "OK");
                return;
            }
            if (m_BuildDirectory[m_BuildDirectory.Length - 1] != '/' && m_BuildDirectory[m_BuildDirectory.Length - 1] != '\\')
            {
                m_BuildDirectory += '/';
            }
            EditorPrefs.SetString(k_BuildDirPref, m_BuildDirectory);
            if (m_Windows)
            {
                BTVReplayBuilder.BuildProjectAndZipIt(m_BuildDirectory, m_DevelopmentBuild, BuildTarget.StandaloneWindows64);
            }
            if (m_Linux)
            {
                BTVReplayBuilder.BuildProjectAndZipIt(m_BuildDirectory, m_DevelopmentBuild, BuildTarget.StandaloneLinux64);
            }
            if (m_MacOSX)
            {
                BTVReplayBuilder.BuildProjectAndZipIt(m_BuildDirectory, m_DevelopmentBuild, BuildTarget.StandaloneOSX);
            }
            Close();
        }
    }
}