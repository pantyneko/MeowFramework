#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace Panty
{
    public class ArrayGridEditor : Editor
    {
        private SerializedProperty spritesProperty;
        private const float SpriteSize = 64f;
        private const float Padding = 10f;
        private const float VerticalSpacing = 20f;

        private int columns;
        private Texture2D[] previewCache;
        private GUIStyle indexStyle;
        private GUIStyle buttonStyle;
        private GUIStyle sizeLabelStyle;
        private GUILayoutOption BtnWidth;

        private void OnEnable()
        {
            spritesProperty = serializedObject.FindProperty("arr");
            UpdatePreviewCache();
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // 动态计算每行的列数
            float viewWidth = EditorGUIUtility.currentViewWidth;
            columns = Mathf.Max(1, Mathf.FloorToInt((viewWidth - Padding) / (SpriteSize + Padding)));

            EnsureStylesInitialized();
            DrawGridLayout();

            serializedObject.ApplyModifiedProperties();
        }
        private void EnsureStylesInitialized()
        {
            if (indexStyle == null)
            {
                indexStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { background = TextureEx.GetSolidTex(1, 1, new Color32(0, 0, 0, 128)) }
                };
                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 30f
                };
                sizeLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = 30f
                };
                BtnWidth = GUILayout.Width(80);
            }
        }
        private void DrawGridLayout()
        {
            int totalItems = spritesProperty.arraySize;
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("AddLast", "在末尾添加一个元素"), buttonStyle, BtnWidth))
            {
                spritesProperty.arraySize++;
                spritesProperty.GetArrayElementAtIndex(totalItems).objectReferenceValue = null;
            }
            EditorGUI.BeginDisabledGroup(totalItems == 0);
            if (GUILayout.Button(new GUIContent("RmvLast", "移除最后一个元素"), buttonStyle, BtnWidth))
            {
                spritesProperty.DeleteArrayElementAtIndex(totalItems - 1);
            }
            if (GUILayout.Button(new GUIContent("ClearAll", "清空数据"), buttonStyle, BtnWidth))
            {
                if (EditorKit.Dialog("是否清空数组 该操作无法撤销"))
                {
                    spritesProperty.ClearArray();
                }
            }
            EditorGUI.EndDisabledGroup();

            if (previewCache.Length != spritesProperty.arraySize)
            {
                UpdatePreviewCache();
            }
            // 前面的操作有可能变更数组长度 
            totalItems = spritesProperty.arraySize;

            GUILayout.Space(Padding);
            GUILayout.Label($"Size: {totalItems}", sizeLabelStyle);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(VerticalSpacing);

            if (totalItems == 0)
            {
                GUILayout.Label("无可用元素 使用上面的按钮添加或删除元素", EditorStyles.boldLabel);
                return;
            }
            for (int i = 0; i < totalItems; i++)
            {
                if (i % columns == 0)
                {
                    if (i != 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(VerticalSpacing);
                    }
                    EditorGUILayout.BeginHorizontal();
                }
                EditorGUILayout.BeginVertical(GUILayout.Width(SpriteSize), GUILayout.Height(SpriteSize));
                DrawSpriteField(spritesProperty.GetArrayElementAtIndex(i), i);
                EditorGUILayout.EndVertical();
                GUILayout.Space(Padding);
            }
            // 确保最后一行结束
            EditorGUILayout.EndHorizontal();
        }
        private void DrawSpriteField(SerializedProperty property, int index)
        {
            Rect rect = GUILayoutUtility.GetRect(SpriteSize, SpriteSize);
            EditorGUI.PropertyField(rect, property, GUIContent.none);

            var obj = property.objectReferenceValue;
            if (obj != null)
            {
                if (GUI.changed) previewCache[index] = GetPreview(obj);
                if (previewCache[index] != null)
                {
                    Graphics.DrawTexture(rect, TextureEx.InvTPG);
                    Graphics.DrawTexture(rect, obj is Texture2D ? (Texture2D)obj : previewCache[index]);
                }
            }
            GUI.Label(new Rect(rect.x, rect.y, 20, 20), index.ToString(), indexStyle);
        }
        private void UpdatePreviewCache()
        {
            int len = spritesProperty.arraySize;
            previewCache = new Texture2D[len];
            for (int i = 0; i < len; i++)
            {
                var obj = spritesProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                if (obj == null)
                    previewCache[i] = null;
                else
                {
                    previewCache[i] = GetPreview(obj);
                    previewCache[i].filterMode = FilterMode.Point;
                }
            }
        }
        private Texture2D GetPreview(Object obj)
        {
            return AssetPreview.GetAssetPreview(obj) ?? TextureEx.GetSolidTex(1, 1, ColorEx.white32);
        }
    }
}
#endif