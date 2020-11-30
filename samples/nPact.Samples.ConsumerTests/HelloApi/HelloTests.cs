using System.Net;
using System.Net.Http;
using System;
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

        private HttpClient _httpClient;

        [SetUp]
        public void SetUp()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:5001/")
            };

            _helloApiConsumer = new HelloApiConsumer(_httpClient);
        }

        [Test]
        public async Task APactWithTheHelloApi()
        {
            var name = "World";

            var expectedBody = new Jsonable(@"{ message: ""Hello, World"" }");

            using var helloApiPact = await PactBuilder.Build()
                .ForProvider(ProviderName)
                .Given(@"The Hello Api Is Called With ""World"" as name")
                .WhenRequesting($"/api/hello/{name}")
                .ThenRespondsWith(HttpStatusCode.OK)
                .WithBody(expectedBody)
                .InPact();

            var result = await _helloApiConsumer.SayHello(name);

            result.Should().BeEquivalentTo("Hello, World");
        }
    }
}
