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
    public class QuickCmdEditor : EditorWindow
    {
        private bool IsAsync;
        private string mCmd = "#help";
        private string mPath = "Codes";
        private string mSpace = "Panty.Test";
        private string mHub;
        private TextField mField;

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
            string _name = nameof(QuickCmdEditor);
            mCmd = EditorPrefs.GetString($"{_name}Cmd", mCmd);
            mPath = EditorPrefs.GetString($"{_name}Path", mPath);
            mSpace = EditorPrefs.GetString($"{_name}Space", mSpace);
            mHub = EditorPrefs.GetString($"{_name}Hub", mHub);
        }
        private void OnDisable()
        {
            string _name = nameof(QuickCmdEditor);
            EditorPrefs.SetString($"{_name}Cmd", mCmd);
            EditorPrefs.SetString($"{_name}Path", mPath);
            EditorPrefs.SetString($"{_name}Space", mSpace);
            if (string.IsNullOrEmpty(mHub)) return;
            EditorPrefs.SetString($"{_name}Hub", mHub);
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
            // 将TextField添加到根元素
            root.Add(mField);
        }
        private static bool Eq(string source, string en, string ch) => StringComparer.OrdinalIgnoreCase.Equals(source, en) || source == ch;
        private void CreateModule(string name, string tag)
        {
            string tmp = $"namespace {mSpace}\r\n{{\r\n    public interface I@{tag} : IModule\r\n    {{\r\n\r\n    }}\r\n    public class @{tag} : AbsModule, I@{tag}\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
            EditorKit.CreatScript(mPath, name, tag, tmp);
            ResetCmd($"F:{tag}:");
        }
        private void CreateScript(string name, string tag, string type = null)
        {
            if (string.IsNullOrEmpty(mHub))
            {
                ResetCmd("F:hub:");
                $"请先设置架构 {mCmd}架构名".Log();
                return;
            }
            string father = type == null ? "MonoBehaviour" : mHub + type;
            string tmp = $"using UnityEngine;\r\n\r\nnamespace {mSpace}\r\n{{\r\n    public class @ : {father}\r\n    {{\r\n\r\n    }}\r\n}}";
            EditorKit.CreatScript(mPath, name, "", tmp);
            ResetCmd($"F:{tag}:");
        }
        private void ResetCmd(string cmd)
        {
            mCmd = cmd;
            mField.SetValueWithoutNotify(mCmd);
        }
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                if (string.IsNullOrEmpty(mCmd))
                {
                    evt.StopPropagation();
                    return;
                }
                string cmd = mCmd.Trim(); // 去掉头尾的空格
                if (cmd[0] == '#') // 说明是快捷指令
                {
                    cmd = cmd.TrimStart('#', ' ');
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        if (Eq(cmd, "help", "帮助"))
                        {
                            string instructions = $"// 以下指令已兼容大小写和全角半角\r\n\r\n[#  ] : 快捷指令 无需后缀  例如：#help\r\n[    ] : 组合指令 通常由指令头+类型+名字组成 例如：@f:hub:Project\r\n[： ] : 分隔符(指示功能) 例如：@指令:类型:名字\r\n[f/p]： 标识创建文件或路径 未完\r\n\r\n// 快捷指令\r\n\r\n#help \r\n显示帮助面板 显示一些提示信息 也可使用 #帮助\r\n#path \r\n在Console 窗口显示已标记路径 也可使用 #路径\r\n#space \r\n在Console 窗口显示已标记命名空间 也可以使用 #命名空间\r\n\r\n// 衍生指令\r\n\r\n#path:路径字符串\r\n标记自定义路径 如果路径不存在 尝试创建目录\r\n\r\n#space:命名空间字符串\r\n标记自定义命名空间 并保存\r\n\r\n// 创建型指令  创建文件的路径以 #path 的标记为准\r\n\r\nf:hub:Name 或 f:架构:架构名\r\n尝试创建一个该名字的架构  \r\n\r\nf:Mono:Name 或 f:脚本:脚本名\r\n尝试创建一个该名字的普通脚本  \r\n\r\nf:UI:Name 或 f:表现:脚本名\r\n尝试创建一个该名字的UI脚本  \r\n\r\nf:Game:Name 或 f:游戏:脚本名\r\n尝试创建一个该名字的Game脚本 \r\n\r\nf:Model:Name 或 f:数据:脚本名\r\n尝试创建一个该名字的Model脚本 \r\n\r\nf:System:Name 或 f:系统:脚本名\r\n尝试创建一个该名字的System脚本 \r\n\r\nf:Module:Name 或 f:模块:脚本名\r\n尝试创建一个该名字的Module脚本 ";
                            TextDialog.Open($"{instructions}\r\n\r\n已标记路径：Assets/{mPath}\r\n已标记命名空间：{mSpace}\r\n已标记架构：{mHub}Hub\r\n");
                            ResetCmd("#");
                        }
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
                                    ResetCmd("#");
                                });
                            }
                        }
                        else
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
                            }
                            else if (Eq(info, "path", "路径"))
                            {
                                if (cmds.Length == 1)
                                {
                                    $"当前路径为:Assets/{mPath}".Log();
                                }
                                else
                                {
                                    string sub = cmds[1].Trim();
                                    cmd = "Assets/" + sub;
                                    if (FileKit.EnsurePathExists(ref cmd))
                                    {
                                        if (!Directory.Exists(cmd))
                                        {
                                            if (EditorKit.Dialog($"确定要创建该路径？\r\nPath：{cmd}"))
                                            {
                                                ResetCmd("#path:");
                                                mPath = sub;
                                                $"{cmd}创建成功,已标记该路径".Log();
                                                Directory.CreateDirectory(cmd);
                                                AssetDatabase.Refresh();
                                            }
                                            else $"取消创建,标记原路径 Assets/{mPath}".Log();
                                        }
                                        else
                                        {
                                            $"路径存在,已标记{cmd}".Log();
                                            mPath = sub;
                                            ResetCmd("#path:");
                                        }
                                    }
                                    else $"路径不合法 {cmd}".Log();
                                }
                            }
                        }
                    }
                }
                else
                {
                    string[] cmds = cmd.Split(':', '：');
                    if (cmds.Length >= 3)
                    {
                        string inst = cmds[0].Trim();
                        switch (inst)
                        {
                            case "F":
                            case "f":
                                string info = cmds[1].Trim();
                                // 创建文件
                                string name = cmds[2].TrimStart();
                                if (Eq(info, "Module", "模块")) CreateModule(name, "Module");
                                else if (Eq(info, "System", "系统")) CreateModule(name, "System");
                                else if (Eq(info, "Model", "数据")) CreateModule(name, "Model");
                                else if (Eq(info, "Game", "游戏")) CreateScript(name, "Game", "Game");
                                else if (Eq(info, "UI", "表现")) CreateScript(name, "UI", "UI");
                                else if (Eq(info, "Mono", "脚本")) CreateScript(name, "Mono");
                                else if (Eq(info, "hub", "架构"))
                                {
                                    string tmp = $"using UnityEngine;\r\n\r\nnamespace {mSpace}\r\n{{\r\n    public class @Hub : ModuleHub<@Hub>\r\n    {{\r\n        protected override void BuildModule()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n    public class @Game : MonoBehaviour, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n    public class @UI : UIPanel, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n}}";
                                    EditorKit.CreatScript(mPath, name, "Hub", tmp);
                                    ResetCmd($"F:{info}:");
                                    mHub = name;
                                }
                                else $"{cmd}指令错误".Log();
                                break;
                            default:
                                break;
                        }
                    }
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
                    ResetCmd(tips);
                    var response = await client.GetAsync(url, cts.Token);
                    var content = response.EnsureSuccessStatusCode().Content;
                    call?.Invoke(await content.ReadAsStringAsync());
                }
            }
            catch (TaskCanceledException e)
            {
                EditorKit.Tips(e.CancellationToken.IsCancellationRequested ? "请求被用户取消。" : "请求超时!");
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