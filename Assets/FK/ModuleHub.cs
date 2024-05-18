using System;
using System.Collections.Generic;
/*
* ==============================================================================
* 项目名称: MeowFramework<精简版QF>
* 版权: (C) 版权所有 2024 胖次猫 - 保留所有权利。
* 编码标准: UTF-8
* 创建人: [ PantyNeko ]
* 创建日期: 2024-05-09
* 语言版本: C# 8.0 建议使用 Unity 2021
* 
* 描述 => 
*  这是一套由QF架构进行魔改的高性能架构,提供了高度开放的扩展权限,适合喜欢自定义,追求极限轻量的开发者
*  架构旨在支持高度模块化和灵活的系统设计,通过实现单例模式、命令/查询处理模式、事件处理等
* ==============================================================================
* 联系方式:
* - Gitee: https://gitee.com/PantyNeko
* - Github: https://github.com/pantyneko
* - Video: https://space.bilibili.com/656352
* ==============================================================================
* 原架构作者[凉鞋]:
* - Github: https://github.com/liangxiegame/QFramework
* ==============================================================================
*/
namespace Panty
{
    public interface ICmd { void Do(IModuleHub hub); }
    public interface ICmd<P> { void Do(IModuleHub hub, P info); }

    public interface IQuery<R> { R Do(IModuleHub hub); }
    public interface IQuery<P, R> { R Do(IModuleHub hub, P info); }

    public interface IModule { void TryInit(); }
    public interface IUtility { }
    public interface ICanInit : IModule
    {
        bool Preload { get; }
        void PreInit(IModuleHub hub);
        void Deinit();
    }
    public interface IPermissionProvider { IModuleHub Hub { get; } }
    public abstract class AbsModule : ICanInit, IPermissionProvider
    {
        protected bool Inited;
        private IModuleHub mHub;

        void ICanInit.PreInit(IModuleHub hub)
        {
            mHub = hub;
            if (Preload)
            {
                OnInit();
                Inited = true;
            }
        }
        void IModule.TryInit()
        {
            if (Inited) return;
            OnInit();
            Inited = true;
        }
        void ICanInit.Deinit()
        {
            if (Inited)
            {
                OnDeInit();
                Inited = false;
            }
        }
        protected abstract void OnInit();
        protected virtual void OnDeInit() { }
        public virtual bool Preload => false;
        IModuleHub IPermissionProvider.Hub => mHub;
    }
    public static partial class ModuleHubEx
    {
#if DEBUG
        public static T Log<T>(this T o)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.unityLogger.Log(o);
#endif
            return o;
        }
#endif
        public static void Combine(this Dictionary<Type, Delegate> dic, Type type, Delegate e)
        {
            if (dic.TryGetValue(type, out var del))
                dic[type] = Delegate.Combine(del, e);
            else dic.Add(type, e);
        }
        public static void Separate(this Dictionary<Type, Delegate> dic, Type type, Delegate e)
        {
            if (dic.TryGetValue(type, out var del))
            {
                del = Delegate.Remove(del, e);
                if (del == null || del.Target == null)
                    dic.Remove(type);
                else dic[type] = del;
            }
        }
        public static M Module<M>(this IPermissionProvider self) where M : class, IModule => self.Hub.Module<M>();
        public static U Utility<U>(this IPermissionProvider self) where U : class, IUtility => self.Hub.Utility<U>();

        public static void AddEvent<E>(this IPermissionProvider self, Action<E> call) where E : struct => self.Hub.AddEvent<E>(call);
        public static void RmvEvent<E>(this IPermissionProvider self, Action<E> call) where E : struct => self.Hub.RmvEvent<E>(call);
        public static void SendEvent<E>(this IPermissionProvider self, E e) where E : struct => self.Hub.SendEvent<E>(e);
        public static void SendEvent<E>(this IPermissionProvider self) where E : struct => self.Hub.SendEvent<E>();

        public static void AddNotify<N>(this IPermissionProvider self, Action call) where N : struct => self.Hub.AddNotify<N>(call);
        public static void RmvNotify<N>(this IPermissionProvider self, Action call) where N : struct => self.Hub.RmvNotify<N>(call);
        public static void SendNotify<N>(this IPermissionProvider self) where N : struct => self.Hub.SendNotify<N>();

        public static void SendCmd<C>(this IPermissionProvider self, C cmd) where C : ICmd => self.Hub.SendCmd(cmd);
        public static void SendCmd<C>(this IPermissionProvider self) where C : struct, ICmd => self.Hub.SendCmd(new C());
        public static void SendCmd<C, P>(this IPermissionProvider self, C cmd, P info) where C : ICmd<P> => self.Hub.SendCmd(cmd, info);
        public static void SendCmd<C, P>(this IPermissionProvider self, P info) where C : struct, ICmd<P> => self.Hub.SendCmd(new C(), info);

