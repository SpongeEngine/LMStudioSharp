using FluentAssertions;
using LocalAI.NET.LMStudio.Models.Base;
using LocalAI.NET.LMStudio.Models.Chat;
using LocalAI.NET.LMStudio.Models.Completion;
using LocalAI.NET.LMStudio.Models.Embedding;
using LocalAI.NET.LMStudio.Models.Model;
using LocalAI.NET.LMStudio.Providers.Native;
using LocalAI.NET.LMStudio.Tests.Common;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace LocalAI.NET.LMStudio.Tests.Unit.Providers.Native
{
    public class NativeLmStudioProviderTests : LmStudioTestBase
    {
        private readonly NativeLmStudioProvider _provider;
        private readonly HttpClient _httpClient;

        public NativeLmStudioProviderTests(ITestOutputHelper output) : base(output)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            _provider = new NativeLmStudioProvider(_httpClient, Logger);
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
                    .WithPath("/api/v0/models")
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
                    .WithPath("/api/v0/models/test-model")
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
            // Arrange
            var request = new LmStudioCompletionRequest
            {
                Model = "test-model",
                Prompt = "Hello",
                MaxTokens = 100,
                Temperature = 0.7f
            };

            var expectedResponse = new LmStudioCompletionResponse
            {
                Id = "cmpl-123",
                Object = "text_completion",
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = "test-model",
                Choices = new List<LmStudioChoice>
                {
                    new()
                    {
                        Index = 0,
                        Text = "Hello world!",
                        FinishReason = "stop"
                    }
                }
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/v0/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonConvert.SerializeObject(expectedResponse)));

            // Act
            var response = await _provider.CompleteAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Choices.Should().HaveCount(1);
            response.Choices[0].Text.Should().Be("Hello world!");
        }

        [Fact]
        public async Task ChatCompleteAsync_ShouldReturnChatResponse()
        {
            // Arrange
            var request = new LmStudioChatRequest
            {
                Model = "test-model",
                Messages = new List<LmStudioChatMessage>
                {
                    new() { Role = "user", Content = "Hello" }
                },
                Temperature = 0.7f
            };

            var expectedResponse = new LmStudioChatResponse
            {
                Id = "chat-123",
                Object = "chat.completion",
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = "test-model",
                Choices = new List<LmStudioChoice>
                {
                    new()
                    {
                        Index = 0,
                        Text = "Hello! How can I help you?",
                        FinishReason = "stop"
                    }
                }
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/v0/chat/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonConvert.SerializeObject(expectedResponse)));

            // Act
            var response = await _provider.ChatCompleteAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Choices.Should().HaveCount(1);
            response.Choices[0].Text.Should().Be("Hello! How can I help you?");
        }

        [Fact]
        public async Task CreateEmbeddingAsync_ShouldReturnEmbedding()
        {
            // Arrange
            var request = new LmStudioEmbeddingRequest
            {
                Model = "text-embedding-model",
                Input = "Hello world"
            };

            var expectedResponse = new LmStudioEmbeddingResponse
            {
                Object = "list",
                Model = "text-embedding-model",
                Data = new List<LmStudioEmbeddingResponse.EmbeddingData>
                {
                    new()
                    {
                        Object = "embedding",
                        Embedding = new[] { 0.1f, 0.2f, 0.3f },
                        Index = 0
                    }
                }
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/v0/embeddings")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonConvert.SerializeObject(expectedResponse)));

            // Act
            var response = await _provider.CreateEmbeddingAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Data[0].Embedding.Should().BeEquivalentTo(new[] { 0.1f, 0.2f, 0.3f });
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
                    .WithPath("/api/v0/completions")
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
                    .WithPath("/api/v0/chat/completions")
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