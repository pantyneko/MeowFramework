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
        public static void CreatScript(string path, string name, string tag, string tmple)
        {
            string tmp = $"{Application.dataPath}/{path}";
            FileKit.TryCreateDirectory(tmp);
            path = $"{tmp}/{name}{tag}.cs";
            // 查找是否存在同名脚本 如果存在就跳出
            if (File.Exists(path))
            {
                Tips("脚本已存在");
                return;
            }
            File.WriteAllText(path, Regex.Replace(tmple, Regex.Escape("@"), name));
            AssetDatabase.Refresh();
        }
        public static void CreatScript(string name, string tag, string tmple)
        {
            string path = $"{Application.dataPath}/{name}{tag}.cs";
            // 查找是否存在同名脚本 如果存在就跳出
            if (File.Exists(path))
            {
                Tips("脚本已存在");
                return;
            }
            File.WriteAllText(path, Regex.Replace(tmple, Regex.Escape("@"), name));
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
        public static string GetSelectionFolder()
        {
            string[] guids = Selection.assetGUIDs;

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);

                if (AssetDatabase.IsValidFolder(path))
                {
                    string projectPath = Application.dataPath;
                    projectPath = projectPath.Substring(0, projectPath.Length - 6);// 6 => "Assets"
                    path = projectPath + path;
                }
                else
                {
                    path = Directory.GetParent(path).FullName;
                }
                return path;
            }
            return string.Empty; // 没有选中文件夹
        }
    }
}
#endif