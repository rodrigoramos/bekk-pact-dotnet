using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nPact.Common.Contracts;
using nPact.Common.Extensions;
using nPact.Provider.Config;
using nPact.Provider.Contracts;
using nPact.Provider.Exceptions;
using nPact.Provider.Repo;
using nPact.Provider.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using nPact.Provider.Web.Contracts;
using nPact.Provider.Web.Events;

namespace nPact.Provider.Web
{
    /// <summary>
    /// Fetches, parses and verifies pacts for a provider.
    /// </summary>
    /// <typeparam name="TStartup">The class to use to startup the web server</typeparam>
    public class PactRunner<TStartup> where TStartup : class
    {
        private readonly IProviderConfiguration configuration;
        private readonly ProviderStateSetup setup;
        /// <summary>
        /// Creates a new wrapper object to setup a test server and fetch all pacts. Configuration is read solely from environment variables.
        /// </summary>
        /// <param name="setup">A setup object responsible for mocking in front of each verification.</param>
        public PactRunner(ProviderStateSetup setup) : this(null, setup)
        {
        }
        /// <summary>
        /// Creates a new wrapper object to setup a test server and fetch all pacts.
        /// </summary>
        /// <param name="configuration"> A configuration object</param>
        /// <param name="setup">A setup object responsible for mocking in front of each verification.</param>
        public PactRunner(IProviderConfiguration configuration, ProviderStateSetup setup)
        {
            this.setup = setup;
            this.configuration = configuration ?? new EnvironmentBasedConfiguration();
        }
        /// <summary>
        /// Event is raised after each pact verification.
        /// </summary>
        public event EventHandler<PactResultEventArgs> Verified;
        /// <summary>
        /// Event is raised after each failing pact verifion.
        /// </summary>
        public event EventHandler<PactResultEventArgs> VerificationFailed;
        /// <summary>
        /// Fetches all pacts and asserts them.
        /// Throws exception after all pacts are run if any assertion fails.
        /// Use <seealso cref="Verify" /> to avoid the exception.
        /// </summary>
        /// <param name="providerName">The name of the provider, as stated in the pacts.</param>
        /// <exception cref="AssertionFailedException">Thrown when one or more pact assertions fails</exception>
        public async Task Assert(string providerName)
        {
            var results = await Verify(providerName);
            if(results.Any(r=>! r.Success)) throw new AssertionFailedException(results);
        }
        /// <summary>
        /// Fetches all pacts and verifies them. Returns all test results.
        /// </summary>
        /// <param name="providerName">The name of the provider, as stated in the pacts.</param>
        /// <returns>Testresults. Use <seealso cref="ITestResult.Success" /> to check the results of each pact.</returns>
        public async Task<IEnumerable<ITestResult>> Verify(string providerName)
        {
            var repo = new PactRepo(configuration);
            var results = new List<ITestResult>();
            foreach(var pact in repo.FetchAll(providerName))
            {
                using (var server = new TestServer(new WebHostBuilder().UseStartup<TStartup>(pact, setup)))
                using (var client = server.CreateClient())
                {
                    var result = await pact.Verify(client);
                    var args = new PactResultEventArgs(pact, result);
                    Verified?.Invoke(this, args);
                    if(!result.Success)
                    {
                         VerificationFailed?.Invoke(this, args);
                         pact.Configuration.LogSafe(LogLevel.Error, $"Assertion failed:{Environment.NewLine}{result}{Environment.NewLine}");
                    }
                    results.Add(result);
                }
            }
            if(!(DoNotGenerateOneDummyTestResultWhenNoPactsAreFound || results.Any())) results.Add(new DummyTestResult());            
            return results;
        }
        /// <summary>
        /// Set this to false to omit one dummy successful result instead of an empty set when no pacts are fetched.
        /// </summary>
        /// <returns></returns>
        public bool DoNotGenerateOneDummyTestResultWhenNoPactsAreFound { private get; set; } = false;
    }
}