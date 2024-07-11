using System;
using System.Collections.Generic;

namespace Panty
{
    public class EnumEvents<E> where E : Enum
    {
        private Dictionary<E, Delegate> mEvents = new Dictionary<E, Delegate>();

        public IRmv Add<T>(E key, Action<T> evt)
        {
#if DEBUG
            if (evt == null) $"{evt} 不可为Null".Log();
#endif
            mEvents.Combine(key, evt);
            return new CustomRmv(() => mEvents.Separate(key, evt));
        }
        public void Rmv<T>(E key, Action<T> evt)
        {
            mEvents.Separate(key, evt);
        }
        public void Send<T>(E key, T data)
        {
            if (mEvents.TryGetValue(key, out var del))
                (del as Action<T>)?.Invoke(data);
#if DEBUG
            else $"{key} 事件未注册".Log();
#endif
        }
        public bool RmvKey(E type) => mEvents.Remove(type);
        public void Clear() => mEvents.Clear();
    }
    public class EnumNotify<E> where E : Enum
    {
        private Dictionary<E, Action> mNotifies = new Dictionary<E, Action>();
        public IRmv Add(E key, Action evt)
        {
            if (mNotifies.TryGetValue(key, out var del))
                mNotifies[key] = del += evt;
            else mNotifies.Add(key, evt);
            return new CustomRmv(() => Rmv(key, evt));
        }
        public void Rmv(E key, Action action)
        {
            if (mNotifies.TryGetValue(key, out var del))
            {
                if ((del -= action) == null)
                    mNotifies.Remove(key);
                else mNotifies[key] = del;
            }
#if DEBUG
            else $"{key} 事件Key不存在".Log();
#endif
        }
        public void Send(E key)
        {
            if (mNotifies.TryGetValue(key, out var del)) del?.Invoke();
#if DEBUG
            else $"{key} 事件未注册".Log();
#endif
        }
        public bool RmvKey(E key) => mNotifies.Remove(key);
        public void Clear() => mNotifies.Clear();
    }
}