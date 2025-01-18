using SpongeEngine.LLMSharp.Core;

namespace SpongeEngine.LMStudioSharp
{
    public class LmStudioClientOptions : LlmClientBaseOptions 
    {
        public override string BaseUrl { get; set; } = "http://localhost:1234";
    }
}