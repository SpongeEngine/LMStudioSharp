using System.Net.Http.Headers;
using LocalAI.NET.LMStudio.Models.Base;
using LocalAI.NET.LMStudio.Models.Chat;
using LocalAI.NET.LMStudio.Models.Completion;
using LocalAI.NET.LMStudio.Models.Embedding;
using LocalAI.NET.LMStudio.Models.Model;
using LocalAI.NET.LMStudio.Providers.Native;
using LocalAI.NET.LMStudio.Providers.OpenAiCompatible;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LocalAI.NET.LMStudio.Client
{
    public class LmStudioClient : IDisposable
    {
        private readonly INativeLmStudioProvider? _nativeProvider;
        private readonly IOpenAiLmStudioProvider? _openAiProvider;
        private readonly LmStudioOptions _options;
        private bool _disposed;

        public string Name => "LMStudio";
        public string? Version { get; private set; }
        public bool SupportsStreaming => true;

        public LmStudioClient(LmStudioOptions options, ILogger? logger = null, JsonSerializerSettings? jsonSettings = null)
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

            if (options.UseOpenAiApi)
            {
                _openAiProvider = new OpenAiLmStudioProvider(httpClient, logger: logger, jsonSettings: jsonSettings);
            }
            else
            {
                _nativeProvider = new NativeLmStudioProvider(httpClient, logger: logger, jsonSettings: jsonSettings);
            }
        }

        /// <summary>
        /// Lists all loaded and downloaded models.
        /// GET /api/v0/models
        /// </summary>
        public Task<LmStudioModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.ListModelsAsync(cancellationToken);
        }

        /// <summary>
        /// Gets info about a specific model.
        /// GET /api/v0/models/{model}
        /// </summary>
        public Task<LmStudioModel> GetModelAsync(string modelId, CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.GetModelAsync(modelId, cancellationToken);
        }

        /// <summary>
        /// Text Completions API. Provides a prompt and receives a completion.
        /// POST /api/v0/completions
        /// </summary>
        public Task<LmStudioCompletionResponse> CompleteAsync(
            LmStudioCompletionRequest request, 
            CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.CompleteAsync(request, cancellationToken);
        }

        /// <summary>
        /// Streaming Text Completions API.
        /// POST /api/v0/completions with stream=true
        /// </summary>
        public IAsyncEnumerable<string> StreamCompletionAsync(
            LmStudioCompletionRequest request, 
            CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.StreamCompletionAsync(request, cancellationToken);
        }

        /// <summary>
        /// Chat Completions API. Provides messages array and receives assistant response.
        /// POST /api/v0/chat/completions
        /// </summary>
        public Task<LmStudioChatResponse> ChatCompleteAsync(
            LmStudioChatRequest request, 
            CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.ChatCompleteAsync(request, cancellationToken);
        }

        /// <summary>
        /// Streaming Chat Completions API.
        /// POST /api/v0/chat/completions with stream=true
        /// </summary>
        public IAsyncEnumerable<string> StreamChatAsync(
            LmStudioChatRequest request, 
            CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.StreamChatAsync(request, cancellationToken);
        }

        /// <summary>
        /// Text Embeddings API. Provides text and receives embedding vector.
        /// POST /api/v0/embeddings
        /// </summary>
        public Task<LmStudioEmbeddingResponse> CreateEmbeddingAsync(
            LmStudioEmbeddingRequest request, 
            CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.CreateEmbeddingAsync(request, cancellationToken);
        }

        // OpenAI API methods
        public Task<string> CompleteWithOpenAiAsync(
            string prompt, 
            CompletionOptions? options = null, 
            CancellationToken cancellationToken = default)
        {
            EnsureOpenAiProvider();
            return _openAiProvider!.CompleteAsync(prompt, options, cancellationToken);
        }

        public IAsyncEnumerable<string> StreamCompletionWithOpenAiAsync(
            string prompt, 
            CompletionOptions? options = null, 
            CancellationToken cancellationToken = default)
        {
            EnsureOpenAiProvider();
            return _openAiProvider!.StreamCompletionAsync(prompt, options, cancellationToken);
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            return _options.UseOpenAiApi 
                ? await _openAiProvider!.IsAvailableAsync(cancellationToken)
                : await _nativeProvider!.IsAvailableAsync(cancellationToken);
        }

        private void EnsureNativeProvider()
        {
            if (_nativeProvider == null)
                throw new InvalidOperationException("Native API is not enabled. Set UseOpenAiApi to false in options.");
        }

        private void EnsureOpenAiProvider()
        {
            if (_openAiProvider == null)
                throw new InvalidOperationException("OpenAI API is not enabled. Set UseOpenAiApi to true in options.");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _nativeProvider?.Dispose();
                    _openAiProvider?.Dispose();
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