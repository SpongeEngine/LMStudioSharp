using System.Text.Json.Serialization;
using SpongeEngine.LMStudioSharp.Models.Chat;

namespace SpongeEngine.LMStudioSharp.Models.Base
{
    public class Choice
    {
        [JsonPropertyName("index")]
        public int? Index { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("message")]
        public MessageResponse? Message { get; set; }

        [JsonPropertyName("logprobs")]
        public object? LogProbs { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }

        // Helper property to get the text content regardless of type
        public string GetText() => Text ?? Message?.Content ?? string.Empty;
    }
}