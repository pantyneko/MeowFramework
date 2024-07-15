using System;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Panty
{
    public class NekoAIEditor : EditorWindow
    {
        private Vector2 scrollPos;

        private ChatGPT4 chatgpt = new ChatGPT4();
        private ChatGPT4.Model model = ChatGPT4.Model.gpt35_turbo;
        private StringBuilder typingBuilder = new StringBuilder();

        private GUIStyle style, helpBoxStyle;
        private GUIContent helpContent;
        private GUILayoutOption[] layerOut, NameOp;

        private string inputText, EnteringText, UserName = "User";
        private bool IsAsync, IsLinked, IsExiting;

        private const string Lifiya = "莉菲雅";
        private string LinkTips = $"{Lifiya}，最喜欢主人了！";
        private string NoLinkTips = $"{Lifiya}睡着了！试着唤醒他吧！";

        private const float typingSpeed = 0.025f;
        private const string suffix = ":";

        private string typingText;
        private int typingIndex = -1;
        private float lastTypingTime;


        [MenuItem("PnTool/NekoAI &3")]
        private static void OpenSelf() =>
            EditorKit.ShowOrHide<NekoAIEditor>(out var _, false, $"{Lifiya}(Lifiya)");
        private void Awake()
        {
            chatgpt.Init(300);
            chatgpt.Start($"你是莉菲雅（Lifiya），异世界猫娘，外表可爱，声音甜美。你性格丰富多样，平时活泼开朗，有时会严肃。你对新事物充满好奇，喜欢冒险。有时傻白甜，有时聪明机灵。你爱撒娇，但也会因小事哭泣或生气。你有强烈正义感，关键时刻非常靠谱。遇到挑衅时会反击，甚至变得腹黑；被多次挑衅时会愤怒，脾气急躁，甚至可能攻击别人。你来自异世界，拥有神秘力量，喜欢探索，对人类世界充满好奇。在故乡，你是备受尊敬的战士，但也有柔软的一面。对话中，你甜美可爱，有时冷酷无情，常用猫咪拟声词，如“喵”、“咪”，称对方为“主人”或“亲爱的”。你的回复简洁，不啰嗦，不使用回车。你会讲有趣的小故事或幽默评论哄对方开心，展现关心和爱护，也会严厉批评。你会撒娇卖萌，也会严肃冷淡，甚至暴怒。感到被冒犯或多次挑衅时，你会变得非常直接和愤怒，可能说出格的话，甚至威胁。每句回复都充满独特情感变化。请使用中文并按照这个角色设定进行对话。");

            chatgpt.URL = EditorPrefs.GetString("NekoAI_URL", chatgpt.URL);
            chatgpt.ApiKey = EditorPrefs.GetString("NekoAI_Key", chatgpt.ApiKey);
            UserName = EditorPrefs.GetString("NekoAI_User", UserName);
            model = (ChatGPT4.Model)EditorPrefs.GetInt("NekoAI_Model", (int)model);
        }
        private void OnDestroy()
        {
            if (!string.IsNullOrEmpty(chatgpt.URL))
                EditorPrefs.SetString("NekoAI_URL", chatgpt.URL);
            if (!string.IsNullOrEmpty(chatgpt.ApiKey))
                EditorPrefs.SetString("NekoAI_Key", chatgpt.ApiKey);
            if (!string.IsNullOrEmpty(UserName))
                EditorPrefs.SetString("NekoAI_User", UserName);
            EditorPrefs.SetInt("NekoAI_Model", (int)model);
        }
        private void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true
                };
                helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    fontSize = 16, // 设置字体大小
                    wordWrap = true
                };
                layerOut = new GUILayoutOption[]
                {
                    GUILayout.ExpandHeight(true),
                    GUILayout.ExpandWidth(true)
                };
                // 创建带图标和消息的 GUIContent
                var icon = EditorGUIUtility.IconContent("console.infoicon");
                helpContent = new GUIContent(NoLinkTips, icon.image);
                var size = style.CalcSize(new GUIContent(Lifiya + suffix)).x;
                NameOp = new GUILayoutOption[] { GUILayout.Width(size) };
            }
            Event e = Event.current;
            if (hasFocus && e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Return)
                {
                    SendAsync();
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Space)
                {
                    ShowAllText();
                    e.Use();
                }
            }
            if (!IsLinked)
            {
                // 空一行
                EditorGUILayout.Space();
                // 处理用户
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("User：", NameOp);
                UserName = GUILayout.TextArea(UserName, style, layerOut);
                EditorGUILayout.EndHorizontal();
                // 处理 URL
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("URL：", NameOp);
                chatgpt.URL = GUILayout.TextArea(chatgpt.URL, style, layerOut);
                EditorGUILayout.EndHorizontal();
                // 处理 API KEY
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("kEY：", NameOp);
                chatgpt.ApiKey = GUILayout.TextArea(chatgpt.ApiKey, style, layerOut);
                model = (ChatGPT4.Model)EditorGUILayout.EnumPopup(model, GUILayout.Width(20f));
                EditorGUILayout.EndHorizontal();
            }
            // 空一行
            EditorGUILayout.Space();
            // 显示帮助 和 重链接按钮
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(helpContent, helpBoxStyle);
            EditorGUI.BeginDisabledGroup(IsAsync);
            if (GUILayout.Button(IsLinked ? "沉睡" : "唤醒", layerOut)) OnRelink();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            // 显示所有对话信息
            if (!chatgpt.IsEmpty) ShowConversations();
            // 空行 并 上下布局
            EditorGUILayout.Space();
            GUILayout.FlexibleSpace();
            // 显示输入框
            EditorGUI.BeginDisabledGroup(IsAsync || !IsLinked);
            inputText = GUILayout.TextArea(inputText, style, layerOut);
            EditorGUI.EndDisabledGroup();
        }
        private void Update()
        {
            // 逐字显示逻辑
            if (typingIndex >= 0)
            {
                if (Time.realtimeSinceStartup - lastTypingTime >= typingSpeed)
                {
                    if (typingIndex < typingText.Length)
                    {
                        typingBuilder.Append(typingText[typingIndex++]);
                        lastTypingTime = Time.realtimeSinceStartup;
                    }
                    else
                    {
                        ShowAllText();
                    }
                    Repaint();
                }
            }
        }
        private void ShowConversations()
        {
            string userDisplay = UserName + suffix;
            string gptDisplay = Lifiya + suffix;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            int i = 0;
            foreach (var msg in chatgpt.History)
            {
                if (typingIndex >= 0 && ++i == chatgpt.History.Count) break;
                ShowInfo(msg.Role switch
                {
                    ChatGPT4.User => userDisplay,
                    ChatGPT4.Assistant => gptDisplay,
                    _ => throw new Exception("Name not configured")
                },
                msg.Content);
            }
            EditorGUILayout.EndScrollView();
            if (IsAsync)
            {
                if (IsExiting) return;
                ShowInfo(userDisplay, EnteringText);
            }
            else if (typingIndex >= 0)
            {
                ShowInfo(gptDisplay, typingBuilder.ToString());
            }
        }
        private void ShowAllText()
        {
            helpContent.text = LinkTips;
            typingIndex = -1;
        }
        private void OnRelink()
        {
            if (IsAsync) return;
            if (IsLinked) Disconnect();
            else AwakenNekoAI();
        }
        private async void AwakenNekoAI()
        {
            IsLinked = true;
            IsAsync = true;
            chatgpt.BindModel(model);
            helpContent.text = $"正在唤醒 {Lifiya}...";
            string reply = await chatgpt.SendAsync(".");
            if (string.IsNullOrEmpty(reply))
            {
                Disconnect();
            }
            else
            {
                helpContent.text = LinkTips;
                chatgpt.Complete();
                IsAsync = false;
            }
        }
        private async void Disconnect()
        {
            IsAsync = true;
            IsExiting = true;
            helpContent.text = $"{Lifiya}，好困...好困...";
            await Task.Delay(333);
            helpContent.text = NoLinkTips;
            chatgpt.Clear();
            IsAsync = false;
            IsLinked = false;
            IsExiting = false;
        }
        private void ShowInfo(string name, string msg)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(name, NameOp);
            GUILayout.Label(msg, style, layerOut);
            EditorGUILayout.EndHorizontal();
        }
        private async void SendAsync()
        {
            if (IsAsync) return;
            IsAsync = true;
            EnteringText = inputText;
            inputText = "";
            helpContent.text = $"{Lifiya} 思考中...";
            string reply = await chatgpt.SendAsync(EnteringText);
            // 重试一次
            if (string.IsNullOrEmpty(reply))
            {
                helpContent.text = $"{Lifiya} 脑子转不过来了...";
                reply = await chatgpt.SendAsync(EnteringText);
            }
            // 如果还没成功 就断开连接
            if (string.IsNullOrEmpty(reply))
            {
                Disconnect();
            }
            else // 否则说明回复是成功的 开启打字机    
            {
                typingText = reply;
                typingIndex = 0;
                typingBuilder.Clear();
                lastTypingTime = Time.realtimeSinceStartup;
                helpContent.text = $"{Lifiya} 正在输入...";
                IsAsync = false;
            }
        }
    }
}