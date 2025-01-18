using System.Text.Json.Serialization;

namespace SpongeEngine.LMStudioSharp.Models.Base
{
    public class ModelInfo
    {
        [JsonPropertyName("arch")]
        public string Architecture { get; set; } = string.Empty;
    
        [JsonPropertyName("quant")]
        public string Quantization { get; set; } = string.Empty;
    
        [JsonPropertyName("format")]
        public string Format { get; set; } = string.Empty;
    
        [JsonPropertyName("context_length")]
        public int ContextLength { get; set; }
    }

}