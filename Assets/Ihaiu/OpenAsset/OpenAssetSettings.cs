using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class OpenAssetSettings : ScriptableObject
{
    const string AssetName = "Settings/OpenAssetSettings";

    private static OpenAssetSettings instance = null;
    public static OpenAssetSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load(AssetName) as OpenAssetSettings;
                if (instance == null)
                {
                    UnityEngine.Debug.Log("没有找到OpenAssetSettings");
                    instance = CreateInstance<OpenAssetSettings>();
                    instance.name = "OpenAssetSettings";

#if UNITY_EDITOR
                    string path = "Assets/Game/Resources/" + AssetName + ".asset";
                    PathUtil.CheckPath(path);
                    AssetDatabase.CreateAsset(instance, path);
#endif
                }
            }
            return instance;
        }
    }


#if UNITY_EDITOR
    public static void EditSettings()
    {
        Selection.activeObject = Instance;
        EditorApplication.ExecuteMenuItem("Window/Inspector");
    }


    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);


    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool SetActiveWindow(IntPtr hWnd);


    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool SetFocus(IntPtr hWnd);
#endif




    [Header("IDEA")]
    [SerializeField]
    public OpenAssetIdea ideaSumlime = new OpenAssetIdea(OpenAssetIdeaType.Sublime, "D:/Program Files/Sublime Text 3/sublime_text.exe");

    [SerializeField]
    public OpenAssetIdea ideaIntelliJ = new OpenAssetIdea(OpenAssetIdeaType.IntelliJ, "D:/Program Files/JetBrains/IntelliJ IDEA Community Edition 2017.2.2/bin/idea64.exe");

    [SerializeField]
    public OpenAssetIdea ideaExcel = new OpenAssetIdea(OpenAssetIdeaType.Excel, "C:/Program Files/Microsoft Office/root/Office16/EXCEL.EXE");


    [Header("文件后缀: 指定文件后缀用哪个IDEA打开")]
    [SerializeField]
    public List<OpenAssetExtension> extensions = new List<OpenAssetExtension>();

    public OpenAssetExtension GetExtension(string path)
    {
        foreach(OpenAssetExtension item in extensions)
        {
            if (path.EndsWith(item.extension))
                return item;
        }
        return null;
    }

    public OpenAssetIdea GetIdea(string path)
    {
        OpenAssetExtension ext = GetExtension(path);
        if(ext == null)
        {
            return null;
        }
        
        switch(ext.idea)
        {
            case OpenAssetIdeaType.Sublime:
                return ideaSumlime;

            case OpenAssetIdeaType.IntelliJ:
                return ideaIntelliJ;

            case OpenAssetIdeaType.Excel:
                return ideaExcel;
        }
        return null;
    }


    public void OpenFile(string path, int line = 1)
    {
        bool result = Open(path, line);
        if(!result)
        {
            InternalEditorUtility.OpenFileAtLineExternal(path, line);
        }
    }

    public bool Open(string path, int line = 1)
    {
#if UNITY_EDITOR
        OpenAssetExtension ext = GetExtension(path);
        if (ext == null) return false;
        if(ext.idea == OpenAssetIdeaType.InternalEditor)
        {
            InternalEditorUtility.OpenFileAtLineExternal(path, line);
            return true;
        }

        OpenAssetIdea idea = GetIdea(path);

        if(idea == null)
            return false;

        if (!File.Exists(idea.appPath))
            return false;



        if (!path.StartsWith(Application.dataPath))
        {
            path = Application.dataPath.Replace("Assets", "/") + path;
        }

        path = path.Replace("//", "/");

        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = idea.appPath;
        switch(idea.type)
        {
            case OpenAssetIdeaType.Sublime:
                startInfo.Arguments = string.Format("{0}:{1}:0", path, line);
                break;
            case OpenAssetIdeaType.IntelliJ:
                startInfo.Arguments = string.Format("{0} --line {1} {2}", "", line, path);
                break;
            default:
                startInfo.Arguments = string.Format("{0}", path); ;
                break;
        }
        process.StartInfo = startInfo;

        if (process.Start())
        {
            SetForegroundWindow(process.Handle);
            SetActiveWindow(process.Handle);
            SetFocus(process.Handle);
        }

        return true;


#else
        return false;
#endif
    }


}

public enum OpenAssetIdeaType
{
    // 内置
    InternalEditor = 0,

    Sublime,

    IntelliJ,

    Excel,
}

[Serializable]
public class OpenAssetIdea
{
    // 应用类型
    public OpenAssetIdeaType    type;

    // 应用路径
    public string               appPath;


    public OpenAssetIdea()
    {
    }

    public OpenAssetIdea(OpenAssetIdeaType type, string appPath)
    {
        this.type = type;
        this.appPath = appPath;
    }
}


[Serializable]
public class OpenAssetExtension
{
    // 扩展名
    public string extension;

    // 应用类型
    public OpenAssetIdeaType idea;
}