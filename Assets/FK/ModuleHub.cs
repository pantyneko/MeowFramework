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
    #region 架构模块和规则接口    
    /// <summary>
    /// 用于分离命令中的具体执行逻辑 
    /// </summary>
    public interface IReceiver { }
    /// <summary>
    /// 无参数的命令接口，用于执行不需要额外信息的操作
    /// </summary>
    public interface ICmd { void Do(IModuleHub hub); }
    /// <summary>
    /// 带参数的命令接口，用于执行需要额外信息的操作
    /// </summary>
    public interface ICmd<P> { void Do(IModuleHub hub, P info); }
    /// <summary>
    /// 仅返回结果的无参数查询接口
    /// </summary>
    public interface IQuery<R> { R Do(IModuleHub hub); }
    /// <summary>
    /// 带参数且返回结果的查询接口
    /// </summary>
    public interface IQuery<P, R> { R Do(IModuleHub hub, P info); }
    /// <summary>
    /// 权限提供者接口 > 为对象赋予访问架构的能力
    /// </summary>
    public interface IPermissionProvider { IModuleHub Hub { get; } }
    /// <summary>
    /// 模块接口 > 标识该对象为带状态模块
    /// </summary>
    public interface IModule { void TryInit(); }
    /// <summary>
    /// 工具接口 > 标识对象为无状态工具
    /// </summary>
    public interface IUtility { }
    /// <summary>
    /// 可初始化接口，用于对外隐藏模块的初始化方法。
    /// </summary>
    public interface ICanInit : IModule
    {
        bool Preload { get; }
        void PreInit(IModuleHub hub);
        void Deinit();
    }
    /// <summary>
    /// 抽象模块基类，实现基本生命周期和权限提供。
    /// </summary>
    public abstract class AbsModule : ICanInit, IPermissionProvider
    {
        protected bool Inited;
        private IModuleHub mHub;
        // 预初始化 > 当所有模块被注册时调用。如果Preload标记为true，会立即初始化模块。
        void ICanInit.PreInit(IModuleHub hub)
        {
            mHub = hub;

            if (Preload && !Inited)
            {
#if DEBUG
                $"{this} 预初始化成功".Log();
#endif  
                OnInit();
                Inited = true;
            }
        }
        // 尝试初始化 > 每次访问模块时调用 会在第一次访问时对模块进行初始化
        void IModule.TryInit()
        {
            if (Inited) return;
#if DEBUG
            $"{this} 被初始化".Log();
#endif  
            OnInit();
            Inited = true;
        }
        // 逆初始化 > 用于集中处理模块的内存释放和销毁处理
        void ICanInit.Deinit()
        {
            if (Inited)
            {
                OnDeInit();
                Inited = false;
            }
        }
        /// <summary>
        /// 抽象的初始化方法，由具体模块实现其初始化逻辑。
        /// </summary>
        protected abstract void OnInit();
        /// <summary>
        /// 可重写的逆初始化方法，供具体模块实现其资源释放逻辑。
        /// </summary>
        protected virtual void OnDeInit() { }
        /// <summary>
        /// 可重写的预初始化属性 用来指示是否提前初始化
        /// </summary>
        public virtual bool Preload => false;
        // 实现IPermissionProvider接口，提供模块的访问能力。
        IModuleHub IPermissionProvider.Hub => mHub;
    }
    #endregion
    #region 架构扩展
    public static partial class HubTool
    {
#if DEBUG
        public const string version = "1.1.2";
        /// <summary>
        /// 在调试模式下 将对象信息输出到控制台 可支持多个平台。
        /// </summary>
        public static T Log<T>(this T o)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.unityLogger.Log(o);
#endif
            return o;
        }
        public static void DicLog<K, V>(this Dictionary<K, V> dic, string dicName, string prefix)
        {
            if (dic.Count == 0)
            {
                $"{dicName} 为空".Log();
            }
            else
            {
                var builder = new System.Text.StringBuilder();
                foreach (var pair in dic)
                {
                    builder.AppendLine($"键 = {pair.Key} ");
                    builder.AppendLine($"值 = {pair.Value} \r\n");
                }
                $"{prefix}\r\n\r\n{builder}".Log();
            }
        }
