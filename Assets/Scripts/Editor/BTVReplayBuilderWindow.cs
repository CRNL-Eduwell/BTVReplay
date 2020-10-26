using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BTVReplayBuilderWindow : EditorWindow
{
    private string m_BuildDirectory = @"/Users/florian/Desktop/builds/";
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
            if (m_BuildDirectory[m_BuildDirectory.Length - 1] != '/' && m_BuildDirectory[m_BuildDirectory.Length - 1] != '\\')
            {
                m_BuildDirectory += '/';
            }
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