using System.Collections.Generic;
using nPact.Consumer.Contracts;
using nPact.Consumer.Rendering;

namespace nPact.Consumer.Extensions
{
    public static class BuilderExtensions
    {
        /// <summary>
        /// A shortcut to add header with key <value>Content-Type</value> and value <value>application/json; charset=utf-8</value>
        /// </summary>
        public static TBuilder WithContentTypeJson<TBuilder>(this IMessageBuilder<TBuilder> builder) => builder.WithHeader("Content-Type", "application/json; charset=utf-8");
        /// <summary>
        /// A shortcut to add header with key <value>Content-Type</value> and value <value>application/x-www-form-urlencoded</value>
        /// </summary>
        public static IRequestBuilder WithContentTypeFormUrlEncoded(this IRequestBuilder builder) => builder.WithHeader("Content-Type", "application/x-www-form-urlencoded");
        /// <summary>
        /// Synonymous with <see cref="IRequestBuilder.ThenRespondsWith(int)"/> with a value of <c>200</c>. 
        /// </summary>
        /// <returns>A builder for defining the response.</returns>
        public static IResponseBuilder ThenResponds(this IRequestBuilder builder) => builder.ThenRespondsWith();
        /// <summary>
        /// Provide a header that is required in the request.
        /// </summary>
        public static TBuilder WithHeader<TBuilder>(this IMessageBuilder<TBuilder> builder, string key, int value) => builder.WithHeader(key, value.ToString());
        /// <summary>
        /// Provide a query to add to the url.
        /// </summary>
        public static IRequestBuilder WithQuery(this IRequestBuilder builder, string key, int value) => builder.WithQuery(key, value.ToString());
        /// <summary>
        /// Defines the body as url encoded form data, setting the content type header and adding the body to the interaction.
        /// </summary>
        public static IRequestBuilder WithUrlEncodedFormData(this IRequestBuilder builder, IEnumerable<KeyValuePair<string, string>> values) => 
            builder.WithContentTypeFormUrlEncoded().WithBody(new JsonBody(values));
        /// <summary>
        /// Defines the body as url encoded form data, setting the content type header and adding the body to the interaction.
        /// </summary>
        public static IRequestBuilder WithUrlEncodedFormData(this IRequestBuilder builder, FormData data) => 
            WithUrlEncodedFormData(builder, (Dictionary<string,string>)data);
        /// <summary>
        /// Defines the body as json, setting the content type header and adding the body to the interaction.
        /// </summary>
        public static TBuilder WithJsonBody<TBuilder>(this IMessageBuilder<TBuilder> builder, object body) 
        {
            builder.WithContentTypeJson();
            return builder.WithBody(new JsonBody(body));
        }
        /// <summary>
        /// Defines the body as a json array, setting the content type header and adding the body to the interaction.
        /// </summary>
        public static TBuilder WithJsonArrayBody<TBuilder>(this IMessageBuilder<TBuilder> builder, params object[] collection) => builder.WithJsonBody(collection);
    }
}