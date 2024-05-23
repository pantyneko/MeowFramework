using UnityEngine;
using System.Reflection;
using System;

namespace Panty
{
    public interface ISingleton { void Init(); }
    public abstract class Singleton<S> where S : class, ISingleton
    {
        private static S mInstance;
        public static S GetIns()
        {
            if (mInstance == null)
            {
                var ctor = Array.Find(
                    typeof(S).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic),
                    c => c.GetParameters().Length == 0);
                if (ctor == null) throw new Exception($"{typeof(S).Name}缺少私有构造函数");
                mInstance = ctor.Invoke(null) as S;
                mInstance.Init();
            }
            return mInstance;
        }
    }
    /// <summary>
    /// Unity的Mono单例基类
    /// </summary>
    public abstract class MonoSingle<T> : MonoBehaviour where T : MonoSingle<T>
    {
        private static T ins;
        public static T GetIns()
        {
#if UNITY_EDITOR
            // 防止编辑器意外创建 主要是 ExecuteInEditMode
            if (!UnityEditor.EditorApplication.isPlaying) return null;
#endif
            if (ins == null)
            {
                if ((ins = FindObjectOfType<T>()) == null)
                {
                    var o = new GameObject(typeof(T).Name);
                    GameObject.DontDestroyOnLoad(o);
                    ins = o.AddComponent<T>();
                }
            }
            return ins;
        }
        private void Awake()
        {
            if (ins == null)
            {
                ins = this as T;
                InitSingle();
            }
            else Destroy(gameObject);
        }
        protected virtual void InitSingle() { }
    }
}