using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot migration: switches the project to Force Text asset serialization and
/// re-serializes every asset so the scene, prefabs and settings become diffable YAML.
/// Run headless with:
///   Unity -batchmode -quit -projectPath <path> -executeMethod ForceTextMigration.Run
/// </summary>
public static class ForceTextMigration
{
    [MenuItem("Tools/Force Text Migration")]
    public static void Run()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        AssetDatabase.ForceReserializeAssets();
        AssetDatabase.SaveAssets();
        Debug.Log("ForceTextMigration: serialization mode set to ForceText, all assets re-serialized.");
    }
}
