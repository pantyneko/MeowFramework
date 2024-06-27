#if DEBUG
using System;

namespace Panty
{
    public static class ThrowEx
    {
        public static void InvalidCapacity(int capacity)
        {
            if (capacity <= 0)
                throw new Exception($"无效的容量:{capacity}");
        }
        public static void EmptyItem<T>(int count)
        {
            if (count == 0)
                throw new Exception($"{typeof(T)}无元素");
        }
        public static void IsIndexLegal(int index, int count)
        {
            if (index >= count)
                throw new Exception($"索引越界 Index:{index},N:{count}");
        }
        public static void EmptyArray<T>(T[] arr)
        {
            if (arr == null || arr.Length == 0)
                throw new Exception($"空数组:{arr},Type:{typeof(T)}");
        }
        public static void EmptyCallback<T>(T callback) where T : Delegate
        {
            if (callback == null)
                throw new Exception($"{typeof(T)}回调函数为空");
        }
        public static void EmptyAssets<T>(T ass)
        {
            if (ass == null)
                throw new Exception($"{typeof(T)}资源加载失败");
        }
        public static void EmptyValue<T>(T value)
        {
            if (value == null)
                throw new Exception($"{typeof(T)}值为空");
        }
    }
}
#endif