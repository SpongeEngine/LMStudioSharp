using Newtonsoft.Json;

namespace LocalAI.NET.LMStudio.Models.Chat
{
    public class LmStudioChatMessageResponse
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string? Content { get; set; } = string.Empty;
    }
}