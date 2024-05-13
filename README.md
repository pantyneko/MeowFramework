# MeowFramework 用户手册（详细版）

## 概述

MeowFramework是一套基于QF架构改良的高性能框架，适合追求自定义和轻量级设计的开发者。本框架支持模块化和灵活的系统设计，实现了单例模式、命令/查询处理模式和事件处理。

## 环境要求

- C# 8.0
- 建议使用Unity 2021

## 主要组件介绍

### IModuleHub

`IModuleHub`是框架的核心，负责管理模块和实用程序。主要方法如下：

#### Module<M>()

获取一个已注册的模块实例。

**示例：**

```
csharp
Copy code
var myModule = hub.Module<MyModule>();
```

#### Utility<U>()

获取一个已注册的工具实例。

**示例：**

```
csharp
Copy code
var myUtility = hub.Utility<MyUtility>();
```

#### AddEvent<E>(Action<E> call)

注册一个事件监听器。

**示例：**

```
csharp
Copy code
hub.AddEvent<MyEvent>(e => Console.WriteLine("Event triggered: " + e.Message));
```

#### RmvEvent<E>(Action<E> call)

移除一个事件监听器。

**示例：**

```
csharp
Copy code
hub.RmvEvent<MyEvent>(eventHandler);
```

#### SendEvent<E>(E e)

触发一个事件。

**示例：**

```
csharp
Copy code
hub.SendEvent(new MyEvent { Message = "Hello World" });
```

### IModule 接口

定义了模块的基本生命周期方法。

#### TryInit()

尝试初始化模块，如果已经初始化，则不重复进行。

**示例：**

```
csharpCopy codepublic void TryInit()
{
    if (!Inited)
    {
        OnInit();
        Inited = true;
    }
}
```

### IUtility 接口

一个标记接口，用于标识实用工具类。

### AbsModule 类

所有模块的基类，提供了生命周期管理功能。

#### OnInit()

模块初始化时调用的方法，需在子类中具体实现。

#### OnDeInit()

模块去初始化时调用的方法，可在子类中重写。

**示例：**

```
csharpCopy codeprotected override void OnInit()
{
    // 初始化代码
}

protected override void OnDeInit()
{
    // 清理代码
}
```

### ICmd 接口

定义了命令的执行方法。

#### Do(IModuleHub hub)

在给定的模块环境中执行命令。

**示例：**

```
csharpCopy codepublic class MyCommand : ICmd
{
    public void Do(IModuleHub hub)
    {
        // 命令执行逻辑
    }
}
```

### IQuery<R> 接口

定义了查询的执行方法，返回结果类型为R。

#### Do(IModuleHub hub)

执行查询并返回结果。

**示例：**

```
csharpCopy codepublic class MyQuery : IQuery<int>
{
    public int Do(IModuleHub hub)
    {
        // 查询逻辑
        return 42;
    }
}
```

## 版本更新记录

- **1.0.4 (2024-05-13)**: 修复语法错误，增加对Deinit的状态变更，避免重复调用。
- **1.0.3 (2024-05-12)**: 移除反射机制，增加延迟初始化。
- **1.0.2 (2024-05-11)**: 调整架构生命周期，避免重复初始化。
- **1.0.1 (2024-05-09)**: 区分有参数和无参数事件API。

## 联系方式

如需帮助或有任何疑问，请通过以下方式联系：

- [GitHub](https://github.com/pantyneko)
- [Gitee](https://gitee.com/PantyNeko)
- [B站](https://space.bilibili.com/656352)
