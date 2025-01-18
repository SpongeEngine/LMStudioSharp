using System.Text.Json.Serialization;

namespace SpongeEngine.LMStudioSharp.Models.Completion
{
    public class CompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;
    
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = -1;
    
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; } = 0.7f;
    
        [JsonPropertyName("top_p")]
        public float TopP { get; set; } = 0.9f;
    
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    
        [JsonPropertyName("stop")]
        public string[]? Stop { get; set; }
    }
}