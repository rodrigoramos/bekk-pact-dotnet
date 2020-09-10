using Newtonsoft.Json.Linq;

namespace nPact.Consumer.Contracts
{
    interface IPactResponder
    {
         IPactResponseDefinition Respond(IPactRequestDefinition request);
    }
}