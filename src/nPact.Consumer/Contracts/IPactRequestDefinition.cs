using nPact.Common.Contracts;

namespace nPact.Consumer.Contracts
{
    interface IPactRequestDefinition
    {
        string RequestPath { get; }
        string Query { get; }
        IHeaderCollection RequestHeaders { get; }
        string HttpVerb { get; }
        IJsonable RequestBody { get; }
    }
}