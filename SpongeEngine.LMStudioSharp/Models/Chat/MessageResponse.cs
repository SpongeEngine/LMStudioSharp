using System.Text.Json.Serialization;

namespace SpongeEngine.LMStudioSharp.Models.Chat
{
    public class MessageResponse
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string? Content { get; set; } = string.Empty;
    }
}