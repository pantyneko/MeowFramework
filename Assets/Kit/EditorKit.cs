#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine;

namespace Panty
{
    public static class EditorKit
    {
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