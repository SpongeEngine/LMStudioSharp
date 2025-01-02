using Newtonsoft.Json;

namespace SpongeEngine.LMStudioSharp.Models.Completion
{
    public class LmStudioCompletionRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonProperty("prompt")]
        public string Prompt { get; set; } = string.Empty;
    
        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = -1;
    
        [JsonProperty("temperature")]
        public float Temperature { get; set; } = 0.7f;
    
        [JsonProperty("top_p")]
        public float TopP { get; set; } = 0.9f;
    
        [JsonProperty("stream")]
        public bool Stream { get; set; }
    
        [JsonProperty("stop")]
        public string[]? Stop { get; set; }
    }
}