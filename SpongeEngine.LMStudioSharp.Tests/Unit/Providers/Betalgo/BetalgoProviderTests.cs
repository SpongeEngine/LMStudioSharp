using Betalgo.Ranul.OpenAI;
using Betalgo.Ranul.OpenAI.Managers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Embedding;
using SpongeEngine.LMStudioSharp.Providers.Betalgo;
using SpongeEngine.LMStudioSharp.Tests.Common;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Unit.Providers.Betalgo
{
   public class BetalgoProviderTests : LmStudioTestBase
   {
       private readonly BetalgoProvider _provider;

       public BetalgoProviderTests(ITestOutputHelper output) : base(output)
       {
           var options = new OpenAIOptions { ApiKey = "test", BaseDomain = BaseUrl };
           var openAiService = new OpenAIService(options);
           _provider = new BetalgoProvider(openAiService, logger: Logger);
       }

       [Fact]
       public async Task ListModelsAsync_ShouldReturnModels()
       {
           Server
               .Given(Request.Create().WithPath("/v1/models").UsingGet())
               .RespondWith(Response.Create()
                   .WithStatusCode(200)
                   .WithBody(@"{
                ""object"": ""list"",
                ""data"": [
                    {""id"": ""model1"", ""object"": ""model"", ""owned_by"": ""owner""},
                    {""id"": ""model2"", ""object"": ""model"", ""owned_by"": ""owner""}
                ]
            }"));

           var response = await _provider.ListModelsAsync();
           response.Data.Should().HaveCount(2);
           response.Data[0].Id.Should().Be("model1");
       }

       [Fact]
       public async Task GetModelAsync_ShouldReturnModel()
       {
           var modelId = "test-model";
           Server
               .Given(Request.Create().WithPath($"/v1/models/{modelId}").UsingGet())
               .RespondWith(Response.Create()
                   .WithStatusCode(200)
                   .WithBody($@"{{
                       ""Id"": ""{modelId}"",
                       ""Object"": ""model"",
                       ""Successful"": true
                   }}"));

           var model = await _provider.GetModelAsync(modelId);

           model.Should().NotBeNull();
           model.Id.Should().Be(modelId);
       }

       [Fact]
       public async Task CompleteAsync_ShouldReturnCompletion()
       {
           Server
               .Given(Request.Create().WithPath("/v1/completions").UsingPost())
               .RespondWith(Response.Create()
                   .WithStatusCode(200)
                   .WithHeader("Content-Type", "application/json")
                   .WithBody("{\"choices\":[{\"text\":\"test response\",\"index\":0,\"finish_reason\":\"stop\"}]}"));

           var request = new LmStudioCompletionRequest
           {
               Model = "test-model",
               Prompt = "test prompt",
               MaxTokens = 100,
               Temperature = 0.7f
           };

           Logger.LogInformation("Sending completion request: {Request}", JsonConvert.SerializeObject(request));
           var response = await _provider.CompleteAsync(request);
           Logger.LogInformation("Response received: {Response}", JsonConvert.SerializeObject(response));

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
                   .WithHeader("Content-Type", "application/json")
                   .WithBody("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Hello! How can I help?\"}}]}"));

           var request = new LmStudioChatRequest
           {
               Model = "test-model",
               Messages = new List<LmStudioChatMessage>
               {
                   new() { Role = "user", Content = "Hello" }
               }
           };

           Logger.LogInformation("Sending chat request: {Request}", JsonConvert.SerializeObject(request));
           var response = await _provider.ChatCompleteAsync(request);
           Logger.LogInformation("Response received: {Response}", JsonConvert.SerializeObject(response));

           response.Should().NotBeNull();
           response.Choices.Should().NotBeEmpty();
           response.Choices[0].Message.Should().NotBeNull();
           response.Choices[0].Message.Content.Should().Be("Hello! How can I help?");
       }

       [Fact]
       public async Task StreamCompletionAsync_ShouldStreamTokens()
       {
           var streamContent = 
               "data: {\"id\":\"cmpl-1\",\"object\":\"text_completion\",\"created\":1589478378,\"choices\":[{\"text\":\"Hello\",\"index\":0,\"finish_reason\":null}]}\n\n" +
               "data: {\"id\":\"cmpl-2\",\"object\":\"text_completion\",\"created\":1589478378,\"choices\":[{\"text\":\" world\",\"index\":0,\"finish_reason\":null}]}\n\n" +
               "data: {\"id\":\"cmpl-3\",\"object\":\"text_completion\",\"created\":1589478378,\"choices\":[{\"text\":\"!\",\"index\":0,\"finish_reason\":\"stop\"}]}\n\n" +
               "data: [DONE]";

           Server
               .Given(Request.Create().WithPath("/v1/completions").UsingPost())
               .RespondWith(Response.Create()
                   .WithStatusCode(200)
                   .WithBody(streamContent)
                   .WithHeader("Content-Type", "text/event-stream"));

           var request = new LmStudioCompletionRequest
           {
               Model = "test-model",
               Prompt = "Hello",
               Stream = true
           };

           var receivedTokens = new List<string>();
           await foreach (var token in _provider.StreamCompletionAsync(request))
           {
               receivedTokens.Add(token);
           }

           receivedTokens.Should().BeEquivalentTo(new[] { "Hello", " world", "!" });
       }

       [Fact]
       public async Task StreamChatAsync_ShouldStreamTokens()
       {
           var streamContent = 
               "data: {\"id\":\"chat-1\",\"object\":\"chat.completion.chunk\",\"created\":1589478378,\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":null}]}\n\n" +
               "data: {\"id\":\"chat-2\",\"object\":\"chat.completion.chunk\",\"created\":1589478378,\"choices\":[{\"index\":0,\"delta\":{\"content\":\" there\"},\"finish_reason\":null}]}\n\n" +
               "data: {\"id\":\"chat-3\",\"object\":\"chat.completion.chunk\",\"created\":1589478378,\"choices\":[{\"index\":0,\"delta\":{\"content\":\"!\"},\"finish_reason\":\"stop\"}]}\n\n" +
               "data: [DONE]";

           Server
               .Given(Request.Create().WithPath("/v1/chat/completions").UsingPost())
               .RespondWith(Response.Create()
                   .WithStatusCode(200)
                   .WithBody(streamContent)
                   .WithHeader("Content-Type", "text/event-stream"));

           var request = new LmStudioChatRequest
           {
               Model = "test-model",
               Messages = new List<LmStudioChatMessage>
               {
                   new() { Role = "user", Content = "Hello" }
               },
               Stream = true
           };

           var receivedTokens = new List<string>();
           await foreach (var token in _provider.StreamChatAsync(request))
           {
               receivedTokens.Add(token);
           }

           receivedTokens.Should().BeEquivalentTo(new[] { "Hello", " there", "!" });
       }

       
       [Fact]
       public async Task CreateEmbeddingAsync_ShouldReturnEmbedding()
       {
           Server
               .Given(Request.Create().WithPath("/v1/embeddings").UsingPost())
               .RespondWith(Response.Create()
                   .WithStatusCode(200)
                   .WithHeader("Content-Type", "application/json")
                   .WithBody("{\"data\":[{\"embedding\":[0.1,0.2,0.3],\"index\":0}]}"));

           var request = new LmStudioEmbeddingRequest
           {
               Model = "test-model",
               Input = "test text"
           };

           Logger.LogInformation("Sending embedding request: {Request}", JsonConvert.SerializeObject(request));
           var response = await _provider.CreateEmbeddingAsync(request);
           Logger.LogInformation("Response received: {Response}", JsonConvert.SerializeObject(response));

           response.Should().NotBeNull();
           response.Data.Should().NotBeEmpty();
           response.Data[0].Embedding.Should().BeEquivalentTo(new[] { 0.1f, 0.2f, 0.3f });
       }

       [Fact]
       public async Task IsAvailableAsync_ShouldReturnTrue()
       {
           Server
               .Given(Request.Create().WithPath("/v1/models").UsingGet())
               .RespondWith(Response.Create()
                   .WithStatusCode(200)
                   .WithBody(@"{""Successful"": true}"));

           var isAvailable = await _provider.IsAvailableAsync();

           isAvailable.Should().BeTrue();
       }
   }
}