using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text;
using System.Linq;
using UnityEditor.SceneManagement;
using System.Reflection;

namespace Panty
{
    public class TextDialog : EditorWindow
    {
        private Vector2 scrollPosition;
        private string longText = "";
        private Action succeed, fail;

        public static void Open(string msg, Action succeed = null, Action fail = null)
        {
            var wd = GetWindow<TextDialog>("喵喵提示器", true);
            wd.succeed = succeed;
            wd.fail = fail;
            wd.longText = msg;
        }
        private void Awake()
        {
            position.Set(position.x, position.y, 100f, 300f);
        }
        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("您的喵喵女友来电话啦！不接的话打洗你哦", MessageType.Info);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            var style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            GUILayout.Label(longText, style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            var e = Event.current;
            var op = new GUILayoutOption[] { GUILayout.Height(30f) };
            bool trigger = hasFocus && e.type == EventType.KeyDown;
            if (GUILayout.Button("确认", op))
            {
                succeed?.Invoke();
                Close();
            }
            else if (trigger && e.keyCode == KeyCode.Return)
            {
                e.Use();
                succeed?.Invoke();
                Close();
            }
            if (GUILayout.Button("取消", op))
            {
                fail?.Invoke();
                Close();
            }
            else if (trigger && e.keyCode == KeyCode.Escape)
            {
                fail?.Invoke();
                e.Use();
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
        private void OnDestroy()
        {
            succeed = null;
            fail = null;
        }
    }
    public class QuickCmdEditor : EditorWindow
    {
        private bool IsAsync;
        private string mCmd = "help";

        private TextField mField;
        private static bool PathMode;
        private static MonoScript SCRIPT;
        private static PArray<GameObject> mGos = new PArray<GameObject>();

        [MenuItem("PnTool/QuickCmd &Q")]
        private static void OpenSelf()
        {
            if (EditorKit.ShowOrHide<QuickCmdEditor>(out var win))
            {
                win.maxSize = new Vector2(960f, 40f);
                win.minSize = new Vector2(360f, 40f);
            }
        }
        [MenuItem("PnTool/Cmd/PathMode")]
        private static void ChangePathMode()
        {
            $"切换为 {((PathMode = !PathMode) ? "Hub" : "TPath")} 作为路径".Log();
        }
        [MenuItem("PnTool/Cmd/AddBind #B")]
        private static void AddBind()
        {
            if (Selection.objects.Length == 0)
            {
                "添加Bind失败 请在场景中选择需要添加Bind组件的对象".Log();
            }
            else
            {
                foreach (var go in Selection.objects.OfType<GameObject>())
                {
                    if (go == null) continue;
                    if (mGos.Count == 0)
                    {
                        "未设置Root对象 请在场景中选择对应的Root对象拖曳到QuickCmdEditor编辑器中 并再次绑定".Log();
                    }
                    else
                    {
                        if (go == mGos[0])
                            $"Bind组件无法添加到根节点：{go.name}".Log();
                        else
                        {
                            if (mGos.Count == 1) $"{go.name}已成功添加Bind组件".Log();
                            else "Bind无法绑定多个Root 将以第1个Root对象作为目标进行绑定".Log();
                            go.GetOrAddComponent<Bind>().Init(mGos[0]);
                        }
                    }
                    EditorUtility.SetDirty(go);
                    EditorSceneManager.MarkSceneDirty(go.scene);
                }
            }
        }
        [MenuItem("PnTool/Cmd/AddUIRoot #V")]
        private static void AddUIRoot()
        {
            if (Selection.objects.Length == 0)
            {
                "添加Root失败 请在场景中选择需要添加Root组件的对象".Log();
                return;
            }
            foreach (var go in Selection.objects.OfType<GameObject>())
            {
                if (go == null) continue;
                AddUIRoot(go);
            }
        }
        private static string GetPrefix(Bind.E_Type type)
        {
            return type switch
            {
                Bind.E_Type.Button => "btn",
                Bind.E_Type.Canvas => "cnv",
                Bind.E_Type.Image => "img",
                Bind.E_Type.Text => "txt",
                Bind.E_Type.Toggle => "tgl",
                Bind.E_Type.Slider => "sld",
                Bind.E_Type.Transform => "tr",
                Bind.E_Type.Dropdown => "drpDwn",
                Bind.E_Type.Scrollbar => "scrlBr",
                Bind.E_Type.ScrollRect => "scrlRt",
                Bind.E_Type.InputField => "inpFld",
                Bind.E_Type.RawImage => "rwImg",

                Bind.E_Type.SpriteRenderer => "Spr",

                Bind.E_Type.TextMeshPro => "tmpTxt",
                Bind.E_Type.TextMeshProUGUI => "tmpTxtUGUI",
                Bind.E_Type.TMP_InputField => "tmp_InpFld",
                Bind.E_Type.TMP_Dropdown => "tmp_DrpDwn",
                _ => ""
            };
        }
        private static string CreateRootMono(string fileName)
        {
            string tmple = $"using UnityEngine;\r\n\r\nnamespace {I.Space}\r\n{{\r\n    public partial class {fileName} : MonoBehaviour, IBindRoot\r\n    {{\r\n\r\n    }}\r\n}}";
            string path = I.TPath;
            if (GetPathByMode(ref path))
            {
                FileKit.TryCreateDirectory($"{path}/Binds");
                File.WriteAllText($"{path}/Binds/{fileName}.cs", tmple);
                FileKit.TryCreateDirectory($"{path}/Fields");
                return $"{path}/Fields/F{fileName}.cs";
            }
            return null;
        }
        private static void AddUIRoot(GameObject go)
        {
            var binds = go.GetComponentsInChildren<Bind>(true);
            if (binds.Length == 0)
            {
                "没有可绑定对象".Log();
            }
            else
            {
                string assetPath = null;
                var cp = go.GetComponent<IBindRoot>();
                string fileName = $"R_{go.name.CapitalizeFirstLetter()}";
                string space = I.Space;
                string full = $"{space}.{fileName}";
                var type = HubTool.BaseAss.GetType(full);

                if (cp == null)
                {
                    // 说明没有这个资源
                    if (type == null)
                    {
                        assetPath = CreateRootMono(fileName);
                        type = HubTool.BaseAss.GetType(full);
                        (type == null ? "主程序集不存在该类" : $"构建{type.Name}").Log();
                        "请在资源刷新完成后 再次触发以确保脚本的挂载".Log();
                    }
                    else
                    {
                        if (type.IsSubclassOf(typeof(Component)))
                        {
                            string path = Path.GetDirectoryName(EditorKit.GetMonoPath(fileName, $"{fileName}.cs"));
                            assetPath = $"{path.Substring(0, path.Length - 5)}Fields/F{fileName}.cs"; // Binds.Length = 5
                            SetRootData(type, binds, go);
                            $"{type.Name}脚本已存在 即将刷新数据类".Log();
                        }
                        else // 说明只有数据类
                        {
                            assetPath = CreateRootMono(fileName);
                            $"只找到数据类 重新构建{type.Name} 请在资源刷新完成后 再次触发以确保脚本的挂载".Log();
                        }
                    }
                }
                else
                {
                    type = cp.GetType();
                    string typeName = type.FullName;
                    SetRootData(type, binds, go);
                    $"{type.Name}脚本已挂载".Log();
                    var tp = typeName.Split('.');
                    fileName = tp[tp.Length - 1];
                    space = typeName.Substring(0, typeName.Length - fileName.Length - 1);
                    string path = Path.GetDirectoryName(EditorKit.GetMonoPath(fileName, $"{fileName}.cs"));
                    assetPath = $"{path.Substring(0, path.Length - 5)}Fields/F{fileName}.cs"; // Binds.Length = 5
                }
                string[] data = $"using UnityEngine;\r\nusing UnityEngine.UI;\r\n\r\nnamespace {space}\r\n{{\r\n    public partial class {fileName}\r\n    {{\r\n        @\r\n    }}\r\n}}".Split('@');
                var bd = new StringBuilder(data[0]);
                for (int i = 0, len = binds.Length; i < len; i++)
                {
                    var bind = binds[i];
                    if (bind.Root == null)
                        $"{bind}的Root is Null".Log();
                    else if (bind.Root == go)
                    {
                        if (i > 1) bd.Append("\t\t");
                        var str = bind.CType switch
                        {
                            Bind.E_Type.TextMeshPro => "TMPro.",
                            Bind.E_Type.TextMeshProUGUI => "TMPro.",
                            Bind.E_Type.TMP_Dropdown => "TMPro.",
                            Bind.E_Type.TMP_InputField => "TMPro.",
                            _ => ""
                        };
                        bd.Append($"[SerializeField] private {str}{bind.CType} {HandleBind(bind)};");
                        if (i < len - 1) bd.Append("\r\n");
                    }
                    else $"{bind.Root}不属于当前父级".Log();
                }
                bd.Append(data[1]);
                FileKit.WriteFile(assetPath, bd.ToString(), Encoding.UTF8);
                AssetDatabase.Refresh();
                $"{fileName}数据已更新".Log();
            }
        }
        private static void SetRootData(Type rootType, Bind[] binds, GameObject go)
        {
            var cmpnt = go.GetOrAddComponent(rootType);
            var fields = rootType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0, len = binds.Length; i < len; i++)
            {
                var bind = binds[i];
                if (bind.Root == null)
                {
                    $"{bind}的Root is Null".Log();
                    continue;
                }
                if (bind.Root == go)
                {
                    string n = HandleBind(bind).Trim();
                    var info = fields.FirstOrDefault(t => t.Name == n);
                    if (info == null) continue;
                    var bindCp = bind.GetComponent(info.FieldType);
                    if (bindCp) info.SetValue(cmpnt, Convert.ChangeType(bindCp, info.FieldType));
                    else $"无法找到{bind.Root}下{info.FieldType}脚本".Log();
                }
            }
            EditorUtility.SetDirty(cmpnt);
            EditorSceneManager.MarkSceneDirty(go.scene);
        }
        private static string HandleBind(Bind bind)
        {
            string goName = bind.gameObject.name;
            if (string.IsNullOrWhiteSpace(goName)) bind.usePrefix = true;
            else goName = goName.RemoveSpecialCharacters();
            string prefix = GetPrefix(bind.CType) + "_";
            prefix = bind.usePrefix ? prefix : char.IsDigit(goName[0]) ? prefix : "";
            return prefix + goName;
        }
        private void OnEnable()
        {
            string n = nameof(QuickCmdEditor);
            PathMode = EditorPrefs.GetBool($"{n}Pmod", false);
            mCmd = EditorPrefs.GetString($"{n}Cmd", mCmd);
            I.TPath = EditorPrefs.GetString($"{n}Path", I.TPath);
            I.Space = EditorPrefs.GetString($"{n}Space", I.Space);
            I.Hub = EditorPrefs.GetString($"{n}Hub", I.Hub);
            I.Search[0] = EditorPrefs.GetString($"{n}Search", I.Search[0]);
        }
        private void OnDisable()
        {
            string n = nameof(QuickCmdEditor);
            EditorPrefs.SetBool($"{n}Pmod", PathMode);
            EditorPrefs.SetString($"{n}Cmd", mCmd);
            if (!string.IsNullOrEmpty(I.TPath))
                EditorPrefs.SetString($"{n}Path", I.TPath);
            if (!string.IsNullOrEmpty(I.Space))
                EditorPrefs.SetString($"{n}Space", I.Space);
            if (!string.IsNullOrEmpty(I.Hub))
                EditorPrefs.SetString($"{n}Hub", I.Hub);
            if (!string.IsNullOrEmpty(I.Search[0]))
                EditorPrefs.SetString($"{n}Search", I.Search[0]);
        }
        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));

