using FluentAssertions;
using Newtonsoft.Json;
using SpongeEngine.LMStudioSharp.Client;
using SpongeEngine.LMStudioSharp.Models.Base;
using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Embedding;
using SpongeEngine.LMStudioSharp.Models.Model;
using SpongeEngine.LMStudioSharp.Tests.Common;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Unit.Client
{
    public class Tests : LmStudioTestBase
    {
        private readonly LmStudioSharpClient _sharpClient;

        public Tests(ITestOutputHelper output) : base(output)
        {
            _sharpClient = new LmStudioSharpClient(new Options
            {
                BaseUrl = BaseUrl
            }, Logger);
        }

        [Fact]
        public async Task ListModelsAsync_ShouldReturnModels()
        {
            // Arrange
            var expectedResponse = new ModelsResponse
            {
                Object = "list",
                Data = new List<Model>
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
            var response = await _sharpClient.ListModelsAsync();

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Data[0].Id.Should().Be("test-model");
        }

        [Fact]
        public async Task GetModelAsync_ShouldReturnModel()
        {
            // Arrange
            var expectedModel = new Model
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
            var model = await _sharpClient.GetModelAsync("test-model");

            // Assert
            model.Should().NotBeNull();
            model.Id.Should().Be("test-model");
        }

        [Fact]
        public async Task CompleteAsync_WithNativeApi_ShouldReturn()
        {
            // Arrange
            var request = new CompletionRequest
            {
                Model = "test-model",
                Prompt = "Hello",
                MaxTokens = 100,
                Temperature = 0.7f
            };

            var expectedResponse = new CompletionResponse
            {
                Id = "cmpl-123",
                Object = "text_completion",
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = "test-model",
                Choices = new List<Choice>
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
                    .WithPath("/v1/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonConvert.SerializeObject(expectedResponse)));

            // Act
            var response = await _sharpClient.CompleteAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Choices.Should().HaveCount(1);
            response.Choices[0].Text.Should().Be("Hello world!");
        }

        [Fact]
        public async Task ChatCompleteAsync_WithNativeApi_ShouldReturn()
        {
            // Arrange
            var request = new ChatRequest
            {
                Model = "test-model",
                Messages = new List<ChatMessage>
                {
                    new() { Role = "user", Content = "Hello" }
                },
                Temperature = 0.7f
            };

            var expectedResponse = new ChatResponse
            {
                Id = "chat-123",
                Object = "chat.completion",
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = "test-model",
                Choices = new List<Choice>
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
                    .WithPath("/v1/chat/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonConvert.SerializeObject(expectedResponse)));

            // Act
            var response = await _sharpClient.ChatCompleteAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Choices.Should().HaveCount(1);
            response.Choices[0].Text.Should().Be("Hello! How can I help you?");
        }

        [Fact]
        public async Task CreateEmbeddingAsync_WithNativeApi_ShouldReturn()
        {
            // Arrange
            var request = new EmbeddingRequest
            {
                Model = "text-embedding-model",
                Input = "Hello world"
            };

            var expectedResponse = new EmbeddingResponse
            {
                Object = "list",
                Model = "text-embedding-model",
                Data = new List<EmbeddingResponse.EmbeddingData>
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
                    .WithPath("/v1/embeddings")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonConvert.SerializeObject(expectedResponse)));

            // Act
            var response = await _sharpClient.CreateEmbeddingAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Data[0].Embedding.Should().BeEquivalentTo(new[] { 0.1f, 0.2f, 0.3f });
        }

        [Fact]
        public async Task StreamCompletionAsync_WithNativeApi_ShouldStreamTokens()
        {
            // Arrange
            var request = new CompletionRequest
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
            await foreach (var token in _sharpClient.StreamCompletionAsync(request))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }

        [Fact]
        public async Task StreamChatAsync_WithNativeApi_ShouldStreamTokens()
        {
            // Arrange
            var request = new ChatRequest
            {
                Model = "test-model",
                Messages = new List<ChatMessage>
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
            await foreach (var token in _sharpClient.StreamChatAsync(request))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }

        [Fact]
        public void UseNativeApi_WhenUsingOpenAiApi_ShouldThrow()
        {
            // Arrange
            var client = new LmStudioSharpClient(new Options
            {
                BaseUrl = BaseUrl,
                UseOpenAiApi = true
            }, Logger);

            // Act & Assert
            var act = () => client.ListModelsAsync();
            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Native API is not enabled. Set UseOpenAiApi to false in options.");
        }

        [Fact]
        public void UseOpenAiApi_WhenUsingNativeApi_ShouldThrow()
        {
            // Arrange
            var client = new LmStudioSharpClient(new Options
            {
                BaseUrl = BaseUrl,
                UseOpenAiApi = false
            }, Logger);

            // Act & Assert
            var act = () => client.CompleteWithOpenAiAsync("test");
            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("OpenAI API is not enabled. Set UseOpenAiApi to true in options.");
        }

        [Fact]
        public async Task IsAvailableAsync_WhenServerResponds_WithNativeApi_ShouldReturnTrue()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/v1/models")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200));

            // Act
            var isAvailable = await _sharpClient.IsAvailableAsync();

            // Assert
            isAvailable.Should().BeTrue();
        }
    }
}