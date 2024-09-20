#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace Panty
{
    // 用于处理打开资源选择器
    // EditorGUIUtility.ShowObjectPicker<Object>()
    // EditorGUIUtility.GetObjectPickerControlID()
    // EditorGUIUtility.GetObjectPickerObject()
    // 自定义属性类，用于在属性上添加预览图像
    public class AssetPreviewAttribute : PropertyAttribute
    {
        public int Width { get; }
        public int Height { get; }
        public AssetPreviewAttribute(int width = 32, int height = 32)
        {
            Width = width;
            Height = height;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AssetPreviewAttribute))]
    public class AssetPreviewDrawer : PropertyDrawer
    {
        private AssetPreviewAttribute cachedAttribute;
        private bool InvBg, altPressed;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                cachedAttribute ??= EditorKit.GetAttribute<AssetPreviewAttribute>(property);
                return EditorGUIUtility.singleLineHeight + cachedAttribute.Height + 4;
            }
            return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUI.BeginProperty(position, label, property);

                Rect labelRect = position;
                labelRect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(labelRect, label);

                var attribute = cachedAttribute ??= EditorKit.GetAttribute<AssetPreviewAttribute>(property);
                var previewRect = new Rect(position.x, position.y + labelRect.height + 2, attribute.Width + 20, attribute.Height);
                EditorGUI.ObjectField(previewRect, property, GUIContent.none);

                Event e = Event.current;
                // 检查是否按下 Alt 键
                if (e.alt)
                {
                    if (!altPressed && previewRect.Contains(e.mousePosition))
                    {
                        InvBg = !InvBg;
                        altPressed = true;
                        e.Use(); // 使用事件，防止其他控件处理
                    }
                }
                else altPressed = false;
                // 绘制预览图像
                var previewTexture = AssetPreview.GetAssetPreview(property.objectReferenceValue);
                if (previewTexture != null)
                {
                    previewRect.width = attribute.Width;
                    previewTexture.filterMode = FilterMode.Point;
                    Graphics.DrawTexture(previewRect, InvBg ? TextureEx.InvTPG : TextureEx.BaseTPG);
                    Graphics.DrawTexture(previewRect, previewTexture);
                }
                EditorGUI.EndProperty();
            }
            else $"{property.name} doesn't have an asset preview".Log();
        }
    }
#endif
}