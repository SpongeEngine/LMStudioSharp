using Newtonsoft.Json;

namespace LocalAI.NET.LMStudio.Models.Embedding
{
    public class LmStudioEmbeddingRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonProperty("input")]
        public string Input { get; set; } = string.Empty;
    }

}