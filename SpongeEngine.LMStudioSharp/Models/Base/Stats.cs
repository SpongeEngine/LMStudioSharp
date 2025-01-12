using Newtonsoft.Json;

namespace SpongeEngine.LMStudioSharp.Models.Base
{
    public class Stats 
    {
        [JsonProperty("tokens_per_second")]
        public double TokensPerSecond { get; set; }
    
        [JsonProperty("time_to_first_token")]
        public double TimeToFirstToken { get; set; }
    
        [JsonProperty("generation_time")]
        public double GenerationTime { get; set; }
    
        [JsonProperty("stop_reason")]
        public string StopReason { get; set; } = string.Empty;
    }
}