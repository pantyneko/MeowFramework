# MeowFramework 用户手册

## 框架概述

MeowFramework是一套基于QF架构改良的高性能框架，适合追求自定义和轻量级设计的开发者。架构支持模块化和灵活的系统设计，实现了命令/查询处理模式和事件处理模式。

(注：该架构会自动提示更新 如运行后提示更新 请及时前往Github 获取最新版本)

https://github.com/pantyneko/MeowFramework

https://gitee.com/PantyNeko/MeowFramework

## 环境要求

建议使用 C# 8.0 - Unity 2021及以上版本

## 核心组件

### IModule  接口

- 架构模块接口 通常用来统一<**模块层**>的基接口

- ```c#
  public interface IBaseModule : IModule
  {
  	// 这里定义模块接口方法
  }
  public class BaseModule : IBaseModule
  {
      public void TryInit()
      {
          // 尝试初始化模块，如果已经初始化，则不重复进行
      }
  }
  ```

### AbsModule 抽象类

- 架构抽象模块基类 用于拓展模块的初始化规则 提供了初始化和逆初始化的状态变更

- ```c#
  public class MyModule : AbsModule, IMyModule
  {
      protected override void OnInit()
      {
          // 初始化阶段
      }
      protected override void OnDeInit()
      {
          // 销毁阶段
      }
      void IMyModule.Say() => "Say".Log();
      void IMyModule.Say(string msg) => msg.Log();
      string IMyModule.Get(int id) => id.ToString();
      string IMyModule.Msg => "说话呀！";
  }
  ```

### IUtility 接口

- 架构工具接口 通常用来统一<**工具层**>的基接口

- ```c#
  public interface IMyUtility : IUtility
  {
      void Use();
  }
  public class MyUtility : IMyUtility
  {
      void IMyUtility.Use() => "Use".Log();
  }
  ```

### ICanInit 接口

- 架构可初始化接口 通常用来统一初始化动作 

- ```c#
  // 这里将接口作为模块的子类 外部调用涉及IModule接口时 初始化方法不至于被错误的调用
  public interface ICanInit : IModule
  {
      void SetHub(IModuleHub hub);
      void Deinit();
  }
  ```

### IPermissionProvider 接口

- 架构权限接口 通常用来给表现层对象或特殊对象提供架构的访问权限

- ```c#
  public class FK_Example : MonoBehaviour, IPermissionProvider
  {
      IModuleHub IPermissionProvider.Hub => ExampleHub.GetIns();
  }
  ```

### ICmd  接口

- 架构命令接口 通常用来统一所有<**无**>参数命令的执行

- ```c#
  public struct ExampleCmd : ICmd
  {
      public void Do(IModuleHub hub)
      {
          // 执行命令逻辑 hub 可以在命令中调用架构权限 例如
          hub.Module<IMyModule>().Say();
      }
  }
  ```

### ICmd< P>  接口

- 架构命令接口 通常用来统一所有<**有**>参数命令的执行

- ```c#
  public struct ExampleDataCmd : ICmd<string>
  {
      public void Do(IModuleHub hub, string msg)
      {
          // 执行命令逻辑 hub 可以在命令中调用架构权限 例如
          hub.Module<IMyModule>().Say(msg);
      }
  }
  ```

### IQuery< R >  接口

- 架构查询接口 通常用来统一所有<**无**>参数查询的执行

- ```c#
  public struct ExampleQuery : IQuery<string>
  {
      public string Do(IModuleHub hub)
      {
          return hub.Module<IMyModule>().Msg;
      }
  }
  ```

### IQuery<P, R>  接口

- 架构查询接口 通常用来统一所有<**有**>参数查询的执行

- ```c#
  public struct ExampleDataQuery : IQuery<int, string>
  {
      public string Do(IModuleHub hub, int num)
      {
          return hub.Module<IMyModule>().Get(10);
      }
  }
  ```

