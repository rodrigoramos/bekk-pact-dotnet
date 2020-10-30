using System.IO;
using nPact.Consumer.Config;
using nPact.Consumer.Server;
using NUnit.Framework;

namespace nPact.Samples.Consumer.nUnit.Tests
{
    class ContractTestsFixture
    {
        private Context _serverContext;

        private readonly string _pactsLocation = Path.Combine("..", "..", "..", "sample-pacts");
        private readonly string _logsLocation = Path.Combine("..", "..", "..", "contract.log");

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            var configuration = Configuration.With
                .LogLevel(Common.Contracts.LogLevel.Verbose)
                .MockServiceBaseUri("http://localhost:5001")
                .LogFile(_logsLocation)
                .PublishPath(_pactsLocation);

            _serverContext = new Context(configuration)
                .ForConsumer("nPact.Samples.Consumer");
        }

        [OneTimeTearDown]
        public void FixtureTearDown() => _serverContext?.Dispose();
    }
}
