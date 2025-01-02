using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Embedding;
using SpongeEngine.LMStudioSharp.Models.Model;
using SpongeEngine.LMStudioSharp.Providers.Native;
using SpongeEngine.LMStudioSharp.Tests.Common;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Unit.Providers.Native
{
    public class NativeProviderTests : LmStudioTestBase
    {
        private readonly NativeProvider _provider;
        private readonly HttpClient _httpClient;

        public NativeProviderTests(ITestOutputHelper output) : base(output)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            _provider = new NativeProvider(_httpClient, Logger);
        }

        [Fact]
        public async Task ListModelsAsync_ShouldReturnModels()
        {
            // Arrange
            var expectedResponse = new LmStudioModelsResponse
            {
                Object = "list",
                Data = new List<LmStudioModel>
                {
                    new()
                    {
                        Id = "test-model",
                        Type = "llm",
                        Publisher = "test-publisher",
                        Architecture = "test-arch",
                        CompatibilityType = "gguf",
                        Quantization = "Q4_K_M",
                        State = "not-loaded",
                        MaxContextLength = 4096
                    }
                }
            };

            Server
                .Given(Request.Create()
                    .WithPath("/v1/models")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonConvert.SerializeObject(expectedResponse)));

            // Act
            var response = await _provider.ListModelsAsync();

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Data[0].Id.Should().Be("test-model");
        }

        [Fact]
        public async Task GetModelAsync_ShouldReturnModel()
        {
            // Arrange
            var expectedModel = new LmStudioModel
            {
                Id = "test-model",
                Type = "llm",
                Publisher = "test-publisher",
                Architecture = "test-arch",
                CompatibilityType = "gguf",
                Quantization = "Q4_K_M",
                State = "not-loaded",
                MaxContextLength = 4096
            };

            Server
                .Given(Request.Create()
                    .WithPath("/v1/models/test-model")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonConvert.SerializeObject(expectedModel)));

            // Act
            var model = await _provider.GetModelAsync("test-model");

            // Assert
            model.Should().NotBeNull();
            model.Id.Should().Be("test-model");
        }

        [Fact]
        public async Task CompleteAsync_ShouldReturnCompletion()
        {
            Server
                .Given(Request.Create().WithPath("/v1/completions").UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                ""id"": ""cmpl-123"",
                ""object"": ""text_completion"",
                ""created"": 1589478378,
                ""model"": ""test-model"",
                ""choices"": [{
                    ""text"": ""test response"",
                    ""index"": 0,
                    ""finish_reason"": ""stop""
                }],
                ""usage"": {
                    ""prompt_tokens"": 10,
                    ""completion_tokens"": 20,
                    ""total_tokens"": 30
                },
                ""Successful"": true
            }"));

            var request = new LmStudioCompletionRequest
            {
                Model = "test-model",
                Prompt = "Hello",
                MaxTokens = 100,
                Temperature = 0.7f
            };

            // Act & Assert
            var response = await _provider.CompleteAsync(request);
            response.Should().NotBeNull();
            response.Choices.Should().NotBeEmpty();
            response.Choices[0].Text.Should().Be("test response");
        }

        [Fact]
        public async Task ChatCompleteAsync_ShouldReturnChatResponse()
        {
            Server
                .Given(Request.Create().WithPath("/v1/chat/completions").UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                ""id"": ""chat-123"",
                ""choices"": [{
                    ""message"": {
                        ""role"": ""assistant"",
                        ""content"": ""Hello! How can I help?""
                    }
                }],
                ""Successful"": true
            }"));

            var request = new LmStudioChatRequest
            {
                Model = "test-model",
                Messages = new List<LmStudioChatMessage>
                {
                    new() { Role = "user", Content = "Hello" }
                }
            };

            Logger.LogInformation("Sending chat request: {Request}", JsonConvert.SerializeObject(request, Formatting.Indented));
            var response = await _provider.ChatCompleteAsync(request);
            Logger.LogInformation("Chat response received: {Response}", JsonConvert.SerializeObject(response, Formatting.Indented));

            response.Should().NotBeNull();
            response.Choices.Should().NotBeEmpty();
            var firstChoice = response.Choices.FirstOrDefault();
            firstChoice.Should().NotBeNull();
            firstChoice.Message.Should().NotBeNull();
            firstChoice.Message.Content.Should().Be("Hello! How can I help?");
        }

        [Fact]
        public async Task CreateEmbeddingAsync_ShouldReturnEmbedding()
        {
            Server
                .Given(Request.Create().WithPath("/v1/embeddings").UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(@"{
                ""data"": [{
                    ""embedding"": [0.1, 0.2, 0.3],
                    ""index"": 0
                }],
                ""Successful"": true
            }"));

            var request = new LmStudioEmbeddingRequest
            {
                Model = "test-model",
                Input = "test text"
            };

            Logger.LogInformation("Sending embedding request: {Request}", JsonConvert.SerializeObject(request, Formatting.Indented));
            var response = await _provider.CreateEmbeddingAsync(request);
            Logger.LogInformation("Embedding response received: {Response}", JsonConvert.SerializeObject(response, Formatting.Indented));

            response.Should().NotBeNull();
            response.Data.Should().NotBeEmpty();
            response.Data.FirstOrDefault().Should().NotBeNull();
            response.Data.First().Embedding.Should().BeEquivalentTo(new[] { 0.1f, 0.2f, 0.3f });
        }

        [Fact]
        public async Task StreamCompletionAsync_ShouldStreamTokens()
        {
            // Arrange
            var request = new LmStudioCompletionRequest
            {
                Model = "test-model",
                Prompt = "Hello",
                MaxTokens = 100,
                Temperature = 0.7f,
                Stream = true
            };

            var tokens = new[] { "Hello", " world", "!" };
            var streamResponses = tokens.Select(token => $"data: {{\"choices\": [{{\"text\": \"{token}\"}}]}}\n\n");

            Server
                .Given(Request.Create()
                    .WithPath("/v1/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(string.Join("", streamResponses) + "data: [DONE]\n\n")
                    .WithHeader("Content-Type", "text/event-stream"));

            // Act
            var receivedTokens = new List<string>();
            await foreach (var token in _provider.StreamCompletionAsync(request))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }

        [Fact]
        public async Task StreamChatAsync_ShouldStreamTokens()
        {
            // Arrange
            var request = new LmStudioChatRequest
            {
                Model = "test-model",
                Messages = new List<LmStudioChatMessage>
                {
                    new() { Role = "user", Content = "Hello" }
                },
                Temperature = 0.7f,
                Stream = true
            };

            var tokens = new[] { "Hello", " there", "!" };
            var streamResponses = tokens.Select(token => $"data: {{\"choices\": [{{\"text\": \"{token}\"}}]}}\n\n");

            Server
                .Given(Request.Create()
                    .WithPath("/v1/chat/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(string.Join("", streamResponses) + "data: [DONE]\n\n")
                    .WithHeader("Content-Type", "text/event-stream"));

            // Act
            var receivedTokens = new List<string>();
            await foreach (var token in _provider.StreamChatAsync(request))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }

        public override void Dispose()
        {
            _httpClient.Dispose();
            base.Dispose();
        }
    }
}