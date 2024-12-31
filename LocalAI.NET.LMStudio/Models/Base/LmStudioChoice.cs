using LocalAI.NET.LMStudio.Models.Chat;
using Newtonsoft.Json;

namespace LocalAI.NET.LMStudio.Models.Base
{
    public class LmStudioChoice
    {
        [JsonProperty("index")]
        public int? Index { get; set; }

        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("message")]
        public LmStudioChatMessageResponse? Message { get; set; }

        [JsonProperty("logprobs")]
        public object? LogProbs { get; set; }

        [JsonProperty("finish_reason")]
        public string? FinishReason { get; set; }

        // Helper property to get the text content regardless of type
        public string GetText() => Text ?? Message?.Content ?? string.Empty;
    }
}