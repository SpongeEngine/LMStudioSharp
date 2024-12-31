using Newtonsoft.Json;

namespace LocalAI.NET.LMStudio.Models.Chat
{
    public class LmStudioChatRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonProperty("messages")]
        public List<LmStudioChatMessage> Messages { get; set; } = new();
    
        [JsonProperty("temperature")]
        public float Temperature { get; set; } = 0.7f;
    
        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = -1;
    
        [JsonProperty("stream")]
        public bool Stream { get; set; }
    }
}