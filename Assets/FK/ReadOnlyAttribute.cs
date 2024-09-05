#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Panty
{
    public class ReadOnlyAttribute : PropertyAttribute { }
    // 自定义一个ReadOnly属性的绘制器
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;  // 禁用GUI使其变为只读
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;   // 重新启用GUI
        }
    }
}
#endif