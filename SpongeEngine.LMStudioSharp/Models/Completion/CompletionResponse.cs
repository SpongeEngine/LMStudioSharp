using Newtonsoft.Json;
using SpongeEngine.LMStudioSharp.Models.Base;

namespace SpongeEngine.LMStudioSharp.Models.Completion
{
    public class CompletionResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
    
        [JsonProperty("object")]
        public string Object { get; set; } = string.Empty;
    
        [JsonProperty("created")]
        public long Created { get; set; }
    
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; } = new();
    
        [JsonProperty("usage")]
        public Usage Usage { get; set; } = new();
    
        [JsonProperty("stats")]
        public Stats Stats { get; set; } = new();
    
        [JsonProperty("model_info")]
        public ModelInfo ModelInfo { get; set; } = new();
    
        [JsonProperty("runtime")]
        public Runtime Runtime { get; set; } = new();
    }
}