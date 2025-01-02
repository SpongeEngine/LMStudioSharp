using Newtonsoft.Json;

namespace SpongeEngine.LMStudioSharp.Models.Chat
{
    public class LmStudioChatMessageResponse
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string? Content { get; set; } = string.Empty;
    }
}