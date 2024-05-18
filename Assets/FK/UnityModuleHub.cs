using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Panty
{
    public static partial class ModuleHubEx
    {
        public static S GetSystem<S>(this IPermissionProvider self) where S : class, IModule => self.Hub.Module<S>();
        public static M GetModel<M>(this IPermissionProvider self) where M : class, IModule => self.Hub.Module<M>();

        private static Dictionary<Type, Delegate> mWaitUninstEvents;
        private static Dictionary<Type, Delegate> mWaitUninstNotifies;
        public static void AddEventAndUnregisterOnUnload<E>(this IPermissionProvider self, Action<E> evt) where E : struct
        {
            self.Hub.AddEvent<E>(evt);
            mWaitUninstEvents ??= new Dictionary<Type, Delegate>();
            mWaitUninstEvents.Combine(typeof(E), evt);
        }
        public static void AddNotifyAndUnregisterOnUnload<N>(this IPermissionProvider self, Action evt) where N : struct
        {
            self.Hub.AddNotify<N>(evt);
            mWaitUninstNotifies ??= new Dictionary<Type, Delegate>();
            mWaitUninstNotifies.Combine(typeof(N), evt);
        }
        public static void ExecuteSceneUnloadEvent<H>(this ModuleHub<H> hub) where H : ModuleHub<H>, new()
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
    }
    public abstract partial class ModuleHub<H>
    {
        protected ModuleHub()
        {
            SceneManager.sceneUnloaded += op => this.ExecuteSceneUnloadEvent();
            MonoKit.GetIns().OnDeInit += Deinit;
        }
    }
}