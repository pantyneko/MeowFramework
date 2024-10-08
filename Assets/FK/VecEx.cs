﻿using System;
using Unity.Mathematics;
using UnityEngine;

namespace Panty
{
    public static class VecEx
    {
        public readonly static float2 Up = new float2(0f, 1f);
        public static float2 RandomVec(this float2 dir, float offset)
        {
            var rad = MathF.Atan2(dir.y, dir.x);
            rad = UnityEngine.Random.Range(rad - offset, rad + offset);
            dir.x = MathF.Cos(rad);
            dir.y = MathF.Sin(rad);
            return dir;
        }
        public static bool Approximate(this Vector3 lhs, Vector2 rhs)
        {
            float num = lhs.x - rhs.x;
            float num2 = lhs.y - rhs.y;
            return num * num + num2 * num2 < 9.99999944E-11f;
        }
        // 将角度限制在 [-180, 180] 范围内
        public static Vector3 WrapAngle(this Vector3 e)
        {
            e.x.WrapAngle();
            e.y.WrapAngle();
            e.z.WrapAngle();
            return e;
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
        public static Vector2 Abs(this Vector2 v) => new Vector2(v.x.Abs(), v.y.Abs());
        /// <summary>
        /// 获取2维2次贝塞尔曲线
        /// </summary>
        public static Vector2 Get2DBezier2(Vector2 s, Vector2 m, Vector2 e, float t)
        {
            float omt = 1f - t;
            return s * (omt * omt) + m * (2f * t * omt) + e * (t * t);
        }
        /// <summary>
        /// 获取3维2次贝塞尔曲线
        /// </summary>
        public static Vector3 Get3DBezier2(Vector3 s, Vector3 m, Vector3 e, float t)
        {
            float omt = 1f - t;
            return s * (omt * omt) + m * (2f * t * omt) + e * (t * t);
        }
        /// <summary>
        /// 获取2维3次贝塞尔曲线
        /// </summary>
        public static Vector2 Get2DBezier3(Vector2 s, Vector2 c1, Vector2 c2, Vector2 e, float t)
        {
            float t2 = t * t;
            float omt = 1f - t;
            float omt2 = omt * omt;
            return s * (omt2 * omt) + c1 * (3f * omt2 * t) + c2 * (3f * omt * t2) + e * (t * t2);
        }
        /// <summary>
        /// 获取3维3次贝塞尔曲线
        /// </summary>
        public static Vector3 Get3DBezier3(Vector3 s, Vector3 c1, Vector3 c2, Vector3 e, float t)
        {
            float t2 = t * t;
            float omt = 1f - t;
            float omt2 = omt * omt;
            return s * (omt2 * omt) + c1 * (3f * omt2 * t) + c2 * (3f * omt * t2) + e * (t * t2);
        }
        /// <summary>
        /// 判断当前点是否在线段附近
        /// </summary>
        public static bool OnLine(this Vector2 p, Vector2 s, Vector2 e, float min, out Vector2 projectionPos)
        {
            float esx = e.x - s.x;
            float esy = e.y - s.y;

            float abac = esx * (p.x - s.x) + esy * (p.y - s.y);
            // 内积小于0，夹角大于90度，c在ab线段外面靠近a的一侧
            if (abac < 0)
            {
                projectionPos = s;
                return (s - p).sqrMagnitude + min < min;
            }
            float denominator = esx * esx + esy * esy;
            // 内积大于ab模的平方，ac在ab方向的投影大于ab，c在ab线段外面靠近b的一侧
            if (abac > denominator)
            {
                projectionPos = e;
                return (e - p).sqrMagnitude + min < min;
            }
            projectionPos = s + (e - s) * (abac / denominator);
            return (p - projectionPos).sqrMagnitude < min;
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