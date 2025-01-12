namespace SpongeEngine.LMStudioSharp.Tests.Common
{
    public static class TestConfig
    {
        private const string DefaultHost = "http://localhost:1234";

        public static string NativeApiBaseUrl => 
            Environment.GetEnvironmentVariable("LMSTUDIO_BASE_URL") ?? $"{DefaultHost}/api";

        public static string OpenAiApiBaseUrl => 
            Environment.GetEnvironmentVariable("LMSTUDIO_OPENAI_BASE_URL") ?? $"{DefaultHost}/v1";
            
        // Extended timeout for large models
        public static int TimeoutSeconds => 120;
    }
}