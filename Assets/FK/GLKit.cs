using System;
using UnityEngine;

namespace Panty
{
    /// <summary>
    /// 坐标模式
    /// </summary>
    public enum COOR_Mode : byte { Screen, World, Viewport, UILocal }
    public static class GLKit
    {
        /// <summary>
        /// 渲染 GL 图形
        /// </summary>
        /// <param name="callback">渲染回调</param>
        /// <param name="mode">渲染方式</param>
        public static void Render(COOR_Mode mode, Action callback)
        {
            GL.PushMatrix();
            switch (mode)
            {
                case COOR_Mode.Screen: GL.LoadPixelMatrix(); break;
                case COOR_Mode.Viewport: GL.LoadOrtho(); break;
            }
            callback?.Invoke();
            GL.PopMatrix();
        }
        public static void Draw(this Rect rect, float z = 0f, bool isWire = true)
        {
            DrawRectLeft(rect.x, rect.y, rect.width, rect.height, z, isWire);
        }
        public static void DrawRect(this Vector2 p, float size = 1f, float z = 0f, bool isWire = true) =>
            DrawRect(p.x, p.y, size, size, z, isWire);
        /// <summary>
        /// 根据中心点绘制矩形
        /// </summary>
        /// <param name="w">矩形宽度</param>
        /// <param name="h">矩形高度</param>
        /// <param name="isWire">是否为线框渲染</param>
        public static void DrawRect(float x, float y, float w = 1f, float h = 1f, float z = 0f, bool isWire = true)
        {
            GL.Begin(isWire ? GL.LINE_STRIP : GL.QUADS);

            w *= 0.5f; h *= 0.5f;

            float xl = x - w;
            float xr = x + w;
            float yl = y - h;
            float yr = y + h;

            GL.Vertex3(xl, yr, z);
            GL.Vertex3(xr, yr, z);
            GL.Vertex3(xr, yl, z);
            GL.Vertex3(xl, yl, z);
            GL.Vertex3(xl, yr, z);

            GL.End();
        }
        /// <summary>
        /// 根据两个对角点绘制矩形
        /// </summary>
        /// <param name="start">开始点</param>
        /// <param name="end">结束点</param>
        /// <param name="isWire">是否为线框渲染</param>
        public static void DrawRect(Vector2 start, Vector2 end, float z = 0f, bool isWire = true)
        {
            GL.Begin(isWire ? GL.LINE_STRIP : GL.QUADS);
            // 如果不是线框 并且没有反向绘制顶点
            if (!isWire && ((start.x > end.x && start.y < end.y) ||
                (start.x < end.x && start.y > end.y))) start.Swap(ref end);

            GL.Vertex3(start.x, start.y, z);
            GL.Vertex3(start.x, end.y, z);
            GL.Vertex3(end.x, end.y, z);
            GL.Vertex3(end.x, start.y, z);
            GL.Vertex3(start.x, start.y, z);

            GL.End();
        }
        /// <summary>
        /// 根据左下角绘制矩形
        /// </summary>
        public static void DrawRectLeft(float x, float y, float w = 1f, float h = 1f, float z = 0f, bool isWire = true)
        {
            GL.Begin(isWire ? GL.LINE_STRIP : GL.QUADS);

            float xw = x + w;
            float yh = y + h;

            GL.Vertex3(x, y, z);
            GL.Vertex3(x, yh, z);
            GL.Vertex3(xw, yh, z);
            GL.Vertex3(xw, y, z);
            GL.Vertex3(x, y, z);

            GL.End();
        }
        /// <summary>
        /// 绘制网格
        /// </summary>
        /// <param name="sx">左侧起始x</param>
        /// <param name="sy">下侧起始y</param>
        /// <param name="row">多少行</param>
        /// <param name="colm">多少列</param>
        public static void DrawGrid(int sx, int sy, int row, int colm, float z = 0f)
        {
            float tmp;
            float e = sy + row;
            int i = 0;
            GL.Begin(GL.LINES);
            while (i <= colm)
            {
                tmp = sx + i++;
                GL.Vertex3(tmp, sy, z);
                GL.Vertex3(tmp, e, z);
            }
            i = 0;
            e = sx + colm;
            while (i <= row)
            {
                tmp = sy + i++;
                GL.Vertex3(sx, tmp, z);
                GL.Vertex3(e, tmp, z);
            }
            GL.End();
        }
        /// <summary>
        /// 绘制网格
        /// </summary>
        /// <param name="sx">左侧起始x</param>
        /// <param name="sy">下侧起始y</param>
        /// <param name="row">多少行</param>
        /// <param name="colm">多少列</param>
        public static void DrawGrid(float sx, float sy, int row, int colm, float cw, float ch, float z = 0f)
        {
            float tmp;
            float e = sy + row * ch;
            int i = 0;
            GL.Begin(GL.LINES);
            while (i <= colm)
            {
                tmp = sx + i++ * cw;
                GL.Vertex3(tmp, sy, z);
                GL.Vertex3(tmp, e, z);
            }
            i = 0;
            e = sx + colm * cw;
            while (i <= row)
            {
                tmp = sy + i++ * ch;
                GL.Vertex3(sx, tmp, z);
                GL.Vertex3(e, tmp, z);
            }
            GL.End();
        }
        /// <summary>
        /// 根据顶点绘制连续线条
        /// </summary>
        public static void DrawLine(this Vector2[] points, bool isClosed, float z = 0f)
        {
            int len = points.Length;
            if (len < 2) return;
            GL.Begin(GL.LINE_STRIP);
            foreach (var p in points)
            {
                GL.Vertex3(p.x, p.y, z);
            }
            if (isClosed)
            {
                var p = points[0];
                GL.Vertex3(p.x, p.y, z);
            }
            GL.End();
        }
    }
}