            mField = new TextField
            {
                value = mCmd,
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    fontSize = 24,
                    flexGrow = 1 // 占用剩余空间
                }
            };
            mField.RegisterCallback<ChangeEvent<string>>(OnChangeText);
            mField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            mField.RegisterCallback<DragPerformEvent>(OnDragPerform);
            mField.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);

            mField.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            // 将TextField添加到根元素
            root.Add(mField);
        }
        private void OnChangeText(ChangeEvent<string> evt) => mCmd = evt.newValue;
        private void ChangeField(string value) => mField.value = value;
        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            evt.StopImmediatePropagation();
        }
        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();
            if (DragAndDrop.paths.Length == 1)
            {
                if (evt.ctrlKey)
                {
                    string p = DragAndDrop.paths[0];
                    I.TPath = File.Exists(p) ? Path.GetDirectoryName(p) : p;
                    $"已标记生成路径:{I.TPath}".Log();
                }
                else if (evt.altKey)
                {
                    string p = DragAndDrop.paths[0];
                    I.Search[0] = File.Exists(p) ? Path.GetDirectoryName(p) : p;
                    $"已标记搜索路径:{I.Search[0]}".Log();
                }
                else
                {
                    var o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(DragAndDrop.paths[0]);
                    if (o is MonoScript mono)
                    {
                        Type scriptType = mono.GetClass();
                        if (scriptType == null)
                        {
                            "此脚本未定义类或存在编译错误".Log();
                        }
                        else if (scriptType.IsSubclassOf(typeof(ScriptableObject)) &&
                                !scriptType.IsSubclassOf(typeof(EditorWindow)))
                        {
                            SCRIPT = mono;
                            mField.value = "SoIns:" + mono.name;
                            $"SO => {mono.name}已标记".Log();
                        }
                        else $"{mono.name}不是SO".Log();
                    }
                    else "无法获取拖曳数据".Log();
                }
            }
            else
            {
                var refs = DragAndDrop.objectReferences;
                if (refs.Length > 0)
                {
                    mGos.ToFirst();
                    foreach (var obj in refs.OfType<GameObject>())
                    {
                        mGos.Push(obj);
                        mField.value = $"已缓存{obj.name}：GameObject";
                    }
                }
            }
            evt.StopImmediatePropagation();
        }
        private static bool GetPathByMode(ref string path)
        {
            if (PathMode)
            {
                string hub = I.Hub + "Hub";
                path = EditorKit.GetMonoPath(hub, hub + ".cs");
                path = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(path))
                {
                    $"无法找到{hub} 该脚本可能被删除 请指定正确的架构 Hub:架构名".Log();
                    return false;
                }
            }
            return true;
        }
        private static bool Eq(string source, string en, string ch) =>
            StringComparer.OrdinalIgnoreCase.Equals(source, en) || source == ch;
        private bool IsNotHub()
        {
            if (string.IsNullOrEmpty(I.Hub))
            {
                mField.value = "hub:";
                $"请先设置架构 {mCmd}架构名".Log();
                return true;
            }
            return false;
        }
        private void CreateModule(string name, string tag)
        {
            if (IsNotHub()) return;
            string tmp = $"namespace {I.Space}\r\n{{\r\n    public interface I@{tag} : IModule\r\n    {{\r\n\r\n    }}\r\n    public class @{tag} : AbsModule, I@{tag}\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
            string path = I.TPath;
            if (GetPathByMode(ref path))
            {
                if (PathMode) path = $"{path}/{tag}";
                EditorKit.CreatScript(path, name, tag, tmp);
                mField.value = $"{tag}:";
            }
            else mField.value = "hub:";
        }
        private void CreateScript(string name, string tag)
        {
            if (IsNotHub()) return;
            string father = tag switch
            {
                "Mono" => "MonoBehaviour",
                "SO" => "ScriptableObject",
                _ => I.Hub + tag
            };
            string tmp = $"using UnityEngine;\r\n\r\nnamespace {I.Space}\r\n{{\r\n    public class @ : {father}\r\n    {{\r\n\r\n    }}\r\n}}";
            string path = I.TPath;
            if (GetPathByMode(ref path))
            {
                if (PathMode) path = $"{path}/{tag}";
                EditorKit.CreatScript(path, name, "", tmp);
                mField.value = $"{tag}:";
            }
            else mField.value = "hub:";
        }
        private void ShowHelp()
        {
            string instructions = $"# Panty 工具集使用手册\r\n\r\n```\r\n==============================\r\n  Panty 工具集使用手册\r\n==============================\r\n\r\n目录:\r\n1. TextDialog 使用指南\r\n2. QuickCmdEditor 使用指南\r\n   - 基础操作\r\n   - 命令说明\r\n   - 拖曳操作与上下文菜单\r\n\r\n------------------------------\r\n1. TextDialog 使用指南\r\n------------------------------\r\n\r\n打开提示框:\r\n1. 调用 TextDialog.Open 方法，传入需要显示的消息和可选的回调函数:\r\n   TextDialog.Open(\"你的消息\", 确认回调, 取消回调);\r\n2. 在提示框中，用户可以通过点击确认或取消按钮进行相应的操作。\r\n\r\n------------------------------\r\n2. QuickCmdEditor 使用指南\r\n------------------------------\r\n\r\n### 基础操作\r\n\r\n快速打开:\r\n1. 在 Unity 编辑器顶部菜单栏选择 PnTool/QuickCmd &Q 打开 QuickCmdEditor 窗口。\r\n   - 快捷键: Alt + Q\r\n\r\n添加绑定:\r\n1. 在场景中选择一个或多个 GameObject。\r\n2. 在 Unity 编辑器顶部菜单栏选择 PnTool/Quick/AddBind &B 为所选对象添加绑定组件。\r\n   - 快捷键: Alt + B\r\n\r\n添加UI根节点:\r\n1. 在场景中选择一个或多个 GameObject。\r\n2. 在 Unity 编辑器顶部菜单栏选择 PnTool/Quick/AddUIRoot &W 将所选对象设置为 UI 根节点。\r\n   - 快捷键: Alt + W\r\n\r\n检查更新:\r\n1. 打开 QuickCmdEditor 窗口。\r\n2. 在命令输入框中输入 check 或 检查更新，然后按回车键。\r\n3. 程序将会检查更新并显示相应信息。\r\n   - 上下文菜单: 右键点击命令输入框选择 检查更新。\r\n\r\n### 命令说明\r\n\r\n基础命令:\r\n- 帮助: 输入 help 或 帮助 查看帮助信息。\r\n  - 上下文菜单: 右键点击命令输入框选择 显示帮助。\r\n- 清理: 输入 clear 或 清理 清空已标记的信息。\r\n  - 上下文菜单: 右键点击命令输入框选择 清空数据。\r\n- UI绑定: 输入 uiBind 或 UI绑定 进行 UI 绑定操作。\r\n  - 上下文菜单: 右键点击命令输入框选择 绑定 UI。\r\n- 检查更新: 输入 check 或 检查更新 检查更新。\r\n  - 上下文菜单: 右键点击命令输入框选择 检查更新。\r\n\r\n模块和脚本创建命令:\r\n- 命名空间: 输入 space:命名空间 设置命名空间，例如 space:MyNamespace。\r\n- 路径: 输入 path:路径 设置路径，例如 path:Assets/MyPath。输入 path:base 或 path:基础 创建基础目录。\r\n- SoIns: 创建 ScriptableObject 实例。例如 SoIns:MyScriptableObject。\r\n- Module: 创建模块。例如 Module:MyModule。\r\n- System: 创建系统。例如 System:MySystem。\r\n- Model: 创建数据模型。例如 Model:MyModel。\r\n- Game: 创建游戏脚本。例如 Game:MyGameScript。\r\n- UI: 创建表现层脚本。例如 UI:MyUIScript。\r\n- Mono: 创建 MonoBehaviour 脚本。例如 Mono:MyMonoScript。\r\n- so: 创建 ScriptableObject 脚本。例如 so:MyScriptableObject。\r\n- hub: 创建架构。例如 hub:MyHub。\r\n\r\n### 拖曳操作与上下文菜单\r\n\r\n拖曳操作:\r\n1. 将一个 MonoScript 脚本文件拖到 QuickCmdEditor 窗口中，并按住 Ctrl 键以标记路径。\r\n2. 将一个或多个 GameObject 拖到 QuickCmdEditor 窗口中，以缓存这些对象供后续操作。\r\n\r\n上下文菜单:\r\n1. 右键点击 QuickCmdEditor 命令输入框可弹出上下文菜单，选择以下操作:\r\n   - 显示帮助: 查看帮助信息。\r\n   - 绑定 UI: 进行 UI 绑定操作。\r\n   - 基础目录: 设置基础目录。\r\n   - 检查更新: 检查更新。\r\n   - 清空数据: 清空已标记的信息。\r\n\r\n通过以上操作，用户可以快速利用 Panty 工具集在 Unity 编辑器中进行各种便捷的操作。\r\n```";
            string sc = SCRIPT == null ? "null" : SCRIPT.name;
            string hub = I.Hub + "Hub";
            string path = EditorKit.GetMonoPath(hub, hub + ".cs");
            TextDialog.Open($"{instructions}\r\n\r\n标记架构：{hub} [{path}]\r\n路径模式：以 {(PathMode ? "Hub" : "TPath")}作为生成路径\r\n标记路径：{I.TPath}\r\n标记命名空间：{I.Space}\r\n标记搜索路径：{I.Search[0]}\r\n标记SO资源：{sc}\r\n");
            mField.value = "";
        }
        private void ClearInfo()
        {
            mField.value = null;
            SCRIPT = null;
            I.TPath = "Assets/Scripts";
            I.Search[0] = "Assets";
            $"SCRIPT已置空,创建路径:{I.TPath},搜索路径{I.Search[0]}".Log();
        }
        private void BasicCatalog()
        {
            bool lose = true;
            string[] fileNames = { "ArtRes/Sprites", "Resources/Audios/Bgm", "Resources/Audios/Sound", "Resources/Prefabs", "Scripts/Framework/Module", "Scripts/Project/Game", "StreamingAssets/Csv" };
            for (int i = 0; i < fileNames.Length; i++)
            {
                string path = $"Assets/{fileNames[i]}";
                if (FileKit.TryCreateDirectory(path))
                {
                    $"{path}创建成功".Log();
                    lose = false;
                }
            }
            I.Search[0] = "Assets/Scripts";
            if (lose) "所有文件夹已就位".Log();
            else AssetDatabase.Refresh();
        }
        private void CheckUpdate()
        {
            if (IsAsync) return;
            string url = "https://gitee.com/PantyNeko/MeowFramework/raw/main/Assets/VersionInfo.txt";
            RequestInfo(url, "正在检查更新 请稍后...", txt =>
            {
                string[] res = txt.Split("@");
                string version = HubTool.version;
                if (res[0] == version)
                {
                    TextDialog.Open($"当前为最新版本：[ {version} ] > 无需更新\r\n{res[1]}");
                }
                else
                {
                    TextDialog.Open($"当前版本：{version}\r\n最新版本：{res[0]}\r\n\r\n{res[1]}");
                }
                mField.value = "";
            });
        }
        private void OnUIBind()
        {
            if (mGos.Count == 0)
            {
                "无可操作对象".Log();
                return;
            }
            foreach (var go in mGos)
            {
                if (go == null) continue;
                AddUIRoot(go);
            }
        }
        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // 清除现有菜单项
            evt.menu.MenuItems().Clear();
            // 添加新的菜单项
            evt.menu.AppendAction("创建命令/模块层(Module)", e => mField.value = "Module:脚本名");
            evt.menu.AppendAction("创建命令/系统层(System)", e => mField.value = "System:脚本名");
            evt.menu.AppendAction("创建命令/数据层(Model)", e => mField.value = "Model:脚本名");
            evt.menu.AppendSeparator("创建命令/");
            evt.menu.AppendAction("创建命令/控制脚本(Game)", e => mField.value = "Game:脚本名");
            evt.menu.AppendAction("创建命令/普通脚本(Mono)", e => mField.value = "Mono:脚本名");
            evt.menu.AppendAction("创建命令/表现脚本(View)", e => mField.value = "UI:脚本名");
            evt.menu.AppendAction("创建命令/数据脚本(SO)", e => mField.value = "So:脚本名");
            evt.menu.AppendAction("创建命令/数据资源(SoIns)", e => mField.value = "SoIns:资源名");
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("绑定命令/命名空间(NameSpace)", e => mField.value = "Space:命名空间名");
            evt.menu.AppendAction("绑定命令/路径(Path)", e => mField.value = "Path:路径名");
            evt.menu.AppendAction("绑定命令/架构(Hub)", e => mField.value = "Hub:架构名");
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("显示帮助", e => ShowHelp());
            evt.menu.AppendAction("绑定 UI", e => OnUIBind());
            evt.menu.AppendAction("基础目录", e => BasicCatalog());
            evt.menu.AppendAction("检查更新", e => CheckUpdate());
            evt.menu.AppendAction("清空数据", e =>
            {
                if (EditorKit.Dialog("真的要清空数据嘛！")) ClearInfo();
            });
        }
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                // 如果空指令就跳出
                if (string.IsNullOrWhiteSpace(mCmd))
                {
                    evt.StopPropagation();
                    return;
                }
                // 去除指令头尾
                string cmd = mCmd.Trim();
                // 帮助指令
                if (Eq(cmd, "Bind", "绑定")) OnUIBind();
                else if (Eq(cmd, "help", "帮助")) ShowHelp();
                else if (Eq(cmd, "clear", "清理")) ClearInfo();
                else if (Eq(cmd, "check", "检查更新")) CheckUpdate();
                else // 带后缀的指令
                {
                    string[] cmds = cmd.Split(':', '：');
                    string info = cmds[0].TrimEnd();
                    if (Eq(info, "space", "命名空间"))
                    {
                        if (cmds.Length > 1)
                        {
                            cmd = cmds[1].TrimStart();
                            if (char.IsLetterOrDigit(cmd[cmd.Length - 1]))
                                I.Space = cmd;
                        }
                        $"当前命名空间为:{I.Space}".Log();
                        mField.value = "space:";
                    }
                    else if (Eq(info, "path", "路径"))
                    {
                        if (cmds.Length == 1)
                        {
                            $"当前路径为:{I.TPath}".Log();
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(cmds[1]))
                            {
                                $"当前路径为:{I.TPath}".Log();
                            }
                            else
                            {
                                // 得到实际路径 并去掉空格 2种情况 相对路径模式 绝对路径模式
                                string sub = cmds[1].Trim();
                                if (char.IsDigit(sub[0]))
                                {
                                    "指令不能以数字开头".Log();
                                }
                                else if (Eq(sub, "base", "基础"))
                                    BasicCatalog();
                                else
                                {
                                    // 相对路径处理
                                    if (sub[0] == '/') cmd = I.TPath + sub;
                                    // 如果没有根目录 需要加上根目录
                                    else cmd = sub.Split('/')[0] == "Assets" ? sub : $"Assets/{sub}";
                                    // 进行路径合法性判断
                                    if (FileKit.EnsurePathExists(ref cmd))
                                    {
                                        if (Directory.Exists(cmd))
                                        {
                                            $"路径存在,已标记{cmd}".Log();
                                            I.TPath = cmd;
                                            mField.value = "path:";
                                        }
                                        else if (EditorKit.Dialog($"确定要创建该路径？\r\nPath：{cmd}"))
                                        {
                                            mField.value = "path:";
                                            I.TPath = cmd;
                                            $"{cmd}创建成功,已标记该路径".Log();
                                            Directory.CreateDirectory(cmd);
                                            AssetDatabase.Refresh();
                                        }
                                        else $"取消创建,标记原路径 {I.TPath}".Log();
                                    }
                                    else $"路径不合法 {cmd}".Log();
                                }
                            }
                        }
                    }
                    else if (cmds.Length > 1)
                    {
                        string sub = cmds[1].TrimStart();
                        if (char.IsDigit(sub[0]))
                        {
                            "指令不能以数字开头".Log();
                        }
                        else if (Eq(info, "SoIns", "So实例"))
                        {
                            if (SCRIPT != null)
                            {
                                string path = I.TPath;
                                if (GetPathByMode(ref path))
                                {
                                    if (PathMode) path = $"{path}/SoIns";
                                    FileKit.TryCreateDirectory(path);
                                    string file = $"{path}/{sub}.asset";
                                    if (File.Exists(file))
                                    {
                                        $"{sub}.asset 已存在".Log();
                                    }
                                    else
                                    {
                                        string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(file);
                                        var instance = new SerializedObject(CreateInstance(SCRIPT.GetClass()));
                                        instance.Update();
                                        instance.ApplyModifiedPropertiesWithoutUndo();
                                        AssetDatabase.CreateAsset(instance.targetObject, uniqueFileName);
                                        AssetDatabase.ImportAsset(uniqueFileName);
                                        // 清空serializedObject，以便连续创建
                                        // serializedObject = null;
                                        $"{sub}.asset 创建成功".Log();
                                    }
                                }
                                else mField.value = "hub:";
                            }
                        }
                        else if (Eq(info, "Module", "模块")) CreateModule(sub, "Module");
                        else if (Eq(info, "System", "系统")) CreateModule(sub, "System");
                        else if (Eq(info, "Model", "数据")) CreateModule(sub, "Model");
                        else if (Eq(info, "Game", "游戏")) CreateScript(sub, "Game");
                        else if (Eq(info, "UI", "表现")) CreateScript(sub, "UI");
                        else if (Eq(info, "Mono", "脚本")) CreateScript(sub, "Mono");
                        else if (Eq(info, "so", "SO")) CreateScript(sub, "SO");
                        else if (Eq(info, "hub", "架构"))
                        {
                            string tmp = $"using UnityEngine;\r\n\r\nnamespace {I.Space}\r\n{{\r\n    public class @Hub : ModuleHub<@Hub>\r\n    {{\r\n        protected override void BuildModule()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n    public class @Game : MonoBehaviour, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n    public class @UI : UIPanel, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n}}";
                            EditorKit.CreatScript(I.TPath, sub, "Hub", tmp);
                            $"已标记{sub}Hub架构".Log();
                            mField.value = $"{info}:";
                            I.Hub = sub;
                        }
                    }
                    else $"{cmd}指令错误".Log();
                }
                evt.StopPropagation();
            }
        }
        private async void RequestInfo(string url, string tips, Action<string> call)
        {
            try
            {
                using (var client = new HttpClient())
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8))) // 设置超时时间
                {
                    IsAsync = true;
                    mField.value = (tips);
                    var response = await client.GetAsync(url, cts.Token);
                    var content = response.EnsureSuccessStatusCode().Content;
                    call?.Invoke(await content.ReadAsStringAsync());
                }
            }
            catch (TaskCanceledException e)
            {
                bool trigger = e.CancellationToken.IsCancellationRequested;
                EditorKit.Tips(trigger ? "请求被用户取消。" : "请求超时!");
            }
            catch (HttpRequestException e)
            {
                EditorKit.Tips($"请求错误: {e.Message}");
            }
            finally
            {
                IsAsync = false;
            }
        }
    }
}