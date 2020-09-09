using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using nPact.Common.Contracts;
using nPact.Provider.Model;
using Xunit;

namespace nPact.Provider.Tests.Model
{
    public class InteractionPactTests
    {
        private string resultMessage;
        
        public InteractionPactTests()
        {
            var interactionPactMock = new Mock<Interaction>();

            interactionPactMock.SetupGet(x => x.Request)
                .Returns(new Request
                {
                    Method = "Get",
                    Headers = new Dictionary<string, string>(),
                    Path = "/"
                });

            interactionPactMock.Setup(x => x.Response)
                .Returns(new Response
                {
                    Status = HttpStatusCode.OK,
                    Headers = new Dictionary<string, string>
                    {
                        {"content-type", "application/json; "}
                    },
                    Body = JObject.Parse("{ a: 'b'}")
                });

            var providerConfigMock = new Mock<IProviderConfiguration>();
            var httpClient = new Mock<HttpClientWrapper>();
            httpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("{ a: 'b' }", Encoding.UTF8, "application/json"),
                });

            var sut = new InteractionPact(interactionPactMock.Object, providerConfigMock.Object,null);
            var resultTask = sut.Verify(httpClient.Object);
            resultTask.Wait();
            resultMessage = resultTask.Result.ToString();
        }
        
        [Fact]
        public void LogStatusCode() => resultMessage.Should().Contain("Status code was Unauthorized. Expected OK");

        [Fact]
        public void LogActualMessage() =>
            resultMessage.Should().Contain(@"Actual Response
Status Code: Unauthorized
Reason Phrase: Unauthorized
Headers
Content-Type: application/json; charset=utf-8");

        [Fact]
        public void LogActualMessageLog() 
            => resultMessage.Should().Contain("Content: { a: 'b' }");

        [Fact]
        public void LogExpectedMessageLog() =>
            resultMessage.Should().Contain(@"Expected Response
Status Code: OK
Headers
content-type: application/json; 
Content: {
  ""a"": ""b""
}");
    }
}