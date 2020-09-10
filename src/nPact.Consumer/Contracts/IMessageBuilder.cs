using nPact.Common.Contracts;

namespace nPact.Consumer.Contracts
{
    public interface IMessageBuilder<TBuilder>
    {
        /// <summary>
        /// Define header to require in the message.
        /// </summary>
        TBuilder WithHeader(string key, params string[] values);
        /// <summary>
        /// Define the message body.
        /// </summary>
        /// <param name="body">An object serializable to json.</param>
        TBuilder WithBody(IJsonable body);
    }
}