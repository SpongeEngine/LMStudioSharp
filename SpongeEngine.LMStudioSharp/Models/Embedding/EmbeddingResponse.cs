using System.Text.Json.Serialization;
using SpongeEngine.LMStudioSharp.Models.Base;

namespace SpongeEngine.LMStudioSharp.Models.Embedding
{
    public class EmbeddingResponse
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = "list";
    
        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; set; } = new();
    
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonPropertyName("usage")]
        public Usage Usage { get; set; } = new();
    
        public class EmbeddingData
        {
            [JsonPropertyName("object")]
            public string Object { get; set; } = "embedding";
        
            [JsonPropertyName("embedding")]
            public float[] Embedding { get; set; } = Array.Empty<float>();
        
            [JsonPropertyName("index")]
            public int Index { get; set; }
        }
    }
}