# Panty 工具集使用手册

## 目录
1. [TextDialog 使用指南](#textdialog-使用指南)
2. [QuickCmdEditor 使用指南](#quickcmdeditor-使用指南)
   - [基础操作](#基础操作)
   - [命令说明](#命令说明)
   - [拖曳操作与上下文菜单](#拖曳操作与上下文菜单)

---

## TextDialog 使用指南

### 打开提示框
1. 调用 `TextDialog.Open` 方法，传入需要显示的消息和可选的回调函数：
   ```csharp
   TextDialog.Open("你的消息", 确认回调, 取消回调);
   ```
2. 在提示框中，用户可以通过点击确认或取消按钮进行相应的操作。

---

## QuickCmdEditor 使用指南

### 基础操作

#### 快速打开
1. 在 Unity 编辑器顶部菜单栏选择 `PnTool/QuickCmd &Q` 打开 `QuickCmdEditor` 窗口。
   - **快捷键**: `Alt + Q`

#### 添加绑定
1. 在场景中选择一个或多个 GameObject。
2. 在 Unity 编辑器顶部菜单栏选择 `PnTool/Quick/AddBind &B` 为所选对象添加绑定组件。
   - **快捷键**: `Alt + B`

#### 添加UI根节点
1. 在场景中选择一个或多个 GameObject。
2. 在 Unity 编辑器顶部菜单栏选择 `PnTool/Quick/AddUIRoot &W` 将所选对象设置为 UI 根节点。
   - **快捷键**: `Alt + W`

#### 检查更新
1. 打开 `QuickCmdEditor` 窗口。
2. 在命令输入框中输入 `check` 或 `检查更新`，然后按回车键。
3. 程序将会检查更新并显示相应信息。
   - **上下文菜单**: 右键点击命令输入框选择 `检查更新`。

### 命令说明

#### 基础命令
- **帮助**: 输入 `help` 或 `帮助` 查看帮助信息。
  - **上下文菜单**: 右键点击命令输入框选择 `显示帮助`。
- **清理**: 输入 `clear` 或 `清理` 清空已标记的信息。
  - **上下文菜单**: 右键点击命令输入框选择 `清空数据`。
- **UI绑定**: 输入 `uiBind` 或 `UI绑定` 进行 UI 绑定操作。
  - **上下文菜单**: 右键点击命令输入框选择 `绑定 UI`。
- **检查更新**: 输入 `check` 或 `检查更新` 检查更新。
  - **上下文菜单**: 右键点击命令输入框选择 `检查更新`。

#### 模块和脚本创建命令
- **命名空间**: 输入 `space:命名空间` 设置命名空间，例如 `space:MyNamespace`。
- **路径**: 输入 `path:路径` 设置路径，例如 `path:Assets/MyPath`。输入 `path:base` 或 `path:基础` 创建基础目录。
- **SoIns**: 创建 `ScriptableObject` 实例。例如 `SoIns:MyScriptableObject`。
- **Module**: 创建模块。例如 `Module:MyModule`。
- **System**: 创建系统。例如 `System:MySystem`。
- **Model**: 创建数据模型。例如 `Model:MyModel`。
- **Game**: 创建游戏脚本。例如 `Game:MyGameScript`。
- **UI**: 创建表现层脚本。例如 `UI:MyUIScript`。
- **Mono**: 创建 `MonoBehaviour` 脚本。例如 `Mono:MyMonoScript`。
- **so**: 创建 `ScriptableObject` 脚本。例如 `so:MyScriptableObject`。
- **hub**: 创建架构。例如 `hub:MyHub`。

### 拖曳操作与上下文菜单

#### 拖曳操作
1. 将一个 MonoScript 脚本文件拖到 `QuickCmdEditor` 窗口中，并按住 `Ctrl` 键以标记路径。
2. 将一个或多个 GameObject 拖到 `QuickCmdEditor` 窗口中，以缓存这些对象供后续操作。

#### 上下文菜单
1. 右键点击 `QuickCmdEditor` 命令输入框可弹出上下文菜单，选择以下操作：
   - **显示帮助**: 查看帮助信息。
   - **绑定 UI**: 进行 UI 绑定操作。
   - **基础目录**: 设置基础目录。
   - **检查更新**: 检查更新。
   - **清空数据**: 清空已标记的信息。

