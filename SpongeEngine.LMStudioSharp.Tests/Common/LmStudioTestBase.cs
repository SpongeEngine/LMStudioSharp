using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SpongeEngine.LMStudioSharp.Models.Model;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Common
{
    public abstract class LmStudioTestBase : IDisposable
    {
        protected readonly ITestOutputHelper Output;
        protected LMStudioSharpClient Client { get; init; } = null!;
        protected Model? DefaultModel { get; set; }

        protected LmStudioTestBase(ITestOutputHelper output)
        {
            Output = output;
        }

        public virtual void Dispose()
        {
            //Server.Dispose();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}