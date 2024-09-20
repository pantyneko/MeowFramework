using System;

namespace Panty
{
    #region 备忘笔记
    // BrotliStream  ISO-8859-1
    // 文本分隔符（ASCII 28）␜
    // 群组分隔符（ASCII 29）␝
    // 记录分隔符（ASCII 30）␞
    // 单元分隔符（ASCII 31）␟
    // 2的N次方    => [1 << n]
    // N*2的E次方  => [n << e]
    // N/2的E次方  => [n >> e]
    // 1=-1取反    => [~n + 1] 
    // AND 判断o数 => [(n & 1) == 0]
    // (char)(48 + n.Abs() % 10) 将一个整数的个位转换为对应的字符表示 '0'= 48(ASCII)
    // 计算以中值两侧的索引 => [i - count/2] 如果是偶数 需要 +0.5f 来补充
    // 是否在0-Max范围内 => [(uint)n < max] 如果n是一个负数，转成uint越界，变成一个很大的正数
    // cur / max 将 0 - max 映射到 0 - 1;
    // cur / (max - min) 将最小值-最大值的范围 映射到 0 - 1
    // maxA / max * cur; 最大值A to 最大值B [最小值都是0的情况]
    // 两数接近 t为误差范围 Abs(a - b) <= t 
    #endregion
    public static class MathEx
    {
        public const float PI2 = MathF.PI * 2f;
        public const float Rad90 = MathF.PI * 0.5f;
        public const float Rad45 = MathF.PI * 0.25f;
        public const float Deg2Rad = MathF.PI / 180f;
        public const float Rad2Deg = 180f / MathF.PI;

        // 求绝对值 [位运算优化] -2147483648 会溢出 
        public static int Abs(this int n) => (n + (n >> 31)) ^ (n >> 31);
        public static float Abs(this float n) => n >= 0f ? n : -n;
        public static double Abs(this double n) => n >= 0.0 ? n : -n;

        public static void Max(this ref float x, float y) => x = x > y ? x : y;
        public static void Min(this ref float x, float y) => x = x < y ? x : y;
        public static void Max(this ref int x, int y) => x = x > y ? x : y;
        public static void Min(this ref int x, int y) => x = x < y ? x : y;

        public static void LerpUnclamped(this ref float a, float b, float t) => a += (b - a) * t;

        public static void Clamp(this ref float v, float min, float max) => v = v < min ? min : (v > max ? max : v);
        public static void Clamp(this ref int v, int min, int max) => v = v < min ? min : (v > max ? max : v);

        public static void ClampMax(this ref float v, float max) => v = v < 0f ? 0f : (v > max ? max : v);
        public static void ClampMax(this ref int v, int max) => v = v < 0 ? 0 : (v > max ? max : v);

