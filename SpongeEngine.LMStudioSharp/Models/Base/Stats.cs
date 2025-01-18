using System.Text.Json.Serialization;

namespace SpongeEngine.LMStudioSharp.Models.Base
{
    public class Stats 
    {
        [JsonPropertyName("tokens_per_second")]
        public double TokensPerSecond { get; set; }
    
        [JsonPropertyName("time_to_first_token")]
        public double TimeToFirstToken { get; set; }
    
        [JsonPropertyName("generation_time")]
        public double GenerationTime { get; set; }
    
        [JsonPropertyName("stop_reason")]
        public string StopReason { get; set; } = string.Empty;
    }
}