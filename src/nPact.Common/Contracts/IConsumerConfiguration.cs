using System;

namespace nPact.Common.Contracts
{
    public interface IConsumerConfiguration : IConfiguration
    {
        /// <summary>
        /// Base <c>Uri</c> for tcplistener.
        /// (Typically http://127.0.0.1:1234)
        /// </summary>
        Uri MockServiceBaseUri { get; }
    }
}