        public static float Clamp_01(this float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
        public static void Clamp01(this ref float v) => v = v < 0f ? 0f : (v > 1f ? 1f : v);
        public static void Clamp01(this ref double v) => v = v < 0.0 ? 0.0 : (v > 1.0 ? 1.0 : v);
        /// <summary>
        /// 获取数字的位数 如果是负数 会加上符号位
        /// </summary>
        public static int DigitBit(this int n) => n == 0 ? 1 : (int)Math.Log10(n.Abs()) + (n < 0 ? 2 : 1);
        /// <summary>
        /// 获取符号 无输入 0
        /// </summary>
        public static int Sign(this float n) => n == 0f ? 0 : n > 0f ? 1 : -1;
        /// <summary>
        /// 大小排序 Int
        /// </summary>
        public static int Sort(this int n) => n == 0 ? 0 : n > 0 ? 1 : -1;
        /// <summary>
        /// 向目标点移动
        /// </summary>
        public static void MoveTowards(this ref float cur, float target, float step) =>
            cur = (target - cur).Abs() > step ? cur + (target - cur >= 0f ? step : -step) : target;
        public static void MoveTowardsF(this ref int cur, int target, int step) =>
            cur = (target - cur).Abs() > step ? cur + (target - cur >= 0 ? step : -step) : target;
        /// <summary>
        /// 两个元素交换
        /// </summary>
        public static void Swap<T>(this ref T a, ref T b) where T : struct { T c = a; a = b; b = c; }
        /// <summary>
        /// 规范化（归一化）函数，将当前值按给定的最小值和最大值转换为0到1之间的值。
        /// </summary>
        public static float Normalize(this float cur, float min, float max)
        {
#if DEBUG
            if (max == min) throw new Exception("原始范围的最大值不能等于最小值");
#endif
            return (cur - min) / (max - min);
        }
        /// <summary>
        /// 将一个范围映射到另一个范围
        /// </summary>
        public static float ToRange(this float cur, float min, float max, float minA, float maxA)
        {
#if DEBUG
            if (max == min) throw new Exception("原始范围的最大值不能等于最小值");
#endif
            return minA + (maxA - minA) / (max - min) * (cur - min);
        }
        /// <summary>
        /// 将一个范围映射到另一个范围 最小值相同
        /// </summary>
        public static float ToRange(this float cur, float min, float max, float maxA)
        {
#if DEBUG
            if (max == min) throw new Exception("原始范围的最大值不能等于最小值");
#endif
            return maxA / (max - min) * (cur - min);
        }
        /// <summary>
        /// 将max范围映射到maxA范围 最小值为 0
        /// </summary>
        public static float ToRange(this float cur, float max, float maxA)
        {
#if DEBUG
            if (max == 0) throw new Exception("原始范围的最大值不能为 0");
#endif
            return maxA / max * cur;
        }
        /// <summary>
        /// 计算数值的倒数平方根。
        /// </summary>
        /// <param name="x">输入值</param>
        /// <returns>输入值的倒数平方根</returns>
        public unsafe static float InvSqrt(this float x)
        {
#if DEBUG
            if (x <= 0f) throw new Exception("Input must be a positive number.");
#endif
            float xhalf = 0.5f * x;
            int i = *(int*)&x;
            i = 0x5f3759df - (i >> 1);
            x = *(float*)&i;
            return x * (1.5f - xhalf * x * x);
        }
        /// <summary>
        /// 拟真弹跳
        /// </summary>
        public static double Bounce01(this double n)
        {
            n.Clamp01();
            return Bounce(n);
        }
        /// <summary>
        /// 简易弹跳
        /// </summary>
        public static double Bounce01(this double n, byte count = 2)
        {
            n.Clamp01();
            return Math.Sin(n * PI2 * count) * (1.0 - n);
        }
        /// <summary>
        /// 周期弹跳
        /// </summary>
        public static double LoopBounce(this double n, byte count = 2)
        {
            if ((n %= 1.0) < 0.0) n += 1.0;
            return Math.Sin(n * PI2 * count) * (1.0 - n);
        }
        /// <summary>
        /// 周期拟真弹跳
        /// </summary>
        public static double LoopBounce(this double n)
        {
            if ((n %= 1.0) < 0.0) n += 1.0;
            return Bounce(n);
        }
        private static double Bounce(double n)
        {
            if (n < 0.3636)
            {
                return 7.5625 * n * n;
            }
            else if (n < 0.7272)
            {
                n -= 0.5455;
                return 7.5625 * n * n + 0.75;
            }
            else if (n < 0.9091)
            {
                n -= 0.8182;
                return 7.5625 * n * n + 0.9375;
            }
            else
            {
                n -= 0.9545;
                return 7.5625 * n * n + 0.984375;
            }
        }
        /// <summary>
        /// 将单个角度限制在 [-180, 180] 范围内
        /// </summary>
        public static void WrapAngle(this ref float angle)
        {
            angle = angle % 360f; // 先将角度转换为 [0, 360) 范围内
            angle = angle < -180f ? angle + 360f : (angle > 180f ? angle - 360f : angle);
        }
    }
    public static class SysRandom
    {
        private static Random random = new Random();
        public static int Range(int max) => random.Next(max);
        public static int Range(int min, int max) => random.Next(min, max);
        public static float NextFloat(float min, float max) => ((float)random.NextDouble() * (max - min)) + min;
        public static double NextDouble(double min, double max) => random.NextDouble() * (max - min) + min;
        public static double Range01() => random.NextDouble();
    }
}