# MeowFramework<精简版QF> 文档

## 项目概述

**项目名称:** 喵喵框架  
**作者:** [PantyNeko](https://gitee.com/PantyNeko)  
**创建日期:** 2024-05-09  
**描述:** 这是一个基于高性能QF架构的抠门级框架，提供了高度开放的扩展权限，适合喜欢自定义、追求极限轻量的开发者。架构旨在支持高度模块化和灵活的系统设计，实现了单例模式、命令/查询处理模式和事件处理等功能。框架的设计理念是简化开发流程，提供高效、可扩展的解决方案，适用于各种规模的项目。

![](https://gitee.com/PantyNeko/MeowFramework/raw/main/Assets/Doc/Logo.png)

## 底层接口

### 接口 IReceiver

**用途:** 用于分离命令中的具体执行逻辑。  
**示例:**
```csharp
public class MyReceiver : IReceiver
{
    // 实现细节
}
```

### 接口 ICmd

**用途:** 无参数的命令接口，用于执行不需要额外信息的操作。  
**方法:**

- `void Do(IModuleHub hub);`  
**示例:**
```csharp
public class SimpleCommand : ICmd
{
    public void Do(IModuleHub hub)
    {
        // 命令逻辑
    }
}
```

### 接口 ICmd<P>

**用途:** 带参数的命令接口，用于执行需要额外信息的操作。  
**方法:**
- `void Do(IModuleHub hub, P info);`  
**示例:**
```csharp
public class ParameterizedCommand : ICmd<string>
{
    public void Do(IModuleHub hub, string info)
    {
        // 命令逻辑
    }
}
```

### 接口 IQuery<R>

**用途:** 仅返回结果的无参数查询接口。  
**方法:**
- `R Do(IModuleHub hub);`  
**示例:**
```csharp
public class SimpleQuery : IQuery<int>
{
    public int Do(IModuleHub hub)
    {
        // 查询逻辑
        return 42;
    }
}
```

### 接口 IQuery<P, R>

**用途:** 带参数且返回结果的查询接口。  
**方法:**
- `R Do(IModuleHub hub, P info);`  
**示例:**
```csharp
public class ParameterizedQuery : IQuery<string, int>
{
    public int Do(IModuleHub hub, string info)
    {
        // 查询逻辑
        return info.Length;
    }
}
```

### 接口 IPermissionProvider

**用途:** 权限提供者接口，为对象赋予访问架构的能力。  
**属性:**
- `IModuleHub Hub { get; }`  
**示例:**
```csharp
public class MyPermissionProvider : IPermissionProvider
{
    public IModuleHub Hub { get; private set; }

    public MyPermissionProvider(IModuleHub hub)
    {
        Hub = hub;
    }
}
```

### 接口 IModule

**用途:** 模块接口，标识该对象为带状态模块。  
**方法:**
- `void TryInit();`  
**示例:**
```csharp
public class MyModule : IModule
{
    public void TryInit()
    {
        // 初始化逻辑
    }
}
```

### 接口 IUtility

**用途:** 工具接口，标识对象为无状态工具。  
**示例:**
```csharp
public class MyUtility : IUtility
{
    // 工具逻辑
}
```

### 接口 ICanInit

**用途:** 可初始化接口，用于对外隐藏模块的初始化方法。  
**继承:** 继承自 IModule  
**属性和方法:**
- `bool Preload { get; }`
- `void PreInit(IModuleHub hub);`
- `void Deinit();`  
**示例:**
```csharp
public class MyInitModule : ICanInit
{
    public bool Preload => true;

    public void PreInit(IModuleHub hub)
    {
        // 预初始化逻辑
    }

    public void Deinit()
    {
        // 逆初始化逻辑
    }

    public void TryInit()
    {
        // 尝试初始化逻辑
    }
}
```

### 抽象类 AbsModule

**用途:** 抽象模块基类，实现基本生命周期和权限提供。  
**实现的接口:** ICanInit, IPermissionProvider  
**属性和方法:**
- `bool Inited`: 模块是否已初始化。
- `IModuleHub mHub`: 模块中心实例。
- `void ICanInit.PreInit(IModuleHub hub)`: 预初始化方法。
- `void IModule.TryInit()`: 尝试初始化方法。
- `void ICanInit.Deinit()`: 逆初始化方法。
- `protected abstract void OnInit()`: 抽象的初始化方法，由具体模块实现其初始化逻辑。
- `protected virtual void OnDeInit()`: 可重写的逆初始化方法，供具体模块实现其资源释放逻辑。
- `public virtual bool Preload => false`: 可重写的预初始化属性，用来指示是否提前初始化。
- `IModuleHub IPermissionProvider.Hub => mHub`: 实现IPermissionProvider接口，提供模块的访问能力。  
**示例:**
```csharp
public class MyModule : AbsModule
{
    protected override void OnInit()
    {
        // 初始化逻辑
    }

    protected override void OnDeInit()
    {
        // 逆初始化逻辑
    }
}
```

---

## 架构工具

### 静态类 HubTool

**用途:** 提供扩展方法和工具函数。  
**版本:** "1.1.1"（调试模式下）  
**方法和示例:**

- `public static T Log<T>(this T o)`: 在调试模式下将对象信息输出到控制台。
  ```csharp
  "调试信息".Log();
  ```

- `public static void DicLog<K, V>(this Dictionary<K, V> dic, string dicName, string prefix)`: 输出字典信息到控制台。
  ```csharp
  var myDic = new Dictionary<string, int>();
  myDic.DicLog("myDic", "前缀信息");
  ```

- `public static void Combine(this Dictionary<Type, Delegate> events, Type type, Delegate evt)`: 将委托添加到字典中。
  ```csharp
  var events = new Dictionary<Type, Delegate>();
  events.Combine(typeof(MyEvent), new Action<MyEvent>(e => { /* 事件逻辑 */ }));
  ```

- `public static void Separate(this Dictionary<Type, Delegate> events, Type type, Delegate evt)`: 将委托从字典中移除。
  ```csharp
  var events = new Dictionary<Type, Delegate>();
  events.Separate(typeof(MyEvent), new Action<MyEvent>(e => { /* 事件逻辑 */ }));
  ```

- `public static T GetOrAdd<T>(this Component o) where T : Component`: 从组件获取脚本，如果获取不到就添加一个。
  ```csharp
  var component = this.GetOrAdd<MyComponent>();
  ```

- `public static void FindComponents(this Component mono)`: 查找所有带标记的组件。
  ```csharp
  this.FindComponents();
  ```

- `public static void FindChildrenControl<T>(this Component mono, Action<string, T> callback) where T : Component`: 找到面板父节点下所有对应控件。
  ```csharp
  this.FindChildrenControl<MyComponent>((name, component) => {
      // 控件逻辑
  });
  ```

### 静态类 HubEx

**用途:** 提供对模块、工具、事件和命令的扩展方法。  
**方法和示例:**

- `public static M Module<M>(this IPermissionProvider self) where M : class, IModule`
  ```csharp
  var myModule = this.Module<MyModule>();
  ```

- `public static U Utility<U>(this IPermissionProvider self) where U : class, IUtility`
  ```csharp
  var myUtility = this.Utility<MyUtility>();
  ```

- `public static IRmv AddEvent<E>(this IPermissionProvider self, Action<E> evt) where E : struct`
  ```csharp
  IRmv removal = this.AddEvent<MyEvent>(e => { /* 事件处理逻辑 */ });
  ```

- `public static void RmvEvent<E>(this IPermissionProvider self, Action<E> evt) where E : struct`
  ```csharp
  this.RmvEvent<MyEvent>(myEventHandler);
  ```

- `public static void SendEvent<E>(this IPermissionProvider self, E e) where E : struct`
  ```csharp
  this.SendEvent(new MyEvent());
  ```

- `public static void SendEvent<E>(this IPermissionProvider self) where E : struct`
  ```csharp
  this.SendEvent<MyEvent>();
  ```

- `public static IRmv AddNotify<N>(this IPermissionProvider self, Action evt) where N : struct`
  ```csharp
  IRmv notifyRemoval = this.AddNotify<MyNotification>(() => { /* 通知处理逻辑 */ });
  ```

- `public static void RmvNotify<N>(this IPermissionProvider self, Action evt) where N : struct`
  
  ```csharp
  this.RmvNotify<MyNotification>(myNotifyHandler);
  ```
  
- `public static void SendNotify<N>(this IPermissionProvider self) where N : struct`
  ```csharp
  this.SendNotify<MyNotification>();
  ```

- `public static void SendCmd<C>(this IPermissionProvider self, C cmd) where C : ICmd`
  ```csharp
  this.SendCmd(new MyCommand());
  ```

- `public static void SendCmd<C>(this IPermissionProvider self) where C : struct, ICmd`
  ```csharp
  this.SendCmd<MyCommand>();
  ```

- `public static void SendCmd<C, P>(this IPermissionProvider self, C cmd, P info) where C : ICmd<P>`
  ```csharp
  this.SendCmd(new MyParameterizedCommand(), "参数信息");
  ```

- `public static void SendCmd<C, P>(this IPermissionProvider self, P info) where C : struct, ICmd<P>`
  ```csharp
  this.SendCmd<MyParameterizedCommand, string>("参数信息");
  ```

- `public static R Query<Q, R>(this IPermissionProvider self) where Q : struct, IQuery<R>`
  ```csharp
  var result = this.Query<MyQuery, int>();
  ```

- `public static R Query<Q, P, R>(this IPermissionProvider self, P info) where Q : struct, IQuery<P, R>`
  ```csharp
  var result = this.Query<MyParameterizedQuery, string, int>("参数信息");
  ```

- `public static Q Query<Q>(this IPermissionProvider self) where Q : struct, IQuery<Q>`
  ```csharp
  var result = this.Query<MyQuery>();
  ```

- `public static Q Query<Q, P>(this IPermissionProvider self, P info) where Q : struct, IQuery<P, Q>`
  ```csharp
  var result = this.Query<MyParameterizedQuery, string>("参数信息");
  ```

---

## 架构接口

### 接口 IModuleHub

**用途:** 定义模块中心的接口，负责管理模块、工具、事件、通知、命令和查询。  
**方法:**
- `M Module<M>() where M : class, IModule;`
- `U Utility<U>() where U : class, IUtility;`
- `IRmv AddEvent<E>(Action<E> evt) where E : struct;`
- `void RmvEvent<E>(Action<E> evt) where E : struct;`
- `void SendEvent<E>(E e) where E : struct;`
- `void SendEvent<E>() where E : struct;`
- `IRmv AddNotify<N>(Action evt) where N : struct;`
- `void RmvNotify<N>(Action evt) where N : struct;`
- `void SendNotify<N>() where N : struct;`
- `void SendCmd<C>(C cmd) where C : ICmd;`
- `void SendCmd<C>() where C : struct, ICmd;`
- `void SendCmd<C, P>(C cmd, P info) where C : ICmd<P>;`
- `void SendCmd<C, P>(P info) where C : struct, ICmd<P>;`
- `R Query<Q, R>() where Q : struct, IQuery<R>;`
- `R Query<Q, P, R>(P info) where Q : struct, IQuery<P, R>;`
- `Q Query<Q>() where Q : struct, IQuery<Q>;`
- `Q Query<Q, P>(P info) where Q : struct, IQuery<P, Q>;`

---

## 架构基类

### 类 CustomRmv

**用途:** 实现自身移除委托。  
**方法:**
- `void Do()`  
**示例:**
```csharp
public class CustomRmv : IRmv
{
    private Action call;
    public CustomRmv(Action call) => this.call = call;
    void IRmv.Do() => call?.Invoke();
}
// 使用示例
IRmv rmv = new CustomRmv(() => { /* 移除逻辑 */ });
rmv.Do();
```

### 类 DelegateDicRmv<T>

**用途:** 实现字典中移除委托。  
**方法:**
- `void Do()`  
**示例:**
```csharp
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
// 使用示例
var events = new Dictionary<Type, Delegate>();
var handler = new Action<MyEvent>(e => { /* 事件逻辑 */ });
events.Combine(typeof(MyEvent), handler);
IRmv rmv = new DelegateDicRmv<MyEvent>(events, handler);
rmv.Do();
```

### 抽象类 ModuleHub<H>

**用途:** 模块中心抽象类，负责模块和工具的管理。  
**方法和属性:**
- `public static IModuleHub GetIns()`: 获取单例实例。
- `protected abstract void BuildModule()`: 构建模块和工具。
- `protected void AddModule<M>(M module) where M : IModule`: 添加模块并尝试预初始化。
- `protected void AddUtility<U>(U utility) where U : IUtility`: 添加工具。
- `protected void Dispose()`: 释放所有已初始化模块的状态信息。  
**示例:**
```csharp
public abstract class MyModuleHub : ModuleHub<MyModuleHub>
{
    protected override void BuildModule()
    {
        // 构建模块和工具
        AddModule(new MyModule());
        AddUtility(new MyUtility());
    }
}

// 使用示例
var hub = MyModuleHub.GetIns();
var module = hub.Module<MyModule>();
var utility = hub.Utility<MyUtility>();
hub.SendCmd(new MyCommand());
var result = hub.Query<MyQuery, int>();
```

---

## 单例模式

### 接口 ISingleton

**用途:** 单例接口。  
**方法:**
- `void Init();`  
**示例:**
```csharp
public class MySingleton : ISingleton
{
    public void Init()
    {
        // 初始化逻辑
    }
}
```

### 抽象类 Singleton<S>

**用途:** 单例基类。  
**属性和方法:**
- `public static S GetIns()`: 获取单例实例。  
**示例:**
```csharp
public class MySingleton : Singleton<MySingleton>, ISingleton
{
    private MySingleton() { }

    public void Init()
    {
        // 初始化逻辑
    }
}

// 使用示例
var instance = MySingleton.GetIns();
instance.Init();
```

### 抽象类 MonoSingle<T>

**用途:** Unity的Mono单例基类。  
**属性和方法:**
- `public static T GetIns()`: 获取单例实例。
- `protected virtual void InitSingle()`: 初始化单例。  
**示例:**
```csharp
public class MyMonoSingle : MonoSingle<MyMonoSingle>
{
    protected override void InitSingle()
    {
        // 初始化逻辑
    }
}

// 使用示例
var instance = MyMonoSingle.GetIns();
```

### 属性 FindComponentAttribute

**用途:** 查找游戏物体组件的特性。  
**构造函数参数:**
- `string GoName`: 游戏物体名字。
- `bool GetChild`: 是否查找对应名字对象的下一级子物体。  
**示例:**
```csharp
public class MyComponent : MonoBehaviour
{
    [FindComponent("ChildObject", true)]
    public ChildComponent childComponent;
}
```

### 抽象类 RmvTrigger

**用途:** 用于在特定条件下移除事件和通知。  
**方法:**
- `public void Add(IRmv rmv)`: 添加需要移除的事件或通知。
- `protected void RmvAll()`: 移除所有事件和通知。  
**示例:**
```csharp
public class MyRmvTrigger : RmvTrigger
{
    // 触发器逻辑
}

// 使用示例
var trigger = gameObject.AddComponent<MyRmvTrigger>();
trigger.Add(new CustomRmv(() => { /* 移除逻辑 */ }));
trigger.RmvAll();
```

### 类 RmvOnDestroyTrigger

**用途:** 在对象销毁时移除所有事件和通知。  
**方法:**
- `private void OnDestroy()`: 触发移除逻辑。  
**示例:**
```csharp
public class MyRmvOnDestroyTrigger : RmvOnDestroyTrigger
{
    // 销毁逻辑
}

// 使用示例
var trigger = gameObject.AddComponent<MyRmvOnDestroyTrigger>();
trigger.Add(new CustomRmv(() => { /* 移除逻辑 */ }));
// 当对象销毁时，自动调用 trigger.OnDestroy();
```

### 类 RmvOnDisableTrigger

**用途:** 在对象失活时移除所有事件和通知。  
**方法:**
- `private void OnDisable()`: 触发移除逻辑。  
**示例:**
```csharp
public class MyRmvOnDisableTrigger : RmvOnDisableTrigger
{
    // 失活逻辑
}

// 使用示例
var trigger = gameObject.AddComponent<MyRmvOnDisableTrigger>();
trigger.Add(new CustomRmv(() => { /* 移除逻辑 */ }));
// 当对象失活时，自动调用 trigger.OnDisable();
```

### 类 MonoKit

**用途:** 提供Unity生命周期事件。  
**事件:**
- `public static event Action OnUpdate;`
- `public static event Action OnFixedUpdate;`
- `public static event Action OnLateUpdate;`
- `public static event Action OnGuiUpdate;`  
**方法:**
- `private void Awake()`: 初始化逻辑。
- `private void Update()`: 更新逻辑。
- `private void FixedUpdate()`: 固定更新逻辑。
- `private void LateUpdate()`: 延迟更新逻辑。
- `private void OnGUI()`: GUI更新逻辑。  
**示例:**
```csharp
public class MyMonoKit : MonoKit
{
    // MonoKit逻辑
}

// 使用示例
MonoKit.OnUpdate += () => { /* 更新逻辑 */ };
MonoKit.OnFixedUpdate += () => { /* 固定更新逻辑 */ };
MonoKit.OnLateUpdate += () => { /* 延迟更新逻辑 */ };
MonoKit.OnGuiUpdate += () => { /* GUI更新逻辑 */ };
```

---

## 数据绑定

### 抽象类 PnBinder<V>

**用途:** 数据绑定基类。  
**属性和方法:**
- `protected Action<V> mCallBack`: 绑定的回调函数。
- `protected V mValue`: 绑定的值。
- `public static implicit operator V(PnBinder<V> binder)`: 隐式转换为绑定的值。
- `public IRmv RegisterWithInitValue(Action<V> onValueChanged)`: 注册回调并立即调用。
- `public IRmv Register(Action<V> onValueChanged)`: 注册回调。
- `public void Unregister(Action<V> onValueChanged)`: 注销回调。
- `public void SetOnly(V value)`: 仅设置值不触发回调。  
**示例:**
```csharp
var binder = new PnBinder<int>();
binder.Register(value => { /* 值变化处理逻辑 */ });
binder.SetOnly(42);
int value = binder; // 隐式转换
```

### 类 ValueBinder<V>

**用途:** 值类型的绑定类。  
**继承:** 继承自 PnBinder<V>  
**方法:**
- `public V Value{get;set;}`: 设置值并触发回调。
- `public static implicit operator ValueBinder<V>(V value)`: 隐式转换为绑定类实例。
- `public static bool operator ==(ValueBinder<V> binder, V value)`: 判断绑定的值是否相等。
- `public static bool operator !=(ValueBinder<V> binder, V value)`: 判断绑定的值是否不等。  
**示例:**
```csharp
var valueBinder = 42; // 隐式转换
valueBinder.Register(value => { /* 值变化处理逻辑 */ });
valueBinder.Value = 421;
int value = valueBinder; // 隐式转换
```

### 类 StringBinder

**用途:** 字符串类型的绑定类。  
**继承:** 继承自 PnBinder<string>  
**方法:**
- `public string Value{get;set;}`: 设置值并触发回调。
- `public static implicit operator StringBinder(string value)`: 隐式转换为绑定类实例。
- `public static bool operator ==(StringBinder binder, string value)`: 判断绑定的值是否相等。
- `public static bool operator !=(StringBinder binder, string value)`: 判断绑定的值是否不等。  
**示例:**
```csharp
var stringBinder = "Hello"; // 隐式转换
stringBinder.Register(value => { /* 值变化处理逻辑 */ });
stringBinder.Value = "Hello？";
string value = stringBinder; // 隐式转换
```

### 类 ObjectBinder<O>

**用途:** 引用类型的绑定类。  
**继承:** 继承自 PnBinder<O>  
**方法:**

- `public void Modify<D>(D newValue, string fieldOrPropName)`: 修改对象的字段或属性。
- `public void Modify<D>(D newValue, Func<O, D> oldValue, Action<O, D> modifyAction)`: 修改对象的属性或字段。
- `public static implicit operator ObjectBinder<O>(O value)`: 隐式转换为绑定类实例。  
**示例:**
```csharp
public class MyObject
{
    public int Value;
}

var obj = new MyObject { Value = 10 };
var objectBinder = new ObjectBinder<MyObject>(obj);
objectBinder.Register(value => { /* 对象变化处理逻辑 */ });
objectBinder.Modify(20, "Value");
objectBinder.Modify(30, o => o.Value, (o, newValue) => o.Value = newValue);
MyObject value = objectBinder; // 隐式转换
```

---

## 容器类

### 类 PArray<T>

**用途:** 可自动扩容的动态数组，需要手动释放未使用的结构。  
**属性和方法:**
- `public int Count { get; }`: 获取元素数量。
- `public int Capacity { get; }`: 获取数组容量。
- `public bool IsEmpty { get; }`: 判断数组是否为空。
- `public T First { get; }`: 获取第一个元素。
- `public T Last { get; }`: 获取最后一个元素。
- `public T this[int index] { get; set; }`: 索引器，获取或设置指定位置的元素。
- `public PArray(int capacity = 4, bool isFill = false)`: 构造函数，初始化数组容量。
- `public PArray(IEnumerable<T> items)`: 构造函数，从集合初始化数组。
- `public PArray<T> Clone()`: 克隆数组。
- `public void Push(T e)`: 添加元素到数组末尾。
- `public void AddLast(T e)`: 添加元素到数组末尾。
- `public void RmvLast()`: 移除最后一个元素。
- `public T Pop()`: 移除并返回最后一个元素。
- `public void RmvAt(int index)`: 交换式移除指定位置的元素。
- `public void Sort(IComparer<T> comparer)`: 对数组进行排序。
- `public bool Find(Predicate<T> match, out T r)`: 查找符合条件的元素。
- `public bool Contains(T e)`: 判断数组是否包含指定元素。
- `public int IndexOf(T e)`: 获取指定元素的索引。
- `public void Shrinkage()`: 缩减数组容量。
- `public void Clear()`: 清空数组元素。
- `private void Resize(int newSize)`: 调整数组大小。
- `public void ResizeToN()`: 将容量重置为当前元素数量。
- `public void ResizeToDefault()`: 将容量重置为默认值。
- `public void ResetToNoCopy(int count)`: 重置容量但不复制数据。
- `public void ToFirst()`: 重置游标。
- `public void ToLast()`: 将游标移动到数组末端。
- `public IEnumerator<T> GetEnumerator()`: 获取迭代器。
- `IEnumerator IEnumerable.GetEnumerator()`: 获取迭代器。  
**示例:**
```csharp
var pArray = new PArray<int>(10);
pArray.Push(1);
pArray.Push(2);
pArray.Push(3);

Debug.Log(pArray.First);  // 输出: 1
Debug.Log(pArray.Last);   // 输出: 3

pArray.Sort(Comparer<int>.Default);
foreach (var item in pArray)
{
    Debug.Log(item);
}

if (pArray.Contains(2))
{
    Debug.Log("包含元素2");
}

int index = pArray.IndexOf(3);
Debug.Log($"元素3的索引: {index}");

pArray.RmvAt(1);
Debug.Log(pArray.Count);  // 输出: 2

pArray.Clear();
Debug.Log(pArray.IsEmpty);  // 输出: true
```

---

## UI 基类

### 类 UIPanel

**用途:** UI基类，封装找组件功能以及注册委托简化使用，提供显示或隐藏的行为。  
**属性和方法:**
- `public enum Layer { Top, Mid, Bot, Sys }`: UI层级枚举。
- `public virtual void OnShow()`: 显示面板时的逻辑。
- `public virtual void OnHide()`: 隐藏面板时的逻辑。
- `public virtual void Activate(bool active)`: 显示或隐藏面板。
- `protected virtual void OnClick(string btnName)`: 按钮点击时的逻辑。
- `public virtual bool IsOpen { get; }`: 判断面板是否打开。
- `protected virtual void Awake()`: 初始化逻辑，注册所有子对象的按钮。  
**示例:**
```csharp
public class MyUIPanel : UIPanel
{
    protected override void OnClick(string btnName)
    {
        Debug.Log($"{btnName} clicked");
    }
}

// 使用示例
var panel = gameObject.AddComponent<MyUIPanel>();
panel.Activate(true);
```

---

## 完整示例

### 流程图

![](https://gitee.com/PantyNeko/MeowFramework/raw/main/Assets/Doc/img3.png)

### 示例代码

```csharp
using UnityEngine;

namespace Panty.Test
{
    // 定义模块中心 CalcHub，负责注册模块 ICalcModel 和 IOpSystem
    public class CalcHub : ModuleHub<CalcHub>
    {
        // 构建模块，在这里注册所有需要的模块
        protected override void BuildModule()
        {
            // 注册计算模型模块
            AddModule<ICalcModel>(new CalcModel());
            // 注册操作符系统模块
            AddModule<IOpSystem>(new OpSystem());
        }
    }

    // 定义 CalcGame 类，作为权限提供者，允许访问 CalcHub 中的模块
    public class CalcGame : MonoBehaviour, IPermissionProvider
    {
        // 实现 IPermissionProvider 接口，返回模块中心实例
        IModuleHub IPermissionProvider.Hub => CalcHub.GetIns();
    }

    // 定义 CalcUI 类，继承自 UIPanel 并实现权限提供者接口
    public class CalcUI : UIPanel, IPermissionProvider
    {
        // 实现 IPermissionProvider 接口，返回模块中心实例
        IModuleHub IPermissionProvider.Hub => CalcHub.GetIns();
    }
}

namespace Panty.Test
{
    // 定义 CalcResultQuery 结构体，实现查询接口，返回计算结果
    public struct CalcResultQuery : IQuery<float>
    {
        // 实现 Do 方法，执行查询操作，返回计算结果
        public float Do(IModuleHub hub)
        {
            // 获取计算模型模块
            var model = hub.Module<ICalcModel>();
            // 获取当前操作符
            string op = hub.Module<IOpSystem>().Op;
            // 获取两个操作数
            int a = model.NumA;
            int b = model.NumB;
            // 根据操作符执行相应的计算
            return op switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => (float)a * b,
                "/" => (float)a / b,
                _ => int.MaxValue,
            };
        }
    }

    // 定义 NextOpIndexCmd 结构体，实现命令接口，用于切换操作符
    public struct NextOpIndexCmd : ICmd
    {
        // 实现 Do 方法，执行命令操作，切换操作符索引并发送计算命令
        public void Do(IModuleHub hub)
        {
            // 获取操作符系统模块
            hub.Module<IOpSystem>().NextOpIndex();
            // 发送计算命令
            hub.SendCmd<CalcCmd>();
        }
    }

    // 定义 RandomCalcCmd 结构体，实现命令接口，用于生成随机数并发送计算命令
    public struct RandomCalcCmd : ICmd
    {
        // 实现 Do 方法，执行命令操作，生成随机数并发送计算命令
        public void Do(IModuleHub hub)
        {
            // 获取计算模型模块
            var model = hub.Module<ICalcModel>();
            // 生成随机数并赋值给操作数A和B
            model.NumA.Value = Random.Range(1, 100);
            model.NumB.Value = Random.Range(1, 100);
            // 发送计算命令
            hub.SendCmd<CalcCmd>();
        }
    }

    // 定义 CalcCmd 结构体，实现命令接口，用于执行计算并发送事件
    public struct CalcCmd : ICmd
    {
        // 实现 Do 方法，执行计算命令，查询计算结果并发送计算事件
        public void Do(IModuleHub hub)
        {
            // 查询计算结果
            var result = hub.Query<CalcResultQuery, float>();
            // 发送计算结果事件
            hub.SendEvent(new CalcEvent() { result = result });
        }
    }

    // 定义 OpChangeEvent 结构体，用于表示操作符变化事件
    public struct OpChangeEvent
    {
        // 表示当前操作符
        public string op;
    }

    // 定义 CalcEvent 结构体，用于表示计算结果事件
    public struct CalcEvent
    {
        // 表示计算结果
        public float result;
    }

    // 定义 IOpSystem 接口，用于表示操作符系统
    public interface IOpSystem : IModule
    {
        // 获取当前操作符
        string Op { get; }
        // 切换到下一个操作符
        void NextOpIndex();
    }

    // 定义 OpSystem 类，实现操作符系统
    public class OpSystem : AbsModule, IOpSystem
    {
        // 操作符索引
        private int opIndex;
        // 操作符数组
        private string[] ops;
        // 获取当前操作符
        public string Op => ops[opIndex];

        // 实现模块初始化方法，初始化操作符数组和索引
        protected override void OnInit()
        {
            ops = new string[4] { "+", "-", "*", "/" };
            opIndex = 0;
        }

        // 切换到下一个操作符
        public void NextOpIndex()
        {
            opIndex = (opIndex + 1) % ops.Length;
            // 发送操作符变化事件
            this.SendEvent(new OpChangeEvent() { op = ops[opIndex] });
        }
    }

    // 定义 ICalcModel 接口，用于表示计算模型
    public interface ICalcModel : IModule
    {
        // 操作数A
        ValueBinder<int> NumA { get; }
        // 操作数B
        ValueBinder<int> NumB { get; }
    }

    // 定义 CalcModel 类，实现计算模型
    public class CalcModel : AbsModule, ICalcModel
    {
        // 初始化操作数A和B的绑定器
        public ValueBinder<int> NumA { get; } = new ValueBinder<int>(1);
        public ValueBinder<int> NumB { get; } = new ValueBinder<int>(2);

        // 实现模块初始化方法
        protected override void OnInit() { }
    }
}

namespace Panty.Test
{
    // 定义 CalcPanel 类，继承自 CalcUI，负责管理 UI 逻辑
    public class CalcPanel : CalcUI
    {
        // 查找并绑定 UI 组件
        [FindComponent("Op")] private Text mOPText;
        [FindComponent("Result")] private Text mResultText;
        [FindComponent("InputA")] private Text mInputA;
        [FindComponent("InputB")] private Text mInputB;
        // 存储计算模型模块实例
        private ICalcModel mModel;

        // 初始化方法，在 Start 中注册操作数和事件的回调
        private void Start()
        {
            // 获取计算模型模块
            mModel = this.Module<ICalcModel>();

            // 注册操作数A和B的值变化回调，并在销毁时移除
            mModel.NumA.RegisterWithInitValue(v => mInputA.text = v.ToString()).RmvOnDestroy(this);
            mModel.NumB.RegisterWithInitValue(v => mInputB.text = v.ToString()).RmvOnDestroy(this);
            // 注册计算结果事件的回调，并在销毁时移除
            this.AddEvent<CalcEvent>(e => mResultText.text = e.result.ToString()).RmvOnDestroy(this);
            // 注册操作符变化事件的回调，并在销毁时移除
            this.AddEvent<OpChangeEvent>(e => mOPText.text = e.op).RmvOnDestroy(this);
        }

        // 处理按钮点击事件
        protected override void OnClick(string btnName)
        {
            // 根据按钮名称执行不同的命令
            switch (btnName)
            {
                case "Op":
                    // 切换操作符
                    this.SendCmd<NextOpIndexCmd>();
                    break;
                case "Eq":
                    // 执行计算
                    this.SendCmd<CalcCmd>();
                    break;
                case "Add_NumA":
                    // 增加操作数A
                    mModel.NumA.Value++;
                    break;
                case "Add_NumB":
                    // 增加操作数B
                    mModel.NumB.Value++;
                    break;
                case "Sub_NumA":
                    // 减少操作数A
                    mModel.NumA.Value--;
                    break;
                case "Sub_NumB":
                    // 减少操作数B
                    mModel.NumB.Value--;
                    break;
                case "Random":
                    // 生成随机数并执行计算
                    this.SendCmd<RandomCalcCmd>();
                    break;
            }
        }
    }
}
```