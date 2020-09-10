using nPact.Common.Contracts;

namespace nPact.Consumer.Contracts
{
    interface IPactInteractionDefinition : IPactRequestDefinition, IPactResponseDefinition, IPactPathMetadata
    {
        string State { get; }
        string Description { get; }
    }
}