using SpongeEngine.LMStudioSharp.Tests.Common;
using WireMock.Server;
using Xunit.Abstractions;

namespace SpongeEngine.LMStudioSharp.Tests.Unit;

public class UnitTestBase: LmStudioTestBase
{
    protected WireMockServer Server { get; }

    protected UnitTestBase(ITestOutputHelper output) : base(output)
    {
        Server = WireMockServer.Start();
        // HttpClient = new HttpClient
        // {
        //     BaseAddress = new Uri(Server.Urls[0]),
        // };
    }
}