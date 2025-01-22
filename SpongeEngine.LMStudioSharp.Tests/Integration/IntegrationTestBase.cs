using System.Text.Json;
using SpongeEngine.LMStudioSharp.Models.Model;
using SpongeEngine.LMStudioSharp.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Integration;

public class IntegrationTestBase: LmStudioTestBase
{
    protected IntegrationTestBase(ITestOutputHelper output): base(output) {}
    
    public async Task InitializeAsync()
    {
        try
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
        catch (Exception ex) when (ex is not SkipException)
        {
            Output.WriteLine($"Failed to connect to LM Studio server: {ex.Message}");
            throw new SkipException("Failed to connect to LM Studio server");
        }
    }
}