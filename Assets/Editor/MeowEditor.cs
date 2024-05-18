using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Panty
{
    public class TextDialog : EditorWindow
    {
        private Vector2 scrollPosition;
        private GUIStyle style;
        private string longText = "";
        private Action succeed, fail;

        public static void Show(string msg, Action succeed = null, Action fail = null)
        {
            var wd = GetWindow<TextDialog>("喵喵提示器").Init(msg);
            wd.succeed = succeed;
            wd.fail = fail;
        }
        private TextDialog Init(string text)
        {
            longText = text;
            style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            return this;
        }
        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("您的喵喵女友来电话啦！不接的话打洗你哦", MessageType.Info);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUILayout.Label(longText, style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("确认", GUILayout.Height(30f)))
            {
                succeed?.Invoke();
                Close();
            }
            if (GUILayout.Button("取消", GUILayout.Height(30f)))
            {
                fail?.Invoke();
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
    public class MeowEditor : PnEditor<MeowEditor.E_Type>
    {
        private bool IsAsync;
        public enum E_Type : byte { Empty, A }
        protected string NameSpace => "Panty.Test";
        [MenuItem("PnTool/MeowEditor")]
        public static void ShowWindow() => GetWindow<MeowEditor>("MeowEditor", true);
        protected override E_Type Empty => E_Type.Empty;
        protected override (string, Action)[] InitBtnInfo()
        {
            return new (string, Action)[]
            {
                ("创建脚本",() =>
                {
                    if (CheckInputLegal())
                    {
                        CreatScript("", $"using UnityEngine;\r\n\r\nnamespace {NameSpace}\r\n{{\r\n    public class @ : MonoBehaviour\r\n    {{\r\n\r\n    }}\r\n}}");
                    }
                }),
                ("创建编辑器",()=>
                {
                    if (CheckInputLegal())
                    {
                        CreatScript("Editor", $"using UnityEditor;\r\n\r\nnamespace {NameSpace}\r\n{{\r\n    public class @Editor : PnEditor<@Editor.E_Type>\r\n    {{\r\n        public enum E_Type : byte {{ Empty }}\r\n\r\n        [MenuItem(\"PnTool/@Editor\")]\r\n        public static void ShowWindow()\r\n        {{\r\n            GetWindow<@Editor>(\"@Editor\", true);\r\n        }}\r\n        protected override E_Type Empty => E_Type.Empty;\r\n    }}\r\n}}");
                    }
                }),
                ("创建Hub类",() =>
                {
                    if (CheckInputLegal())
                    {
                        string code = $"using UnityEngine;\r\n\r\nnamespace {NameSpace}\r\n{{\r\n    public class @Hub : ModuleHub<@Hub>\r\n    {{\r\n        protected override void BuildModule()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n    public class @Game : MonoBehaviour, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n    public class @UI : UIPanel, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n}}";
                        CreatScript("Hub",code);
                    }
                }),
                ("创建模块",() =>
                {
                    if (CheckInputLegal())
                    {
                        string code = $"namespace {NameSpace}\r\n{{\r\n    public interface I@Module : IModule\r\n    {{\r\n\r\n    }}\r\n    public class @Module : AbsModule, I@Module\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
                        CreatScript("Module",code);
                    }
                }),
                ("QF_创建架构",() =>
                {
                    if (CheckInputLegal())
                    {
                        CreatScript("Game",$"using UnityEngine;\r\n\r\nnamespace {NameSpace}\r\n{{\r\n    public class @Game : Architecture<@Game>\r\n    {{\r\n        protected override void Init()\r\n        {{\r\n            // 注册模块\r\n        }}\r\n    }}\r\n    public class @GameController : MonoBehaviour, IController\r\n    {{\r\n        IArchitecture IBelongToArchitecture.GetArchitecture() => @Game.Interface;\r\n    }}\r\n    public class @UIController : UIPanel, IController\r\n    {{\r\n        IArchitecture IBelongToArchitecture.GetArchitecture() => @Game.Interface;\r\n    }}\r\n}}");
                    }
                }),
                ("QF_创建系统",() =>
                {
                    if (CheckInputLegal())
                    {
                        CreatScript("System", $"namespace {NameSpace}\r\n{{\r\n    public interface I@System : ISystem\r\n    {{\r\n\r\n    }}\r\n    public class @System : AbstractSystem, I@System\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}");
                    }
                }),
                ("QF_创建数据模型",() =>
                {
                    if (CheckInputLegal())
                    {
                        CreatScript("Model", $"namespace {NameSpace}\r\n{{\r\n    public interface I@Model : IModel\r\n    {{\r\n\r\n    }}\r\n    public class @Model : AbstractModel, I@Model\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}");
                    }
                }),
                ("检查更新",CheckUpdate),
            };
        }
        private async void CheckUpdate()
        {
            if (IsAsync) return;
            IsAsync = true;
            string url = "https://gitee.com/PantyNeko/MeowFramework/raw/main/Assets/VersionInfo.txt";
            string version = "1.0.6";
            using (var client = new HttpClient())
            {
                try
                {
                    using (var cts = new CancellationTokenSource())
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(8)); // 设置超时时间
                        inputText = "正在检查更新 请稍后...";
                        var response = await client.GetAsync(url, cts.Token);
                        response.EnsureSuccessStatusCode();
                        string text = await response.Content.ReadAsStringAsync();
                        string[] res = text.Split("@");
                        if (res[0] == version)
                        {
                            EditorKit.ShowTips($"当前架构版本为最新版本{version}");
                        }
                        else
                        {
                            mPath = E_Path.Custom;
                            inputText = "https://github.com/pantyneko/MeowFramework";
                            TextDialog.Show($"当前架构版本为:{version},最新版本为:{res[0]},请点击 >>> 打开路径 <<< 按钮访问最新版本{6}");
                        }
                        IsAsync = false;
                    }
                }
                catch (TaskCanceledException e)
                {
                    EditorKit.ShowTips(e.CancellationToken.IsCancellationRequested ? "请求被用户取消。" : "请求超时!");
                    IsAsync = false;
                }
                catch (HttpRequestException e)
                {
                    EditorKit.ShowTips($"请求错误: {e.Message}");
                    IsAsync = false;
                }
            }
        }
        private bool CheckInputLegal()
        {
            if (EditorKit.ShowDialog("真的要这么做嘛？喵!!"))
            {
                // 如果输入框锁住 不调用
                if (mDisabledInputBox)
                {
                    EditorKit.ShowTips("输入框为锁定状态,解锁后重试");
                    return false;
                }
                // 如果文本为空 不调用
                if (string.IsNullOrEmpty(inputText))
                {
                    EditorKit.ShowTips("请在面板上正确输入类型名");
                    return false;
                }
                int index = inputText.GetSpecialCharsCount();
                if (index >= 0)
                {
                    EditorKit.ShowTips($"编辑器名字有特殊符号\"{inputText[index]}\"");
                    return false;
                }
                return true;
            }
            return false;
        }
        private void CreatScript(string tag, string tmple)
        {
            string path = $"{Application.dataPath}/{inputText}{tag}.cs";
            // 查找是否存在同名脚本 如果存在就跳出
            if (File.Exists(path))
            {
                EditorKit.ShowTips("脚本已存在");
                return;
            }
            File.WriteAllText(path, Regex.Replace(tmple, Regex.Escape("@"), inputText));
            AssetDatabase.Refresh();
        }
    }
}