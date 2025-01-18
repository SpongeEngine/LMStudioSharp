using System.Text.Json.Serialization;
using SpongeEngine.LMStudioSharp.Models.Base;

namespace SpongeEngine.LMStudioSharp.Models.Completion
{
    public class CompletionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;
    
        [JsonPropertyName("created")]
        public long Created { get; set; }
    
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; } = new();
    
        [JsonPropertyName("usage")]
        public Usage Usage { get; set; } = new();
    
        [JsonPropertyName("stats")]
        public Stats Stats { get; set; } = new();
    
        [JsonPropertyName("model_info")]
        public ModelInfo ModelInfo { get; set; } = new();
    
        [JsonPropertyName("runtime")]
        public Runtime Runtime { get; set; } = new();
    }
}