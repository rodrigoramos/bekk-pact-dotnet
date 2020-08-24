using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using nPact.Common.Contracts;
using nPact.Common.Utils;
using nPact.Consumer.Contracts;
using nPact.Consumer.Rendering;
using nPact.Consumer.Repo;
using nPact.Consumer.Server;
using IPact = nPact.Consumer.Contracts.IPact;

namespace nPact.Consumer.Builders
{
    class InteractionBuilder : IRequestPathBuilder, IRequestBuilder, IResponseBuilder, Contracts.IPact, IPactInteractionDefinition, IPactDefinition
    {
        private readonly IConsumerConfiguration configuration;
        private readonly List<string> queries = new List<string>();
        private IVerifyAndClosable handler;
        public string State { get; }
        public Version Version { get; }
        public string Description { get; }
        public string Provider { get; }
        public string Consumer { get; }
        public string RequestPath { get; private set; }
        public IJsonable RequestBody { get; private set; }
        public string Query => queries.Any() ? $"?{string.Join("&", queries)}" : null;
        public IHeaderCollection RequestHeaders { get; } = new HeaderCollection();
        public IHeaderCollection ResponseHeaders { get; } = new HeaderCollection();
        public string HttpVerb { get; private set; } = "GET";
        public int? ResponseStatusCode { get; private set; }
        public IJsonable ResponseBody { get; private set; }

        public InteractionBuilder(string state, string consumer, string provider, string description, Version version, IConsumerConfiguration config)
        {
            if(string.IsNullOrWhiteSpace(consumer)) throw new ArgumentException("Please provide a consumer name", nameof(consumer));
            if(string.IsNullOrWhiteSpace(provider)) throw new ArgumentException("Please provide a provider name", nameof(provider));
            this.configuration = config;
            State = state;
            Consumer = consumer;
            Provider = provider;
            Description = description;
            Version = version ?? new Version(1,0);
        }

        IRequestBuilder IRequestPathBuilder.WhenRequesting(string path)
        {
            RequestPath = path;
            return this;
        }

        IRequestBuilder IRequestBuilder.WithQuery(string key, string value)
        {
            queries.Add($"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)}");
            return this;
        }

        IRequestBuilder IRequestBuilder.WithVerb(HttpMethod verb) => ((IRequestBuilder) this).WithVerb(verb.ToString());

        IRequestBuilder IRequestBuilder.WithVerb(string verb)
        {
            HttpVerb = verb;
            return this;
        }
    
        IRequestBuilder IMessageBuilder<IRequestBuilder>.WithBody(IJsonable body)
        {
            RequestBody = body;
            return this;
        }

        IResponseBuilder IRequestBuilder.ThenRespondsWith(int statusCode)
        {
            ResponseStatusCode = statusCode;
            return this;
        }

        IResponseBuilder IRequestBuilder.ThenRespondsWith(HttpStatusCode statusCode) => ((IRequestBuilder) this).ThenRespondsWith((int) statusCode);

        IResponseBuilder IMessageBuilder<IResponseBuilder>.WithHeader(string key, params string[] values)
        {
            ResponseHeaders.Add(key, values);
            return this;
        }

        IResponseBuilder IMessageBuilder<IResponseBuilder>.WithBody(IJsonable body)
        {
            ResponseBody = body;
            return this;
        }

        async Task<Contracts.IPact> IResponseBuilder.InPact()
        {
            handler = await Context.RegisterListener(this, configuration);
            return this;
        }

        void Contracts.IPact.Verify()
        {
            var matches = handler.VerifyAndClose(1);
            if(matches != 1) throw new Exception($"The pact was matched {matches} times. Expected one.");
        }

        private async Task Save()
        {
            using(var repo = new PactRepo(configuration))
            {
                await repo.Put(this);
            }
        }

        async Task Contracts.IPact.VerifyAndSave()
        {
            var pact = (Contracts.IPact)this;
            pact.Verify();
            await Save();
        }

        IRequestBuilder IMessageBuilder<IRequestBuilder>.WithHeader(string key, params string[] values)
        {
            RequestHeaders.Add(key, values);
            return this;
        }

        IEnumerable<IPactInteractionDefinition> IPactDefinition.Interactions => new []{this};
        public void Dispose()
        {
            handler.VerifyAndClose(1);
        }

        public override string ToString()
        {
            return new PactJsonRenderer(this).ToString();
        }
    }
}