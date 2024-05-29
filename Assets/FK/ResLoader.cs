using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Panty
{
    using ResDic = Dictionary<string, UnityEngine.Object>;
    public interface IResLoader : IModule
    {
        T SyncLoad<T>(string shortPath) where T : UnityEngine.Object;
        void AsyncLoad<T>(string shortPath, Action<T> call) where T : UnityEngine.Object;
    }
    public class ResLoader : AbsModule, IResLoader
    {
        private Dictionary<Type, ResDic> mInfos;
        protected override void OnInit()
        {
            mInfos = new Dictionary<Type, ResDic>();
            SceneManager.sceneUnloaded += arg => mInfos.Clear();
        }
        T IResLoader.SyncLoad<T>(string shortPath)
        {
            var type = typeof(T);
            if (mInfos.TryGetValue(type, out var cache))
            {
                if (!cache.TryGetValue(shortPath, out var ass))
                {
                    ass = Resources.Load(GetPrefix(type) + shortPath, type);
#if UNITY_EDITOR
                    if (ass == null) throw new Exception("资源加载失败");
#endif
                    cache.Add(shortPath, ass);
                }
                return ass as T;
            }
            var res = Resources.Load(GetPrefix(type) + shortPath, type);
#if UNITY_EDITOR
            if (res == null) throw new Exception("资源加载失败");
#endif
            mInfos.Add(type, new ResDic() { { shortPath, res } });
            return res as T;
        }
        void IResLoader.AsyncLoad<T>(string shortPath, Action<T> call)
        {
#if UNITY_EDITOR
            if (call == null) throw new Exception("无意义的回调");
#endif
            var type = typeof(T);
            string fullPath = GetPrefix(type) + shortPath;
            if (mInfos.TryGetValue(type, out var cache))
            {
                if (cache.TryGetValue(shortPath, out var res))
                {
                    call.Invoke(res as T);
                }
                else ResKit.AsyncLoad(fullPath, type, ass =>
                {
                    cache[shortPath] = ass;
                    call(ass as T);
                });
            }
            else
            {
                cache = new ResDic() { { shortPath, null } };
                ResKit.AsyncLoad(fullPath, type, ass =>
                {
                    cache[shortPath] = ass;
                    call(ass as T);
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
                throw new Exception($"{_path} => 文件夹被创建");
            }
#endif
            return prefix;
        }
    }
}