        public static R Query<Q, R>(this IPermissionProvider self) where Q : struct, IQuery<R> => self.Hub.Query<Q, R>();
        public static R Query<Q, P, R>(this IPermissionProvider self, P info) where Q : struct, IQuery<P, R> => self.Hub.Query<Q, P, R>(info);
        public static Q Query<Q>(this IPermissionProvider self) where Q : struct, IQuery<Q> => self.Hub.Query<Q>();
        public static Q Query<Q, P>(this IPermissionProvider self, P info) where Q : struct, IQuery<P, Q> => self.Hub.Query<Q, P>(info);
    }
    public partial interface IModuleHub
    {
        M Module<M>() where M : class, IModule;
        U Utility<U>() where U : class, IUtility;

        void AddEvent<E>(Action<E> call) where E : struct;
        void RmvEvent<E>(Action<E> call) where E : struct;
        void SendEvent<E>(E e) where E : struct;
        void SendEvent<E>() where E : struct;

        void AddNotify<N>(Action call) where N : struct;
        void RmvNotify<N>(Action call) where N : struct;
        void SendNotify<N>() where N : struct;

        void SendCmd<C>(C cmd) where C : ICmd;
        void SendCmd<C, P>(C cmd, P info) where C : ICmd<P>;
        void SendCmd<C>() where C : struct, ICmd;
        void SendCmd<C, P>(P info) where C : struct, ICmd<P>;

        R Query<Q, R>() where Q : struct, IQuery<R>;
        R Query<Q, P, R>(P info) where Q : struct, IQuery<P, R>;
        Q Query<Q>() where Q : struct, IQuery<Q>;
        Q Query<Q, P>(P info) where Q : struct, IQuery<P, Q>;
    }
    public abstract partial class ModuleHub<H> : IModuleHub where H : ModuleHub<H>, new()
    {
        private static H mHub;
        public static IModuleHub GetIns()
        {
            if (mHub == null)
            {
                mHub = new H();
                mHub.BuildModule();
            }
            return mHub;
        }
        private Dictionary<Type, IUtility> mUtilities = new Dictionary<Type, IUtility>();
        private Dictionary<Type, IModule> mModules = new Dictionary<Type, IModule>();
        private Dictionary<Type, Delegate> mEvents = new Dictionary<Type, Delegate>();
        private Dictionary<Type, Delegate> mNotifies = new Dictionary<Type, Delegate>();

        protected abstract void BuildModule();
        protected void AddModule<M>(M module) where M : IModule
        {
            if (mModules.TryAdd(typeof(M), module))
                (module as ICanInit).PreInit(this);
        }
        protected void AddUtility<U>(U utility) where U : IUtility
        {
            mUtilities[typeof(U)] = utility;
        }
        protected void Deinit()
        {
            if (mModules.Count > 0)
            {
                foreach (var item in mModules.Values)
                    (item as ICanInit).Deinit();
                mModules.Clear();
            }
            mUtilities.Clear();
        }
        M IModuleHub.Module<M>()
        {
            if (mModules.TryGetValue(typeof(M), out var ret))
            {
                ret.TryInit();
                return ret as M;
            }
#if DEBUG
            $"{typeof(M)}模块未被注册".Log();
#endif
            return null;
        }
        U IModuleHub.Utility<U>()
        {
            if (mUtilities.TryGetValue(typeof(U), out var ret)) return ret as U;
#if DEBUG
            $"{typeof(U)}工具未被注册".Log();
#endif
            return null;
        }
        void IModuleHub.AddEvent<E>(Action<E> action)
        {
#if DEBUG
            if (action == null) $"{action}不可为Null".Log();
#endif
            mEvents.Combine(typeof(E), action);
        }
        void IModuleHub.AddNotify<E>(Action action)
        {
#if DEBUG
            if (action == null) $"{action}不可为Null".Log();
#endif
            mNotifies.Combine(typeof(E), action);
        }
        void IModuleHub.SendEvent<E>(E e)
        {
            if (mEvents.TryGetValue(typeof(E), out var del))
            {
                (del as Action<E>).Invoke(e);
                return;
            }
#if DEBUG
            $"{typeof(E)}事件未被注册".Log();
#endif
        }
        void IModuleHub.SendEvent<E>()
        {
            if (mEvents.TryGetValue(typeof(E), out var del))
            {
                (del as Action<E>).Invoke(new E());
                return;
            }
#if DEBUG
            $"{typeof(E)}事件未被注册".Log();
#endif
        }
        void IModuleHub.SendNotify<E>()
        {
            if (mNotifies.TryGetValue(typeof(E), out var del))
            {
                (del as Action).Invoke();
                return;
            }
#if DEBUG
            $"{typeof(E)}通知未被注册".Log();
#endif
        }
        public void RmvEvent(Type type, Delegate action) => mEvents.Separate(type, action);
        public void RmvNotify(Type type, Delegate action) => mNotifies.Separate(type, action);

        void IModuleHub.RmvEvent<E>(Action<E> action) => mEvents.Separate(typeof(E), action);
        void IModuleHub.RmvNotify<E>(Action action) => mNotifies.Separate(typeof(E), action);

        void IModuleHub.SendCmd<C>() => SendCmd(new C());
        void IModuleHub.SendCmd<C, P>(P info) => SendCmd(new C(), info);
        public virtual void SendCmd<C>(C cmd) where C : ICmd => cmd.Do(this);
        public virtual void SendCmd<C, P>(C cmd, P info) where C : ICmd<P> => cmd.Do(this, info);

        R IModuleHub.Query<Q, R>() => new Q().Do(this);
        R IModuleHub.Query<Q, P, R>(P info) => new Q().Do(this, info);
        Q IModuleHub.Query<Q>() => new Q().Do(this);
        Q IModuleHub.Query<Q, P>(P info) => new Q().Do(this, info);
    }
}