using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpongeEngine.LMStudioSharp.Models.Base;
using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Embedding;
using SpongeEngine.LMStudioSharp.Models.Model;
using SpongeEngine.LMStudioSharp.Providers.LmStudioSharpNative;

namespace SpongeEngine.LMStudioSharp.Client
{
    public class LmStudioSharpClient : IDisposable
    {
        private readonly LmStudioSharpNativeProvider? _nativeProvider;
        private readonly Options _options;
        private bool _disposed;

        public string Name => "LMStudio";
        public string? Version { get; private set; }
        public bool SupportsStreaming => true;

        public LmStudioSharpClient(Options options, ILogger? logger = null, JsonSerializerSettings? jsonSettings = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(options.BaseUrl),
                Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
            };

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/event-stream"));

            if (!string.IsNullOrEmpty(options.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
            }

            _nativeProvider = new LmStudioSharpNativeProvider(httpClient, logger: logger, jsonSettings: jsonSettings);
        }

        /// <summary>
        /// Lists all loaded and downloaded models.
        /// GET /v1/models
        /// </summary>
        public Task<ModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.ListModelsAsync(cancellationToken);
        }

        /// <summary>
        /// Gets info about a specific model.
        /// GET /v1/models/{model}
        /// </summary>
        public Task<Model> GetModelAsync(string modelId, CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.GetModelAsync(modelId, cancellationToken);
        }

        /// <summary>
        /// Text Completions API. Provides a prompt and receives a completion.
        /// POST /v1/completions
        /// </summary>
        public Task<CompletionResponse> CompleteAsync(
            CompletionRequest request, 
            CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.CompleteAsync(request, cancellationToken);
        }

        /// <summary>
        /// Streaming Text Completions API.
        /// POST /v1/completions with stream=true
        /// </summary>
        public IAsyncEnumerable<string> StreamCompletionAsync(
            CompletionRequest request, 
            CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.StreamCompletionAsync(request, cancellationToken);
        }

        /// <summary>
        /// Chat Completions API. Provides messages array and receives assistant response.
        /// POST /v1/chat/completions
        /// </summary>
        public Task<ChatResponse> ChatCompleteAsync(
            ChatRequest request, 
            CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.ChatCompleteAsync(request, cancellationToken);
        }

        /// <summary>
        /// Streaming Chat Completions API.
        /// POST /v1/chat/completions with stream=true
        /// </summary>
        public IAsyncEnumerable<string> StreamChatAsync(
            ChatRequest request, 
            CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.StreamChatAsync(request, cancellationToken);
        }

        /// <summary>
        /// Text Embeddings API. Provides text and receives embedding vector.
        /// POST /v1/embeddings
        /// </summary>
        public Task<EmbeddingResponse> CreateEmbeddingAsync(
            EmbeddingRequest request, 
            CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.CreateEmbeddingAsync(request, cancellationToken);
        }
        
        private void EnsureNativeProvider()
        {
            if (_nativeProvider == null)
                throw new InvalidOperationException("Native API is not enabled. Set UseOpenAiApi to false in options.");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _nativeProvider?.Dispose();
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