### ModuleHub 抽象类
- 框架的核心基类，负责管理框架中的所有模块和工具以及提供所有内嵌功能函数的具体实现

#### GetIns()

- 获取当前架构的单例对象实例 <懒汉式>。


```c#
var ins = ExampleHub.GetIns()
```

#### DEBUG 宏

- 定义某些操作仅用于调试

#### CheckUpdate()

- 用于检查更新的函数 <架构会主动调用 不要去动它>

#### BuildModule()

- 用于让子类重写注册模块的顺序和逻辑 <架构会主动调用 不要去动它>

### IModuleHub 接口

- 框架的核心接口，负责提供架构中所有的对外功能函数。主要方法如下：

#### 模块

##### Module< M>() 

- 获取一个已注册的模块实例 通常会在命令或查询等特殊情况使用。


```c#
var myModule = hub.Module<MyModule>();
```

##### Utility< U>() 

获取一个已注册的工具实例 通常会在命令或查询等特殊情况使用。

```c#
var myUtility = hub.Utility<MyUtility>();
```

#### 事件

##### => Define Event 

```c#
public struct MyEvent
{
    public string Message;
}
```

##### AddEvent< E>(Action< E> call)

注册一个携带自身类型的事件监听器 注意不要注册到一些不可控的对象中。

```c#
hub.AddEvent<MyEvent>(eventHandler);
```

##### RmvEvent< E>(Action< E> call)

移除一个携带自身类型的事件监听器 与注册同理 。

```c#
hub.RmvEvent<MyEvent>(eventHandler);
```

##### SendEvent< E>(E e)

触发一个携带自身类型的事件 <外部赋值> 通常会在命令或查询等特殊情况使用。

```c#
hub.SendEvent(new MyEvent { Message = "Hello World" });
```

##### SendEvent< E>()

触发一个携带自身类型的事件 <内部赋值> 通常会在命令或查询等特殊情况使用。

```c#
hub.SendEvent<MyEvent>();
```

#### 通知

##### => Define Notify

```c#
public struct MyNotify{}
```

##### AddNotify< N>(Action call)

注册一个仅通知的事件监听器 注意不要注册到一些不可控的对象中。

```c#
hub.AddNotify<MyNotify>(notifyHandler);
```

##### RmvNotify< N>(Action call)

移除一个仅通知的事件监听器 与注册同理。

```c#
hub.RmvNotify<MyNotify>(notifyHandler);
```

##### SendNotify< N>()

触发一个仅通知的事件 通常会在命令或查询等特殊情况使用。

```c#
hub.SendNotify<MyNotify>();
```

#### 命令

##### SendCmd< C>(C cmd)

发送一条无参数命令 <外部赋值> 通常会在命令或查询等特殊情况使用。

```c#
public struct ExampleCmd : ICmd
{
    public void Do(IModuleHub hub)
    {
        // 执行命令逻辑 hub 可以在命令中调用架构权限 例如
        hub.Module<IMyModule>().Say();
    }
}
hub.SendCmd(new ExampleCmd());
```

##### SendCmd<C, P>(C cmd, P info)

发送一条有参数命令 <外部赋值> 通常会在命令或查询等特殊情况使用。

```c#
public struct ExampleDataCmd : ICmd<string>
{
    public void Do(IModuleHub hub, string msg)
    {
        // 执行命令逻辑 hub 可以在命令中调用架构权限 例如
        hub.Module<IMyModule>().Say(msg);
    }
}
hub.SendCmd(new ExampleDataCmd(), "消息");
```

##### SendCmd< C>()

发送一条无参数命令 <内部赋值> 通常会在命令或查询等特殊情况使用。

```c#
public struct ExampleCmd : ICmd
{
    public void Do(IModuleHub hub)
    {
        // 执行命令逻辑 hub 可以在命令中调用架构权限 例如
        hub.Module<IMyModule>().Say();
    }
}
hub.SendCmd<ExampleCmd>();
```

##### SendCmd<C, P>(P info)

