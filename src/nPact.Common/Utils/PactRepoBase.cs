using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using nPact.Common.Extensions;
using nPact.Common.Exceptions;
using System.Text;
using System.IO;
using System.Net.Http.Headers;
using nPact.Common.Contracts;

namespace nPact.Common.Utils
{
    public abstract class PactRepoBase : IDisposable
    {
        protected PactRepoBase(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        private HttpClient _client;
        protected HttpClient BrokerClient => _client ?? (_client = InitializeClient());
        protected IConfiguration Configuration { get; }
        protected Uri BuildUri(string relativeUrl)
        {
            return new Uri(Configuration.BrokerUri, relativeUrl);
        }
        private HttpClient InitializeClient()
        {
            if(Configuration.BrokerUri == null) throw new InvalidOperationException("Broker url is not configured.");
            var client = new HttpClient();
            client.BaseAddress = Configuration.BrokerUri;
            if(Configuration.BrokerUserName != null || Configuration.BrokerPassword != null)
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(string.Concat(Configuration.BrokerUserName,":",Configuration.BrokerPassword))
                    ));
            }
            return client;
        }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }
    }
}