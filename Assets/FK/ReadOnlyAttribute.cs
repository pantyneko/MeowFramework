using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Panty
{
    public class ReadOnlyAttribute : PropertyAttribute { }
    public class PropLabelAttribute : PropertyAttribute
    {
        public string Name { get; }
        public PropLabelAttribute(string name) => Name = name;
    }
#if UNITY_EDITOR
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