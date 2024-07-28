using UnityEngine;

namespace Panty
{
    [ExecuteInEditMode]
    public class ControllableGrid : MonoBehaviour
    {
        [SerializeField] protected SO_StGrid mData;
        public StGrid Grid => mData.Grid;
#if UNITY_EDITOR       
        [SerializeField] private bool mProportional, mIsContrl, mIsRow;
        [SerializeField] private Material mat;
        [SerializeField] private float CtrlPointSize = 0.2f;
        [SerializeField] private COOR_Mode CoorMode = COOR_Mode.World;
        [SerializeField] private Color color = Color.red;

        private bool mIsDrag;
        private float OffX, OffY;
        private Vector2 mLastMouse;
        private GUIStyle mStyle;
        private Dir4 mDir = Dir4.None;

        private void Start()
        {
            mStyle = new GUIStyle()
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
            };
            mStyle.normal.textColor = Color.white;
        }
        private void OnRenderObject()
        {
            if (mData == null) return;
            GLKit.Render(CoorMode, GizmosDraw);
        }
        private void OnGUI()
        {
            var rect = Rect.zero;
            foreach (var (r, c) in Grid.RowMajorIndices())
            {
                Grid.CellIndexToCoordCenter(r, c, out float x, out float y);

                Vector2 screenPos = CoorMode switch
                {
                    COOR_Mode.World => Camera.main.WorldToScreenPoint(new Vector3(x, y)),
                    COOR_Mode.Viewport => Camera.main.ViewportToScreenPoint(new Vector3(x, y)),
                    _ => new Vector3(x, y)
                };
                rect.x = screenPos.x;
                rect.y = Screen.height - screenPos.y;
                int id = mIsRow ? Grid.CellIndexToLinearIndex_RowMajor(r, c) : Grid.CellIndexToLinearIndex_ColMajor(r, c);
                GUI.Label(rect, $"[ 行：{r},列：{c} ]\r\n\r\n[ 索引：{id} ]", mStyle);
            }
        }
        protected virtual void GizmosDraw()
        {
            mat.color = color;
            mat.SetPass(0);
            Grid.DrawGrid();
            if (mDir == Dir4.None || Input.GetMouseButton(0)) return;
            Grid.GetCorner(mDir).DrawRect(CtrlPointSize);
        }
        protected virtual void GridMove(float x, float y)
        {
            Grid.xMin = x - OffX;
            Grid.yMin = y - OffY;
        }
        protected virtual void CalcOffset(float x, float y)
        {
            OffX = x - Grid.xMin;
            OffY = y - Grid.yMin;
        }
        private void Update()
        {
            if (!mIsContrl) return;
            Vector2 mouse = Input.mousePosition;
            if (CoorMode == COOR_Mode.World)
                mouse = Camera.main.ScreenToWorldPoint(mouse);
            if (Input.GetMouseButtonDown(0))
            {
                if (mDir == Dir4.None && Grid.Contains(mouse))
                {
                    CalcOffset(mouse.x, mouse.y);
                    mIsDrag = true;
                }
                mLastMouse = mouse;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                mLastMouse = mouse;
            }
            else if (Input.GetMouseButton(1))
            {
                Vector2 delta = mouse - mLastMouse;
                if (delta.x.Abs() >= 0.001f || delta.y.Abs() >= 0.001f)
                {
                    if (mProportional)
                        Grid.ScaleFromCenter(delta.x, delta.y);
                    else
                        Grid.Resize(Dir4.All, delta.x, delta.y);
                }
                mLastMouse = mouse;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                mIsDrag = false;
            }
            else if (mIsDrag)
            {
                GridMove(mouse.x, mouse.y);
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 delta = mouse - mLastMouse;
                if (delta.x.Abs() >= 0.001f || delta.y.Abs() >= 0.001f)
                {
                    Grid.DragResize(mDir, delta.x, delta.y);
                }
                mLastMouse = mouse;
            }
            else
            {
                mDir = Grid.CheckEdgeCorner(mouse.x, mouse.y, CtrlPointSize * 0.5f);
            }
        }
#endif
    }
}