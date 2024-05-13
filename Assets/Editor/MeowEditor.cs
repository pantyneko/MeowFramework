using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Panty
{
    public class MeowEditor : PnEditor<MeowEditor.E_Type>
    {
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
                        string code = $"using UnityEngine;\r\n\r\nnamespace {NameSpace}\r\n{{\r\n    public class @Hub : ModuleHub<@Hub>\r\n    {{\r\n        protected override void BuildModule()\r\n        {{\r\n            MonoKit.GetIns().OnDeInit += Deinit;\r\n        }}\r\n    }}\r\n    public class @Game : MonoBehaviour, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n    public class @UI : UIPanel, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n}}";
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
            };
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