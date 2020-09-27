using nPact.Consumer.Config;
using nPact.Consumer.Server;
using NUnit.Framework;

namespace nPact.Samples.ConsumerTests
{
    class ContractTestsFixture
    {
        private Context _serverContext;

        private string PactsLocation = @"..\..\..\..\sample-pacts";
        private string LogsLocation = @"..\..\..\contract.log";

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            var configuration = Configuration.With
                .LogLevel(Common.Contracts.LogLevel.Verbose)
                .MockServiceBaseUri("http://localhost:5001")
                .LogFile(LogsLocation)
                .PublishPath(PactsLocation);

            _serverContext = new Context(configuration)
                .ForConsumer("nPact.Samples.Consumer");
        }

        [OneTimeTearDown]
        public void FixtureTearDown() => _serverContext?.Dispose();
    }
}
