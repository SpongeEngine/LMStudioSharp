using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Embedding;
using SpongeEngine.LMStudioSharp.Models.Model;

namespace SpongeEngine.LMStudioSharp.Providers.Native
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