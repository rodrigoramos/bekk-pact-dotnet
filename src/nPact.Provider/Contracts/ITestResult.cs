using System.Net.Http;
using nPact.Common.Contracts;

namespace nPact.Provider.Contracts
{
    /// <summary>
    /// The result of a pact verification.
    /// Use <seealso cref="Success"/> to test
    /// if the verification passed.
    /// </summary>
    public interface ITestResult : IPactInformation
    {
        /// <summary>
        /// True if the verification was successful.
        /// </summary>
        bool Success { get; }
        /// <summary>
        /// The types of error occured.
        /// </summary>
        ValidationTypes ErrorTypes { get; }
        /// <summary>
        /// The expected response
        /// </summary>
        string ExpectedResponseBody { get; }
        /// <summary>
        /// The actual response.
        /// </summary>
        HttpResponseMessage ActualResponse { get; }
    }
}
