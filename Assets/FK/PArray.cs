using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Panty
{
    public class LoopList<T> : IEnumerable<T>
    {
        private T[] arr;
        private int N = 0, first = 0;
        public int Count => N;
        public int Capacity => arr.Length;
        public bool IsEmpty => N == 0;
        public T this[int index]
        {
            get
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
                ThrowEx.InvalidIndex(index, N);
#endif
                return arr[(first + index) % arr.Length];
            }
            set
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
                ThrowEx.InvalidIndex(index, N);
#endif
                arr[(first + index) % arr.Length] = value;
            }
        }
        public T First
        {
            get
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
#endif
                return arr[first];
            }
            set
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
#endif
                arr[first] = value;
            }
        }
        public T Last
        {
            get
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
#endif
                return this[N - 1];
            }
            set
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
#endif
                this[N - 1] = value;
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity">初始容量</param>
        public LoopList(int capacity = 4)
        {
#if DEBUG
            ThrowEx.InvalidCapacity(capacity);
#endif
            arr = new T[capacity];
        }
        public void AddFirst(T e)
        {
            if (N == arr.Length)
                Resize(N << 1);
            LoopNeg(); N++;
            arr[first] = e;
        }
        public void AddLast(T e)
        {
            if (N == arr.Length) Resize(N << 1);
            this[N++] = e;
        }
        public void PosMove()
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
#endif
            this[N] = arr[first];
            LoopPos();
        }
        public void NegMove()
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
#endif
            T e = Last;
            LoopNeg();
            arr[first] = e;
        }
        public void Slice(int start, int count)
        {
            first = start;
            N = count;
        }
        private void Resize(int newSize)
        {
            // 如果长度相等 不需要变容量
            if (newSize <= N) return;
            T[] newArr = new T[newSize];
            for (int i = 0; i < N; i++)
                newArr[i] = this[i];
            first = 0;
            arr = newArr;
        }
        public int FindIndex(Predicate<T> match)
        {
            for (int i = 0; i < N; i++)
            {
                if (match(this[i])) return i;
            }
            return -1;
        }
        /// <summary>
        /// 只重置游标 不释放数据
        /// </summary>
        public void ToFirst() => N = 0;
        public void LoopPos() => first = (first + 1) % arr.Length;
        public void LoopNeg()
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
#endif
            first = (first + N - 1) % arr.Length;
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = first, len = i + N; i < len; i++)
                yield return arr[i % arr.Length];
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = first, len = i + N; i < len; i++)
                yield return arr[i % arr.Length];
        }
        public override string ToString()
        {
            var sb = new StringBuilder($"{nameof(PArray<T>)},Count:{N},Items:\r\n");
            for (int i = first, len = i + N; i < len; i++)
                sb.Append($"[{arr[i % arr.Length]}],");
            return sb.ToString();
        }
    }
    /// <summary>
    /// 可自动扩容的动态数组 注意该结构需要手动释放未使用结构
    /// 当移除时 需手动调用缩容 合理利用报错
    /// </summary>
    public class PArray<T> : IEnumerable<T>
    {
        private T[] arr;
        private int N = 0;
        public int Count => N;
        public int Capacity => arr.Length;
        public bool IsEmpty => N == 0;
        public T[] Self
        {
            get => arr;
            set
            {
#if DEBUG
                ThrowEx.EmptyArray<T>(value);
#endif
                N = value.Length;
                arr = value;
            }
        }
        public T First
        {
            get
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
#endif
                return arr[0];
            }
            set
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
#endif
                arr[0] = value;
            }
        }
        public T Last
        {
            get
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
#endif
                return arr[N - 1];
            }
            set
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
#endif
                arr[N - 1] = value;
            }
        }
        public T this[int index]
        {
            get
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
                ThrowEx.InvalidIndex(index, N);
#endif
                return arr[index];
            }
            set
            {
#if DEBUG
                ThrowEx.EmptyItem<T>(N);
                ThrowEx.InvalidIndex(index, N);
#endif
                arr[index] = value;
            }
        }
        public bool SetIfChanged(int index, T newValue)
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
            ThrowEx.InvalidIndex(index, N);
#endif
            if (EqualityComparer<T>.Default
                .Equals(arr[index], newValue)) return false;
            arr[index] = newValue;
            return true;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity">初始容量</param>
        /// <param name="isFill">使用默认数据填满</param>
        public PArray(int capacity = 2, bool isFill = false)
        {
#if DEBUG
            ThrowEx.InvalidCapacity(capacity);
#endif
            arr = new T[capacity];
            if (isFill) N = capacity;
        }
        public PArray(T[] arr)
        {
#if DEBUG
            ThrowEx.EmptyArray(arr);
#endif
            this.arr = arr;
            N = arr.Length;
        }
        public void Push(T e)
        {
            if (N == arr.Length)
                Resize(N << 1);
            AddLast(e);
        }
        public T Pop()
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
#endif
            return arr[--N];
        }
        public void AddLast(T e) => arr[N++] = e;
        public void RmvLast() => N -= (N == 0 ? 0 : 1);
        public void LoopPosN(ref int c) => c = (c + 1) % N;
        public void LoopNegN(ref int c) => c = (c + N - 1) % N;
        public void LoopPosLen(ref int c) => c = (c + 1) % arr.Length;
        public void LoopNegLen(ref int c) => c = (c + arr.Length - 1) % arr.Length;
        public T RandomGet()
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
#endif
            return arr[SysRandom.Range(N)];
        }
        public void RandomPop(out T e)
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
#endif        
            if (N == 1)
            {
                e = arr[--N];
            }
            else
            {
                int index = SysRandom.Range(N);
                e = arr[index];
                arr[index] = arr[--N];
            }
        }
        /// <summary>
        /// 交换式移除
        /// </summary>
        public void RmvAt(int index)
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
            ThrowEx.InvalidIndex(index, N);
#endif
            if (index == --N) return;
            arr[index] = arr[N];
        }
        public void Rmv(T e)
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
#endif
            RmvAt(Array.IndexOf<T>(arr, e, 0, N));
        }
        public bool TryRmv(T e)
        {
            int index = IndexOf(e);
            if (index >= 0) RmvAt(index);
            return index >= 0;
        }
        public void Sort(IComparer<T> comparer)
        {
            if (N == 0) return;
            Array.Sort<T>(arr, 0, N, comparer);
        }
        public void Sort()
        {
            if (N == 0) return;
            Array.Sort<T>(arr, 0, N);
        }
        public bool Find(Predicate<T> match, out T r)
        {
            for (int i = 0; i < N; i++)
            {
                r = arr[i];
                if (match(r))
                    return true;
            }
            r = default(T);
            return false;
        }
        public int Find(Predicate<T> match)
        {
            for (int i = 0; i < N; i++)
            {
                if (match(arr[i])) return i;
            }
            return -1;
        }
        public bool Contains(T e) =>
            N == 0 ? false : Array.IndexOf<T>(arr, e, 0, N) >= 0;
        public int IndexOf(T e) =>
            N == 0 ? -1 : Array.IndexOf<T>(arr, e, 0, N);
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
            if (newSize <= N) return;
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
            if (arr.Length == 8) return;
            Resize(N > 8 ? N : 8);
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
        public static implicit operator PArray<T>(T[] arr) => new PArray<T>(arr);
    }
}