using System.Net.Http;
using System.Threading.Tasks;
using nPact.Common.Contracts;

namespace nPact.Provider.Contracts
{
    /// <summary>
    /// Represents a pact (or rather an interaction from a pact).
    /// </summary>
    public interface IPact
    {
        /// <summary>
        /// The provider state (assumptions).
        /// </summary>
        string ProviderState { get; }
        /// <summary>
        /// A human readable description of the interaction.
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Call this method to verify the pact.
        /// </summary>
        /// <param name="client">A web client to use to call the server hosting the provider service.</param>
        /// <returns>A test result.</returns>
        Task<ITestResult> Verify(HttpClient client);
        /// <summary>
        /// The configuration used to test the pact.
        /// </summary>
        IProviderConfiguration Configuration { get; }
        /// <summary>
        /// Serializes the pact.
        /// </summary>
        /// <param name="asJson">When true, the full json definition is returned. Otherwise, only a summary.</param>
        string ToString(bool asJson);
    }
}
