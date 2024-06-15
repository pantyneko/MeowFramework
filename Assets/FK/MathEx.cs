using System;

namespace Panty
{
    #region 备忘笔记
    // 2的N次方    => [1 << n]
    // N*2的E次方  => [n << e]
    // N/2的E次方  => [n >> e]
    // 1=-1取反    => [~n + 1] 
    // AND 判断o数 => [(n & 1) == 0]
    // (int) Math.Log10(damage) + 1 计算数字长度
    // (char)('0' + n % 10) 将一个整数的最低位(个位)转换为对应的字符表示 '0'= 48(ASCII)
    // 计算以中值两侧的索引 => [i - count/2] 如果是偶数 需要 +0.5f 来补充
    // 是否在0-Max范围内 => [(uint)n < max] 如果n是一个负数，转成uint越界，变成一个很大的正数
    // cur / max 将 0 - max 映射到 0 - 1;
    // cur / (max - min) 将最小值-最大值的范围 映射到 0 - 1
    // maxA / max * cur; 最大值A to 最大值B [最小值都是0的情况]
    #endregion
    public static class MathEx
    {
        public const float PI = 3.14159274F;
        public const float PI2 = PI * 2f;
        public const float Rad90 = PI * 0.5f;
        public const float Rad45 = PI * 0.25f;
        public const float Deg2Rad = PI / 180f;
        public const float Rad2Deg = 180f / PI;

        // 求绝对值 [位运算优化] -2147483648 会溢出 
        public static int Abs(this int n) => (n + (n >> 31)) ^ (n >> 31);
        public static float Abs(this float a) => a >= 0f ? a : -a;

        public static void Max(this ref float x, float y) => x = x > y ? x : y;
        public static void Min(this ref float x, float y) => x = x < y ? x : y;
        public static void Max(this ref int x, int y) => x = x > y ? x : y;
        public static void Min(this ref int x, int y) => x = x < y ? x : y;

        public static void Clamp(this ref float v, float min, float max) => v = v < min ? min : (v > max ? max : v);
        public static void Clamp(this ref int v, int min, int max) => v = v < min ? min : (v > max ? max : v);
        public static void Clamp01(this ref float v) => v = v < 0f ? 0f : (v > 1f ? 1f : v);

        public static void MoveTowards(this ref float cur, float target, float step) =>
            cur = (target - cur).Abs() > step ? cur + (target - cur >= 0f ? step : -step) : target;
        public static void MoveTowardsF(this ref int cur, int target, int step) =>
            cur = (target - cur).Abs() > step ? cur + (target - cur >= 0 ? step : -step) : target;

        public static void Swap<T>(this ref T a, ref T b) where T : struct { T c = a; a = b; b = c; }
        public static float Normalize(this float cur, float min, float max) => (max - cur) / (max - min);
    }
    public static class RandomEx
    {
        private static Random random = new Random();
        public static int Range(int max) => random.Next(max);
        public static int Range(int min, int max) => random.Next(min, max);
        public static float NextFloat(float min, float max) => ((float)random.NextDouble() * (max - min)) + min;
        public static double NextDouble(double min, double max) => random.NextDouble() * (max - min) + min;
        public static double Range01() => random.NextDouble();
    }
}