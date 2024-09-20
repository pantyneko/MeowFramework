using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panty
{
    [Serializable]
    public class LoopList<T> : IEnumerable<T>
    {
#if UNITY_EDITOR
        [UnityEngine.SerializeField]
#endif
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
            get => this[N - 1];
            set => this[N - 1] = value;
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
        public LoopList(IEnumerable<T> collection)
        {
#if DEBUG
            ThrowEx.EmptyValue(collection);
#endif
            var coll = collection as ICollection<T>;
            arr = new T[coll == null ? collection.Count() : coll.Count];
            foreach (var item in collection) arr[N++] = item;
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
        public void RmvAt(int index)
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
            ThrowEx.InvalidIndex(index, N);
#endif
            // 将要删除的元素与最后一个元素交换
            if (index != --N) this[index] = this[N];
        }
        public void PosMove()
        {
            this[N] = arr[first];
            LoopPos();
        }
        public void NegMove()
        {
            T e = Last;
            LoopNeg();
            arr[first] = e;
        }
        public T RandomGet(out int index)
        {
            index = SysRandom.Range(N);
            return this[index];
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
        public void LoopPos()
        {
#if DEBUG
            ThrowEx.EmptyItem<T>(N);
#endif
            first = (first + 1) % arr.Length;
        }
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
}