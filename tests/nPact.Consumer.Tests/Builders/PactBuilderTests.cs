using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using nPact.Common.Contracts;
using nPact.Consumer.Builders;
using nPact.Consumer.Config;
using nPact.Consumer.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace nPact.Consumer.Tests.Builders
{
    public class PactBuilderTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Server.Context _context;

        public PactBuilderTests(ITestOutputHelper output)
        {
            _output = output;
            _context = new Server.Context(null);
        }

        public void Dispose() => _context.Dispose();

        [Fact]
        public async Task BuildPactAndRenderToJson_ReturnsString()
        {
            using var pact = await PactBuilder.Build("A test pact")
                .With(Configuration.With.Log(_output.WriteLine))
                .Between("Test provider").And("Test consumer")
                .WithProviderState("Some test assumptions")
                .WhenRequesting("/serviceurl/something/1")
                .WithHeader("Header", "headerValue", "Another value")
                .WithQuery("a", "b")
                .WithVerb("PUT")
                .ThenRespondsWith()
                .WithHeader("Some_reply-header", "Some result")
                .WithJsonBody(new
                {
                    Test = "ABC"
                })
                .InPact();

            var result = pact.ToString();

            Assert.NotNull(result);
            _output.WriteLine(result);
        }

        [Fact]
        public async Task BuildPactWithUrlEncodedFormDataAndRenderToJson_ServerReplies()
        {
            var data = new Dictionary<string, string>
                {{"A", "Some text, & possibly escaped."}, {"C", "3"}, {"d", "3.567"}};

            var baseAddress = new Uri("http://localhost:8978");

            const string url = "/serviceurl/whatever";

            var pact = await PactBuilder.Build("A test pact fith form data")
                .With(Configuration.With.Log(_output.WriteLine).MockServiceBaseUri(baseAddress)
                    .LogLevel(LogLevel.Verbose))
                .Between("Test provider").And("Test consumer")
                .WithProviderState("Some other test assumptions")
                .WhenRequesting(url)
                .WithVerb("POST")
                .WithUrlEncodedFormData(
                    FormData.With("A", "Some text, & possibly escaped.").And("C", 3).And("d", 3.567))
                .ThenRespondsWith()
                .WithHeader("Some-reply-header", "Some result")
                .InPact();

            using var client = new HttpClient {BaseAddress = baseAddress};

            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Version = HttpVersion.Version10,
                Content = new FormUrlEncodedContent(data)
            };

            var response = await client.SendAsync(message);

            response.Should().Be200Ok().And.Satisfy(_ => pact.Verify());
        }

        [Fact]
        public async Task BuildPactWithJsonBody_AddsHeader()
        {
            var body = new {A = "B", C = new[] {"D"}};
            var baseAddress = new Uri("http://localhost:8958");
            const string url = "/serviceurl/something/else/1";
            
            var pact = await PactBuilder.Build("A test pact")
                .With(Configuration.With
                    .Log(_output.WriteLine)
                    .LogLevel(LogLevel.Verbose)
                    .Comparison(StringComparison.InvariantCultureIgnoreCase)
                    .MockServiceBaseUri(baseAddress))
                .Between("Test provider").And("Test consumer")
                .WithProviderState("Yet some test assumptions")
                .WhenRequesting(url)
                .WithVerb("POST")
                .WithJsonBody(body)
                .ThenRespondsWith()
                .InPact();

            using var client = new HttpClient {BaseAddress = baseAddress};
            
            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Version = HttpVersion.Version10,
                Content = new StringContent(JObject.FromObject(body).ToString(), Encoding.UTF8,
                    "application/json")
            };
            
            var response = await client.SendAsync(message);
            response.Should().Be200Ok().And.Satisfy(_ => pact.Verify());
        }
    }
}
