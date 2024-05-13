using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Panty
{
    public abstract class PnEditor<T> : EditorWindow where T : Enum
    {
        protected enum E_Path : byte
        {
            PersistentDataPath,
            StreamingAssetsPath,
            TemporaryCachePath,
            DesktopPath,
            DocumentsPath,
            DataPath,
            SelectPath,
            Custom,
        }
        protected string inputText = "Welcome!";

        private T modes;
        private E_Path mPath;
        protected bool mIsShowBtn = false, mShowBaseInfo, mDisabledInputBox = true, mCanInit = true;

        private (string name, Action call)[] btnInfos;

        private GUIContent[] mMenuItemContent;
        private GenericMenu.MenuFunction[] mMenuItemFunc;

        private const float textSpacing = 8f;
        private const byte MaxLineItemCount = 4;

        protected abstract T Empty { get; }
        protected virtual void ExecuteMode(T mode) { }
        private void OnEnable()
        {
            mCanInit = true;
        }
        private void Update()
        {
            if (EqualityComparer<T>.Default.Equals(modes, Empty)) return;
            ExecuteMode(modes);
        }
        private void OnInspectorUpdate()
        {
            if (EditorApplication.isPlaying) Repaint();
        }
        private void OnGUI()
        {
            if (mCanInit)
            {
                btnInfos = InitBtnInfo();
                var menu = RightClickMenu();
                if (menu != null && menu.Length > 0)
                {
                    mMenuItemContent = new GUIContent[menu.Length];
                    mMenuItemFunc = new GenericMenu.MenuFunction[menu.Length];
                    for (int i = 0; i < menu.Length; i++)
                    {
                        mMenuItemContent[i] = new GUIContent(menu[i].name);
                        mMenuItemFunc[i] = menu[i].call;
                    }
                }
                float len = btnInfos == null ? 0f : btnInfos.Length;
                // 计算最小窗口宽度为按钮宽度的总和加上间隔，再加上额外宽度
                float buttonWidth = GUI.skin.button.CalcSize(new GUIContent("0000000000")).x;
                float minWindowWidth = MaxLineItemCount * buttonWidth + (MaxLineItemCount - 1) * textSpacing;
                float btnLine = MathF.Ceiling(len / MaxLineItemCount);
                float minWindowHeight = 300 + (btnLine + 1) * 10;
                // 设置初始窗口位置为屏幕中央
                float x = Screen.currentResolution.width * 0.3f;
                float y = Screen.currentResolution.height * 0.1f;
                position = new Rect(x, y, minWindowWidth, minWindowHeight);
                mCanInit = false;
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            mIsShowBtn = GUILayout.Toggle(mIsShowBtn, "显示功能按钮");
            mDisabledInputBox = GUILayout.Toggle(mDisabledInputBox, "禁用输入框");
            mShowBaseInfo = GUILayout.Toggle(mShowBaseInfo, "基础信息");
            modes = (T)EditorGUILayout.EnumPopup(modes);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(mDisabledInputBox);
            var op = GUILayout.MaxHeight(18);
            inputText = EditorGUILayout.TextField(inputText, op);
            EditorGUI.EndDisabledGroup();
            mPath = (E_Path)EditorGUILayout.EnumPopup(mPath);
            EditorGUILayout.EndHorizontal();

            if (Event.current.type == EventType.ContextClick)
            {
                // 创建右键菜单
                var menu = new GenericMenu();
                if (mMenuItemFunc != null)
                {
                    for (int i = 0; i < mMenuItemFunc.Length; i++)
                    {
                        menu.AddItem(mMenuItemContent[i], false, mMenuItemFunc[i]);
                    }
                }
                menu.AddItem(new GUIContent("创建基础目录"), false, () =>
                {
                    string[] fileNames = { "ArtRes", "Editor", "Presets", "Resources", "Scenes", "Framework", "Project", "StreamingAssets" };
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        string path = Application.dataPath + "/" + fileNames[i];
                        FileKit.TryCreateDirectory(path);
                    }
                    AssetDatabase.Refresh();
                });
                // 显示右键菜单
                menu.ShowAsContext();
                Event.current.Use();
            }

            EditorGUILayout.BeginHorizontal();
            if (OnClick("重置状态"))
            {
                mIsShowBtn = true;
                mDisabledInputBox = true;
                mShowBaseInfo = false;
                modes = Empty;
                inputText = "状态已重置!";
            }
            else if (OnClick("打开文件夹"))
            {
                inputText = mPath switch
                {
                    E_Path.DataPath => Application.dataPath,
                    E_Path.PersistentDataPath => Application.persistentDataPath,
                    E_Path.StreamingAssetsPath => Application.streamingAssetsPath,
                    E_Path.TemporaryCachePath => Application.temporaryCachePath,
                    E_Path.DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    E_Path.DocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    E_Path.SelectPath => EditorKit.GetSelectionFolder(),
                    _ => mDisabledInputBox ? "https://gitee.com/PantyNeko" : inputText,
                };
                try
                {
                    if (string.IsNullOrEmpty(inputText))
                    {
                        inputText = "路径为空";
                    }
                    else
                    {
                        System.Diagnostics.Process.Start(inputText);
                    }
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    inputText = ex.Message;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (mIsShowBtn) ShowBtn(btnInfos);

            ExtensionControl();

            if (mShowBaseInfo)
            {
                EditorGUILayout.Space();
                GUILayout.Label($"世界 : {Camera.main.ScreenToWorldPoint(Input.mousePosition)}");
                GUILayout.Label($"视口 : {Camera.main.ScreenToViewportPoint(Input.mousePosition)}");
                GUILayout.Label($"屏幕 : {Input.mousePosition}");
                GUILayout.Label("帧率 : " + (1F / Time.deltaTime).ToString("F0"));
            }
        }
        private void ShowBtn((string name, Action call)[] btns)
        {
            if (btns == null) return;
            int row = btns.Length / MaxLineItemCount;
            if (btns.Length > MaxLineItemCount)
            {
                // 如果按钮的数量大于一行 调整按钮行布局，三行按钮
                for (byte r = 0; r < row; r++)
                {
                    EditorGUILayout.BeginHorizontal();
                    int index = r * MaxLineItemCount;
                    for (byte i = 0; i < MaxLineItemCount; i++)
                    {
                        var pair = btns[i + index];
                        if (OnClick(pair.name))
                        {
                            pair.call();
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.BeginHorizontal();
            int num = btns.Length % MaxLineItemCount;
            int max = row * MaxLineItemCount;

            for (int i = 0; i < num; i++)
            {
                var pair = btns[i + max];
                if (OnClick(pair.name))
                {
                    pair.call();
                    break;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        protected virtual void ExtensionControl() { }
        protected virtual (string name, GenericMenu.MenuFunction call)[] RightClickMenu() => null;
        protected virtual (string, Action)[] InitBtnInfo() => null;
        protected bool OnClick(string name)
        {
            return GUILayout.Button(name, GUILayout.Height(30)) && Event.current.button == 0;
        }
    }
}