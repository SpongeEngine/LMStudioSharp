using Microsoft.Extensions.Logging;
using WireMock.Server;
using Xunit.Abstractions;

namespace LocalAI.NET.LMStudio.Tests.Common
{
    public abstract class LmStudioTestBase : IDisposable
    {
        protected readonly WireMockServer Server;
        protected readonly ILogger Logger;
        protected readonly string BaseUrl;

        protected LmStudioTestBase(ITestOutputHelper output)
        {
            Server = WireMockServer.Start();
            BaseUrl = Server.Urls[0];
            Logger = LoggerFactory
                .Create(builder => builder.AddXUnit(output))
                .CreateLogger(GetType());
        }

        public virtual void Dispose()
        {
            Server.Dispose();
        }
    }
}