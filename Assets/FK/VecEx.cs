using System;
using Unity.Mathematics;
using UnityEngine;

namespace Panty
{
    [Flags]
    public enum Dir4 : byte
    {
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        All = 16,
        None = 32,
    }
    public static class VecEx
    {
        public readonly static float2 Up = new float2(0f, 1f);
        public static float2 RandomVec(this float2 dir, float offset)
        {
            var rad = MathF.Atan2(dir.y, dir.x);
            rad = RandomEx.NextFloat(rad - offset, rad + offset);
            dir.x = MathF.Cos(rad);
            dir.y = MathF.Sin(rad);
            return dir;
        }
        /// <summary>
        /// 得到一根向量的垂线 XNA [-x => -pi/2] [-y => +pi/2]
        /// </summary>
        public static float2 PerpVecX(this float2 vec) => new float2(vec.y, -vec.x);
        public static float2 PerpVecY(this float2 vec) => new float2(-vec.y, vec.x);
        /// <summary>  
        /// 根据弧度获取向量角 [角度为0时 方向为正右]
        /// </summary>
        public static float2 GetVecF(float r) => new float2(MathF.Cos(r), MathF.Sin(r));
        /// <summary>
        /// 获取2维二次贝塞尔曲线
        /// </summary>
        /// <param name="s">起始点</param>
        /// <param name="m">中间点</param>
        /// <param name="e">结束点</param>
        /// <param name="t">时间系数 通常在[0-1]</param>
        /// <returns>插值点</returns>
        public static Vector2 GetBezier2(Vector2 s, Vector2 m, Vector2 e, float t)
        {
            float omt = 1f - t;
            return s * (omt * omt) + m * (2f * t * omt) + e * (t * t);
        }
        /// <summary>
        /// 创建等边多边形顶点
        /// </summary>
        /// <param name="count">顶点数量</param>
        /// <param name="startAngle">起始角度</param>
        /// <param name="r">生成半径</param>
        public static Vector2[] CreateEquilateralPolyVertex(int count, float r = 1, float startAngle = 0)
        {
            count = count < 3 ? 3 : count;
            var ver = new Vector2[count];
            float pi1 = MathEx.Deg2Rad * startAngle;
            float pi2 = MathEx.PI2 / count;
            Vector2 temp; float t;
            for (int i = 0; i < count; i++)
            {
                t = pi1 - i * pi2;
                temp.x = r * MathF.Cos(t);
                temp.y = r * MathF.Sin(t);
                ver[i] = temp;
            }
            return ver;
        }
    }
}