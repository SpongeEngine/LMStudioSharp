using Newtonsoft.Json;

namespace SpongeEngine.LMStudioSharp.Models.Model
{
    public class ModelsResponse
    {
        [JsonProperty("object")]
        public string Object { get; set; } = "list";
    
        [JsonProperty("data")]
        public List<Model> Data { get; set; } = new();
    }
}