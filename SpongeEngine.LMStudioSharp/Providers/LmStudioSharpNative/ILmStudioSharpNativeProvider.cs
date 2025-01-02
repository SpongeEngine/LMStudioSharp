using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Embedding;
using SpongeEngine.LMStudioSharp.Models.Model;

namespace SpongeEngine.LMStudioSharp.Providers.LmStudioSharpNative
{
    public interface ILmStudioSharpNativeProvider : IDisposable
    {
        Task<ModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default);
        Task<Model> GetModelAsync(string modelId, CancellationToken cancellationToken = default);
        Task<CompletionResponse> CompleteAsync(CompletionRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> StreamCompletionAsync(CompletionRequest request, CancellationToken cancellationToken = default);
        Task<ChatResponse> ChatCompleteAsync(ChatRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> StreamChatAsync(ChatRequest request, CancellationToken cancellationToken = default);
        Task<EmbeddingResponse> CreateEmbeddingAsync(EmbeddingRequest request, CancellationToken cancellationToken = default);
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}