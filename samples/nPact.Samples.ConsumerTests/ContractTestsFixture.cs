using nPact.Consumer.Config;
using nPact.Consumer.Server;
using NUnit.Framework;

namespace nPact.Samples.ConsumerTests
{
    class ContractTestsFixture
    {
        private Context _serverContext;

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            var configuration = Configuration.With
                .LogLevel(Common.Contracts.LogLevel.Verbose)
                .MockServiceBaseUri("http://localhost:5001")
                .LogFile(@"..\..\..\contract.log")
                .PublishPath(@"C:\Users\jeos9\Source\repos\nPact\samples\sample-pacts");

            _serverContext = new Context(configuration)
                .ForConsumer("nPact.Samples.Consumer");
        }

        [OneTimeTearDown]
        public void FixtureTearDown() => _serverContext?.Dispose();
    }
}
