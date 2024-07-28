#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Panty
{
    public class SO_StGrid : ScriptableObject
    {
        [SerializeField] private StGrid mGrid;
        public StGrid Grid => mGrid;
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(SO_StGrid))]
    public class StGridEditor : Editor
    {
        private GUIStyle centeredStyle;
        private GUIStyle boldLabelStyle;

        public override void OnInspectorGUI()
        {
            // 缓存样式以提高性能
            if (centeredStyle == null)
            {
                centeredStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }
            if (boldLabelStyle == null)
            {
                boldLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold
                };
            }
            StGrid grid = ((SO_StGrid)target).Grid;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid Properties", boldLabelStyle);
            EditorGUILayout.Space();
            DrawGridProperties(grid);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid Preview", boldLabelStyle);
            EditorGUILayout.Space();
            DrawGridPreview(grid);
        }
        private void DrawGridProperties(StGrid grid)
        {
            // 绘制组合字段
            EditorGUILayout.BeginVertical();

            grid.Origin = EditorGUILayout.Vector2Field("左下角：", grid.Origin);
            EditorGUILayout.Space();
            grid.CellSize = EditorGUILayout.Vector2Field("格子尺寸：", grid.CellSize);
            EditorGUILayout.Space();
            grid.RowColm = EditorGUILayout.Vector2IntField("行列数：", grid.RowColm);
            EditorGUILayout.Space();

            GUI.enabled = false; // 禁用 GUI
            EditorGUILayout.Vector2Field("网格区域大小：", grid.BoundSize);
            EditorGUILayout.Space();
            EditorGUILayout.FloatField("格子对角:", grid.Hypotenuse);
            EditorGUILayout.Space();
            GUI.enabled = true; // 禁用 GUI

            EditorGUILayout.EndVertical();
        }
        private void DrawGridPreview(StGrid grid)
        {
            if (grid == null) return;

            float inspectorWidth = EditorGUIUtility.currentViewWidth - 30; // 添加更多填充
            float cellWidth = inspectorWidth / grid.colm;
            float cellHeight = cellWidth / grid.AspectRatio;
            float gridHeight = cellHeight * grid.row;

            GUILayout.Space(gridHeight);
            Rect rect = GUILayoutUtility.GetLastRect();
            Handles.BeginGUI();
            // 绘制网格线
            Handles.color = Color.green;
            for (int r = 0; r <= grid.row; r++)
            {
                float y = rect.y + r * cellHeight;
                Handles.DrawLine(new Vector3(rect.x, y), new Vector3(rect.x + inspectorWidth, y));
            }
            for (int c = 0; c <= grid.colm; c++)
            {
                float x = rect.x + c * cellWidth;
                Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.y + gridHeight));
            }
            // 绘制单元格信息
            foreach (var (r, c) in grid.RowMajorIndices())
            {
                float x = rect.x + (c + 0.5f) * cellWidth;
                float y = rect.y + gridHeight - (r + 0.5f) * cellHeight;
                Handles.Label(new Vector3(x, y), $"({r}, {c})", centeredStyle);
            }
            Handles.EndGUI();
        }
    }
#endif
}