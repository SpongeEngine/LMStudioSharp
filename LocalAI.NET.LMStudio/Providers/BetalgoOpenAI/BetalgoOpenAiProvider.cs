using System.Runtime.CompilerServices;
using Betalgo.Ranul.OpenAI.Interfaces;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Betalgo.Ranul.OpenAI.ObjectModels.ResponseModels;
using LocalAI.NET.LMStudio.Models.Chat;
using LocalAI.NET.LMStudio.Models.Completion;
using LocalAI.NET.LMStudio.Models.Embedding;
using LocalAI.NET.LMStudio.Models.Model;
using LocalAI.NET.LMStudio.Providers.Native;
using Microsoft.Extensions.Logging;

namespace LocalAI.NET.LMStudio.Providers.BetalgoOpenAI
{
   public class BetalgoOpenAiProvider : INativeLmStudioProvider
   {
       private readonly IOpenAIService _client;
       private readonly ILogger? _logger;
       private bool _disposed;

       public BetalgoOpenAiProvider(IOpenAIService client, ILogger? logger = null)
       {
           _client = client;
           _logger = logger;
       }

       public async Task<LmStudioModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default)
       {
           var response = await _client.Models.ListModel(cancellationToken);
           return new LmStudioModelsResponse
           {
               Data = response.Models.Select(m => new LmStudioModel 
               { 
                   Id = m.Id,
                   Object = "model",
                   Type = "llm"
               }).ToList()
           };
       }

       public async Task<LmStudioModel> GetModelAsync(string modelId, CancellationToken cancellationToken = default)
       {
           var response = await _client.Models.RetrieveModel(modelId, cancellationToken);
           return new LmStudioModel
           {
               Id = response.Id,
               Object = "model",
               Type = "llm"
           };
       }

       public async Task<LmStudioCompletionResponse> CompleteAsync(LmStudioCompletionRequest request, CancellationToken cancellationToken = default)
       {
           var response = await _client.Completions.CreateCompletion(new CompletionCreateRequest
           {
               Model = request.Model,
               Prompt = request.Prompt,
               Temperature = request.Temperature,
               MaxTokens = request.MaxTokens > 0 ? request.MaxTokens : null,
               Stop = request.Stop?.FirstOrDefault()
           }, null, cancellationToken);

           return new LmStudioCompletionResponse
           {
               Id = response.Id,
               Object = "text_completion", 
               Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
               Model = request.Model,
               Choices = response.Choices.Select(c => new Models.Base.LmStudioChoice
               {
                   Text = c.Text,
                   Index = c.Index,
                   FinishReason = c.FinishReason
               }).ToList(),
               Usage = MapUsage(response.Usage)
           };
       }

       public async Task<LmStudioChatResponse> ChatCompleteAsync(LmStudioChatRequest request, CancellationToken cancellationToken = default)
       {
           var response = await _client.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
           {
               Model = request.Model,
               Messages = request.Messages.Select(m => new ChatMessage(m.Role, m.Content)).ToList(),
               Temperature = request.Temperature,
               MaxTokens = request.MaxTokens
           }, null, cancellationToken);

           return new LmStudioChatResponse
           {
               Id = response.Id,
               Object = "chat.completion",
               Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
               Model = request.Model,
               Choices = response.Choices.Select(c => new Models.Base.LmStudioChoice
               {
                   Message = new LmStudioChatMessageResponse
                   {
                       Role = c.Message.Role,
                       Content = c.Message.Content
                   },
                   Index = c.Index,
                   FinishReason = c.FinishReason
               }).ToList(),
               Usage = MapUsage(response.Usage)
           };
       }

       public async Task<LmStudioEmbeddingResponse> CreateEmbeddingAsync(LmStudioEmbeddingRequest request, CancellationToken cancellationToken = default)
       {
           var response = await _client.Embeddings.CreateEmbedding(new EmbeddingCreateRequest
           {
               Model = request.Model,
               Input = request.Input
           }, cancellationToken);

           return new LmStudioEmbeddingResponse
           {
               Object = "list",
               Model = request.Model,
               Data = response.Data.Select((e, i) => new LmStudioEmbeddingResponse.EmbeddingData
               {
                   Object = "embedding",
                   Embedding = e.Embedding.Select(d => (float)d).ToArray(),
                   Index = i
               }).ToList(),
               Usage = MapUsage(response.Usage)
           };
       }

       public async IAsyncEnumerable<string> StreamCompletionAsync(LmStudioCompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

       public async IAsyncEnumerable<string> StreamChatAsync(LmStudioChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

       private static Models.Base.LmStudioUsage MapUsage(UsageResponse usage) => new() 
       { 
           PromptTokens = usage.PromptTokens,
           CompletionTokens = usage.CompletionTokens,
           TotalTokens = usage.TotalTokens
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