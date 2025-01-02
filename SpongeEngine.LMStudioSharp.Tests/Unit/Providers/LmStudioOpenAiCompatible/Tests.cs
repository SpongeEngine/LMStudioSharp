using FluentAssertions;
using SpongeEngine.LMStudioSharp.Providers.LmStudioSharpOpenAiCompatible;
using SpongeEngine.LMStudioSharp.Tests.Common;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Unit.Providers.LocalAI
{
    public class Tests : LmStudioTestBase
    {
        private readonly LmStudioSharpOpenAiCompatibleProvider _compatibleProvider;
        private readonly HttpClient _httpClient;

        public Tests(ITestOutputHelper output) : base(output)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            _compatibleProvider = new LmStudioSharpOpenAiCompatibleProvider(_httpClient, logger: Logger);
        }

        [Fact]
        public async Task CompleteAsync_ShouldReturnValidResponse()
        {
            // Arrange
            const string expectedResponse = "Test response";
            Server
                .Given(Request.Create()
                    .WithPath("/v1/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"{{\"choices\": [{{\"text\": \"{expectedResponse}\"}}]}}"));

            // Act
            var response = await _compatibleProvider.CompleteAsync("Test prompt");

            // Assert
            response.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task StreamCompletionAsync_ShouldStreamTokens()
        {
            // Arrange
            var tokens = new[] { "Hello", " world", "!" };
            var streamResponses = tokens.Select(token => 
                $"data: {{\"choices\": [{{\"text\": \"{token}\"}}]}}\n\n");

            Server
                .Given(Request.Create()
                    .WithPath("/v1/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(string.Join("", streamResponses))
                    .WithHeader("Content-Type", "text/event-stream"));

            // Act
            var receivedTokens = new List<string>();
            await foreach (var token in _compatibleProvider.StreamCompletionAsync("Test prompt"))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }

        [Fact]
        public async Task IsAvailableAsync_WhenServerResponds_ShouldReturnTrue()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/v1/models")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200));

            // Act
            var isAvailable = await _compatibleProvider.IsAvailableAsync();

            // Assert
            isAvailable.Should().BeTrue();
        }

        public override void Dispose()
        {
            _httpClient.Dispose();
            base.Dispose();
        }
    }
}