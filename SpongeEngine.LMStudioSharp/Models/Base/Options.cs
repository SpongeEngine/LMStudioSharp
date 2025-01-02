namespace SpongeEngine.LMStudioSharp.Models.Base
{
    public class Options
    {
        /// <summary>
        /// Base URL of the LM Studio server
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:1234";
        
        public string ApiVersion { get; set; } = "v1";

        /// <summary>
        /// Optional API key if authentication is enabled
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// HTTP request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 600;

        /// <summary>
        /// Whether to use the OpenAI-compatible API endpoints instead of native API
        /// </summary>
        public bool UseOpenAiApi { get; set; }
    }
}