发送一条有参数命令 <内部赋值> 通常会在命令或查询等特殊情况使用。

```c#
public struct ExampleDataCmd : ICmd<string>
{
    public void Do(IModuleHub hub, string msg)
    {
        // 执行命令逻辑 hub 可以在命令中调用架构权限 例如
        hub.Module<IMyModule>().Say(msg);
    }
}
hub.SendCmd<ExampleDataCmd, string>("消息");
```

#### 查询

##### Query<Q, R>()

发送一条无参数的查询 通常会在命令或查询等特殊情况使用

```c#
public struct ExampleQuery : IQuery<string>
{
    public string Do(IModuleHub hub)
    {
        return hub.Module<IMyModule>().Msg;
    }
}
var str = hub.Query<ExampleQuery, string>();
```

##### Query<Q, P, R>(P info)

发送一条有参数的查询 通常会在命令或查询等特殊情况使用

```c#
public struct ExampleDataQuery : IQuery<int, string>
{
    public string Do(IModuleHub hub, int num)
    {
        return hub.Module<IMyModule>().Get(num);
    }
}
var str = hub.Query<ExampleDataQuery, int, string>(10);
```

##### Query< Q>()

发送一条无参数且返回自身的查询 通常会在命令或查询等特殊情况使用

```c#
public struct ExampleSelfQuery : IQuery<ExampleSelfQuery>
{
    public ExampleSelfQuery Do(IModuleHub hub)
    {
        return this;
    }
}
var q = hub.Query<ExampleSelfQuery>();
```

##### Query<Q, P>(P info)

发送一条有参数且返回自身的查询 通常会在命令或查询等特殊情况使用

```c#
public struct ExampleSelfDataQuery : IQuery<int, ExampleSelfDataQuery>
{
    public ExampleSelfDataQuery Do(IModuleHub hub, int num)
    {
        return this;
    }
}
var q = hub.Query<ExampleSelfDataQuery, int>(10);
```

### ModuleHubEx 静态类

- 架构权限的静态扩展类 用于给 IPermissionProvider 接口 提供静态扩展方法 主要功能如下：

#### 模块

##### GetModule< M>() 

- 获取一个已注册的模块实例 。

```c#
var myModule = this.GetModule<MyModule>();
```

##### Model< D>() 

- 获取一个已注册的模块实例 是 GetModule 的别名 用于在视觉上将模块定义为<**数据**>。

```c#
var myModule = this.Model<MyModule>();
```

##### System< S>() 

- 获取一个已注册的模块实例 是 GetModule 的别名 用于在视觉上将模块定义为<**系统**>。

```c#
var myModule = this.System<MyModule>();
```

##### Utility< U>() 

获取一个已注册的工具实例 。

```c#
var myUtility = this.Utility<MyUtility>();
```

#### 事件

##### => Define Event

```c#
public struct MyEvent
{
    public string Message;
}
```

##### AddEvent< E>(Action< E> call)

注册一个携带自身类型的事件监听器 。

```c#
this.AddEvent<MyEvent>(eventHandler);
```

##### RmvEvent< E>(Action< E> call)

移除一个携带自身类型的事件监听器 记得注销 。

```c#
this.RmvEvent<MyEvent>(eventHandler);
```

##### SendEvent< E>(E e)

触发一个携带自身类型的事件 <外部赋值> 。

```c#
this.SendEvent(new MyEvent { Message = "Hello World" });
```

##### SendEvent< E>()

触发一个携带自身类型的事件 <内部赋值> 。

```c#
this.SendEvent<MyEvent>();
```

#### 通知

##### => Define Notify

```c#
public struct MyNotify{}
```

##### AddNotify< N>(Action call)

注册一个仅通知的事件监听器 。

```c#
this.AddNotify<MyNotify>(notifyHandler);
```

##### RmvNotify< N>(Action call)

移除一个仅通知的事件监听器 。

```c#
this.RmvNotify<MyNotify>(notifyHandler);
```

##### SendNotify< N>()

