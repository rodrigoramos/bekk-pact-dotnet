using nPact.Common.Contracts;
using nPact.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using nPact.Common.Utils;
using nPact.Common.Exceptions;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using nPact.Provider.Contracts;
using nPact.Provider.Model;

namespace nPact.Provider.Repo
{
    /// <summary>
    /// This class is responsible for fetching all available pacts.
    /// </summary>
    public class PactRepo : PactRepoBase
    {
        private readonly IProviderConfiguration configuration;
        public PactRepo(IProviderConfiguration configuration): base(configuration)
        {
            this.configuration = configuration;
        }
        /// <summary>
        /// Fetches all pacts for the provider
        /// </summary>
        /// <param name="providerName">The provider name to filter on.</param>
        /// <returns>A collection of parsed pacts.</returns>
        public IEnumerable<IPact> FetchAll(string providerName)
        {
            if(Configuration.BrokerUri == null && string.IsNullOrWhiteSpace(Configuration.PublishPath))
            {
                throw new InvalidOperationException("Broker uri and publish path is missing in configuration. Please provide one of them.");
            }
            return FetchAndParseAllFromBroker(providerName)
                .Union(FetchAndParseNewestFromFileSystem(configuration.PublishPath, providerName));
        }

        private IEnumerable<IPact> FetchAndParseNewestFromFileSystem(string path, string providerName)
        {
            if(string.IsNullOrWhiteSpace(path)) return Enumerable.Empty<IPact>();            
            try
            {
                var result = new List<IPact>();
                foreach(var file in Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories))
                {
                    Configuration.LogSafe(LogLevel.Verbose, $"Parsing pact file {file}");
                    try
                    {
                        using(var stream = File.OpenText(file))
                        using(var reader = new JsonTextReader(stream))
                        {
                            result.AddRange(ReadInteractionFromJson((JObject)JToken.ReadFrom(reader), providerName));
                        }
                        Configuration.LogSafe(LogLevel.Verbose, $"Parsing pact file successful.");
                    }
                    catch(Exception e)
                    {
                        Configuration.LogSafe(LogLevel.Error, $"Error when parsing pact file {file}");
                        Configuration.LogSafe(LogLevel.Error, $"Inner exception: {e.Message}");
                    }
                }
                return result;
            }
            catch(DirectoryNotFoundException e)
            {
                throw new ConfigurationException($"Couldn't open folder path {Configuration.PublishPath} ({e.Message})", Configuration, e);
            }
        }

        private IEnumerable<IPact> FetchAndParseAllFromBroker(string providerName)
        {
            if(Configuration.BrokerUri == null) return Enumerable.Empty<IPact>();
            var brokerUrl = $"/pacts/provider/{providerName}/latest";
            return FetchPacts(brokerUrl).SelectMany(json => ReadInteractionFromJson(json, providerName));
        }

        private IEnumerable<IPact> ReadInteractionFromJson(JObject parsedPact, string providerName)
        {
            var provider = parsedPact.SelectToken("provider.name").ToString();
            if(provider != null && provider != providerName)
            {
                Configuration.LogSafe(LogLevel.Verbose, $"Skipping interactions for provider {provider}");
                yield break;
            };
            var consumer = parsedPact.SelectToken("consumer.name").ToString();
            configuration.LogSafe(LogLevel.Verbose, $"Parsing pact for {consumer}");
            foreach(var interaction in parsedPact["interactions"].Children().Select(i => i.ToObject<Interaction>()))
            {
                interaction.Consumer = consumer;
                interaction.Created = parsedPact["createdAt"]?.ToObject<DateTime>()??DateTime.Now;
                yield return new InteractionPact(interaction, configuration, parsedPact);
            }
        }

        private IEnumerable<JObject> FetchPacts(string brokerUrl)
        {
            var client = BrokerClient;
            foreach(var url in FetchUrls(brokerUrl).ConfigureAwait(false).GetAwaiter().GetResult())
            {
                Configuration.LogSafe(LogLevel.Verbose, $"Fetching pact at {url}");
                var brokerResponse = client.GetAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();
                brokerResponse.EnsureSuccessStatusCode();
                var parsedPact = JObject.Parse(brokerResponse.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
                yield return parsedPact;
            }
        }

        private async Task<IEnumerable<Uri>> FetchUrls(string url)
        {
            try
            {
                var client = BrokerClient;
                var response = await client.GetAsync(url);
                if(response.StatusCode == HttpStatusCode.NotFound)
                {
                    Configuration.LogSafe(LogLevel.Error, $"No pacts where found at {url}. Please verify the configuration.");
                    return Enumerable.Empty<Uri>();
                }
                response.EnsureSuccessStatusCode();
                return JObject.Parse(
                    response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                    .SelectToken("_links.pacts").Children().Select(t => t["href"].ToObject<Uri>());
            }
            catch(HttpRequestException e)
            {
                throw new PactBrokerException($"An error occured during request to the pact broker on {BuildUri(url)}. ({e.Message})", e);
            }
            catch(Exception e)
            {
                Configuration.LogSafe(LogLevel.Error, $"An error occured during request to the pact broker on {BuildUri(url)}. ({e.Message})");
                throw;
            }
        }
    }
}
