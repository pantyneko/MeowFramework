using UnityEngine;
using UnityEditor;

namespace Panty
{
    public class SOCreateEditor : EditorWindow
    {
        private SerializedObject serializedObject;
        private string savePath = "Assets/Resources/SO";
        private string fileName = "NewSO";

        [MenuItem("PnTool/SoCreator &2")]
        private static void Create() => EditorKit.ShowOrHide<SOCreateEditor>(out var _);
        private void OnGUI()
        {
            GUILayout.Space(10);
            Rect dropArea = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUI.Box(dropArea, "Drop ScriptableObject Here", EditorStyles.helpBox);
            Event evt = Event.current;
            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    if (DragAndDrop.paths.Length > 0)
                    {
                        MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(DragAndDrop.paths[0]);
                        if (script != null)
                        {
                            System.Type scriptType = script.GetClass();
                            if (scriptType != null && scriptType.IsSubclassOf(typeof(ScriptableObject)))
                            {
                                serializedObject = new SerializedObject(ScriptableObject.CreateInstance(scriptType));
                                fileName = script.name;
                            }
                        }
                    }
                }
                evt.Use();
            }
            GUILayout.Space(10);
            if (serializedObject != null) EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            fileName = EditorGUILayout.TextField("File Name", fileName);
            GUILayout.Space(5);
            if (GUILayout.Button("Create") && serializedObject != null)
            {
                FileKit.TryCreateDirectory(savePath);
                string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/{fileName}.asset");
                var instance = new SerializedObject(serializedObject.targetObject);
                instance.Update();
                instance.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(instance.targetObject, uniqueFileName);
                AssetDatabase.ImportAsset(uniqueFileName);
                // 清空serializedObject，以便连续创建
                serializedObject = null;
            }
            GUILayout.Space(10);
        }
    }
}