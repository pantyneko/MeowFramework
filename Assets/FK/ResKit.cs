using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Panty
{
    public static class ResKit
    {
        public static Task<T> AsyncLoad<T>(string path) where T : UnityEngine.Object
        {
            var tcs = new TaskCompletionSource<T>();
            Resources.LoadAsync(path, typeof(T)).completed += op =>
            {
                var ass = (op as ResourceRequest).asset;
#if UNITY_EDITOR
                if (ass == null) throw new Exception("资源加载失败");
#endif
                tcs.SetResult(ass as T);
            };
            return tcs.Task;
        }
        public static void AsyncLoad<T>(string path, Action<T> call) where T : UnityEngine.Object
        {
            Resources.LoadAsync(path, typeof(T)).completed +=
            op =>
            {
                var ass = (op as ResourceRequest).asset;
#if UNITY_EDITOR
                if (ass == null) throw new Exception("资源加载失败");
#endif
                call(ass as T);
            };
        }
        public static void AsyncLoad(string path, Type type, Action<UnityEngine.Object> call)
        {
            Resources.LoadAsync(path, type).completed +=
            op =>
            {
                var ass = (op as ResourceRequest).asset;
#if UNITY_EDITOR
                if (ass == null) throw new Exception("资源加载失败");
#endif
                call(ass);
            };
        }
        public static void AsyncLoadGo(string path, Action<GameObject> call = null)
        {
            AsyncLoad<GameObject>(path, o =>
            {
                o = GameObject.Instantiate(o);
                call?.Invoke(o);
            });
        }
        public static async Task<GameObject> AsyncLoadGo(string path, Vector3 pos)
        {
            return GameObject.Instantiate(await AsyncLoad<GameObject>(path), pos, Quaternion.identity);
        }
        public static async Task<GameObject> AsyncLoadGo(string path, Vector3 pos, Quaternion q)
        {
            return GameObject.Instantiate(await AsyncLoad<GameObject>(path), pos, q);
        }
        public static GameObject SyncLoadGo(string path, Vector3 pos)
        {
            return GameObject.Instantiate(Resources.Load<GameObject>(path), pos, Quaternion.identity);
        }
        public static GameObject SyncLoadGo(string path, Vector3 pos, Quaternion q)
        {
            return GameObject.Instantiate(Resources.Load<GameObject>(path), pos, q);
        }
    }
}