触发一个仅通知的事件 。

```c#
this.SendNotify<MyNotify>();
```

#### 命令

##### SendCmd< C>(C cmd)

发送一条无参数命令 <外部赋值> 。

```c#
public struct ExampleCmd : ICmd
{
    public void Do(IModuleHub hub)
    {
        // 执行命令逻辑 hub 可以在命令中调用架构权限 例如
        hub.Module<IMyModule>().Say();
    }
}
this.SendCmd(new ExampleCmd());
```

##### SendCmd<C, P>(C cmd, P info)

发送一条有参数命令 <外部赋值> 。

```c#
public struct ExampleDataCmd : ICmd<string>
{
    public void Do(IModuleHub hub, string msg)
    {
        // 执行命令逻辑 hub 可以在命令中调用架构权限 例如
        hub.Module<IMyModule>().Say(msg);
    }
}
this.SendCmd(new ExampleDataCmd(), "消息");
```

##### SendCmd< C>()

发送一条无参数命令 <内部赋值> 。

```c#
public struct ExampleCmd : ICmd
{
    public void Do(IModuleHub hub)
    {
        // 执行命令逻辑 hub 可以在命令中调用架构权限 例如
        hub.Module<IMyModule>().Say();
    }
}
this.SendCmd<ExampleCmd>();
```

##### SendCmd<C, P>(P info)

发送一条有参数命令 <内部赋值> 。

```c#
public struct ExampleDataCmd : ICmd<string>
{
    public void Do(IModuleHub hub, string msg)
    {
        // 执行命令逻辑 hub 可以在命令中调用架构权限 例如
        hub.Module<IMyModule>().Say(msg);
    }
}
this.SendCmd<ExampleDataCmd, string>("消息");
```

#### 查询

##### Query<Q, R>()

发送一条无参数的查询 

```c#
public struct ExampleQuery : IQuery<string>
{
    public string Do(IModuleHub hub)
    {
        return hub.Module<IMyModule>().Msg;
    }
}
var str = this.Query<ExampleQuery, string>();
```

##### Query<Q, P, R>(P info)

发送一条有参数的查询 

```c#
public struct ExampleDataQuery : IQuery<int, string>
{
    public string Do(IModuleHub hub, int num)
    {
        return hub.Module<IMyModule>().Get(num);
    }
}
var str = this.Query<ExampleDataQuery, int, string>(10);
```

##### Query< Q>()

发送一条无参数且返回自身的查询 

```c#
public struct ExampleSelfQuery : IQuery<ExampleSelfQuery>
{
    public ExampleSelfQuery Do(IModuleHub hub)
    {
        return this;
    }
}
var q = this.Query<ExampleSelfQuery>();
```

##### Query<Q, P>(P info)

发送一条有参数且返回自身的查询 

```c#
public struct ExampleSelfDataQuery : IQuery<int, ExampleSelfDataQuery>
{
    public ExampleSelfDataQuery Do(IModuleHub hub, int num)
    {
        return this;
    }
}
var q = this.Query<ExampleSelfDataQuery, int>(10);
```

## 快速搭建

可以使用架构中带的 MeowEditor 来快速生成模板代码 

首先从GitHub中拿到框架后 会拿到一个名字叫 MeowEditor 的编辑器 我们使用在顶部菜单 ”PnTool“中 点击打开 MeowEditor 会弹出一个编辑窗口

