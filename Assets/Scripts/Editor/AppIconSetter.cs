using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Applies Assets/Branding/AppIcon.png to the Standalone application icon slots.
/// Run once via Tools > Set App Icon; the result is stored in ProjectSettings.asset.
/// </summary>
public static class AppIconSetter
{
    private const string k_IconPath = "Assets/Branding/AppIcon.png";

    [MenuItem("Tools/Set App Icon")]
    public static void SetAppIcon()
    {
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(k_IconPath);
        if (icon == null)
        {
            Debug.LogError("AppIconSetter: no texture found at " + k_IconPath);
            return;
        }

        NamedBuildTarget target = NamedBuildTarget.Standalone;
        int[] sizes = PlayerSettings.GetIconSizes(target, IconKind.Application);

        Texture2D[] icons = new Texture2D[sizes != null && sizes.Length > 0 ? sizes.Length : 1];
        for (int i = 0; i < icons.Length; i++) icons[i] = icon;

        PlayerSettings.SetIcons(target, icons, IconKind.Application);
        AssetDatabase.SaveAssets();
        Debug.Log("AppIconSetter: Standalone application icon set from " + k_IconPath + " (" + icons.Length + " size slot(s)).");
    }
}
