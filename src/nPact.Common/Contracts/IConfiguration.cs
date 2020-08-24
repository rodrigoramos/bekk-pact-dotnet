using System;

namespace nPact.Common.Contracts
{
    public interface IConfiguration
    {
        /// <summary>
        /// The address to the pact broker service. This is used to publish and fetch the pacts.
        /// </summary>
        Uri BrokerUri { get; }
        /// <summary>
        /// The user name for the broker service
        /// </summary>
        string BrokerUserName { get; }
        /// <summary>
        /// The password for the broker service
        /// </summary>
        string BrokerPassword { get; }
        /// <summary>
        /// The file path to store and fetch published pacts
        /// </summary>
        string PublishPath { get; }
        /// <summary>
        /// A delegate for outputting log informastion.
        /// </summary>
        Action<string> Log { get; }
        /// <summary>
        /// A filter for throttling the log output
        /// </summary>
        LogLevel? LogLevel { get; }
        /// <summary>
        /// Location of a log file to append log messages to
        /// </summary>
        string LogFile { get; }
        /// <summary>
        /// The comparison type used when matching property names in the message body.
        /// </summary>
        /// <remarks>Default in the configuration builder is <seeAlso cref="StringComparison.CurrentCultureIgnoreCase"/>.</remarks>
        StringComparison? BodyKeyStringComparison { get; }
    }
}
