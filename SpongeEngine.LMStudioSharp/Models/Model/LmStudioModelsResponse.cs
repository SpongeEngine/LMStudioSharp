using Newtonsoft.Json;

namespace SpongeEngine.LMStudioSharp.Models.Model
{
    public class LmStudioModelsResponse
    {
        [JsonProperty("object")]
        public string Object { get; set; } = "list";
    
        [JsonProperty("data")]
        public List<LmStudioModel> Data { get; set; } = new();
    }
}