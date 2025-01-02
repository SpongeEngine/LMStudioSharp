using System.Runtime.CompilerServices;
using Betalgo.Ranul.OpenAI.Interfaces;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Betalgo.Ranul.OpenAI.ObjectModels.ResponseModels;
using Betalgo.Ranul.OpenAI.ObjectModels.SharedModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpongeEngine.LMStudioSharp.Models.Base;
using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Embedding;
using SpongeEngine.LMStudioSharp.Models.Model;
using SpongeEngine.LMStudioSharp.Providers.LmStudioSharpNative;
using ChatMessage = Betalgo.Ranul.OpenAI.ObjectModels.RequestModels.ChatMessage;
using EmbeddingResponse = SpongeEngine.LMStudioSharp.Models.Embedding.EmbeddingResponse;
using Exception = SpongeEngine.LMStudioSharp.Models.Exception;
using MessageResponse = SpongeEngine.LMStudioSharp.Models.Chat.MessageResponse;

namespace SpongeEngine.LMStudioSharp.Providers.Betalgo
{
   public class BetalgoOpenAiCompatibleProvider : ILmStudioSharpNativeProvider
   {
       private readonly IOpenAIService _client;
       private readonly ILogger? _logger;
       private bool _disposed;

       public BetalgoOpenAiCompatibleProvider(IOpenAIService client, ILogger? logger = null)
       {
           _client = client;
           _logger = logger;
       }

       public async Task<ModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default)
       {
           var response = await _client.Models.ListModel(cancellationToken);
           return new ModelsResponse
           {
               Data = response.Models.Select(m => new Model 
               { 
                   Id = m.Id,
                   Object = "model",
                   Type = "llm"
               }).ToList()
           };
       }

       public async Task<Model> GetModelAsync(string modelId, CancellationToken cancellationToken = default)
       {
           var response = await _client.Models.RetrieveModel(modelId, cancellationToken);
           return new Model
           {
               Id = response.Id,
               Object = "model",
               Type = "llm"
           };
       }

       public async Task<CompletionResponse> CompleteAsync(
           CompletionRequest request, 
           CancellationToken cancellationToken = default)
       {
           _logger?.LogInformation("Sending completion request: {Request}", JsonConvert.SerializeObject(request));
    
           var response = await _client.Completions.CreateCompletion(new CompletionCreateRequest
           {
               Model = request.Model,
               Prompt = request.Prompt,
               Temperature = request.Temperature,
               MaxTokens = request.MaxTokens > 0 ? request.MaxTokens : null,
               Stop = request.Stop?.FirstOrDefault()
           }, null, cancellationToken);

           _logger?.LogInformation("Raw completion response: {Response}", JsonConvert.SerializeObject(response));

           if (!response.Successful || response.Choices == null || !response.Choices.Any())
           {
               var error = JsonConvert.SerializeObject(response);
               _logger?.LogError("Completion failed with response: {Error}", error);
               throw new Exception($"Completion failed: {response.Error?.Message ?? "No choices returned"}", "LMStudio", responseContent: error);
           }

           var result = new CompletionResponse
           {
               Id = response.Id ?? string.Empty,
               Object = "text_completion",
               Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
               Model = request.Model,
               Choices = response.Choices.Select(c => new Choice
               {
                   Text = c.Text ?? string.Empty,
                   Index = c.Index,
                   FinishReason = c.FinishReason
               }).ToList(),
               Usage = MapUsage(response.Usage)
           };

           _logger?.LogInformation("Mapped completion response: {Result}", JsonConvert.SerializeObject(result));
           return result;
       }

       public async Task<ChatResponse> ChatCompleteAsync(ChatRequest request, CancellationToken cancellationToken = default)
       {
           var response = await _client.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
           {
               Model = request.Model,
               Messages = request.Messages.Select(m => new ChatMessage(m.Role, m.Content)).ToList(),
               Temperature = request.Temperature,
               MaxTokens = request.MaxTokens
           }, null, cancellationToken);

           return new ChatResponse
           {
               Id = response.Id ?? string.Empty,
               Object = "chat.completion",
               Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
               Model = request.Model,
               Choices = (response.Choices ?? new List<ChatChoiceResponse>()).Select(c => new Choice
               {
                   Message = new MessageResponse
                   {
                       Role = c.Message.Role ?? string.Empty,
                       Content = c.Message.Content ?? string.Empty
                   },
                   Index = c.Index,
                   FinishReason = c.FinishReason
               }).ToList(),
               Usage = MapUsage(response.Usage)
           };
       }

       public async Task<EmbeddingResponse> CreateEmbeddingAsync(EmbeddingRequest request, CancellationToken cancellationToken = default)
       {
           var response = await _client.Embeddings.CreateEmbedding(new EmbeddingCreateRequest
           {
               Model = request.Model,
               Input = request.Input
           }, cancellationToken);

           return new EmbeddingResponse
           {
               Object = "list",
               Model = request.Model,
               Data = (response.Data ?? new List<global::Betalgo.Ranul.OpenAI.ObjectModels.ResponseModels.EmbeddingResponse>()).Select((e, i) => new EmbeddingResponse.EmbeddingData
               {
                   Object = "embedding",
                   Embedding = (e.Embedding ?? new List<double>()).Select(d => (float)d).ToArray(),
                   Index = i
               }).ToList(),
               Usage = MapUsage(response.Usage)
           };
       }

       public async IAsyncEnumerable<string> StreamCompletionAsync(CompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
       {
           var streamingResponse = _client.Completions.CreateCompletionAsStream(new CompletionCreateRequest
           {
               Model = request.Model,
               Prompt = request.Prompt,
               Temperature = request.Temperature,
               MaxTokens = request.MaxTokens,
               Stop = request.Stop?.FirstOrDefault(),
               Stream = true
           }, null, cancellationToken);

           await foreach (var response in streamingResponse)
           {
               if (response.Successful)
                   yield return response.Choices.FirstOrDefault()?.Text ?? string.Empty;
           }
       }

       public async IAsyncEnumerable<string> StreamChatAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
       {
           var streamingResponse = _client.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
           {
               Model = request.Model,
               Messages = request.Messages.Select(m => new ChatMessage(m.Role, m.Content)).ToList(),
               Temperature = request.Temperature,
               MaxTokens = request.MaxTokens,
               Stream = true
           }, null, true, cancellationToken);

           await foreach (var response in streamingResponse)
           {
               if (response.Successful && !string.IsNullOrEmpty(response.Choices.FirstOrDefault()?.Message.Content))
                   yield return response.Choices.FirstOrDefault()?.Message.Content!;
           }
       }

       public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
       {
           var response = await _client.Models.ListModel(cancellationToken);
           return response.Successful;
       }

       private static Usage MapUsage(UsageResponse? usage) => new() 
       { 
           PromptTokens = usage?.PromptTokens ?? 0,
           CompletionTokens = usage?.CompletionTokens ?? 0,
           TotalTokens = usage?.TotalTokens ?? 0
       };

       protected virtual void Dispose(bool disposing)
       {
           if (!_disposed)
           {
               if (disposing)
               {
                   (_client as IDisposable)?.Dispose();
               }
               _disposed = true;
           }
       }

       public void Dispose()
       {
           Dispose(true);
           GC.SuppressFinalize(this);
       }
   }
}