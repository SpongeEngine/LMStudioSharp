using System.Text.Json.Serialization;

namespace SpongeEngine.LMStudioSharp.Models.Chat
{
    public class ChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new();
    
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; } = 0.7f;
    
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = -1;
    
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }
}