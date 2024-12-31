using System.Text;
using LocalAI.NET.LMStudio.Models.Completion;
using LocalAI.NET.LMStudio.Models.Chat;
using LocalAI.NET.LMStudio.Models;

namespace LocalAI.NET.LMStudio.Utils
{
    public static class LmStudioUtils
    {
        /// <summary>
        /// Creates a default completion request with common settings
        /// </summary>
        public static LmStudioCompletionRequest CreateDefaultCompletionRequest(
            string model,
            string prompt,
            int maxTokens = 80,
            float temperature = 0.7f,
            float topP = 0.9f)
        {
            return new LmStudioCompletionRequest
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
        public static LmStudioChatRequest CreateDefaultChatRequest(
            string model,
            IEnumerable<LmStudioChatMessage> messages,
            int maxTokens = -1,
            float temperature = 0.7f)
        {
            return new LmStudioChatRequest
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
        public static void ValidateCompletionRequest(LmStudioCompletionRequest request)
        {
            if (string.IsNullOrEmpty(request.Model))
                throw new LmStudioException("Model cannot be empty");

            if (string.IsNullOrEmpty(request.Prompt))
                throw new LmStudioException("Prompt cannot be empty");

            ValidateCommonParameters(request.Temperature, request.TopP);
        }

        /// <summary>
        /// Validates a chat request, throwing if invalid
        /// </summary>
        public static void ValidateChatRequest(LmStudioChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Model))
                throw new LmStudioException("Model cannot be empty");

            if (!request.Messages.Any())
                throw new LmStudioException("Messages cannot be empty");

            ValidateCommonParameters(request.Temperature);
        }

        private static void ValidateCommonParameters(float temperature, float? topP = null)
        {
            if (temperature < 0)
                throw new LmStudioException("Temperature must be non-negative");

            if (topP.HasValue && (topP.Value < 0 || topP.Value > 1))
                throw new LmStudioException("Top P must be between 0 and 1");
        }
    }
}