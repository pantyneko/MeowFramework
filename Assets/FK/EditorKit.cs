#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Panty
{
    public static class EditorKit
    {
        public static bool ShowOrHide<T>(out T win, bool isUtility = false, string title = null) where T : EditorWindow
        {
            T[] array = Resources.FindObjectsOfTypeAll<T>();
            if (array == null || array.Length == 0)
            {
                win = ScriptableObject.CreateInstance<T>();
                win.titleContent = new GUIContent(title ?? typeof(T).Name);
                if (isUtility)
                {
                    win.ShowUtility();
                }
                else
                {
                    win.Show();
                    win.Focus();
                }
                return true;
            }
            else
            {
                win = array[0];
                win.Close();
                return false;
            }
        }
        public static void CreatScript(string path, string name, string tag, string tmple, bool ignoreRoot = true)
        {
            path = ignoreRoot ? path : $"Assets/{path}";
            FileKit.TryCreateDirectory(path);
            var tmp = $"{name}{tag}";
            if (FindMonoAsset(tmp))
            {
                Tips("脚本已存在");
                return;
            }
            string str = Regex.Replace(tmple, Regex.Escape("@"), name);
            File.WriteAllText($"{path}/{tmp}.cs", str);
            AssetDatabase.Refresh();
        }
        public static bool Dialog(string msg)
        {
            return EditorUtility.DisplayDialog("让我来想想...喵~", msg, "Yes", "No");
        }
        public static void Tips(string msg)
        {
            EditorUtility.DisplayDialog("让我来想想...喵~", msg, "晓得了");
        }
        public static MonoScript FindMonoAsset(string typeName, string[] searchInFolders = null)
        {
            var guids = AssetDatabase.FindAssets(typeName, searchInFolders);
            if (guids.Length > 0)
            {
                string full = $"{I.Space}.{typeName}";
                foreach (var id in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(id);
                    var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (mono.GetClass().FullName == full) return mono;
                }
            }
            return null;
        }
        public static string GetMonoPath(string typeName, string fileName, string[] searchInFolders = null)
        {
            var guids = AssetDatabase.FindAssets(typeName, searchInFolders);
            if (guids.Length > 0)
            {
                string full = $"{I.Space}.{typeName}";
                foreach (var id in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(id);
                    if (Path.GetFileName(path) == fileName)
                    {
                        var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                        var type = mono.GetClass();
                        if (type == null || type.FullName == full) return path;
                    }
                }
            }
            return null;
        }
        public static string GetSelectionFolder()
        {
            string[] guids = Selection.assetGUIDs;

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.IsValidFolder(path) ? path : Directory.GetParent(path).FullName;
            }
            return string.Empty; // 没有选中文件夹
        }
    }
}
#endif