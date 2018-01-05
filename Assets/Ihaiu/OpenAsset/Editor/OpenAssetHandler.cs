using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class OpenAssetHandler
{

    [MenuItem("Game/Settings/OpenAssetSettings")]
    public static void GenerateOpenAssetSettings()
    {
        OpenAssetSettings.EditSettings();
    }

    [OnOpenAssetAttribute(1)]
    public static bool step1(int instanceID, int line)
    {
        string path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));
        string name = Application.dataPath + "/" + path.Replace("Assets/", "");

        return OpenAssetSettings.Instance.Open(path);
    }
}
