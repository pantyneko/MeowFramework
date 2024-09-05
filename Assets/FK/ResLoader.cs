using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Panty
{
    // 定义资源字典类型的简写
    using ResDic = Dictionary<string, UnityEngine.Object>;
    public partial interface IResLoader : IModule
    {
        Task<T> AsyncLoad<T>(string path) where T : UnityEngine.Object;
        T SyncLoad<T>(string path) where T : UnityEngine.Object;
        void AsyncLoad<T>(string path, Action<T> call) where T : UnityEngine.Object;
        void AsyncLoadGo(string path, Action<GameObject> call = null);
        Task<GameObject> AsyncLoadGo(string path, Vector3 pos);
        Task<GameObject> AsyncLoadGo(string path, Vector3 pos, Quaternion q);
        GameObject SyncLoadGo(string path, Vector3 pos);
        GameObject SyncLoadGo(string path, Vector3 pos, Quaternion q);
        T SyncLoadFromCache<T>(string path) where T : UnityEngine.Object;
        void AsyncLoadFromCache<T>(string path, Action<T> call) where T : UnityEngine.Object;
        Task<T> AsyncLoadFromCache<T>(string path) where T : UnityEngine.Object;
    }
    public partial class ResLoader : AbsModule, IResLoader
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
        public virtual Task<T> AsyncLoad<T>(string path) where T : UnityEngine.Object
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
        void IResLoader.AsyncLoadGo(string path, Action<GameObject> call)
        {
            AsyncLoad<GameObject>(path, o =>
            {
                o = GameObject.Instantiate(o);
                call?.Invoke(o);
            });
        }
        async Task<GameObject> IResLoader.AsyncLoadGo(string path, Vector3 pos)
        {
            return GameObject.Instantiate(await AsyncLoad<GameObject>(path), pos, Quaternion.identity);
        }
        async Task<GameObject> IResLoader.AsyncLoadGo(string path, Vector3 pos, Quaternion q)
        {
            return GameObject.Instantiate(await AsyncLoad<GameObject>(path), pos, q);
        }
        GameObject IResLoader.SyncLoadGo(string path, Vector3 pos)
        {
            return GameObject.Instantiate(SyncLoad<GameObject>(path), pos, Quaternion.identity);
        }
        GameObject IResLoader.SyncLoadGo(string path, Vector3 pos, Quaternion q)
        {
            return GameObject.Instantiate(SyncLoad<GameObject>(path), pos, q);
        }
        private static string GetPrefix(Type type)
        {
            string prefix = type switch
            {
                Type t when t == typeof(GameObject) => "Prefabs/",
                Type t when t == typeof(AudioClip) => "Audios/",
                Type t when t == typeof(ScriptableObject) => "So/",
                Type t when t == typeof(Sprite) => "Sprites/",
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
        T IResLoader.SyncLoadFromCache<T>(string path)
        {
            var type = typeof(T);
            // 检查是否存在这个池子
            if (mInfos.TryGetValue(type, out var cache))
            {
                // 有没有该名字的缓存
                if (cache.TryGetValue(path, out var ass))
                    return ass as T;
                // 没有就加载一个
                T asset = SyncLoad<T>(path);
#if UNITY_EDITOR
                if (asset == null) throw new Exception("资源加载失败");
#endif
                // 加载完存起来
                cache.Add(path, asset);
                return asset;
            }
            // 如果没有池子 说明也没有缓存 直接加载
            T res = SyncLoad<T>(path);
#if UNITY_EDITOR
            if (res == null) throw new Exception("资源加载失败");
#endif
            // 加入池子和缓存
            mInfos.Add(type, new ResDic() { { path, res } });
            return res;
        }
        void IResLoader.AsyncLoadFromCache<T>(string path, Action<T> call)
        {
#if UNITY_EDITOR
            if (call == null) throw new Exception("无意义的回调");
#endif
            if (mInfos.TryGetValue(typeof(T), out var cache))
            {
                if (cache.TryGetValue(path, out var res))
                {
                    call.Invoke(res as T);
                }
                else AsyncLoad<T>(path, ass =>
                {
                    cache[path] = ass;
                    call(ass);
                });
            }
            else
            {
                cache = new ResDic() { { path, null } };
                mInfos.Add(typeof(T), cache);

                AsyncLoad<T>(path, ass =>
                {
                    cache[path] = ass;
                    call(ass);
                });
            }
        }
        async Task<T> IResLoader.AsyncLoadFromCache<T>(string path)
        {
            if (mInfos.TryGetValue(typeof(T), out var cache))
            {
                if (cache.TryGetValue(path, out var res))
                    return res as T;
                T _ass = await AsyncLoad<T>(path);
                cache[path] = _ass;
                return _ass;
            }
            cache = new ResDic() { { path, null } };
            mInfos.Add(typeof(T), cache);

            T ass = await AsyncLoad<T>(path);
            cache[path] = ass;
            return ass;
        }
    }
}