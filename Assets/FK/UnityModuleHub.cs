using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Panty
{
    public abstract class UnRegisterTrigger : MonoBehaviour
    {
        private Action mUnRegisterAction;
        public void Add(Action e) => mUnRegisterAction += e;
        protected void UnRegister() => mUnRegisterAction?.Invoke();
    }
    public class UnRegisterOnDestroyTrigger : UnRegisterTrigger
    {
        private void OnDestroy() => UnRegister();
    }
    public class UnRegisterOnDisableTrigger : UnRegisterTrigger
    {
        private void OnDisable() => UnRegister();
    }
    public static partial class ModuleHubTool
    {
        /// <summary>
        /// 尝试从一个物体身上获取脚本 如果获取不到就添加一个
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject o) where T : Component => o.GetComponent<T>() ?? o.AddComponent<T>();
        /// <summary>
        /// 找到面板父节点下所有对应控件
        /// </summary>
        public static void FindChildrenControl<T>(this Component mono, Action<string, T> callback = null) where T : Component
        {
#if UNITY_EDITOR
            if (callback == null) throw new Exception("无效回调");
#endif
            T[] controls = mono.GetComponentsInChildren<T>(true);
            if (controls.Length == 0) return;
            foreach (T ctrl in controls)
                callback.Invoke(ctrl.gameObject.name, ctrl);
        }
    }
    public static partial class ModuleHubEx
    {
        /// <summary>
        /// 获取系统层 Module 的别名
        /// </summary>
        public static S GetSystem<S>(this IPermissionProvider self) where S : class, IModule => self.Hub.Module<S>();
        /// <summary>
        /// 获取模型层 Module 的别名
        /// </summary>
        public static M GetModel<M>(this IPermissionProvider self) where M : class, IModule => self.Hub.Module<M>();
        /// <summary>
        /// 添加事件的监听 并标记为物体被销毁时注销
        /// </summary>
        public static void AddEvent_OnDestroyed_UnRegister<E>(this IPermissionProvider self, Action<E> evt, GameObject o) where E : struct
        {
            self.Hub.AddEvent<E>(evt);
            o.GetOrAddComponent<UnRegisterOnDestroyTrigger>().Add(() => self.Hub.RmvEvent<E>(evt));
        }
        /// <summary>
        /// 添加通知的监听 并标记为物体被销毁时注销
        /// </summary>
        public static void AddNotify_OnDestroyed_UnRegister<E>(this IPermissionProvider self, Action evt, GameObject o) where E : struct
        {
            self.Hub.AddNotify<E>(evt);
            o.GetOrAddComponent<UnRegisterOnDestroyTrigger>().Add(() => self.Hub.RmvNotify<E>(evt));
        }
        /// <summary>
        /// 添加事件的监听 并标记为物体失活时注销
        /// </summary>
        public static void AddEvent_OnDisabled_UnRegister<E>(this IPermissionProvider self, Action<E> evt, GameObject o) where E : struct
        {
            self.Hub.AddEvent<E>(evt);
            o.GetOrAddComponent<UnRegisterOnDisableTrigger>().Add(() => self.Hub.RmvEvent<E>(evt));
        }
        /// <summary>
        /// 添加通知的监听 并标记为物体失活时注销
        /// </summary>
        public static void AddNotify_OnDisabled_UnRegister<E>(this IPermissionProvider self, Action evt, GameObject o) where E : struct
        {
            self.Hub.AddNotify<E>(evt);
            o.GetOrAddComponent<UnRegisterOnDisableTrigger>().Add(() => self.Hub.RmvNotify<E>(evt));
        }
        /// <summary>
        /// 添加事件的监听 并标记为场景卸载时注销
        /// </summary>
        public static void AddEvent_OnSceneUnload_UnRegister<E>(this IPermissionProvider self, Action<E> evt) where E : struct
        {
            self.Hub.AddEvent<E>(evt);
            mWaitUninstEvents ??= new Dictionary<Type, Delegate>();
            mWaitUninstEvents.Combine(typeof(E), evt);
        }
        /// <summary>
        /// 添加通知的监听 并标记为场景卸载时注销
        /// </summary>
        public static void AddNotify_OnSceneUnload_UnRegister<N>(this IPermissionProvider self, Action evt) where N : struct
        {
            self.Hub.AddNotify<N>(evt);
            mWaitUninstNotifies ??= new Dictionary<Type, Delegate>();
            mWaitUninstNotifies.Combine(typeof(N), evt);
        }
        /// <summary>
        /// 用于当前场景卸载时 注销所有事件和通知
        /// </summary>
        public static void UnRegisterAllUnloadEvents<H>(this ModuleHub<H> hub) where H : ModuleHub<H>, new()
        {
            if (mWaitUninstEvents != null && mWaitUninstEvents.Count > 0)
            {
                foreach (var pair in mWaitUninstEvents)
                    hub.RmvEvent(pair.Key, pair.Value);
                mWaitUninstEvents = null;
            }
            if (mWaitUninstNotifies != null && mWaitUninstNotifies.Count > 0)
            {
                foreach (var pair in mWaitUninstNotifies)
                    hub.RmvNotify(pair.Key, pair.Value);
                mWaitUninstNotifies = null;
            }
        }
        // 用于存储所有当前场景卸载时 需要注销的事件和通知
        private static Dictionary<Type, Delegate> mWaitUninstEvents;
        private static Dictionary<Type, Delegate> mWaitUninstNotifies;
    }
    public abstract partial class ModuleHub<H>
    {
        protected ModuleHub()
        {
            // 预注册场景卸载事件
            SceneManager.sceneUnloaded += op => this.UnRegisterAllUnloadEvents();
            // 预注册 DeInit 事件
            MonoKit.GetIns().OnDeInit += Deinit;
        }
    }
}