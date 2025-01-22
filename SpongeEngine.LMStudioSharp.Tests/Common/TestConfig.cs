namespace SpongeEngine.LMStudioSharp.Tests.Common
{
    public static class TestConfig
    {
        private const string DefaultHost = "http://localhost:1234";

        public static string NativeApiBaseUrl => Environment.GetEnvironmentVariable("LMSTUDIO_BASE_URL") ?? $"{DefaultHost}/api";
    }
}