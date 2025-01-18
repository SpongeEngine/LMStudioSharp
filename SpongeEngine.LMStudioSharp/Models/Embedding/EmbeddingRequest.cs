using System.Text.Json.Serialization;

namespace SpongeEngine.LMStudioSharp.Models.Embedding
{
    public class EmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonPropertyName("input")]
        public string Input { get; set; } = string.Empty;
    }

}