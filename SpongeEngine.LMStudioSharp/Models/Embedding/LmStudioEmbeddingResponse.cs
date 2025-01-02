using Newtonsoft.Json;
using SpongeEngine.LMStudioSharp.Models.Base;

namespace SpongeEngine.LMStudioSharp.Models.Embedding
{
    public class LmStudioEmbeddingResponse
    {
        [JsonProperty("object")]
        public string Object { get; set; } = "list";
    
        [JsonProperty("data")]
        public List<EmbeddingData> Data { get; set; } = new();
    
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;
    
        [JsonProperty("usage")]
        public LmStudioUsage Usage { get; set; } = new();
    
        public class EmbeddingData
        {
            [JsonProperty("object")]
            public string Object { get; set; } = "embedding";
        
            [JsonProperty("embedding")]
            public float[] Embedding { get; set; } = Array.Empty<float>();
        
            [JsonProperty("index")]
            public int Index { get; set; }
        }
    }
}