using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SpongeEngine.LLMSharp.Core.Exceptions;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Model;
using SpongeEngine.SpongeLLM.Core;
using SpongeEngine.SpongeLLM.Core.Interfaces;
using SpongeEngine.SpongeLLM.Core.Models;
using ChatRequest = SpongeEngine.LMStudioSharp.Models.Chat.ChatRequest;
using ChatResponse = SpongeEngine.LMStudioSharp.Models.Chat.ChatResponse;
using CompletionRequest = SpongeEngine.LMStudioSharp.Models.Completion.CompletionRequest;
using EmbeddingRequest = SpongeEngine.LMStudioSharp.Models.Embedding.EmbeddingRequest;
using EmbeddingResponse = SpongeEngine.LMStudioSharp.Models.Embedding.EmbeddingResponse;

namespace SpongeEngine.LMStudioSharp
{
    public class LMStudioSharpClient : LLMClientBase, ITextCompletion, IStreamableTextCompletion
    {
        public override LMStudioClientOptions Options { get; }
        
        private const string API_VERSION = "v0";
        private const string BASE_PATH = $"/api/{API_VERSION}";
        private const string MODELS_ENDPOINT = $"{BASE_PATH}/models";
        private const string CHAT_ENDPOINT = $"{BASE_PATH}/chat/completions";
        private const string COMPLETIONS_ENDPOINT = $"{BASE_PATH}/completions";
        private const string EMBEDDINGS_ENDPOINT = $"{BASE_PATH}/embeddings";

        public LMStudioSharpClient(LMStudioClientOptions options) : base(options)
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
                
                throw new SpongeLLMException(
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
                    throw new SpongeLLMException(
                        "Null response after deserialization",
                        null,
                        content);
                }
                return result;
            }
            catch (JsonException ex)
            {
                Options.Logger?.LogError(ex, "Failed to deserialize response: {Content}", content);
                throw new SpongeLLMException(
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
                    var choice = streamResponse?.Choices?.FirstOrDefault();
                    token = choice?.Text ?? choice?.Delta?.Content;
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
                // For plain text completions:
                [JsonPropertyName("text")]
                public string? Text { get; set; }
        
                // For chat completions:
                [JsonPropertyName("delta")]
                public Delta? Delta { get; set; }

                [JsonPropertyName("finish_reason")]
                public string? FinishReason { get; set; }
            }

            public class Delta
            {
                [JsonPropertyName("content")]
                public string? Content { get; set; }
            }
        }
        
        public async Task<TextCompletionResult> CompleteTextAsync(TextCompletionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            // Convert LLMSharp Core request to LMStudio request
            var lmStudioRequest = new Models.Completion.CompletionRequest
            {
                Model = request.ModelId,
                Prompt = request.Prompt,
                MaxTokens = request.MaxTokens ?? -1,
                Temperature = request.Temperature,
                TopP = request.TopP,
                Stop = request.StopSequences.Count > 0 ? request.StopSequences.ToArray() : null,
                Stream = false
            };

            // Apply any additional provider-specific parameters
            foreach (var param in request.ProviderParameters)
            {
                Options.Logger?.LogDebug("Additional provider parameter: {Key}={Value}", param.Key, param.Value);
            }

            // Call LMStudio API and measure time
            var startTime = DateTime.UtcNow;
            var response = await CompleteAsync(lmStudioRequest, cancellationToken);
            var generationTime = DateTime.UtcNow - startTime;

            // Extract the completion text from the first choice
            var completionText = response.Choices.FirstOrDefault()?.GetText() ?? string.Empty;

            // Convert LMStudio response to LLMSharp Core response
            return new TextCompletionResult
            {
                Text = completionText,
                ModelId = response.Model,
                TokenUsage = new TextCompletionTokenUsage
                {
                    PromptTokens = response.Usage.PromptTokens,
                    CompletionTokens = response.Usage.CompletionTokens ?? 0,
                    TotalTokens = response.Usage.TotalTokens
                },
                FinishReason = response.Choices.FirstOrDefault()?.FinishReason,
                GenerationTime = generationTime,
                Metadata = new Dictionary<string, object>
                {
                    ["provider"] = "LMStudio",
                    ["tokensPerSecond"] = response.Stats.TokensPerSecond,
                    ["timeToFirstToken"] = response.Stats.TimeToFirstToken,
                    ["architecture"] = response.ModelInfo.Architecture,
                    ["quantization"] = response.ModelInfo.Quantization,
                    ["format"] = response.ModelInfo.Format,
                    ["contextLength"] = response.ModelInfo.ContextLength,
                    ["runtime"] = response.Runtime.Name,
                    ["runtimeVersion"] = response.Runtime.Version
                }
            };
        }

        public async IAsyncEnumerable<TextCompletionToken> CompleteTextStreamAsync(TextCompletionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            // Convert LLMSharp Core request to LMStudio request
            var lmStudioRequest = new Models.Completion.CompletionRequest
            {
                Model = request.ModelId,
                Prompt = request.Prompt,
                MaxTokens = request.MaxTokens ?? -1,
                Temperature = request.Temperature,
                TopP = request.TopP,
                Stop = request.StopSequences.Count > 0 ? request.StopSequences.ToArray() : null,
                Stream = true
            };

            // Apply any additional provider-specific parameters
            foreach (var param in request.ProviderParameters)
            {
                Options.Logger?.LogDebug("Additional provider parameter: {Key}={Value}", param.Key, param.Value);
            }

            // Stream responses from LMStudio API and track tokens
            var totalTokens = 0;
            var lastChoice = new StreamResponse.StreamChoice();

            await foreach (var token in StreamCompletionAsync(lmStudioRequest, cancellationToken))
            {
                // Estimate token count - in practice you'd want to use a proper tokenizer here
                // This is a rough approximation based on whitespace
                var tokenCount = token.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
                totalTokens += tokenCount;

                yield return new TextCompletionToken
                {
                    Text = token,
                    TokenCount = totalTokens,
                    FinishReason = lastChoice.FinishReason
                };
        
                // Store last choice to get finish reason
                if (lastChoice.FinishReason != null)
                {
                    break;
                }
            }
        }
    }
}