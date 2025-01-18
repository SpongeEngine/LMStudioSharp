using System.Text.Json.Serialization;

namespace SpongeEngine.LMStudioSharp.Models.Model
{
    public class Model
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;
    
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    
        [JsonPropertyName("publisher")]
        public string Publisher { get; set; } = string.Empty;
    
        [JsonPropertyName("arch")]
        public string Architecture { get; set; } = string.Empty;
    
        [JsonPropertyName("compatibility_type")]
        public string CompatibilityType { get; set; } = string.Empty;
    
        [JsonPropertyName("quantization")]
        public string Quantization { get; set; } = string.Empty;
    
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;
    
        [JsonPropertyName("max_context_length")]
        public int MaxContextLength { get; set; }
    }
}