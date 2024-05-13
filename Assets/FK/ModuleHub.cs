using System;
using System.Collections.Generic;
/*
* ==============================================================================
* 项目名称: MeowFramework<精简版QF>
* 版权: (C) 版权所有 2024 PantyNeko - 保留所有权利。
* 编码标准: UTF-8
* 创建人: [ Panty ]
* 创建日期: 2024-05-09
* 语言版本: C# 8.0 建议使用 Unity 2021
* 
* 描述 => 
*  这是一套由QF架构进行魔改的高性能架构,提供了高度开放的扩展权限,适合喜欢自定义,追求极限轻量的开发者
*  架构旨在支持高度模块化和灵活的系统设计,通过实现单例模式、命令/查询处理模式、事件处理等
* ==============================================================================
* 版本号: 1.0.4
* 修改历史:
* - 1.0.4 (2024-05-13) 修复内部语法错误 增加对Deinit的状态变更 避免重复调用
* - 1.0.3 (2024-05-12) 内嵌获取架构函数 移除反射机制 增加延迟初始化机制
* - 1.0.2 (2024-05-11) 调整架构的整体生命周期 避免重复初始化
* - 1.0.1 (2024-05-09) 区分有参数和无参数事件的API 
* - 1.0.0 (2024-05-09) 基本完善内容 
* ==============================================================================
* 联系方式:
* - Gitee: https://gitee.com/PantyNeko
* - Github: https://github.com/pantyneko
* - B站: https://space.bilibili.com/656352
* ==============================================================================
* 原架构作者[凉鞋]:
* - Github: https://github.com/liangxiegame/QFramework
* ==============================================================================
*/
namespace Panty
{
    public interface ICmd { void Do(IModuleHub hub); }
    public interface ICmd<P> { void Do(IModuleHub hub, P parameter); }

    public interface IQuery<R> { R Do(IModuleHub hub); }
    public interface IQuery<P, R> { R Do(IModuleHub hub, P parameter); }

    public interface IModule { void TryInit(); }
    public interface IUtility { }
    public interface ICanInit : IModule
    {
        void SetHub(IModuleHub hub);
        void Deinit();
    }
    public interface IPermissionProvider
    {
        IModuleHub Hub { get; }
    }
    public abstract class AbsModule : ICanInit, IPermissionProvider
    {
        protected bool Inited;
        protected IModuleHub mHub;
        IModuleHub IPermissionProvider.Hub => mHub;
        void ICanInit.SetHub(IModuleHub hub) => mHub = hub;
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
    }
    public static class ModuleHubEx
    {
        public static M GetModule<M>(this IPermissionProvider self) where M : class, IModule => self.Hub.Module<M>();
        public static D Model<D>(this IPermissionProvider self) where D : class, IModule => self.Hub.Module<D>();
        public static S System<S>(this IPermissionProvider self) where S : class, IModule => self.Hub.Module<S>();
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
        public static void SendCmd<C, P>(this IPermissionProvider self, C cmd, P parameter) where C : ICmd<P> => self.Hub.SendCmd(cmd, parameter);
        public static void SendCmd<C, P>(this IPermissionProvider self, P parameter) where C : struct, ICmd<P> => self.Hub.SendCmd(new C(), parameter);

        public static R Query<Q, R>(this IPermissionProvider self) where Q : struct, IQuery<R> => self.Hub.Query<Q, R>();
        public static R Query<Q, P, R>(this IPermissionProvider self, P parameter) where Q : struct, IQuery<P, R> => self.Hub.Query<Q, P, R>(parameter);
        public static Q Query<Q>(this IPermissionProvider self) where Q : struct, IQuery<Q> => self.Hub.Query<Q>();
        public static Q Query<Q, P>(this IPermissionProvider self, P parameter) where Q : struct, IQuery<P, Q> => self.Hub.Query<Q, P>(parameter);
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
#if DEBUG
                CheckUpdate();
#endif
                mHub = new H();
                mHub.BuildModule();
            }
            return mHub;
        }
