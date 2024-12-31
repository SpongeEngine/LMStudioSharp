using LocalAI.NET.LMStudio.Models.Base;
using Newtonsoft.Json;

namespace LocalAI.NET.LMStudio.Models.Completion
{
    public class LmStudioCompletionResponse
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
        public List<LmStudioChoice> Choices { get; set; } = new();
    
        [JsonProperty("usage")]
        public LmStudioUsage Usage { get; set; } = new();
    
        [JsonProperty("stats")]
        public LmStudioStats Stats { get; set; } = new();
    
        [JsonProperty("model_info")]
        public LmStudioModelInfo ModelInfo { get; set; } = new();
    
        [JsonProperty("runtime")]
        public LmStudioRuntime Runtime { get; set; } = new();
    }
}