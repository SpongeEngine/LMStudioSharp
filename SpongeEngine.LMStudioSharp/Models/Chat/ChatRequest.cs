using Newtonsoft.Json;

namespace SpongeEngine.LMStudioSharp.Models.Chat
{
    public class ChatRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonProperty("messages")]
        public List<ChatMessage> Messages { get; set; } = new();
    
        [JsonProperty("temperature")]
        public float Temperature { get; set; } = 0.7f;
    
        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = -1;
    
        [JsonProperty("stream")]
        public bool Stream { get; set; }
    }
}