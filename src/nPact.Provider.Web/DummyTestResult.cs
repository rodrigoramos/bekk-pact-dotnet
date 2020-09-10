using System.Net;
using System.Net.Http;
using nPact.Provider.Contracts;

namespace nPact.Provider.Web
{
    class DummyTestResult : ITestResult
    {
        public bool Success => true;
        public ValidationTypes ErrorTypes => ValidationTypes.None;
        public string ExpectedResponseBody => string.Empty;
        public HttpResponseMessage ActualResponse => new HttpResponseMessage(HttpStatusCode.NoContent);
        public string Consumer => string.Empty;
        public string Description => "A placeholder result.";
        public string ProviderState => string.Empty;
        public override string ToString() => "No pact";
    }
}