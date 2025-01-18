using System.Text.Json.Serialization;

namespace SpongeEngine.LMStudioSharp.Models.Base
{
    public class Runtime
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    
        [JsonPropertyName("supported_formats")]
        public List<string> SupportedFormats { get; set; } = new();
    }
}