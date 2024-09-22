using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Panty
{
#if UNITY_EDITOR
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
#endif
    public class SerializeOrReadOnlyAttribute : PropertyAttribute { }
#if UNITY_EDITOR
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
#endif
    public class ReadOnlyAttribute : PropertyAttribute { }
#if UNITY_EDITOR
    public class PropLabelAttribute : PropertyAttribute
    {
        public string Name { get; }
        public PropLabelAttribute(string name) => Name = name;
    }
    [CustomPropertyDrawer(typeof(SerializeOrReadOnlyAttribute))]
    public class SerializeOrReadOnlyDrawer : PropertyDrawer
    {
        private static bool EnabledSerialize = false; // 初始状态为开启
        [MenuItem("PnTool/DebuggingMode")]
        public static void ToggleSerialization()
        {
            Menu.SetChecked("PnTool/DebuggingMode", EnabledSerialize = !EnabledSerialize);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 检查全局的序列化开关
            if (EnabledSerialize)
            {
                EditorGUI.BeginDisabledGroup(true);  // 禁用编辑
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.EndDisabledGroup();
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 如果序列化开关开启，则返回正常高度；否则返回 0，高度为 0 则不会占用空间
            return EnabledSerialize ? base.GetPropertyHeight(property, label) : 0f;
        }
    }
    [CustomPropertyDrawer(typeof(PropLabelAttribute))]
    public class PropLabelDrawer : PropertyDrawer
    {
        private GUIContent _label = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_label == null)
            {
                string name = (attribute as PropLabelAttribute).Name;
                _label = new GUIContent(name);
            }
            EditorGUI.PropertyField(position, property, _label);
        }
    }
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);  // 禁用编辑
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
        }
    }
#endif
}