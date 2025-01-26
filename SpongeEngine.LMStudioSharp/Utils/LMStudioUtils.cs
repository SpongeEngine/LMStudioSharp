using System.Text;
using SpongeEngine.LMStudioSharp.Models;
using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using Exception = SpongeEngine.LMStudioSharp.Models.Exception;

namespace SpongeEngine.LMStudioSharp.Utils
{
    public static class LMStudioUtils
    {
        /// <summary>
        /// Creates a default completion request with common settings
        /// </summary>
        public static CompletionRequest CreateDefaultCompletionRequest(
            string model,
            string prompt,
            int maxTokens = 80,
            float temperature = 0.7f,
            float topP = 0.9f)
        {
            return new CompletionRequest
            {
                Model = model,
                Prompt = prompt,
                MaxTokens = maxTokens,
                Temperature = temperature,
                TopP = topP
            };
        }

        /// <summary>
        /// Creates a default chat request with common settings
        /// </summary>
        public static ChatRequest CreateDefaultChatRequest(
            string model,
            IEnumerable<ChatMessage> messages,
            int maxTokens = -1,
            float temperature = 0.7f)
        {
            return new ChatRequest
            {
                Model = model,
                Messages = messages.ToList(),
                MaxTokens = maxTokens,
                Temperature = temperature
            };
        }

        /// <summary>
        /// Generates a unique request ID for tracking streaming responses
        /// </summary>
        public static string GenerateRequestId() =>
            $"lms_{Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))}";

        /// <summary>
        /// Validates a completion request, throwing if invalid
        /// </summary>
        public static void ValidateCompletionRequest(CompletionRequest request)
        {
            if (string.IsNullOrEmpty(request.Model))
                throw new Exception("Model cannot be empty");

            if (string.IsNullOrEmpty(request.Prompt))
                throw new Exception("Prompt cannot be empty");

            ValidateCommonParameters(request.Temperature, request.TopP);
        }

        /// <summary>
        /// Validates a chat request, throwing if invalid
        /// </summary>
        public static void ValidateChatRequest(ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Model))
                throw new Exception("Model cannot be empty");

            if (!request.Messages.Any())
                throw new Exception("Messages cannot be empty");

            ValidateCommonParameters(request.Temperature);
        }

        private static void ValidateCommonParameters(float temperature, float? topP = null)
        {
            if (temperature < 0)
                throw new Exception("Temperature must be non-negative");

            if (topP.HasValue && (topP.Value < 0 || topP.Value > 1))
                throw new Exception("Top P must be between 0 and 1");
        }
    }
}