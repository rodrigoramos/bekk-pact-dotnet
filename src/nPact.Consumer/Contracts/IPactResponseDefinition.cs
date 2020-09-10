using nPact.Common.Contracts;

namespace nPact.Consumer.Contracts
{
    interface IPactResponseDefinition
    {
        IHeaderCollection ResponseHeaders { get; }
        int? ResponseStatusCode { get; }
        IJsonable ResponseBody { get; }
    }
}