![](https://github.com/pantyneko/MeowFramework/blob/main/Assets/Doc/img1.png)

弹出编辑器窗口后 按照下图进行设置 文本框中设置的是当前项目的架构名 设置完成 点击创建Hub类 会自动创建一个脚本文件 如下

![](https://github.com/pantyneko/MeowFramework/blob/main/Assets/Doc/img2.png)

```c#
using UnityEngine;

namespace Panty.Test
{
    public class CounterHub : ModuleHub<CounterHub>
    {
        protected override void BuildModule()
        {
            // 这里记得注册模块进去
            AddModule<ICounterModel>(new CounterModel());

            MonoKit.GetIns().OnDeInit += Deinit;
        }
    }
    public class CounterGame : MonoBehaviour, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => CounterHub.GetIns();
    }
    public class CounterUI : UIPanel, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => CounterHub.GetIns();
    }
}
```

也可以通过手动定义来搭建架构环境 样板代码如下：

```c#
using UnityEngine;

namespace Panty.Test
{
    public class ExampleHub : ModuleHub<ExampleHub>
    {
        protected override void BuildModule()
        {
            // 推荐使用 MonoKit 的 OnDeInit事件 来进行销毁
            MonoKit.GetIns().OnDeInit += Deinit;
        }
    }
    public class ExampleGame : MonoBehaviour, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => ExampleHub.GetIns();
    }
    public class ExampleUI : UIPanel, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => ExampleHub.GetIns();
    }
}
```

## 演示项目

```c#
using System;
using UnityEngine;

namespace Panty.Test
{
    public interface ICounterModel : IModule
    {
        ValueBinder<int> A { get; }
        ValueBinder<int> B { get; }
        string GetOpIcon(int id);
        string[] GetItems();
    }
    public class CounterModel : AbsModule, ICounterModel
    {
        ValueBinder<int> ICounterModel.A { get; } = new ValueBinder<int>();
        ValueBinder<int> ICounterModel.B { get; } = new ValueBinder<int>();

        private string[] Items;

        protected override void OnInit()
        {
            "第一次调用 该模块 时 执行".Log();
            Items = new string[] { "+", "-", "*", "/" };
        }
        protected override void OnDeInit()
        {
            "当应用或编辑器退出 时 执行".Log();
        }
        string ICounterModel.GetOpIcon(int id)
        {
            return Items[id];
        }
        string[] ICounterModel.GetItems()
        {
            return Items;
        }
    }
    public struct ChangeOpCmd : ICmd<int>
    {
        public void Do(IModuleHub hub, int id)
        {
            var model = hub.Module<ICounterModel>();
            hub.SendEvent(new ChangeOpIconEvent() { icon = model.GetOpIcon(id) });
            hub.SendNotify<OperationSuccessfulNotify>();
        }
    }
    public struct RandomValueCmd : ICmd
    {
        public void Do(IModuleHub hub)
        {
            var model = hub.Module<ICounterModel>();
            model.A.Value = UnityEngine.Random.Range(1, 100);
            model.B.Value = UnityEngine.Random.Range(1, 100);
        }
    }
    public struct ResultQuery : IQuery<CounterApp.Op, int>
    {
        public int Do(IModuleHub hub, CounterApp.Op op)
        {
            var model = hub.Module<ICounterModel>();
            var a = model.A.Value;
            var b = model.B.Value;
            return op switch
            {
                CounterApp.Op.Add => a + b,
                CounterApp.Op.Sub => a - b,
                CounterApp.Op.Mul => a * b,
                CounterApp.Op.Div => a / b,
                _ => throw new Exception("未识别运算符"),
            };
        }
    }
    public struct ChangeOpIconEvent
    {
        public string icon;
    }
    public struct OperationSuccessfulNotify { }
    public struct OperationFailedNotify { }

    public class CounterApp : CounterGame
    {
        public enum Op : byte
        {
            Add, Sub, Mul, Div
        }
        private float startW, startH;
        private GUIStyle style, btnStyle, inputStyle;
        private string A, B, R;
        private string opText = "+";

        private int mSelect;
        private bool ShowList;

        private ICounterModel model;

        private void Start()
        {
            startW = Screen.width >> 1;
            startH = Screen.height >> 1;

            model = this.Model<ICounterModel>();
            model.A.RegisterWithInitValue(OnAChange);
            model.B.RegisterWithInitValue(OnBChange);

            this.AddEvent<ChangeOpIconEvent>(OnChangeOp);
            this.AddNotify<OperationSuccessfulNotify>(OnOperationSuccessful);
            this.AddNotify<OperationFailedNotify>(OnOperationFailed);
        }
        private void OnOperationSuccessful()
        {
            "操作成功".Log();
        }
        private void OnOperationFailed()
        {
            "操作失败".Log();
        }
        private void OnChangeOp(ChangeOpIconEvent e)
        {
            opText = e.icon;
        }
        private void OnDestroy()
        {
            var model = this.Model<ICounterModel>();
            model.A.UnRegister(OnAChange);
            model.B.UnRegister(OnBChange);

            this.RmvEvent<ChangeOpIconEvent>(OnChangeOp);
            this.RmvNotify<OperationSuccessfulNotify>(OnOperationSuccessful);
            this.RmvNotify<OperationFailedNotify>(OnOperationFailed);
        }
        private void OnAChange(int a)
        {
            A = a.ToString();
        }
        private void OnBChange(int b)
        {
            B = b.ToString();
        }
        private void OnGUI()
        {
            if (style == null)
            {
                style = GUI.skin.label;
                style.fontSize = 30;
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.white;

                btnStyle = GUI.skin.button;
                btnStyle.fontSize = style.fontSize;
                btnStyle.alignment = TextAnchor.MiddleCenter;

                inputStyle = GUI.skin.textField;
                inputStyle.fontSize = style.fontSize;
                inputStyle.alignment = TextAnchor.MiddleCenter;
            }
            float size = 50f;
            var startX = startW - size * 4f;
            var rect = new Rect(startX, startH - size, size * 5f, size);
            if (GUI.Button(rect, "RandomNum", btnStyle))
            {
                this.SendCmd<RandomValueCmd>();
            }
            rect = new Rect(startX, startH, size, size);
            string a = GUI.TextField(rect, A, inputStyle);
            if (a != A)
            {
                if (int.TryParse(a, out int r))
                {
                    A = a;
                    model.A.Value = r;
                }
                else
                {
                    this.SendNotify<OperationFailedNotify>();
                }
            }
            rect.x += size;
            GUI.Label(rect, opText, style);
            rect.x += size;
            string b = GUI.TextField(rect, B, inputStyle);
            if (b != B)
            {
                if (int.TryParse(b, out int r))
                {
                    B = b;
                    model.B.Value = r;
                }
                else
                {
                    this.SendNotify<OperationFailedNotify>();
                }
            }
            rect.x += size;
            GUI.Label(rect, "=", style);
            rect.x += size;
            GUI.enabled = false;
            GUI.TextArea(rect, R, inputStyle);
            GUI.enabled = true;
            rect.y += size;
            rect.width = size * 2f;
            rect.x = startW - size;
            if (GUI.Button(rect, "Calc", btnStyle))
            {
                R = this.Query<ResultQuery, Op, int>((Op)mSelect).ToString();
                this.SendNotify<OperationSuccessfulNotify>();
            }
            rect.x = startX;
            rect.width = size * 3f;
            if (GUI.Button(rect, "Operator", btnStyle))
            {
                ShowList = !ShowList;
            }
            if (ShowList)
            {
                rect.height = size * 3f;
                rect.y += size;
                var sel = GUI.SelectionGrid(rect, mSelect, Enum.GetNames(typeof(Op)), 1);
                if (mSelect == sel) return;
                mSelect = sel;
                this.SendCmd<ChangeOpCmd, int>(sel);
                ShowList = false;
            }
        }
    }
}
```



## 版本更新

- **1.0.5 (2024-05-13)**: 增加示例单元 增加文档。
- **1.0.4 (2024-05-13)**: 修复语法错误，增加对 Deinit 的状态变更，避免重复调用。
- **1.0.3 (2024-05-12)**: 移除反射机制，增加延迟初始化。
- **1.0.2 (2024-05-11)**: 调整架构生命周期，避免重复初始化。
- **1.0.1 (2024-05-09)**: 区分有参数和无参数事件API。
