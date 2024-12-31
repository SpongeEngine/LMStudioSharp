using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using LocalAI.NET.LMStudio.Models;
using LocalAI.NET.LMStudio.Models.Chat;
using LocalAI.NET.LMStudio.Models.Completion;
using LocalAI.NET.LMStudio.Models.Embedding;
using LocalAI.NET.LMStudio.Models.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace LocalAI.NET.LMStudio.Providers.Native
{
    public class NativeLmStudioProvider : INativeLmStudioProvider
    {
        private const string BASE_PATH = $"/v1";
        private const string MODELS_ENDPOINT = $"{BASE_PATH}/models";
        private const string CHAT_ENDPOINT = $"{BASE_PATH}/chat/completions"; 
        private const string COMPLETIONS_ENDPOINT = $"{BASE_PATH}/completions";
        private const string EMBEDDINGS_ENDPOINT = $"{BASE_PATH}/embeddings";

        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;
        private readonly JsonSerializerSettings? _jsonSettings;
        private bool _disposed;

        public NativeLmStudioProvider(HttpClient httpClient, ILogger? logger = null, JsonSerializerSettings? jsonSettings = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
            _jsonSettings = jsonSettings;
        }

        public async Task<LmStudioModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(MODELS_ENDPOINT, cancellationToken);
                await HandleResponseError(response, "Failed to list models");
                
                return await DeserializeResponse<LmStudioModelsResponse>(response);
            }
            catch (Exception ex) when (ex is not LmStudioException)
            {
                throw new LmStudioException("Failed to list models", ex);
            }
        }

        public async Task<LmStudioModel> GetModelAsync(string modelId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{MODELS_ENDPOINT}/{modelId}", cancellationToken);
                await HandleResponseError(response, "Failed to get model");
                
                return await DeserializeResponse<LmStudioModel>(response);
            }
            catch (Exception ex) when (ex is not LmStudioException)
            {
                throw new LmStudioException($"Failed to get model {modelId}", ex);
            }
        }

        public async Task<LmStudioCompletionResponse> CompleteAsync(
            LmStudioCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Completion request: {Request}", JsonConvert.SerializeObject(request));
                
                var response = await PostAsJsonAsync(COMPLETIONS_ENDPOINT, request, cancellationToken);
                await HandleResponseError(response, "Completion failed");
                
                var result = await DeserializeResponse<LmStudioCompletionResponse>(response);
                _logger?.LogDebug("Completion response: {Response}", JsonConvert.SerializeObject(result));
                
                return result;
            }
            catch (Exception ex) when (ex is not LmStudioException)
            {
                throw new LmStudioException("Completion failed", ex);
            }
        }

        public async Task<LmStudioChatResponse> ChatCompleteAsync(
            LmStudioChatRequest request, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Chat request: {Request}", JsonConvert.SerializeObject(request));
                
                var response = await PostAsJsonAsync(CHAT_ENDPOINT, request, cancellationToken);
                await HandleResponseError(response, "Chat completion failed");
                
                var result = await DeserializeResponse<LmStudioChatResponse>(response);
                _logger?.LogDebug("Chat response: {Response}", JsonConvert.SerializeObject(result));
                
                return result;
            }
            catch (Exception ex) when (ex is not LmStudioException)
            {
                throw new LmStudioException("Chat completion failed", ex);
            }
        }

        public async Task<LmStudioEmbeddingResponse> CreateEmbeddingAsync(
            LmStudioEmbeddingRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Embedding request: {Request}", JsonConvert.SerializeObject(request));
                
                var response = await PostAsJsonAsync(EMBEDDINGS_ENDPOINT, request, cancellationToken);
                await HandleResponseError(response, "Embedding creation failed");
                
                var result = await DeserializeResponse<LmStudioEmbeddingResponse>(response);
                _logger?.LogDebug("Embedding response: {Response}", JsonConvert.SerializeObject(result));
                
                return result;
            }
            catch (Exception ex) when (ex is not LmStudioException)
            {
                throw new LmStudioException("Embedding creation failed", ex);
            }
        }

        public async IAsyncEnumerable<string> StreamCompletionAsync(
            LmStudioCompletionRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            request.Stream = true;
            await foreach (var token in StreamResponseAsync(COMPLETIONS_ENDPOINT, request, cancellationToken))
            {
                yield return token;
            }
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            LmStudioChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            request.Stream = true;
            await foreach (var token in StreamResponseAsync(CHAT_ENDPOINT, request, cancellationToken))
            {
                yield return token;
            }
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(MODELS_ENDPOINT, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<HttpResponseMessage> PostAsJsonAsync<T>(string endpoint, T content, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(content, _jsonSettings);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(endpoint, stringContent, cancellationToken);
        }

        private async Task HandleResponseError(HttpResponseMessage response, string errorMessage)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger?.LogError("Response error: Status={Status}, Content={Content}", response.StatusCode, content);
                
                throw new LmStudioException(
                    errorMessage,
                    "LMStudio",
                    (int)response.StatusCode,
                    content);
            }
        }

        private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonConvert.DeserializeObject<T>(content, _jsonSettings);
                if (result == null)
                {
                    throw new LmStudioException(
                        "Null response after deserialization",
                        "LMStudio",
                        null,
                        content);
                }
                return result;
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "Failed to deserialize response: {Content}", content);
                throw new LmStudioException(
                    "Failed to deserialize response",
                    "LMStudio",
                    null,
                    $"Content: {content}, Error: {ex.Message}");
            }
        }

        private async IAsyncEnumerable<string> StreamResponseAsync<T>(
            string endpoint,
            T request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(request, _jsonSettings),
                    Encoding.UTF8,
                    "application/json")
            };

            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            using var response = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            await HandleResponseError(response, "Streaming failed");

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                {
                    await Task.Delay(50, cancellationToken);
                    continue;
                }

                _logger?.LogDebug("Received line: {Line}", line);

                if (!line.StartsWith("data: ")) continue;

                var data = line[6..];
                if (data == "[DONE]") break;

                string? token = null;
                try
                {
                    var streamResponse = JsonConvert.DeserializeObject<StreamResponse>(data, _jsonSettings);
                    token = streamResponse?.Choices?.FirstOrDefault()?.Text;
                }
                catch (JsonException ex)
                {
                    _logger?.LogWarning(ex, "Failed to parse SSE message: {Message}", data);
                    continue;
                }

                if (!string.IsNullOrEmpty(token))
                {
                    _logger?.LogDebug("Yielding token: {Token}", token);
                    yield return token;
                }
            }
        }

        private class StreamResponse
        {
            [JsonProperty("choices")]
            public List<StreamChoice> Choices { get; set; } = new();

            public class StreamChoice
            {
                [JsonProperty("text")]
                public string Text { get; set; } = string.Empty;

                [JsonProperty("finish_reason")]
                public string? FinishReason { get; set; }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Don't dispose the HttpClient as it was injected
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