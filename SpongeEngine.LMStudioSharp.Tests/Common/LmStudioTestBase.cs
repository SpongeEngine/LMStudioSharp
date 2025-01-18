using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SpongeEngine.LMStudioSharp.Models.Model;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Common
{
    public abstract class LmStudioTestBase : IDisposable, IAsyncLifetime
    {
        protected readonly LmStudioSharpClient Client;
        protected readonly ITestOutputHelper Output;
        protected readonly WireMockServer Server;
        protected readonly ILogger Logger;
        protected readonly string BaseUrl;
        protected bool ServerAvailable;
        protected Model? DefaultModel;

        protected LmStudioTestBase(ITestOutputHelper output)
        {
            Server = WireMockServer.Start();
            BaseUrl = Server.Urls[0];
            Logger = LoggerFactory
                .Create(builder => builder.AddXUnit(output))
                .CreateLogger(GetType());
            
            
            
            Output = output;
            Logger = LoggerFactory
                .Create(builder => builder.AddXUnit(output))
                .CreateLogger(GetType());
            
            Client = new LmStudioSharpClient(new LmStudioClientOptions()
            {
                BaseUrl = BaseUrl,
                HttpClient = new HttpClient 
                { 
                    BaseAddress = new Uri(TestConfig.NativeApiBaseUrl),
                },
                JsonSerializerOptions = new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                }
            });
        }

        public virtual void Dispose()
        {
            Server.Dispose();
        }

        public async Task InitializeAsync()
        {
            try
            {
                ServerAvailable = await Client.IsAvailableAsync();
                if (ServerAvailable)
                {
                    Output.WriteLine("LM Studio server is available");
            
                    ModelsResponse modelsResponse = await Client.ListModelsAsync();
                    if (modelsResponse.Data.Any())
                    {
                        DefaultModel = new Model 
                        {
                            Id = modelsResponse.Data[0].Id,
                            Object = modelsResponse.Data[0].Object,
                            // Map other properties as needed
                        };
                        Output.WriteLine($"Found model: {DefaultModel.Id}");
                    }
                    else
                    {
                        Output.WriteLine($"modelsResponse: {JsonSerializer.Serialize(modelsResponse)}");
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
            return Task.CompletedTask;
        }
    }
}