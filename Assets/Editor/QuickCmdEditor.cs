using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Panty
{
    public class QuickCmdEditor : EditorWindow
    {
        private string inputText;

        [MenuItem("PnTool/QuickCmd &Q")]
        private static void OpenSelf()
        {
            var window = GetWindow<QuickCmdEditor>("QuickCmdEditor");
            window.maxSize = window.minSize = new Vector2(820f, 80f);
        }
        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));

            var instructionLabel = new Label
            {
                text = "[@]-特殊 [s]-选择 [#]-创建 [f/p]-文件/路径 [:]-分隔符(指示功能) [Name]-文件或文件夹名 [hub]-架构 [module]-模块 [mono]-脚本 [cs]-类",
                style =
                {
                    fontSize = 12,
                    color = new StyleColor(Color.gray),
                    unityTextAlign = TextAnchor.MiddleCenter,
                }
            };
            var msgLabel = new Label
            {
                text = "使用 [#:f:Project:hub] 创建 ProjectHub 架构基类",
                style =
                {
                    fontSize = 12,
                    color = new StyleColor(Color.gray),
                    unityTextAlign = TextAnchor.MiddleCenter,
                }
            };
            // 创建并设置TextField样式
            var inputField = new TextField
            {
                value = inputText,
                style =
                {
                    fontSize = 24,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    height = 40,
                    flexGrow = 1
                }
            };
            inputField.RegisterCallback<ChangeEvent<string>>(evt => inputText = evt.newValue);
            // 将TextField添加到根元素
            root.Add(inputField);
            root.Add(instructionLabel);
            root.Add(msgLabel);
        }
    }
}