#if UNITY_EDITOR
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
 
[InitializeOnLoad]
public class FileWatcher // modified from https://forum.unity.com/threads/editor-compile-in-background.627952/#post-6989918
{
    public static string ScriptPath = "./Assets/Assemblies/";
    public static bool SetRefresh;
 
    static FileWatcher()
    {
        Debug.Log("Listening to " + ScriptPath);
        ThreadPool.QueueUserWorkItem(MonitorDirectory, ScriptPath);
        EditorApplication.update += OnUpdate;
    }
 
    private static void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        string ext = Path.GetExtension(e.FullPath);

        if (ext == ".dll") {
            Debug.Log("Auto refreshing due to change in " + ScriptPath);
            SetRefresh = true;
        }
    }
 
    private static void MonitorDirectory(object obj)
    {
        string path = (string)obj;
 
        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
        fileSystemWatcher.Path = path;
        fileSystemWatcher.IncludeSubdirectories = false;
        fileSystemWatcher.Changed += FileSystemWatcher_Changed;
        fileSystemWatcher.Created += FileSystemWatcher_Changed;
        fileSystemWatcher.Renamed += FileSystemWatcher_Changed;
        fileSystemWatcher.Deleted += FileSystemWatcher_Changed;
        fileSystemWatcher.EnableRaisingEvents = true;
    }
 
    private static void OnUpdate()
    {
        if (!SetRefresh) return;
 
        if (EditorApplication.isCompiling) return;
        if (EditorApplication.isUpdating) return;
 
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport & ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(ScriptPath, ImportAssetOptions.ForceSynchronousImport & ImportAssetOptions.ForceUpdate);
        SetRefresh = false;
    }
}
#endif
