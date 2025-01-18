using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SpongeEngine.LLMSharp.Core;
using SpongeEngine.LLMSharp.Core.Exceptions;
using SpongeEngine.LMStudioSharp.Models.Chat;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Embedding;
using SpongeEngine.LMStudioSharp.Models.Model;

namespace SpongeEngine.LMStudioSharp
{
    public class LmStudioSharpClient : LlmClientBase
    {
        public override LmStudioClientOptions Options { get; }
        
        private const string BASE_PATH = $"/v1";
        private const string MODELS_ENDPOINT = $"{BASE_PATH}/models";
        private const string CHAT_ENDPOINT = $"{BASE_PATH}/chat/completions"; 
        private const string COMPLETIONS_ENDPOINT = $"{BASE_PATH}/completions";
        private const string EMBEDDINGS_ENDPOINT = $"{BASE_PATH}/embeddings";

        public LmStudioSharpClient(LmStudioClientOptions options) : base(options)
        {
            Options = options;
        }

        public async Task<ModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await Options.HttpClient.GetAsync(MODELS_ENDPOINT, cancellationToken);
                await HandleResponseError(response, "Failed to list models");
                
                return await DeserializeResponse<ModelsResponse>(response);
            }
            catch (System.Exception ex) when (ex is not Exception)
            {
                throw new Exception("Failed to list models", ex);
            }
        }

        public async Task<Model> GetModelAsync(string modelId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await Options.HttpClient.GetAsync($"{MODELS_ENDPOINT}/{modelId}", cancellationToken);
                await HandleResponseError(response, "Failed to get model");
                
                return await DeserializeResponse<Model>(response);
            }
            catch (System.Exception ex) when (ex is not Exception)
            {
                throw new Exception($"Failed to get model {modelId}", ex);
            }
        }

        public async Task<CompletionResponse> CompleteAsync(CompletionRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                Options.Logger?.LogDebug("Completion request: {Request}", JsonSerializer.Serialize(request));
                
                var response = await PostAsJsonAsync(COMPLETIONS_ENDPOINT, request, cancellationToken);
                await HandleResponseError(response, "Completion failed");
                
                var result = await DeserializeResponse<CompletionResponse>(response);
                Options.Logger?.LogDebug("Completion response: {Response}", JsonSerializer.Serialize(result));
                
                return result;
            }
            catch (System.Exception ex) when (ex is not Exception)
            {
                throw new Exception("Completion failed", ex);
            }
        }

        public async Task<ChatResponse> ChatCompleteAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                Options.Logger?.LogDebug("Chat request: {Request}", JsonSerializer.Serialize(request));
                
                var response = await PostAsJsonAsync(CHAT_ENDPOINT, request, cancellationToken);
                await HandleResponseError(response, "Chat completion failed");
                
                var result = await DeserializeResponse<ChatResponse>(response);
                Options.Logger?.LogDebug("Chat response: {Response}", JsonSerializer.Serialize(result));
                
                return result;
            }
            catch (System.Exception ex) when (ex is not Exception)
            {
                throw new Exception("Chat completion failed", ex);
            }
        }

        public async Task<EmbeddingResponse> CreateEmbeddingAsync(EmbeddingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                Options.Logger?.LogDebug("Embedding request: {Request}", JsonSerializer.Serialize(request));
                
                var response = await PostAsJsonAsync(EMBEDDINGS_ENDPOINT, request, cancellationToken);
                await HandleResponseError(response, "Embedding creation failed");
                
                var result = await DeserializeResponse<EmbeddingResponse>(response);
                Options.Logger?.LogDebug("Embedding response: {Response}", JsonSerializer.Serialize(result));
                
                return result;
            }
            catch (System.Exception ex) when (ex is not Exception)
            {
                throw new Exception("Embedding creation failed", ex);
            }
        }

        public async IAsyncEnumerable<string> StreamCompletionAsync(CompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            request.Stream = true;
            await foreach (var token in StreamResponseAsync(COMPLETIONS_ENDPOINT, request, cancellationToken))
            {
                yield return token;
            }
        }

        public async IAsyncEnumerable<string> StreamChatAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                var response = await Options.HttpClient.GetAsync(MODELS_ENDPOINT, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<HttpResponseMessage> PostAsJsonAsync<T>(string endpoint, T content, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(content, Options.JsonSerializerOptions);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return await Options.HttpClient.PostAsync(endpoint, stringContent, cancellationToken);
        }

        private async Task HandleResponseError(HttpResponseMessage response, string errorMessage)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Options.Logger?.LogError("Response error: Status={Status}, Content={Content}", response.StatusCode, content);
                
                throw new LlmSharpException(
                    errorMessage,
                    (int)response.StatusCode,
                    content);
            }
        }

        private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<T>(content, Options.JsonSerializerOptions);
                if (result == null)
                {
                    throw new LlmSharpException(
                        "Null response after deserialization",
                        null,
                        content);
                }
                return result;
            }
            catch (JsonException ex)
            {
                Options.Logger?.LogError(ex, "Failed to deserialize response: {Content}", content);
                throw new LlmSharpException(
                    "Failed to deserialize response",
                    null,
                    $"Content: {content}, Error: {ex.Message}");
            }
        }

        private async IAsyncEnumerable<string> StreamResponseAsync<T>(string endpoint, T request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(request, Options.JsonSerializerOptions),
                    Encoding.UTF8,
                    "application/json")
            };

            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            using var response = await Options.HttpClient.SendAsync(
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

                Options.Logger?.LogDebug("Received line: {Line}", line);

                if (!line.StartsWith("data: ")) continue;

                var data = line[6..];
                if (data == "[DONE]") break;

                string? token = null;
                try
                {
                    var streamResponse = JsonSerializer.Deserialize<StreamResponse>(data, Options.JsonSerializerOptions);
                    token = streamResponse?.Choices?.FirstOrDefault()?.Text;
                }
                catch (JsonException ex)
                {
                    Options.Logger?.LogWarning(ex, "Failed to parse SSE message: {Message}", data);
                    continue;
                }

                if (!string.IsNullOrEmpty(token))
                {
                    Options.Logger?.LogDebug("Yielding token: {Token}", token);
                    yield return token;
                }
            }
        }

        private class StreamResponse
        {
            [JsonPropertyName("choices")]
            public List<StreamChoice> Choices { get; set; } = new();

            public class StreamChoice
            {
                [JsonPropertyName("text")]
                public string Text { get; set; } = string.Empty;

                [JsonPropertyName("finish_reason")]
                public string? FinishReason { get; set; }
            }
        }
    }
}