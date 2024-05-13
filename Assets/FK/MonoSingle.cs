using System.Reflection;
using System;
using UnityEngine;

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
        private static T _instance;
        protected static bool _applicationIsQuitting = false;

        public static T GetIns()
        {
#if UNITY_EDITOR
            // 防止编辑器意外创建 主要是 ExecuteInEditMode
            if (!UnityEditor.EditorApplication.isPlaying) return _instance;
#endif
            if (_instance == null && !_applicationIsQuitting)
            {
                _instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                _instance.Init();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                _instance.Init();
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        protected virtual void Init() { }
        protected abstract void DeInit();
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance.DeInit();
                _applicationIsQuitting = true;
            }
        }
        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}