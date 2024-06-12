using System;
using UnityEditor;
using UnityEngine;

namespace Panty
{
    public class MeowEditor : PnEditor
    {
        protected string NameSpace => "Panty.Test";
        [MenuItem("PnTool/MeowEditor &1")]
        private static void OpenSelf() => EditorKit.ShowOrHide<MeowEditor>(out var _);
        protected override (string, Action)[] InitBtnInfo()
        {
            return new (string, Action)[]
            {
                ("创建脚本",() =>
                {
                    if (CheckInputLegal())
                    {
                        EditorKit.CreatScript(inputText,"", $"using UnityEngine;\r\n\r\nnamespace {NameSpace}\r\n{{\r\n    public class @ : MonoBehaviour\r\n    {{\r\n\r\n    }}\r\n}}");
                    }
                }),
                ("创建编辑器",()=>
                {
                    if (CheckInputLegal())
                    {
                        EditorKit.CreatScript(inputText,"Editor", $"using UnityEditor;\r\n\r\nnamespace {NameSpace}\r\n{{\r\n    public class @Editor : PnEditor<@Editor.E_Type>\r\n    {{\r\n        public enum E_Type : byte {{ Empty }}\r\n\r\n        [MenuItem(\"PnTool/@Editor\")]\r\n        public static void ShowWindow()\r\n        {{\r\n            GetWindow<@Editor>(\"@Editor\", true);\r\n        }}\r\n        protected override E_Type Empty => E_Type.Empty;\r\n    }}\r\n}}");
                    }
                }),
                ("创建Hub类",() =>
                {
                    if (CheckInputLegal())
                    {
                        string code = $"using UnityEngine;\r\n\r\nnamespace {NameSpace}\r\n{{\r\n    public class @Hub : ModuleHub<@Hub>\r\n    {{\r\n        protected override void BuildModule()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n    public class @Game : MonoBehaviour, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n    public class @UI : UIPanel, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n}}";
                        EditorKit.CreatScript(inputText,"Hub",code);
                    }
                }),
                ("创建模块",() =>
                {
                    if (CheckInputLegal())
                    {
                        string code = $"namespace {NameSpace}\r\n{{\r\n    public interface I@Module : IModule\r\n    {{\r\n\r\n    }}\r\n    public class @Module : AbsModule, I@Module\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
                        EditorKit.CreatScript(inputText, "Module",code);
                    }
                }),
                ("QF_创建架构",() =>
                {
                    if (CheckInputLegal())
                    {
                        EditorKit.CreatScript(inputText, "Game",$"using UnityEngine;\r\n\r\nnamespace {NameSpace}\r\n{{\r\n    public class @Game : Architecture<@Game>\r\n    {{\r\n        protected override void Init()\r\n        {{\r\n            // 注册模块\r\n        }}\r\n    }}\r\n    public class @GameController : MonoBehaviour, IController\r\n    {{\r\n        IArchitecture IBelongToArchitecture.GetArchitecture() => @Game.Interface;\r\n    }}\r\n    public class @UIController : UIPanel, IController\r\n    {{\r\n        IArchitecture IBelongToArchitecture.GetArchitecture() => @Game.Interface;\r\n    }}\r\n}}");
                    }
                }),
                ("QF_创建系统",() =>
                {
                    if (CheckInputLegal())
                    {
                        EditorKit.CreatScript(inputText, "System", $"namespace {NameSpace}\r\n{{\r\n    public interface I@System : ISystem\r\n    {{\r\n\r\n    }}\r\n    public class @System : AbstractSystem, I@System\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}");
                    }
                }),
                ("QF_创建数据模型",() =>
                {
                    if (CheckInputLegal())
                    {
                        EditorKit.CreatScript(inputText, "Model", $"namespace {NameSpace}\r\n{{\r\n    public interface I@Model : IModel\r\n    {{\r\n\r\n    }}\r\n    public class @Model : AbstractModel, I@Model\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}");
                    }
                }),
                ("检查更新",() =>
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
                            mPath = E_Path.UpdateFK;
                            TextDialog.Open($"当前版本：{version}\r\n最新版本：{res[0]}\r\n\r\n1：点击【打开路径按钮】= 访问最新版本\r\n2：右键【拉取核心代码】= 进行快速更新\r\n{res[1]}");
                        }
                    });
                }),
            };
        }
        private bool CheckInputLegal()
        {
            if (EditorKit.Dialog("真的要这么做嘛？喵!!"))
            {
                // 如果输入框锁住 不调用
                if (mDisabledInputBox)
                {
                    EditorKit.Tips("输入框为锁定状态,解锁后重试");
                    return false;
                }
                // 如果文本为空 不调用
                if (string.IsNullOrEmpty(inputText))
                {
                    EditorKit.Tips("请在面板上正确输入类型名");
                    return false;
                }
                if (inputText.ContainsSpecialSymbols())
                {
                    EditorKit.Tips($"编辑器名字有特殊符号");
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}