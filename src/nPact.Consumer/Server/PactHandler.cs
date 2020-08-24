using System;
using nPact.Common.Contracts;
using nPact.Common.Extensions;
using Newtonsoft.Json.Linq;
using nPact.Consumer.Contracts;
using nPact.Consumer.Matching;

namespace nPact.Consumer.Server
{
    class PactHandler : IPactResponder, IVerifyAndClosable
    {
        private readonly PactComparer matcher;
        private readonly IPactInteractionDefinition pact;
        private readonly Action unregister;
        private int matches;

        public PactHandler(
            IPactInteractionDefinition pact, 
            IConsumerConfiguration config,
            Action<PactHandler> unregister)
        {
            this.matcher = new PactComparer(pact, config);
            this.pact = pact;
            Configuration = config;
            this.unregister = () => unregister(this);
        }

        public IConsumerConfiguration Configuration { get; }
        public JObject DiffGram(IPactRequestDefinition request) => matcher.DiffGram(request);

        public IPactResponseDefinition Respond(IPactRequestDefinition request)
        {
            if(matcher.Matches(request))
            {
                Configuration.LogSafe(LogLevel.Info, $"Request received at {request.RequestPath} matching expectation.");
                matches++;
                return pact;
            }
            return null;
        }
        public int VerifyAndClose(int expectedMatches = 1)
        {
            unregister();
            return matches;
        }
    }
}