using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace Panty
{
    [Serializable]
    public class ChatGPT4
    {
        public enum Model : byte
        {
            gpt35_turbo,
            gpt35_turbo_16k,
            gpt35_turbo_0125,
            gpt35_turbo_0301,
            gpt35_turbo_0613,
            gpt35_turbo_1106,
            gpt4_1106_preview,
            gpt4_0125_preview,
            gpt4,
            gpt4o,
            gpt4_all,
            dall_e2,
            dall_e3,
        }
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(16) // 设置超时时间
        };
        public const string User = "user", Assistant = "assistant";
        private Queue<ApiMsg> mHistory = new Queue<ApiMsg>();
        public Queue<ApiMsg> History => mHistory;
        public bool IsEmpty => mHistory.Count == 0;
        private string apiKey, url = "https://api.openai.com/v1/chat/completions";
        public string URL
        {
            get => url;
            set => url = value.Trim();
        }
        public string ApiKey
        {
            get => apiKey;
            set => apiKey = value.Trim();
        }
        private ApiMsg mSettingMessage;
        private PostData mPostData;

        private bool Inited;

        public void Start(string setting)
        {
            mSettingMessage = new ApiMsg { Role = "system", Content = setting };
        }
        /// <summary>
        /// 配置GPT模型和系统消息
        /// </summary>
        /// <param name="maxTokens">限制生成的最大 token 数 避免过长的响应</param>
        /// <param name="temperature">设置为较低值（如 0.5），减少随机性，生成更一致的文本</param>
        /// <param name="topP">设置为 0.9 或更低，限制模型考虑的词汇范围，生成更紧凑的文本</param>
        /// <param name="stop">设置停止词，确保模型在生成到特定点时停止，避免生成过多无用的文本</param>
        public void Init(int maxTokens = 300, double temperature = 0.75, double topP = 0.9, string[] stop = null)
        {
            mPostData = new PostData
            {
                Messages = null,
                MaxTokens = maxTokens,
                Temperature = temperature,
                TopP = topP,
                Stop = stop
            };
        }
        public void BindModel(Model model)
        {
            mPostData.Model = model switch
            {
                Model.gpt35_turbo => "gpt-3.5-turbo",
                Model.gpt35_turbo_16k => "gpt-3.5-turbo-16k",
                Model.gpt35_turbo_0125 => "gpt-3.5-turbo-0125",
                Model.gpt35_turbo_0301 => "gpt-3.5-turbo-0301",
                Model.gpt35_turbo_0613 => "gpt-3.5-turbo-0613",
                Model.gpt35_turbo_1106 => "gpt-3.5-turbo-1106",
                Model.gpt4_1106_preview => "gpt-4-1106-preview",
                Model.gpt4_0125_preview => "gpt-4-0125-preview",
                Model.gpt4 => "gpt-4",
                Model.gpt4o => "gpt-4o",
                Model.gpt4_all => "gpt-4-all",
                Model.dall_e2 => "dall-e-2",
                Model.dall_e3 => "dall-e-3",
                _ => "gpt-3.5-turbo"
            };
        }
        public void Complete()
        {
            Inited = true;
        }
        public void Clear()
        {
            mHistory.Clear();
            Inited = false;
        }
        /// <summary>
        /// 发送消息并接收响应
        /// </summary>
        /// <param name="userMessage">发送的消息</param>
        /// <returns>当前回复的语句</returns>
        public async Task<string> SendAsync(string userMessage)
        {
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(url)) return null;
            if (string.IsNullOrEmpty(userMessage)) return null;
            var userMsg = new ApiMsg { Role = User, Content = userMessage.Trim() };
            if (mHistory.Count > 16) mHistory.Dequeue();
            if (Inited)
            {
                var msgArr = new ApiMsg[mHistory.Count + 2];
                int i = 0;
                msgArr[i++] = mSettingMessage;
                foreach (var msg in mHistory)
                    msgArr[i++] = msg;
                msgArr[i++] = userMsg;
                mPostData.Messages = msgArr;
            }
            else
            {
                mPostData.Messages = new ApiMsg[] { mSettingMessage };
            }
            try
            {
                string jsonText = JsonConvert.SerializeObject(mPostData);
                var content = new StringContent(jsonText, Encoding.UTF8, "application/json");
                // 使用HttpRequestMessage设置请求
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", apiKey) },
                    Content = content
                };
                var response = await client.SendAsync(requestMessage);
                if (!response.IsSuccessStatusCode)
                {
#if DEBUG
                    $"Error Content: {await response.Content.ReadAsStringAsync()}".Log();
#endif
                    return null;
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                var responseMessage = JsonConvert.DeserializeObject<ResponseMessage>(responseBody);
                var modelReply = responseMessage?.Choices[0]?.Message.Content;
                // 添加用户输入到会话历史
                if (Inited) mHistory.Enqueue(userMsg);
                mHistory.Enqueue(new ApiMsg { Role = Assistant, Content = modelReply });
                return modelReply;
            }
            catch (HttpRequestException e)
            {
#if DEBUG
                $"Request error: {e.Message}".Log();
#endif
                return null;
            }
            catch (TaskCanceledException e)
            {
#if DEBUG
                $"Request timed out: {e.Message}".Log();
#endif
                return null;
            }
            catch (Exception e)
            {
#if DEBUG
                $"Unexpected error: {e.Message}".Log();
#endif
                return null;
            }
        }
        // 定义PostData类以便序列化请求数据
        private class PostData
        {
            [JsonProperty("model")]
            public string Model;
            [JsonProperty("messages")]
            public ApiMsg[] Messages;
            [JsonProperty("max_tokens")]
            public int MaxTokens;
            [JsonProperty("temperature")]
            public double Temperature;
            [JsonProperty("top_p")]
            public double TopP;
            [JsonProperty("stop")]
            public string[] Stop;
        }
        // 定义Message类以便序列化消息数据
        public class ApiMsg
        {
            [JsonProperty("role")]
            public string Role;
            [JsonProperty("content")]
            public string Content;
        }
        // 定义ResponseMessage类以便反序列化响应数据
        private class ResponseMessage
        {
            [JsonProperty("choices")]
            public Choice[] Choices;
        }
        private class Choice
        {
            [JsonProperty("message")]
            public ApiMsg Message;
        }
    }
}