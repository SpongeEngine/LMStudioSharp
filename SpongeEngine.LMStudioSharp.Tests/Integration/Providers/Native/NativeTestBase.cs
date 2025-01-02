using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpongeEngine.LMStudioSharp.Models.Model;
using SpongeEngine.LMStudioSharp.Providers.Native;
using SpongeEngine.LMStudioSharp.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Integration.Providers.Native
{
    public abstract class NativeTestBase : IAsyncLifetime
    {
        protected readonly INativeProvider Provider;
        protected readonly ITestOutputHelper Output;
        protected readonly ILogger Logger;
        protected bool ServerAvailable;
        protected LmStudioModel? DefaultModel;

        protected NativeTestBase(ITestOutputHelper output)
        {
            Output = output;
            Logger = LoggerFactory
                .Create(builder => builder.AddXUnit(output))
                .CreateLogger(GetType());

            var httpClient = new HttpClient 
            { 
                BaseAddress = new Uri(TestConfig.NativeApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(TestConfig.TimeoutSeconds)
            };

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            Provider = new NativeProvider(httpClient, Logger, jsonSettings);
        }

        public async Task InitializeAsync()
        {
            try
            {
                ServerAvailable = await Provider.IsAvailableAsync();
                if (ServerAvailable)
                {
                    Output.WriteLine("LM Studio server is available");
            
                    var modelsResponse = await Provider.ListModelsAsync();
                    if (modelsResponse.Data.Any())
                    {
                        DefaultModel = new LmStudioModel 
                        {
                            Id = modelsResponse.Data[0].Id,
                            Object = modelsResponse.Data[0].Object,
                            // Map other properties as needed
                        };
                        Output.WriteLine($"Found model: {DefaultModel.Id}");
                    }
                    else
                    {
                        Output.WriteLine("No models available");
                        throw new SkipException("No models available in LM Studio");
                    }
                }
                else
                {
                    Output.WriteLine("LM Studio server is not available");
                    throw new SkipException("LM Studio server is not available");
                }
            }
            catch (Exception ex) when (ex is not SkipException)
            {
                Output.WriteLine($"Failed to connect to LM Studio server: {ex.Message}");
                throw new SkipException("Failed to connect to LM Studio server");
            }
        }

        public Task DisposeAsync()
        {
            if (Provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            return Task.CompletedTask;
        }

        private class SkipException : Exception
        {
            public SkipException(string message) : base(message) { }
        }
    }
}