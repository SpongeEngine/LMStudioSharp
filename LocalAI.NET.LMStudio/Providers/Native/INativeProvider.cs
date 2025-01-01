using LocalAI.NET.LMStudio.Models.Chat;
using LocalAI.NET.LMStudio.Models.Completion;
using LocalAI.NET.LMStudio.Models.Embedding;
using LocalAI.NET.LMStudio.Models.Model;

namespace LocalAI.NET.LMStudio.Providers.Native
{
    public interface INativeProvider : IDisposable
    {
        Task<LmStudioModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default);
        Task<LmStudioModel> GetModelAsync(string modelId, CancellationToken cancellationToken = default);
        Task<LmStudioCompletionResponse> CompleteAsync(LmStudioCompletionRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> StreamCompletionAsync(LmStudioCompletionRequest request, CancellationToken cancellationToken = default);
        Task<LmStudioChatResponse> ChatCompleteAsync(LmStudioChatRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> StreamChatAsync(LmStudioChatRequest request, CancellationToken cancellationToken = default);
        Task<LmStudioEmbeddingResponse> CreateEmbeddingAsync(LmStudioEmbeddingRequest request, CancellationToken cancellationToken = default);
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}