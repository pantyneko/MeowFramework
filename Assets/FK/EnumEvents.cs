using System;
using System.Collections.Generic;

namespace Panty
{
    public class EnumDataEvents<E> where E : Enum
    {
        private Dictionary<E, Delegate> mEvents = new Dictionary<E, Delegate>();

        public void Add<T>(E type, Action<T> action)
        {
            if (mEvents.TryGetValue(type, out var del))
                mEvents[type] = Delegate.Combine(del, action);
            else mEvents.Add(type, action);
        }
        public void Rmv<T>(E type, Action<T> action)
        {
            if (mEvents.TryGetValue(type, out var del))
            {
                del = Delegate.Remove(del, action);
                if (del == null)
                    mEvents.Remove(type);
                else
                    mEvents[type] = del;
            }
        }
        public void RmvMate<T>(E type, Action<T> action)
        {
            if (mEvents.TryGetValue(type, out var del))
            {
                del = Delegate.RemoveAll(del, action);
                if (del == null)
                    mEvents.Remove(type);
                else
                    mEvents[type] = del;
            }
        }
        public void Send<T>(E type, T data)
        {
            if (mEvents.TryGetValue(type, out var del))
                (del as Action<T>)?.Invoke(data);
        }
        public bool RmvKey(E type) => mEvents.Remove(type);
        public void Clear() => mEvents.Clear();
    }
    public class EnumEvents<E> where E : Enum
    {
        private Dictionary<E, Action> mEvents = new Dictionary<E, Action>();
        public void Add(E type, Action action)
        {
            if (mEvents.TryGetValue(type, out var del))
                mEvents[type] = del += action;
            else mEvents.Add(type, action);
        }
        public void Rmv(E type, Action action)
        {
            if (mEvents.TryGetValue(type, out var del))
            {
                if ((del -= action) == null)
                    mEvents.Remove(type);
                else mEvents[type] = del;
            }
        }
        public void Send(E type)
        {
            if (mEvents.TryGetValue(type, out var del)) del?.Invoke();
        }
        public bool RmvKey(E type) => mEvents.Remove(type);
        public void Clear() => mEvents.Clear();
    }
}