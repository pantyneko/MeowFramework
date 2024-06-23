using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private static T ins;
        public static T GetIns()
        {
#if UNITY_EDITOR
            // 防止编辑器意外创建 主要是 ExecuteInEditMode
            if (!UnityEditor.EditorApplication.isPlaying) return null;
#endif
            if (ins == null && (ins = FindObjectOfType<T>()) == null)
            {
                var o = new GameObject(typeof(T).Name);
                GameObject.DontDestroyOnLoad(o);
                ins = o.AddComponent<T>();
            }
            return ins;
        }
        private void Awake()
        {
            if (ins == null)
            {
                ins = this as T;
                InitSingle();
            }
            else Destroy(gameObject);
        }
        protected virtual void InitSingle() { }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, Inherited = false)]
    public class EmptyAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FindComponentAttribute : Attribute
    {
        public string GoName;
        public bool GetChild;
        /// <summary>
        /// 查找游戏物体组件的特性
        /// </summary>
        /// <param name="goName">游戏物体名字</param>
        /// <param name="getChild">
        /// true => 查找对应名字对象的下一级子物体 通常名字为父物体名字 类型为子物体
        /// false => 查找对应名字的对象 通常会将类型和名字对应</param>
        public FindComponentAttribute(string goName, bool getChild = true)
        {
            GoName = goName;
            GetChild = getChild;
        }
    }
    public abstract class RmvTrigger : MonoBehaviour
    {
        private readonly Stack<IRmv> rmvs = new Stack<IRmv>();
        public void Add(IRmv rmv) => rmvs.Push(rmv);
        protected void RmvAll()
        {
            while (rmvs.Count > 0) rmvs.Pop().Do();
        }
    }
    public class RmvOnDestroyTrigger : RmvTrigger
    {
        private void OnDestroy() => RmvAll();
    }
    public class RmvOnDisableTrigger : RmvTrigger
    {
        private void OnDisable() => RmvAll();
    }
    public class MonoKit : MonoBehaviour
    {
        private static MonoKit mono;
        public static MonoKit GetIns() => mono;

        public static event Action OnUpdate;
        public static event Action OnFixedUpdate;
        public static event Action OnLateUpdate;
        public static event Action OnGuiUpdate;

        private void Awake()
        {
            if (mono == null) mono = this;
            else Destroy(mono.gameObject);
        }
        private void Update() => OnUpdate?.Invoke();
        private void FixedUpdate() => OnFixedUpdate?.Invoke();
        private void LateUpdate() => OnLateUpdate?.Invoke();
        private void OnGUI() => OnGuiUpdate?.Invoke();
    }
    public static partial class HubTool
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var o = new GameObject(nameof(MonoKit), typeof(MonoKit));
            GameObject.DontDestroyOnLoad(o);
        }
        public static Component GetOrAddComponent(this GameObject o, Type type)
        {
            var t = o.GetComponent(type);
            return t == null ? o.AddComponent(type) : t;
        }
        public static T GetOrAddComponent<T>(this GameObject o) where T : Component
        {
            T t = o.GetComponent<T>();
            return t == null ? o.AddComponent<T>() : t;
        }
        /// <summary>
        /// 尝试从一个物体身上获取脚本 如果获取不到就添加一个
        /// </summary>
        public static T GetOrAddComponent<T>(this Component o) where T : Component
        {
            T t = o.GetComponent<T>();
            return t == null ? o.gameObject.AddComponent<T>() : t;
        }
        public readonly static Assembly BaseAss = Assembly.Load("Assembly-CSharp");
        /// <summary>
        /// 查找所有带标记的组件
        /// </summary>
        public static void FindComponents(this Component mono)
        {
            Dictionary<Type, Component[]> dic = null;
            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            foreach (var field in mono.GetType().GetFields(flags))
            {
                var attribute = field.GetCustomAttribute<FindComponentAttribute>();
                if (attribute == null) continue;
                dic ??= new Dictionary<Type, Component[]>();
                Type type = field.FieldType;
                if (!dic.TryGetValue(type, out var components))
                {
                    components = mono.GetComponentsInChildren(type, true);
                    if (components == null || components.Length == 0)
                    {
#if UNITY_EDITOR
                        $"无法找到{type}对象".Log();
#endif
                        continue;
                    }
                    dic.Add(type, components);
                }
                if (attribute.GetChild)
                {
                    foreach (var component in components)
                    {
                        if (component.transform.parent.name == attribute.GoName)
                        {
                            field.SetValue(mono, component);
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var component in components)
                    {
                        if (component.name == attribute.GoName)
                        {
                            field.SetValue(mono, component);
                            break;
                        }
                    }
                }
            }
        }
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
        public static T Error<T>(this T o)
        {
            Debug.unityLogger.Log(LogType.Error, o);
            return o;
        }
        public static T Warning<T>(this T o)
        {
            Debug.unityLogger.Log(LogType.Warning, o);
            return o;
        }
        public static void Box(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color, float duration = 0)
        {
            Debug.DrawLine(a, b, color, duration);
            Debug.DrawLine(b, c, color, duration);
            Debug.DrawLine(c, d, color, duration);
            Debug.DrawLine(d, a, color, duration);
        }
        public static void Box(Vector2 origin, Vector2 size, Color color, float duration = 0)
        {
            Vector2 _half = size * 0.5f;

            float x1 = origin.x - _half.x;
            float x2 = origin.x + _half.x;
            float y1 = origin.y - _half.y;
            float y2 = origin.y + _half.y;

            var a = new Vector2(x1, y2);
            var b = new Vector2(x2, y2);
            var c = new Vector2(x2, y1);
            var d = new Vector2(x1, y1);

            Box(a, b, c, d, color, duration);
        }
    }
    public class I
    {
        public static string Space = "Panty.Test";
        public static string TPath = "Assets/Scripts";
        public static string Hub = "Test";
        public readonly static string[] Search = new string[] { "Assets" };
    }
    public static partial class HubEx
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
        /// 标记为物体被销毁时注销
        /// </summary>
        public static void RmvOnDestroy(this IRmv rmv, Component c) => c.GetOrAddComponent<RmvOnDestroyTrigger>().Add(rmv);
        /// <summary>
        /// 标记为物体失活时注销
        /// </summary>
        public static void RmvOnDisable(this IRmv rmv, Component c) => c.GetOrAddComponent<RmvOnDisableTrigger>().Add(rmv);
        /// <summary>
        /// 标记为场景卸载时注销
        /// </summary>
        public static void RmvOnSceneUnload(this IRmv rmv) => mWaitUnLoadRmvs.Push(rmv);
        /// <summary>
        /// 用于当前场景卸载时 注销所有事件和通知
        /// </summary>
        public static void OnSceneUnloadComplete()
        {
            while (mWaitUnLoadRmvs.Count > 0)
                mWaitUnLoadRmvs.Pop().Do();
        }
        // 用于存储所有当前场景卸载时 需要注销的事件和通知
        private readonly static Stack<IRmv> mWaitUnLoadRmvs = new Stack<IRmv>();
    }
    public abstract partial class ModuleHub<H>
    {
        protected ModuleHub()
        {
            Application.quitting += async () =>
            {
                await Task.Yield();
                this.Dispose();
            };
            // 预注册场景卸载事件
            SceneManager.sceneUnloaded +=
                scene => HubEx.OnSceneUnloadComplete();
        }
    }
}