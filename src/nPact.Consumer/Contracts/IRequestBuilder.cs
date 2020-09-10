using System.Net;
using System.Net.Http;
using nPact.Common.Contracts;

namespace nPact.Consumer.Contracts
{
    public interface IRequestBuilder : IMessageBuilder<IRequestBuilder>
    {
        /// <summary>
        /// Provide a query to add to the url.
        /// </summary>
        IRequestBuilder WithQuery(string key, string value);
        /// <summary>
        /// Define the http verb to use. Default is <see cref="HttpMethod.Get"/>.
        /// </summary>
        IRequestBuilder WithVerb(HttpMethod verb);
        /// <summary>
        /// Define the http verb to use. Default is <c>GET</c>.
        /// </summary>
        IRequestBuilder WithVerb(string verb);
        /// <summary>
        /// Provide the expected status code in the reply from the provider.
        /// </summary>
        /// <param name="statusCode">A http status code. Default is 200.</param>
        /// <returns>A builder for defining the response.</returns>
        IResponseBuilder ThenRespondsWith(int statusCode=200);
        /// <summary>
        /// Provide the expected status code in the reply from the provider.
        /// </summary>
        /// <param name="statusCode">A http status code. Default is <see cref="HttpStatusCode.OK"/>.</param>
        /// <returns>A builder for defining the response.</returns>
        IResponseBuilder ThenRespondsWith(HttpStatusCode statusCode);
    }
}