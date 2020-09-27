using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using nPact.Common.Utils;
using nPact.Consumer.Builders;
using nPact.Samples.Consumer;
using NUnit.Framework;

namespace nPact.Samples.ConsumerTests.HelloApi
{
    [TestFixture]
    class HelloTests : ContractTestsFixture
    {
        private const string ProviderName = "nPact.Samples.Provider";

        private HelloApiConsumer _helloApiConsumer;

        [SetUp]
        public void SetUp() => _helloApiConsumer = new HelloApiConsumer();

        [Test]
        public async Task ACallToProvidersHelloApireturnsAHelloMessage()
        {
            var name = "World";

            var expectedBody = new Jsonable("Hello, World");

            using var helloApiPact = await PactBuilder.Build()
                .ForProvider(ProviderName)
                .Given(@"Hello Api is called with ""World"" as Name parameter")
                .WhenRequesting($"/api/hello/{name}")
                .ThenRespondsWith(HttpStatusCode.OK)
                .WithBody(expectedBody)
                .InPact();

            var result = await _helloApiConsumer.SayHello(name);

            result.Should().Be("Hello, World");
        }
    }
}
