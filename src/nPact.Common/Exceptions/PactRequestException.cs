using System.Net.Http;

namespace nPact.Common.Exceptions
{
    public class PactRequestException : PactException
    {
        public PactRequestException(string message, HttpResponseMessage response)
         :base(message)
        {
            Response = response;
        }
        public HttpResponseMessage Response { get; }

    }
}