#endif
        /// <summary>
        /// 将委托添加到字典中 
        /// </summary>
        public static void Combine(this Dictionary<Type, Delegate> events, Type type, Delegate evt)
        {
            events[type] = events.TryGetValue(type, out var methods) ? Delegate.Combine(methods, evt) : evt;
        }
        /// <summary>
        /// 将委托从字典中移除
        /// </summary>
        public static void Separate(this Dictionary<Type, Delegate> events, Type type, Delegate evt)
        {
            if (events.TryGetValue(type, out var methods))
            {
                methods = Delegate.Remove(methods, evt);
                if (methods == null) events.Remove(type);
                else events[type] = methods;
            }
        }
    }
    public static partial class HubEx
    {
        public static M Module<M>(this IPermissionProvider self) where M : class, IModule => self.Hub.Module<M>();
        public static U Utility<U>(this IPermissionProvider self) where U : class, IUtility => self.Hub.Utility<U>();
        public static IRmv AddEvent<E>(this IPermissionProvider self, Action<E> evt) where E : struct => self.Hub.AddEvent<E>(evt);
        public static void RmvEvent<E>(this IPermissionProvider self, Action<E> evt) where E : struct => self.Hub.RmvEvent<E>(evt);
        public static void SendEvent<E>(this IPermissionProvider self, E e) where E : struct => self.Hub.SendEvent<E>(e);
        public static void SendEvent<E>(this IPermissionProvider self) where E : struct => self.Hub.SendEvent<E>();
        public static IRmv AddNotify<N>(this IPermissionProvider self, Action evt) where N : struct => self.Hub.AddNotify<N>(evt);
        public static void RmvNotify<N>(this IPermissionProvider self, Action evt) where N : struct => self.Hub.RmvNotify<N>(evt);
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
    #endregion
    #region 架构接口
    /// <summary>
    /// 定义模块中心的接口，负责管理模块、工具、事件、通知、命令和查询。
    /// </summary>
    public partial interface IModuleHub
    {
        M Module<M>() where M : class, IModule;
        U Utility<U>() where U : class, IUtility;
        IRmv AddEvent<E>(Action<E> evt) where E : struct;
        void RmvEvent<E>(Action<E> evt) where E : struct;
        void SendEvent<E>(E e) where E : struct;
        void SendEvent<E>() where E : struct;
        IRmv AddNotify<N>(Action evt) where N : struct;
        void RmvNotify<N>(Action evt) where N : struct;
        void SendNotify<N>() where N : struct;
        void SendCmd<C>(C cmd) where C : ICmd;
        void SendCmd<C>() where C : struct, ICmd;
        void SendCmd<C, P>(C cmd, P info) where C : ICmd<P>;
        void SendCmd<C, P>(P info) where C : struct, ICmd<P>;
        R Query<Q, R>() where Q : struct, IQuery<R>;
        R Query<Q, P, R>(P info) where Q : struct, IQuery<P, R>;
        Q Query<Q>() where Q : struct, IQuery<Q>;
        Q Query<Q, P>(P info) where Q : struct, IQuery<P, Q>;
    }
    #endregion
    #region 架构基类
    public interface IRmv { void Do(); }
    // 实现自身移除委托
    public class CustomRmv : IRmv
    {
        private Action call;
        public CustomRmv(Action call) => this.call = call;
        void IRmv.Do() => call?.Invoke();
    }
    // 实现字典中移除委托
    public class DelegateDicRmv<T> : IRmv where T : struct
    {
        private Dictionary<Type, Delegate> mEvents;
        private Delegate call;
        void IRmv.Do() => mEvents.Separate(typeof(T), call);
        public DelegateDicRmv(Dictionary<Type, Delegate> events, Delegate e)
        {
            mEvents = events;
            call = e;
        }
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
        /// <summary>
        /// 由子类重写实现所有模块和工具的构建与注册
        /// </summary>
        protected abstract void BuildModule();
        // 往架构添加模块 尝试预初始化
        protected void AddModule<M>(M module) where M : IModule
        {
            if (mModules.TryAdd(typeof(M), module))
                (module as ICanInit).PreInit(this);
        }
        // 往架构添加工具
        protected void AddUtility<U>(U utility) where U : IUtility
        {
            mUtilities[typeof(U)] = utility;
        }
        /// <summary>
        /// 清理所有已初始化模块的状态信息
        /// </summary>
        protected void Dispose()
        {
#if DEBUG
            mUtilities.DicLog("mUtilities", "------------///  已释放以下工具  ///------------");
            mModules.DicLog("mModules", "------------///  已释放以下模块  ///------------");
#endif
            if (mModules.Count > 0)
            {
                foreach (var module in mModules.Values)
                    (module as ICanInit).Deinit();
            }
            mModules = null;
            mUtilities = null;
            mEvents = null;
            mNotifies = null;
        }
        M IModuleHub.Module<M>()
        {
            if (mModules.TryGetValue(typeof(M), out var ret))
            {
                ret.TryInit();
                return ret as M;
            }
#if DEBUG
            $"{typeof(M)} 模块未注册".Log();
#endif
            return null;
        }
        U IModuleHub.Utility<U>()
        {
            if (mUtilities.TryGetValue(typeof(U), out var ret)) return ret as U;
#if DEBUG
            $"{typeof(U)} 工具未注册".Log();
#endif
            return null;
        }
        IRmv IModuleHub.AddEvent<E>(Action<E> evt)
        {
#if DEBUG
            if (evt == null) $"{evt} 不可为Null".Log();
#endif
            mEvents.Combine(typeof(E), evt);
            return new DelegateDicRmv<E>(mEvents, evt);
        }
        IRmv IModuleHub.AddNotify<E>(Action evt)
        {
#if DEBUG
            if (evt == null) $"{evt} 不可为Null".Log();
#endif
            mNotifies.Combine(typeof(E), evt);
            return new DelegateDicRmv<E>(mNotifies, evt);
        }
        void IModuleHub.SendEvent<E>(E e)
        {
            if (mEvents.TryGetValue(typeof(E), out var methods))
            {
                (methods as Action<E>).Invoke(e);
                return;
            }
#if DEBUG
            $"{typeof(E)} 事件未注册".Log();
#endif
        }
        void IModuleHub.SendEvent<E>()
        {
            if (mEvents.TryGetValue(typeof(E), out var methods))
            {
                (methods as Action<E>).Invoke(new E());
                return;
            }
#if DEBUG
            $"{typeof(E)} 事件未注册".Log();
#endif
        }
        void IModuleHub.SendNotify<E>()
        {
            if (mNotifies.TryGetValue(typeof(E), out var methods))
            {
                (methods as Action).Invoke();
                return;
            }
#if DEBUG
            $"{typeof(E)} 通知未注册".Log();
#endif
        }
        void IModuleHub.RmvEvent<E>(Action<E> evt) => mEvents.Separate(typeof(E), evt);
        void IModuleHub.RmvNotify<E>(Action evt) => mNotifies.Separate(typeof(E), evt);

        void IModuleHub.SendCmd<C>() => SendCmd(cmd: new C());
        void IModuleHub.SendCmd<C, P>(P info) => SendCmd(cmd: new C(), info);
        // 可重写的命令 架构子类可对该命令逻辑进行重写 例如在命令前后记录日志
        public virtual void SendCmd<C>(C cmd) where C : ICmd => cmd.Do(this);
        public virtual void SendCmd<C, P>(C cmd, P info) where C : ICmd<P> => cmd.Do(this, info);

        R IModuleHub.Query<Q, R>() => new Q().Do(this);
        R IModuleHub.Query<Q, P, R>(P info) => new Q().Do(this, info);
        Q IModuleHub.Query<Q>() => new Q().Do(this);
        Q IModuleHub.Query<Q, P>(P info) => new Q().Do(this, info);
    }
    #endregion
}