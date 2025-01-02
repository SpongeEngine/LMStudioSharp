using Newtonsoft.Json;

namespace SpongeEngine.LMStudioSharp.Models.Base
{
    public class LmStudioRuntime
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;
    
        [JsonProperty("supported_formats")]
        public List<string> SupportedFormats { get; set; } = new();
    }
}