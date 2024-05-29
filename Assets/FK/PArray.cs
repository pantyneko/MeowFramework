using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Panty
{
    /// <summary>
    /// 可自动扩容的动态数组 注意该结构需要手动释放未使用结构
    /// 当移除时 需手动调用缩容 合理利用报错
    /// </summary>
    public class PArray<T> : IEnumerable<T>
    {
        protected T[] arr;
        protected int N = 0;
        public int Count => N;
        public int Capacity => arr.Length;
        public bool IsEmpty => N == 0;
        public T[] Self => arr;
        public T First => arr[0];
        public T Last => arr[N - 1];
        public T this[int index]
        {
            get
            {
#if DEBUG
                CheckIndexLegal(index);
#endif
                return arr[index];
            }
            set
            {
#if DEBUG
                CheckIndexLegal(index);
#endif
                arr[index] = value;
            }
        }
#if DEBUG
        private void CheckIndexLegal(int index)
        {
            if (index >= N)
                throw new ArgumentException($"Index:{index},N:{N}");
        }
#endif
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity">初始容量</param>
        /// <param name="isFill">使用默认数据填满数组</param>
        public PArray(int capacity = 4, bool isFill = false)
        {
#if DEBUG
            if (capacity <= 0)
                throw new ArgumentException($"无效的容量{capacity}");
#endif
            arr = new T[capacity];
            if (isFill) N = capacity;
        }
        public PArray(IEnumerable<T> items)
        {
            arr = items.ToArray();
            N = arr.Length;
        }
        public PArray<T> Clone() =>
            new PArray<T>(arr.Take(N));
        public void Push(T e)
        {
            if (N == arr.Length)
                Resize(N << 1);
            AddLast(e);
        }
        public void AddLast(T e) => arr[N++] = e;
        public void RmvLast() => N -= (N == 0 ? 0 : 1);
        public T Pop() => arr[--N];
        /// <summary>
        /// 交换式移除
        /// </summary>
        public void RmvAt(int index)
        {
#if DEBUG
            CheckIndexLegal(index);
#endif
            if (index == --N) return;
            arr[index] = arr[N];
        }
        public void Sort(IComparer<T> comparer)
        {
            if (N == 0) return;
            Array.Sort<T>(arr, 0, N, comparer);
        }
        public bool Find(Predicate<T> match, out T r)
        {
            if (N > 0)
            {
                for (int i = 0; i < N; i++)
                {
                    r = arr[i];
                    if (match(arr[i]))
                        return true;
                }
            }
            r = default(T);
            return false;
        }
        public bool Contains(T e) => N == 0 ? false : Array.IndexOf<T>(arr, e, 0, N) >= 0;
        public int IndexOf(T e) => N == 0 ? -1 : Array.IndexOf<T>(arr, e, 0, N);
        // 需要主动前置条件
        public void Shrinkage()
        {
            if (arr.Length < 8) return;
            if (N < arr.Length >> 2)
                Resize(arr.Length >> 1);
        }
        /// <summary>
        /// 清空所有元素引用 不进行缩容
        /// </summary>
        public void Clear()
        {
            if (N == 0) return;
            Array.Clear(arr, 0, arr.Length);
            N = 0;
        }
        private void Resize(int newSize)
        {
            // 如果长度相等 不需要变容量
            if (newSize == 0) return;
            T[] newArr = new T[newSize];
            Array.Copy(arr, 0, newArr, 0, N);
            arr = newArr;
        }
        /// <summary>
        /// 重置容量到 N
        /// </summary>
        public void ResizeToN()
        {
            if (N == arr.Length) return;
            Resize(N);
        }
        public void ResizeToDefault()
        {
            // 如果数量大于8 就会出现数据丢失 如果长度就是8 那没必要重置
            if (N > 8 || arr.Length == 8) return;
            Resize(8);
        }
        public void ResetToNoCopy(int count)
        {
            if (arr.Length >= count) return;
            arr = new T[count];
        }
        /// <summary>
        /// 只重置游标 不释放数据
        /// </summary>
        public void ToFirst() => N = 0;
        /// <summary>
        /// 游标移动到数组的末端
        /// </summary>
        public void ToLast() => N = arr.Length;
        /// <summary>
        /// 迭代器
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = 0; i < N; i++)
                yield return arr[i];
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < N; i++)
                yield return arr[i];
        }
    }
}