#if DEBUG
        private static async void CheckUpdate()
        {
            string url = "https://raw.githubusercontent.com/pantyneko/MeowFramework/main/Assets/VersionInfo.txt";
            string version = "1.0.5";
            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (responseBody != version)
                    {
                        string msg = $"当前架构版本为:{version},最新版本为:{responseBody},请及时更新!\r\n更新地址↓\r\n" +
                            $"GitHub => https://github.com/pantyneko/MeowFramework\r\n" +
                            $"Gitee => https://gitee.com/PantyNeko/MeowFramework\r\n";
#if UNITY_EDITOR
                        UnityEngine.Debug.LogWarning(msg);
#else
                        if (Environment.UserInteractive) Console.WriteLine(msg);
                        else System.Diagnostics.Debug.WriteLine(msg);
#endif
                    }
                }
                catch (System.Net.Http.HttpRequestException e)
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogWarning(e.Message);
#else
                    if (Environment.UserInteractive) Console.WriteLine(e.Message);
                    else System.Diagnostics.Debug.WriteLine(e.Message);
#endif
                }

            }
        }
#endif
        private Dictionary<Type, IUtility> mUtilities = new Dictionary<Type, IUtility>();
        private Dictionary<Type, IModule> mModules = new Dictionary<Type, IModule>();
        private Dictionary<Type, Delegate> mEvents = new Dictionary<Type, Delegate>();
        private Dictionary<Type, Action> mNotifies = new Dictionary<Type, Action>();

        protected abstract void BuildModule();
        protected void AddModule<M>(M module) where M : IModule
        {
            if (mModules.TryAdd(typeof(M), module))
                (module as ICanInit).SetHub(this);
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
            throw new Exception($"检查{typeof(M)}模块是否被注册");
#endif
        }
        U IModuleHub.Utility<U>()
        {
            if (mUtilities.TryGetValue(typeof(U), out var ret)) return ret as U;
#if DEBUG
            throw new Exception($"检查{typeof(U)}模块是否被注册");
#endif
        }
        void IModuleHub.AddEvent<E>(Action<E> action)
        {
#if DEBUG
            CheckActionNull(action);
#endif
            var type = typeof(E);
            if (mEvents.TryGetValue(type, out var del))
                mEvents[type] = Delegate.Combine(del, action);
            else mEvents.Add(type, action);
        }
        void IModuleHub.RmvEvent<E>(Action<E> action)
        {
#if DEBUG
            CheckActionNull(action);
#endif
            var type = typeof(E);
            if (mEvents.TryGetValue(type, out var del))
            {
                del = Delegate.Remove(del, action);
                if (del == null)
                    mEvents.Remove(type);
                else
                    mEvents[type] = del;
            }
        }
        void IModuleHub.SendEvent<E>(E e)
        {
            if (mEvents.TryGetValue(typeof(E), out var del))
                (del as Action<E>).Invoke(e);
        }
        void IModuleHub.SendEvent<E>()
        {
            if (mEvents.TryGetValue(typeof(E), out var del))
                (del as Action<E>).Invoke(new E());
        }
        void IModuleHub.AddNotify<E>(Action action)
        {
#if DEBUG
            CheckActionNull(action);
#endif
            var type = typeof(E);
            if (mNotifies.TryGetValue(type, out var del))
                mNotifies[type] = del + action;
            else mNotifies.Add(type, action);
        }
        void IModuleHub.RmvNotify<E>(Action action)
        {
#if DEBUG
            CheckActionNull(action);
#endif
            var type = typeof(E);
            if (mNotifies.TryGetValue(type, out var del))
            {
                if ((del -= action) == null)
                    mNotifies.Remove(type);
                else
                    mNotifies[type] = del;
            }
        }
#if DEBUG
        private void CheckActionNull(Delegate action)
        {
            if (action == null)
                throw new Exception($"{action}不可为Null");
        }
#endif
        void IModuleHub.SendNotify<E>()
        {
            if (mNotifies.TryGetValue(typeof(E), out var del)) del();
        }
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