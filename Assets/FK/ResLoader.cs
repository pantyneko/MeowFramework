using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Panty
{
    using ResDic = Dictionary<string, UnityEngine.Object>;
    public interface IResLoader : IModule
    {
        Task AsyncLoad<T>(string path) where T : UnityEngine.Object;
        Task<T> AsyncLoadT<T>(string path) where T : UnityEngine.Object;
        T SyncLoad<T>(string path) where T : UnityEngine.Object;
        void AsyncLoad<T>(string path, Action<T> call) where T : UnityEngine.Object;
        T SyncLoadFromCache<T>(string path) where T : UnityEngine.Object;
        void AsyncLoadFromCache<T>(string path, Action<T> call) where T : UnityEngine.Object;
    }
    public class ResLoader : AbsModule, IResLoader
    {
        private Dictionary<Type, ResDic> mInfos;
        protected override void OnInit()
        {
            mInfos = new Dictionary<Type, ResDic>();
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        protected virtual void OnSceneUnloaded(Scene scene) => mInfos.Clear();
        public virtual T SyncLoad<T>(string path) where T : UnityEngine.Object
        {
            var type = typeof(T);
            return Resources.Load(GetPrefix(type) + path, type) as T;
        }
        public virtual Task<T> AsyncLoadT<T>(string path) where T : UnityEngine.Object
        {
            var type = typeof(T);
            var tcs = new TaskCompletionSource<T>();
            Resources.LoadAsync(GetPrefix(type) + path, type).completed += op =>
            {
                var ass = (op as ResourceRequest).asset;
#if UNITY_EDITOR
                if (ass == null) throw new Exception("资源加载失败");
#endif
                tcs.SetResult(ass as T);
            };
            return tcs.Task;
        }
        public virtual Task AsyncLoad<T>(string path) where T : UnityEngine.Object
        {
            var type = typeof(T);
            var tcs = new TaskCompletionSource<UnityEngine.Object>();
            Resources.LoadAsync(GetPrefix(type) + path).completed += op =>
            {
                var ass = (op as ResourceRequest).asset;
#if UNITY_EDITOR
                if (ass == null) throw new Exception("资源加载失败");
#endif
                tcs.SetResult(ass);
            };
            return tcs.Task;
        }
        public virtual void AsyncLoad<T>(string path, Action<T> call) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (call == null) throw new Exception("无意义的回调");
#endif
            var type = typeof(T);
            Resources.LoadAsync(GetPrefix(type) + path, type).completed += op =>
            {
                var ass = (op as ResourceRequest).asset;
#if UNITY_EDITOR
                if (ass == null) throw new Exception("资源加载失败");
#endif
                call.Invoke(ass as T);
            };
        }
        T IResLoader.SyncLoadFromCache<T>(string path)
        {
            var type = typeof(T);
            // 检查是否存在这个池子
            if (mInfos.TryGetValue(type, out var cache))
            {
                if (cache.TryGetValue(path, out var ass))
                    return ass as T;
                T asset = SyncLoad<T>(path);
#if UNITY_EDITOR
                if (asset == null) throw new Exception("资源加载失败");
#endif
                cache.Add(path, asset);
                return asset;
            }
            T res = SyncLoad<T>(path);
#if UNITY_EDITOR
            if (res == null) throw new Exception("资源加载失败");
#endif
            mInfos.Add(type, new ResDic() { { path, res } });
            return res;
        }
        void IResLoader.AsyncLoadFromCache<T>(string path, Action<T> call)
        {
#if UNITY_EDITOR
            if (call == null) throw new Exception("无意义的回调");
#endif
            var type = typeof(T);
            string fullPath = GetPrefix(type) + path;
            if (mInfos.TryGetValue(type, out var cache))
            {
                if (cache.TryGetValue(path, out var res))
                {
                    call.Invoke(res as T);
                }
                else AsyncLoad<T>(fullPath, ass =>
                {
                    cache[path] = ass;
                    call(ass);
                });
            }
            else
            {
                cache = new ResDic() { { path, null } };
                AsyncLoad<T>(fullPath, ass =>
                {
                    cache[path] = ass;
                    call(ass);
                });
                mInfos.Add(type, cache);
            }
        }
        private static string GetPrefix(Type type)
        {
            string prefix = type switch
            {
                Type t when t == typeof(GameObject) => "Prefabs/",
                Type t when t == typeof(AudioClip) => "Audios/",
                _ => ""
            };
#if UNITY_EDITOR
            string _path = "Assets/Resources/" + prefix;
            if (FileKit.TryCreateDirectory(_path))
            {
                UnityEditor.AssetDatabase.Refresh();
                throw new Exception($"{_path} 文件夹被创建,请将资源文件放入该根目录");
            }
#endif
            return prefix;
        }
    }
}