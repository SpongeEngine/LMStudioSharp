using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SpongeEngine.LMStudioSharp.Models.Base;
using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Embedding;
using SpongeEngine.LMStudioSharp.Models.Model;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Unit
{
    public class UnitTests : UnitTestBase
    {
        public UnitTests(ITestOutputHelper output) : base(output)
        {
            Client = new LmStudioSharpClient(new LmStudioClientOptions()
            {
                BaseUrl = Server.Urls[0],
                HttpClient = new HttpClient
                {
                    BaseAddress = new Uri(Server.Urls[0]),
                },
                JsonSerializerOptions = new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                },
                Logger = LoggerFactory
                    .Create(builder => builder.AddXUnit(output))
                    .CreateLogger(GetType()),
            });
        }

        [Fact]
        public async Task ListModelsAsync_ShouldReturnModels()
        {
            var expectedResponse = new ModelsResponse
            {
                Object = "list",
                Data = new List<Model>
                {
                    new()
                    {
                        Id = "test-model",
                        Object = "model",
                        Type = "llm",
                        Publisher = "test-publisher",
                        Architecture = "llama",
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
                    .WithBody(JsonSerializer.Serialize(expectedResponse)));

            var response = await Client.ListModelsAsync();

            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Data[0].Id.Should().Be("test-model");
        }

        [Fact]
        public async Task GetModelAsync_ShouldReturnModel()
        {
            var expectedModel = new Model
            {
                Id = "test-model",
                Object = "model",
                Type = "llm",
                Publisher = "test-publisher",
                Architecture = "llama",
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
                    .WithBody(JsonSerializer.Serialize(expectedModel)));

            var model = await Client.GetModelAsync("test-model");

            model.Should().NotBeNull();
            model.Id.Should().Be("test-model");
        }

        [Fact]
        public async Task CompleteAsync_WithNativeApi_ShouldReturn()
        {
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
                },
                Usage = new Usage { PromptTokens = 1, CompletionTokens = 2, TotalTokens = 3 },
                Stats = new Stats { TokensPerSecond = 10, TimeToFirstToken = 0.1, GenerationTime = 0.2 }
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/v0/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonSerializer.Serialize(expectedResponse)));

            var response = await Client.CompleteAsync(request);

            response.Should().NotBeNull();
            response.Choices.Should().HaveCount(1);
            response.Choices[0].Text.Should().Be("Hello world!");
        }

        [Fact]
        public async Task ChatCompleteAsync_WithNativeApi_ShouldReturn()
        {
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
                Id = "chatcmpl-123",
                Object = "chat.completion",
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = "test-model",
                Choices = new List<Choice>
                {
                    new()
                    {
                        Index = 0,
                        Message = new MessageResponse 
                        { 
                            Role = "assistant",
                            Content = "Hello! How can I help you?"
                        },
                        FinishReason = "stop"
                    }
                },
                Usage = new Usage { PromptTokens = 5, CompletionTokens = 7, TotalTokens = 12 },
                Stats = new Stats { TokensPerSecond = 15, TimeToFirstToken = 0.1, GenerationTime = 0.3 }
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/v0/chat/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonSerializer.Serialize(expectedResponse)));

            var response = await Client.ChatCompleteAsync(request);

            response.Should().NotBeNull();
            response.Choices.Should().HaveCount(1);
            response.Choices[0].Message.Should().NotBeNull();
            response.Choices[0].Message!.Content.Should().Be("Hello! How can I help you?");
        }

        [Fact]
        public async Task CreateEmbeddingAsync_WithNativeApi_ShouldReturn()
        {
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
                },
                Usage = new Usage { PromptTokens = 2, TotalTokens = 2 }
            };

            Server
                .Given(Request.Create()
                    .WithPath("/api/v0/embeddings")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(JsonSerializer.Serialize(expectedResponse)));

            var response = await Client.CreateEmbeddingAsync(request);

            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Data[0].Embedding.Should().BeEquivalentTo(new[] { 0.1f, 0.2f, 0.3f });
        }

        [Fact]
        public async Task StreamCompletionAsync_WithNativeApi_ShouldStreamTokens()
        {
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
                    .WithPath("/api/v0/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(string.Join("", streamResponses) + "data: [DONE]\n\n")
                    .WithHeader("Content-Type", "text/event-stream"));

            var receivedTokens = new List<string>();
            await foreach (var token in Client.StreamCompletionAsync(request))
            {
                receivedTokens.Add(token);
            }

            receivedTokens.Should().BeEquivalentTo(tokens);
        }

        [Fact]
        public async Task StreamChatAsync_WithNativeApi_ShouldStreamTokens()
        {
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
                    .WithPath("/api/v0/chat/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(string.Join("", streamResponses) + "data: [DONE]\n\n")
                    .WithHeader("Content-Type", "text/event-stream"));

            var receivedTokens = new List<string>();
            await foreach (var token in Client.StreamChatAsync(request))
            {
                receivedTokens.Add(token);
            }

            receivedTokens.Should().BeEquivalentTo(tokens);
        }
    }
}