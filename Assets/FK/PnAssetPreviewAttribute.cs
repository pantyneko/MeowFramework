#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Panty
{
    [CustomPropertyDrawer(typeof(PnAssetPreviewAttribute))]
    public class PnAssetPreviewDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return base.GetPropertyHeight(property, label);

            var attribute = EditorKit.GetAttribute<PnAssetPreviewAttribute>(property);
            return EditorGUIUtility.singleLineHeight + attribute.Height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                $"{property.name} doesn't have an asset preview".Log();
                return;
            }
            EditorGUI.BeginProperty(position, label, property);

            var attribute = EditorKit.GetAttribute<PnAssetPreviewAttribute>(property);
            Rect labelRect = position;
            labelRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(labelRect, label);

            var previewTexture = AssetPreview.GetAssetPreview(property.objectReferenceValue);

            if (previewTexture == null)
                DrawPropertyFieldWithoutPreview(position, property, attribute);
            else
                DrawPreviewTexture(position, property, previewTexture, attribute);

            EditorGUI.EndProperty();
        }
        private void DrawPreviewTexture(Rect position, SerializedProperty property, Texture2D previewTexture, PnAssetPreviewAttribute attribute)
        {
            var previewRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, attribute.Width + 20, attribute.Height);
            EditorGUI.ObjectField(previewRect, property, GUIContent.none);
            previewRect.width = attribute.Width;
            previewTexture.filterMode = FilterMode.Point;
            GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);
        }
        private void DrawPropertyFieldWithoutPreview(Rect position, SerializedProperty property, PnAssetPreviewAttribute attribute)
        {
            var propertyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, attribute.Width + 20, attribute.Height);
            EditorGUI.PropertyField(propertyRect, property, GUIContent.none);
        }
    }
    public class PnAssetPreviewAttribute : PropertyAttribute
    {
        public const int DefaultWidth = 32;
        public const int DefaultHeight = 32;
        public int Width { get; }
        public int Height { get; }

        public PnAssetPreviewAttribute(int width = DefaultWidth, int height = DefaultHeight)
        {
            Width = width;
            Height = height;
        }
    }
}
#endif