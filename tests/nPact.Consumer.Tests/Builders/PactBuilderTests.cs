using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
        private readonly ITestOutputHelper output;
        private readonly nPact.Consumer.Server.Context context;
        public PactBuilderTests(ITestOutputHelper output)
        {
            this.output = output;
            this.context = new nPact.Consumer.Server.Context(null);
        }

        public void Dispose()
        {
            context.Dispose();
        }
        
        [Fact]
        public async Task BuildPactAndRenderToJson_ReturnsString()
        {
            using(var pact = await PactBuilder.Build("A test pact")
                .With(Configuration.With.Log(output.WriteLine))
                .Between("Test provider").And("Test consumer")
                .WithProviderState("Some test assumptions")
                .WhenRequesting("/serviceurl/something/1")
                .WithHeader("Header", "headerValue", "Another value")
                .WithQuery("a","b")
                .WithVerb("PUT")
                .ThenRespondsWith(200)
                .WithHeader("Some_reply-header", "Some result")
                .WithJsonBody(new {
                    Test="ABC"
                })
                .InPact())
            {
                var result = pact.ToString();

                Assert.NotNull(result);
                output.WriteLine(result);
            }
        }

        [Fact]
        public async Task BuildPactWithUrlEncodedFormDataAndRenderToJson_ServerReplies()
        {
            var data = new Dictionary<string,string>{{"A","Some text, & possibly escaped."},{"C","3"},{"d","3.567"}};
            var baseAddress = new Uri("http://localhost:8978");
            var url = "/serviceurl/whatever";
            using(var pact = await PactBuilder.Build("A test pact fith form data")
                .With(Configuration.With.Log(output.WriteLine).MockServiceBaseUri(baseAddress).LogLevel(LogLevel.Verbose))
                .Between("Test provider").And("Test consumer")
                .WithProviderState("Some other test assumptions")
                .WhenRequesting(url)
                .WithVerb("POST")
                //.WithUrlEncodedFormData(data)
                .WithUrlEncodedFormData(FormData.With("A", "Some text, & possibly escaped.").And("C", 3).And("d", 3.567))
                .ThenRespondsWith(200)
                .WithHeader("Some-reply-header", "Some result")
                .InPact())
            {
                using(var client = new HttpClient())
                {
                    client.BaseAddress = baseAddress;

                    var message = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Version = HttpVersion.Version10,
                        Content = new FormUrlEncodedContent(data)
                    };
                    var response = await client.SendAsync(message);
                    output.WriteLine(await response.Content.ReadAsStringAsync());
                    Assert.Equal(200, (int)response.StatusCode);
                }

                pact.Verify();
            }
        }

        [Fact]
        public async Task BuildPactWithJsonBody_AddsHeader()
        {
            var body = new { A = "B", C = new [] {"D"} };
            var baseAddress = new Uri("http://localhost:8958");
            const string url = "/serviceurl/something/else/1";
            using (var pact = await PactBuilder.Build("A test pact")
                .With(Configuration.With
                    .Log(output.WriteLine)
                    .LogLevel(LogLevel.Verbose)
                    .Comparison(StringComparison.InvariantCultureIgnoreCase)
                    .MockServiceBaseUri(baseAddress))
                .Between("Test provider").And("Test consumer")
                .WithProviderState("Yet some test assumptions")
                .WhenRequesting(url)
                .WithVerb("POST")
                .WithJsonBody(body)
                .ThenRespondsWith(200)
                .InPact())
            {
                using(var client = new HttpClient{BaseAddress=baseAddress})
                {
                    var message = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Version = HttpVersion.Version10,
                        Content = new StringContent(JObject.FromObject(body).ToString() , Encoding.UTF8, "application/json")
                    };
                    var response = await client.SendAsync(message); 
                    output.WriteLine(await response.Content.ReadAsStringAsync());
                    Assert.Equal(200, (int)response.StatusCode);
                }

                pact.Verify();
            }  
        }
    }
}