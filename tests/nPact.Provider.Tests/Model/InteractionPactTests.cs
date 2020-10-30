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
        public class WithBodyObject
        {
            private readonly string _resultMessage;

            public WithBodyObject()
            {
                const string expectedBody =
                    "{ equalProperty: 'equalValue', diffValueProp1: 'diffValue11', diffValueProp2: 'diffValue21', missingProp1: 'missingPropValue1' }";
                const string actualBody =
                    "{ equalProperty: 'equalValue', diffValueProp1: 'diffValue12', diffValueProp2: 'diffValue22', missingProp2: 'missingPropValue2' }";

                var interactionPactMock = SetupInteractionPactMock(JObject.Parse(expectedBody));
                var httpClientMock = SetupHttpClientMock(actualBody);

                var providerConfigMock = new Mock<IProviderConfiguration>();

                var sut = new InteractionPact(interactionPactMock.Object, providerConfigMock.Object, null);

                var verificationTask = sut.Verify(httpClientMock.Object);
                verificationTask.Wait();

                _resultMessage = verificationTask.Result.ToString();
            }

            [Fact]
            public void LogStatusCode()
                => _resultMessage.Should().Contain("Status code was Unauthorized. Expected OK");

            [Fact]
            public void LogActualMessage() =>
                _resultMessage.Should().Contain(@"Actual Response
Status Code: Unauthorized
Reason Phrase: Unauthorized
Headers
Content-Type: application/json; charset=utf-8");

            [Fact]
            public void LogActualMessageLog()
                => _resultMessage.Should()
                    .Contain(
                        "Content: { equalProperty: 'equalValue', diffValueProp1: 'diffValue12', diffValueProp2: 'diffValue22', missingProp2: 'missingPropValue2' }");

            [Fact]
            public void LogExpectedMessageLog() =>
                _resultMessage.Should().Contain(@"Expected Response
Status Code: OK
Headers
content-type: application/json; 
Content: {
  ""equalProperty"": ""equalValue"",
  ""diffValueProp1"": ""diffValue11"",
  ""diffValueProp2"": ""diffValue21"",
  ""missingProp1"": ""missingPropValue1""
}");

            [Fact]
            public void LogDiffFields()
                => _resultMessage.Should()
                    .Contain("Not match at diffValueProp1 in body. Expected: diffValue11 but received diffValue12.").And
                    .Contain("Not match at diffValueProp2 in body. Expected: diffValue21 but received diffValue22.");

            [Fact]
            public void LogMissingFields()
                => _resultMessage.Should()
                    .Contain("Cannot find missingProp1 in body.");
        }

        public class WithBodyArray
        {
            private readonly string _resultMessage;
            
            public WithBodyArray()
            {
                const string expectedBody =
                    "[ { equalProperty: 'equalValue', diffValueProp1: 'diffValue11' }, { equalProperty: 'equalValue', missingProp1: 'missingPropValue1' } ]";
                const string actualBody =
                    "[ { equalProperty: 'equalValue', diffValueProp1: 'diffValue12' }, { equalProperty: 'equalValue', missingProp2: 'missingPropValue2' }, { extraObject: 'extraObjectValue' } ]";

                var interactionPactMock = SetupInteractionPactMock(JArray.Parse(expectedBody));
                var httpClientMock = SetupHttpClientMock(actualBody);

                var providerConfigMock = new Mock<IProviderConfiguration>();

                var sut = new InteractionPact(interactionPactMock.Object, providerConfigMock.Object, null);

                var verificationTask = sut.Verify(httpClientMock.Object);
                verificationTask.Wait();

                _resultMessage = verificationTask.Result.ToString();
            }

            [Fact]
            public void LogFieldDiff()
                => _resultMessage.Should()
                    .Contain(
                        "Not match at [0].diffValueProp1 in body. Expected: diffValue11 but received diffValue12.");

            [Fact]
            public void LogExtraObject()
                => _resultMessage.Should().Contain(@"Unexpected element found in array {
  ""extraObject"": ""extraObjectValue""
} ");

            [Fact]
            public void LogMissingProperty()
                => _resultMessage.Should().Contain("Cannot find [1].missingProp1 in body.");
        }

        private static Mock<HttpClientWrapper> SetupHttpClientMock(string actualBody)
        {
            var httpClientMock = new Mock<HttpClientWrapper>();
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent(actualBody, Encoding.UTF8, "application/json"),
                });

            return httpClientMock;
        }

        private static Mock<Interaction> SetupInteractionPactMock(JContainer expectedBody)
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
                    Body = expectedBody
                });

            return interactionPactMock;
        }
    }
}