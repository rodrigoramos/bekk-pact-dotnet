using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using nPact.Common.Contracts;
using nPact.Consumer.Contracts;

namespace nPact.Consumer.Matching
{
    class PactGrouper : IEnumerable<IPactDefinition>, IEqualityComparer<IPactPathMetadata>
    {
        private readonly IEnumerable<IPactInteractionDefinition> _pacts;

        public PactGrouper(IPactInteractionDefinition pact)
        {
            _pacts = new []{ pact};
        }

        public PactGrouper(IEnumerable<IPactInteractionDefinition> pacts)
        {
            _pacts = pacts;
        }
        public IEnumerator<IPactDefinition> GetEnumerator()
        {
            var result = _pacts.GroupBy(p => (IPactPathMetadata)p, this).Select(g => new Merged(g.Key, g));
            return result.GetEnumerator();
        }

        bool IEqualityComparer<IPactPathMetadata>.Equals(IPactPathMetadata x, IPactPathMetadata y)
        {
            if(x == null) return y == null;
                if(y == null) return x == null;
                return x.Consumer == y.Consumer && x.Provider == y.Provider && x.Version == y.Version;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IEqualityComparer<IPactPathMetadata>.GetHashCode(IPactPathMetadata obj)
        {
            if(obj == null) return 0;
                return obj.Consumer.GetHashCode() + obj.Provider.GetHashCode() + obj.Version.GetHashCode();
        }

        private class Merged : IPactDefinition
        {
            private readonly IPactPathMetadata _metadata;
            public Merged(IPactPathMetadata metadata, IEnumerable<IPactInteractionDefinition> interactions)
            {
                Interactions = interactions;
                _metadata = metadata;
            }

            public IEnumerable<IPactInteractionDefinition> Interactions {get;}
            public string Provider => _metadata.Provider;
            public string Consumer => _metadata.Consumer;
            public Version Version => _metadata.Version;
        }
    }
}