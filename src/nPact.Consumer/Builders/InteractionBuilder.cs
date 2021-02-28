using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    class InteractionBuilder : IRequestPathBuilder, IRequestBuilder, IResponseBuilder, IPact,
        IPactInteractionDefinition, IPactDefinition
    {
        private readonly IConsumerConfiguration _configuration;
        private readonly List<string> _queries = new List<string>();
        private IVerifyAndClosable _handler;
        public string State { get; }
        public Version Version { get; }
        public string Description { get; }
        public string Provider { get; }
        public string Consumer { get; }
        public string RequestPath { get; private set; }
        public IJsonable RequestBody { get; private set; }
        public string Query => _queries.Any() ? $"?{string.Join("&", _queries)}" : null;
        public IHeaderCollection RequestHeaders { get; } = new HeaderCollection();
        public IHeaderCollection ResponseHeaders { get; } = new HeaderCollection();
        public string HttpVerb { get; private set; } = "GET";
        public int? ResponseStatusCode { get; private set; }
        public IJsonable ResponseBody { get; private set; }

        public InteractionBuilder(string state, string consumer, string provider, string description, Version version,
            IConsumerConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(consumer))
                throw new ArgumentException("Please provide a consumer name", nameof(consumer));
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentException("Please provide a provider name", nameof(provider));
            this._configuration = config;
            State = state;
            Consumer = consumer;
            Provider = provider;
            Description = description;
            Version = version ?? new Version(1, 0);
        }

        IRequestBuilder IRequestPathBuilder.WhenRequesting(string path)
        {
            RequestPath = path;
            return this;
        }

        IRequestBuilder IRequestBuilder.WithQuery(string key, string value)
        {
            _queries.Add($"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)}");
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


        IRequestBuilder IMessageBuilder<IRequestBuilder>.WithBody(object body)
        {
            RequestBody = new JsonBody(body);
            return this;
        }

        IResponseBuilder IRequestBuilder.ThenRespondsWith(int statusCode)
        {
            ResponseStatusCode = statusCode;
            return this;
        }

        IResponseBuilder IRequestBuilder.ThenRespondsWith(HttpStatusCode statusCode) =>
            ((IRequestBuilder) this).ThenRespondsWith((int) statusCode);

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

        IResponseBuilder IMessageBuilder<IResponseBuilder>.WithBody(object body)
        {
            ResponseBody = new JsonBody(body);
            return this;
        }

        async Task<IPact> IResponseBuilder.InPact()
        {
            _handler = await Context.RegisterListener(this, _configuration);
            return this;
        }

        void IPact.Verify()
        {
            var matches = _handler.VerifyAndClose();
            if (matches != 1) throw new Exception($"The pact was matched {matches} times. Expected one.");
        }

        private async Task Save()
        {
            using (var repo = new PactRepo(_configuration))
            {
                await repo.Put(this);
            }
        }

        async Task IPact.VerifyAndSave()
        {
            var pact = (IPact) this;
            pact.Verify();
            await Save();
        }

        IRequestBuilder IMessageBuilder<IRequestBuilder>.WithHeader(string key, params string[] values)
        {
            RequestHeaders.Add(key, values);
            return this;
        }

        IEnumerable<IPactInteractionDefinition> IPactDefinition.Interactions => new[] {this};

        public void Dispose() => _handler.VerifyAndClose();

        public override string ToString() => new PactJsonRenderer(this).ToString();
    }
}