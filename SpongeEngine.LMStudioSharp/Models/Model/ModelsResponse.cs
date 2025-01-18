using System.Text.Json.Serialization;

namespace SpongeEngine.LMStudioSharp.Models.Model
{
    public class ModelsResponse
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = "list";
    
        [JsonPropertyName("data")]
        public List<Model> Data { get; set; } = new();
    }
}