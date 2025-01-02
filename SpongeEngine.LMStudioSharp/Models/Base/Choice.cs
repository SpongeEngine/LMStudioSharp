using Newtonsoft.Json;
using SpongeEngine.LMStudioSharp.Models.Chat;

namespace SpongeEngine.LMStudioSharp.Models.Base
{
    public class Choice
    {
        [JsonProperty("index")]
        public int? Index { get; set; }

        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("message")]
        public MessageResponse? Message { get; set; }

        [JsonProperty("logprobs")]
        public object? LogProbs { get; set; }

        [JsonProperty("finish_reason")]
        public string? FinishReason { get; set; }

        // Helper property to get the text content regardless of type
        public string GetText() => Text ?? Message?.Content ?? string.Empty;
    }
}