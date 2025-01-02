# LMStudioSharp
[![NuGet](https://img.shields.io/nuget/v/SpongeEngine.LMStudioSharp.svg)](https://www.nuget.org/packages/SpongeEngine.LMStudioSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SpongeEngine.LMStudioSharp.svg)](https://www.nuget.org/packages/SpongeEngine.LMStudioSharp)
[![License](https://img.shields.io/github/license/SpongeEngine/LMStudioSharp)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0%2B-512BD4)](https://dotnet.microsoft.com/download)

C# client for interacting with LM Studio through its native and OpenAI-compatible endpoints.

## Features
- Complete support for LM Studio's native API
- OpenAI-compatible API endpoint support
- Text completion and chat completion
- Streaming support for both completion types
- Text embeddings generation
- Model information retrieval
- Comprehensive configuration options
- Built-in error handling and logging
- Cross-platform compatibility
- Full async/await support

📦 [View Package on NuGet](https://www.nuget.org/packages/SpongeEngine.LMStudioSharp)

## Installation
Install via NuGet:
```bash
dotnet add package SpongeEngine.LMStudioSharp
```

## Quick Start

### Using Native API
```csharp
using SpongeEngine.LMStudioSharp.Client;
using SpongeEngine.LMStudioSharp.Models.Base;
using SpongeEngine.LMStudioSharp.Models.Completion;
using SpongeEngine.LMStudioSharp.Models.Chat;

// Configure the client
var options = new Options
{
    BaseUrl = "http://localhost:1234",
    TimeoutSeconds = 600
};

// Create client instance
using var client = new LmStudioSharpClient(options);

// List available models
var models = await client.ListModelsAsync();
var modelId = models.Data[0].Id;

// Text completion
var completionRequest = new CompletionRequest
{
    Model = modelId,
    Prompt = "Write a short story about a robot:",
    MaxTokens = 200,
    Temperature = 0.7f,
    TopP = 0.9f
};

var completionResponse = await client.CompleteAsync(completionRequest);
Console.WriteLine(completionResponse.Choices[0].Text);

// Chat completion
var chatRequest = new ChatRequest
{
    Model = modelId,
    Messages = new List<ChatMessage>
    {
        new() { Role = "system", Content = "You are a helpful assistant." },
        new() { Role = "user", Content = "Tell me a joke about programming." }
    },
    Temperature = 0.7f
};

var chatResponse = await client.ChatCompleteAsync(chatRequest);
Console.WriteLine(chatResponse.Choices[0].Message.Content);

// Stream completion
await foreach (var token in client.StreamCompletionAsync(completionRequest))
{
    Console.Write(token);
}
```

### Using OpenAI-Compatible API
```csharp
var options = new Options
{
    BaseUrl = "http://localhost:1234",
    UseOpenAiApi = true
};

using var client = new LmStudioSharpClient(options);

// Simple completion
string response = await client.CompleteWithOpenAiAsync(
    "Write a short story about:",
    new CompletionOptions
    {
        MaxTokens = 200,
        Temperature = 0.7f
    });

// Stream completion
await foreach (var token in client.StreamCompletionWithOpenAiAsync(
    "Once upon a time...",
    new CompletionOptions { MaxTokens = 200 }))
{
    Console.Write(token);
}
```

## Configuration Options

### Basic Options
```csharp
var options = new Options
{
    BaseUrl = "http://localhost:1234",    // LM Studio server URL
    ApiVersion = "v1",                    // API version
    ApiKey = "optional_api_key",          // Optional API key
    TimeoutSeconds = 600,                 // Request timeout
    UseOpenAiApi = false                  // Use OpenAI-compatible API
};
```

### Completion Request Parameters
```csharp
var request = new CompletionRequest
{
    Model = "model-id",
    Prompt = "Your prompt here",
    MaxTokens = 200,                      // Maximum tokens to generate
    Temperature = 0.7f,                   // Randomness (0.0-1.0)
    TopP = 0.9f,                          // Nucleus sampling threshold
    Stop = new[] { "\n" },               // Stop sequences
    Stream = false                        // Enable streaming
};
```

## Error Handling
```csharp
try
{
    var response = await client.CompleteAsync(request);
}
catch (SpongeEngine.LMStudioSharp.Models.Exception ex)
{
    Console.WriteLine($"LM Studio error: {ex.Message}");
    Console.WriteLine($"Provider: {ex.Provider}");
    if (ex.StatusCode.HasValue)
    {
        Console.WriteLine($"Status code: {ex.StatusCode}");
    }
    if (ex.ResponseContent != null)
    {
        Console.WriteLine($"Response content: {ex.ResponseContent}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"General error: {ex.Message}");
}
```

## Logging
The client supports Microsoft.Extensions.Logging:

```csharp
ILogger logger = LoggerFactory
    .Create(builder => builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug))
    .CreateLogger<LmStudioSharpClient>();

var client = new LmStudioSharpClient(options, logger);
```

## JSON Serialization
Custom JSON settings can be provided:

```csharp
var jsonSettings = new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore
};

var client = new LmStudioSharpClient(options, logger: null, jsonSettings: jsonSettings);
```

## Testing
The library includes both unit and integration tests. Integration tests require a running LM Studio server.

To run the tests:
```bash
dotnet test
```

To configure the test environment:
```csharp
// Set environment variables for testing
Environment.SetEnvironmentVariable("LMSTUDIO_BASE_URL", "http://localhost:1234");
Environment.SetEnvironmentVariable("LMSTUDIO_OPENAI_BASE_URL", "http://localhost:1234/v1");
```

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## Support
For issues and feature requests, please use the [GitHub issues page](https://github.com/SpongeEngine/LMStudioSharp/issues).
