#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Reflection;
using System;
using System.Collections;

namespace Panty
{
    public class I
    {
        public static string Space = "Panty.Test";
        public static string TPath = "Assets/Scripts";
        public static string Hub = "Test";
        public readonly static string[] Search = new string[] { "Assets" };
    }
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
        public static T GetAttribute<T>(SerializedProperty property) where T : class
        {
            T[] attributes = GetAttributes<T>(property);
            return (attributes.Length > 0) ? attributes[0] : null;
        }
        private static object GetValue_Imp(object source, string name)
        {
            if (source != null)
            {
                Type type = source.GetType();
                while (type != null)
                {
                    var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (field != null) return field.GetValue(source);
                    var property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property != null) return property.GetValue(source, null);
                    type = type.BaseType;
                }
            }
            return null;
        }
        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null) return null;
            var enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext()) return null;
            }
            return enumerator.Current;
        }
        public static T[] GetAttributes<T>(SerializedProperty property) where T : class
        {
            object obj = property.serializedObject.targetObject;
            string[] elements = property.propertyPath.Replace(".Array.data[", "[").Split('.');
            for (int i = 0; i < elements.Length - 1; i++)
            {
                string element = elements[i];
                int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = element.Contains("[") ?
                    GetValue_Imp(obj, element.Substring(0, element.IndexOf("[")), index) :
                    GetValue_Imp(obj, element);
            }
            var fieldInfo = RefKit.GetField(obj, property.name);
            return fieldInfo == null ? new T[0] : (T[])fieldInfo.GetCustomAttributes(typeof(T), true);
        }
        public static void CreatScript(string path, string name, string tag, string tmple, bool ignoreRoot = true)
        {
            path = ignoreRoot ? path : $"Assets/{path}";
            FileKit.TryCreateDirectory(path);
            var tmp = $"{name}{tag}";
            if (FindMonoAsset(tmp))
                Tips($"已存在相同名字的脚本：{tmp}");
            else
            {
                string str = Regex.Replace(tmple, Regex.Escape("@"), name);
                FileKit.WriteFile($"{path}/{tmp}.cs", str);
                AssetDatabase.Refresh();
            }
        }
        public static bool Dialog(string msg)
        {
            return EditorUtility.DisplayDialog("让我来想想...喵~", msg, "Yes", "No");
        }
        public static void Tips(string msg)
        {
            EditorUtility.DisplayDialog("让我来想想...喵~", msg, "晓得了");
        }
        public static MonoScript FindMonoAsset(string typeName)
        {
            var guids = AssetDatabase.FindAssets(typeName, I.Search);
            if (guids.Length > 0)
            {
                string full = $"{I.Space}.{typeName}";
                foreach (var id in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(id);
                    var o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    if (o is MonoScript mono)
                    {
                        var type = mono.GetClass();
                        if (type == null && mono.name == typeName) return mono;
                        if (type != null && type.FullName == full) return mono;
                    }
                }
            }
            return null;
        }
        public static string GetMonoPath(string typeName, string fileName)
        {
            var guids = AssetDatabase.FindAssets(typeName, I.Search);
            if (guids.Length > 0)
            {
                string full = $"{I.Space}.{typeName}";
                foreach (var id in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(id);
                    if (Path.GetFileName(path) == fileName)
                    {
                        var o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        if (o is MonoScript mono)
                        {
                            var type = mono.GetClass();
                            if (type == null && mono.name == typeName) return path;
                            if (type != null && type.FullName == full) return path;
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 检查所选资源是否为文件夹，如果不是则将其转换为文件夹并返回文件夹路径列表。
        /// </summary>
        /// <returns>文件夹路径列表</returns>
        public static PArray<string> EnsureSelectedIsFolder()
        {
            // 获取所有选中的资源
            var selectedObjects = Selection.objects;
            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                "No assets selected.".Log();
                return null;
            }
            var folderPaths = new PArray<string>();
            foreach (var selectedObject in selectedObjects)
            {
                string selectedPath = AssetDatabase.GetAssetPath(selectedObject);
                // 如果选中的是场景中的对象，尝试找到它的Prefab路径
                if (string.IsNullOrEmpty(selectedPath))
                {
                    var go = selectedObject as GameObject;
                    if (go != null)
                    {
                        PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(go);
                        if (prefabAssetType != PrefabAssetType.NotAPrefab)
                        {
                            selectedPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(go));
                        }
                    }
                    // 如果仍然没有路径，则跳过
                    if (string.IsNullOrEmpty(selectedPath))
                    {
                        $"Selected object {selectedObject.name} is not part of the AssetDatabase.".Log();
                        continue;
                    }
                }
                // 使用C#的文件和目录操作来处理路径
                if (Directory.Exists(selectedPath))
                {
                    folderPaths.Push(selectedPath);
                    continue;
                }
                string parentFolderPath = Path.GetDirectoryName(selectedPath);
                if (string.IsNullOrEmpty(parentFolderPath))
                {
                    $"Unable to determine the parent folder path for {selectedObject.name}.".Log();
                    continue;
                }
                string newFolderPath = Path.GetFileNameWithoutExtension(selectedPath);
                newFolderPath = Path.Combine(parentFolderPath, newFolderPath);

                // 使用C#的文件系统操作来创建新文件夹
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.CreateDirectory(newFolderPath);
                    // 刷新AssetDatabase以确保新创建的文件夹被正确识别
                    AssetDatabase.Refresh();
                }
                folderPaths.Push(newFolderPath);
            }
            return folderPaths;
        }
    }
}
#endif