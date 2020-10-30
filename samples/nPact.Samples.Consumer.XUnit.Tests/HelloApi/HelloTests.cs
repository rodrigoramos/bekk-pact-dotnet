using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using nPact.Common.Utils;
using nPact.Consumer.Builders;
using Xunit;

namespace nPact.Samples.Consumer.XUnit.Tests.HelloApi
{
    public class HelloTests : IClassFixture<ContractTestsFixture>
    {
        private const string ProviderName = "nPact.Samples.Provider";

        private readonly HelloApiConsumer _helloApiConsumer;

        public HelloTests()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5001/")
            };

            _helloApiConsumer = new HelloApiConsumer(httpClient);
        }

        [Fact]
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