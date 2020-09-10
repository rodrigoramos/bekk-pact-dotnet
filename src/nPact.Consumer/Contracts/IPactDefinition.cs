using System.Collections.Generic;
using nPact.Common.Contracts;

namespace nPact.Consumer.Contracts
{
    interface IPactDefinition : IPactPathMetadata
    {
        IEnumerable<IPactInteractionDefinition> Interactions { get; }
    }
}