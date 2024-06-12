using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
        private string mPath = "Codes";
        private string mSpace = "Panty.Test";
        private string mHub;
        private TextField mField;

        private MonoScript SCRIPT;

        [MenuItem("PnTool/QuickCmd &Q")]
        private static void OpenSelf()
        {
            if (EditorKit.ShowOrHide<QuickCmdEditor>(out var win))
            {
                win.maxSize = new Vector2(960f, 40f);
                win.minSize = new Vector2(360f, 40f);
            }
        }
        private void OnEnable()
        {
            string n = nameof(QuickCmdEditor);
            mCmd = EditorPrefs.GetString($"{n}Cmd", mCmd);
            mPath = EditorPrefs.GetString($"{n}Path", mPath);
            mSpace = EditorPrefs.GetString($"{n}Space", mSpace);
            mHub = EditorPrefs.GetString($"{n}Hub", mHub);
        }
        private void OnDisable()
        {
            string n = nameof(QuickCmdEditor);
            EditorPrefs.SetString($"{n}Cmd", mCmd);
            EditorPrefs.SetString($"{n}Path", mPath);
            if (!string.IsNullOrEmpty(mSpace))
                EditorPrefs.SetString($"{n}Space", mSpace);
            if (!string.IsNullOrEmpty(mHub))
                EditorPrefs.SetString($"{n}Hub", mHub);
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
            mField.RegisterCallback<ChangeEvent<string>>(evt => mCmd = evt.newValue);
            mField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            mField.RegisterCallback<DragPerformEvent>(OnDragPerform);
            mField.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            // 将TextField添加到根元素
            root.Add(mField);
        }
        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();
            if (DragAndDrop.paths.Length > 0)
            {
                var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(DragAndDrop.paths[0]);
                if (mono == null) "无法获取拖曳数据".Log();
                else
                {
                    Type scriptType = mono.GetClass();
                    if (scriptType != null)
                    {
                        if (scriptType.IsSubclassOf(typeof(ScriptableObject)))
                        {
                            SCRIPT = mono;
                            mField.value = "SoIns:" + mono.name;
                            $"SO => {mono.name}已标记".Log();
                        }
                        else $"{mono.name}不是ScriptableObject".Log();
                    }
                }
            }
            evt.StopImmediatePropagation();
        }
        private static bool Eq(string source, string en, string ch) =>
            StringComparer.OrdinalIgnoreCase.Equals(source, en) || source == ch;
        private void CreateModule(string name, string tag)
        {
            string tmp = $"namespace {mSpace}\r\n{{\r\n    public interface I@{tag} : IModule\r\n    {{\r\n\r\n    }}\r\n    public class @{tag} : AbsModule, I@{tag}\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
            EditorKit.CreatScript(mPath, name, tag, tmp);
            mField.value = $"{tag}:";
        }
        private void CreateScript(string name, string tag)
        {
            if (string.IsNullOrEmpty(mHub))
            {
                mField.value = "hub:";
                $"请先设置架构 {mCmd}架构名".Log();
                return;
            }
            string father = tag switch
            {
                "Mono" => "MonoBehaviour",
                "So" => "ScriptableObject",
                _ => mHub + tag
            };
            string tmp = $"using UnityEngine;\r\n\r\nnamespace {mSpace}\r\n{{\r\n    public class @ : {father}\r\n    {{\r\n\r\n    }}\r\n}}";
            EditorKit.CreatScript(mPath, name, "", tmp);
            mField.value = $"{tag}:";
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
                if (Eq(cmd, "help", "帮助"))
                {
                    string instructions = $"// 以下指令已兼容大小写和全角半角\r\n\r\n[快捷指令] -> 通常由一个单词组成 例如：help\r\n[组合指令] -> 通常由类型+名字组成 例如：hub:Project\r\n[分隔指令] -> 通常由（:）或（：）组成\r\n\r\n// 快捷指令\r\n\r\n[ help   （帮助）] -> 显示帮助面板 在自定义提示窗显示一些信息\r\n[ path   （路径）] -> 在Console 窗口显示已标记路径\r\n[ space（空间）] -> 在Console 窗口显示已标记命名空间\r\n\r\n// 衍生指令\r\n\r\n[ path : 路径字符串 ] -> 标记自定义路径 如果路径不存在 尝试创建目录\r\n[ space : 命名空间 ] -> 标记自定义命名空间 并保存\r\n\r\n// 创建型指令  创建文件的路径以 path（路径） 的标记为准\r\n\r\n[ hub : Name ] 或 [ 架构 : 名字 ]\r\n尝试创建一个该名字的架构（Framework）\r\n\r\n[ Mono : Name ] 或 [ 脚本 : 名字 ]\r\n尝试创建一个该名字的普通脚本（Script）\r\n\r\n[ UI : Name ] 或 [ 表现 : 名字 ]\r\n尝试创建一个该名字的UI脚本（UI）\r\n\r\n[ Game : Name ] 或 [ 游戏 : 名字 ]\r\n尝试创建一个该名字的控制脚本（Control） \r\n\r\n[ Model : Name ] 或 [ 数据 : 名字 ]\r\n尝试创建一个该名字的数据脚本（Model） \r\n\r\n[ System : Name ] 或 [ 系统 : 名字 ]\r\n尝试创建一个该名字的系统脚本（System）\r\n\r\n[ Module : Name ] 或 [ 模块 : 名字 ]\r\n尝试创建一个该名字的模块脚本（Module）";
                    string sc = SCRIPT == null ? "null" : SCRIPT.name;
                    TextDialog.Open($"{instructions}\r\n\r\n已标记架构：{mHub}Hub\r\n已标记路径：Assets/{mPath}\r\n已标记命名空间：{mSpace}\r\n已标记SO资源：{sc}\r\n");
                    mField.value = "";
                }
                else if (Eq(cmd, "clear", "清理"))
                {
                    mField.value = null;
                    SCRIPT = null;
                    "清理成功".Log();
                }
                // 检查指令
                else if (Eq(cmd, "check", "检查更新"))
                {
                    if (!IsAsync)
                    {
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
                }
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
                                mSpace = cmd;
                        }
                        $"当前命名空间为:{mSpace}".Log();
                        mField.value = "space:";
                    }
                    else if (Eq(info, "path", "路径"))
                    {
                        if (cmds.Length == 1)
                        {
                            $"当前路径为:Assets/{mPath}".Log();
                        }
                        else
                        {
                            string sub = cmds[1].TrimStart();
                            string[] arr = sub.Split('/');

                            if (arr[0] == "Assets")
                                sub = sub.Substring(arr.Length > 1 ? 7 : 6);

                            if (Eq(sub, "base", "基础"))
                            {
                                string[] fileNames = { "ArtRes/Sprites", "Resources/Audios/Bgm", "Resources/Audios/Sound", "Resources/Prefabs", "Scripts/Framework", "Project/Game", "StreamingAssets/Csv" };
                                for (int i = 0; i < fileNames.Length; i++)
                                {
                                    string path = $"Assets/{fileNames[i]}";
                                    if (FileKit.TryCreateDirectory(path)) $"{path}创建成功".Log();
                                }
                                AssetDatabase.Refresh();
                            }
                            else
                            {
                                cmd = $"Assets/{sub}";
                                if (FileKit.EnsurePathExists(ref cmd))
                                {
                                    if (Directory.Exists(cmd))
                                    {
                                        $"路径存在,已标记{cmd}".Log();
                                        mPath = sub;
                                        mField.value = "path:";
                                    }
                                    else if (EditorKit.Dialog($"确定要创建该路径？\r\nPath：{cmd}"))
                                    {
                                        mField.value = "path:";
                                        mPath = sub;
                                        $"{cmd}创建成功,已标记该路径".Log();
                                        Directory.CreateDirectory(cmd);
                                        AssetDatabase.Refresh();
                                    }
                                    else $"取消创建,标记原路径 Assets/{mPath}".Log();
                                }
                                else $"路径不合法 {cmd}".Log();
                            }
                        }
                    }
                    else if (cmds.Length > 1)
                    {
                        string sub = cmds[1].TrimStart();
                        if (Eq(info, "SoIns", "So实例"))
                        {
                            if (SCRIPT != null)
                            {
                                string path = $"Assets/{mPath}";
                                FileKit.TryCreateDirectory(path);
                                path = $"{path}/{sub}.asset";
                                if (File.Exists(path))
                                {
                                    $"{sub}.asset 已存在".Log();
                                }
                                else
                                {
                                    string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(path);
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
                        }
                        else if (Eq(info, "Module", "模块")) CreateModule(sub, "Module");
                        else if (Eq(info, "System", "系统")) CreateModule(sub, "System");
                        else if (Eq(info, "Model", "数据")) CreateModule(sub, "Model");
                        else if (Eq(info, "Game", "游戏")) CreateScript(sub, "Game");
                        else if (Eq(info, "UI", "表现")) CreateScript(sub, "UI");
                        else if (Eq(info, "Mono", "脚本")) CreateScript(sub, "Mono");
                        else if (Eq(info, "so", "SO")) CreateScript(sub, "So");
                        else if (Eq(info, "hub", "架构"))
                        {
                            string tmp = $"using UnityEngine;\r\n\r\nnamespace {mSpace}\r\n{{\r\n    public class @Hub : ModuleHub<@Hub>\r\n    {{\r\n        protected override void BuildModule()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n    public class @Game : MonoBehaviour, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n    public class @UI : UIPanel, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n}}";
                            EditorKit.CreatScript(mPath, sub, "Hub", tmp);
                            $"已标记{sub}Hub架构".Log();
                            mField.value = $"{info}:";
                            mHub = sub;
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