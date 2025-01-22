using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Integration
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class IntegrationTests : LmStudioTestBase
    {
        public IntegrationTests(ITestOutputHelper output) : base(output)
        {
            Client = new LmStudioSharpClient(new LmStudioClientOptions()
            {
                BaseUrl = TestConfig.NativeApiBaseUrl,
                HttpClient = new HttpClient
                {
                    BaseAddress = new Uri(TestConfig.NativeApiBaseUrl),
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

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Complete_WithSimplePrompt_ShouldReturnResponse()
        {
            // Arrange
            var request = new CompletionRequest
            {
                Model = "test-model",
                Prompt = "Once upon a time",
                MaxTokens = 20,
                Temperature = 0.7f,
                TopP = 0.9f
            };

            // Act
            var response = await Client.CompleteAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Choices.Should().NotBeEmpty();
            response.Choices[0].Text.Should().NotBeNullOrEmpty();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task StreamCompletion_ShouldStreamTokens()
        {
            var request = new CompletionRequest
            {
                Model = "test-model",
                Prompt = "Write a short story about",
                MaxTokens = 20,
                Temperature = 0.7f,
                TopP = 0.9f,
                Stream = true
            };

            var tokens = new List<string>();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                await foreach (var token in Client.StreamCompletionAsync(request, cts.Token))
                {
                    tokens.Add(token);
                    Output.WriteLine($"Received token: {token}");

                    if (tokens.Count >= request.MaxTokens)
                    {
                        Output.WriteLine("Reached max length, breaking");
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                Output.WriteLine($"Stream timed out after receiving {tokens.Count} tokens");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Error during streaming: {ex}");
                throw;
            }

            tokens.Should().NotBeEmpty("No tokens were received from the stream");
            string.Concat(tokens).Should().NotBeNullOrEmpty("Combined token text should not be empty");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Complete_WithStopSequence_ShouldReturnResponse()
        {
            // Arrange
            var request = new CompletionRequest
            {
                Model = "test-model",
                Prompt = "Write a short story",
                MaxTokens = 20,
                Temperature = 0.7f,
                TopP = 0.9f,
                Stop = new[] { "." }
            };

            // Act
            var response = await Client.CompleteAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Choices.Should().NotBeEmpty();
            response.Choices[0].Text.Should().NotBeNullOrEmpty();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Complete_WithDifferentTemperatures_ShouldWork()
        {
            // Test various temperature settings
            var temperatures = new[] { 0.1f, 0.7f, 1.5f };
            foreach (var temp in temperatures)
            {
                // Arrange
                var request = new CompletionRequest
                {
                    Model = "test-model",
                    Prompt = "The quick brown fox",
                    MaxTokens = 20,
                    Temperature = temp,
                    TopP = 0.9f
                };

                // Act
                var response = await Client.CompleteAsync(request);

                // Assert
                response.Should().NotBeNull();
                response.Choices.Should().NotBeEmpty();
                response.Choices[0].Text.Should().NotBeNullOrEmpty();
                Output.WriteLine($"Temperature {temp} response: {response.Choices[0].Text}");
                
                await Task.Delay(500);
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task ChatComplete_ShouldReturnResponse()
        {
            // Arrange
            var request = new ChatRequest
            {
                //Model = DefaultModel.Id,  // Make sure to use the actual model ID
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = "Always answer in rhymes." },
                    new() { Role = "user", Content = "Introduce yourself." }
                },
                Temperature = 0.7f
            };

            // Act
            var response = await Client.ChatCompleteAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Choices.Should().NotBeEmpty();
            response.Choices[0].Message.Should().NotBeNull();
            response.Choices[0].Message!.Content.Should().NotBeNullOrEmpty();
        }
    }
}