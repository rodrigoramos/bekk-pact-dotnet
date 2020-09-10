using Newtonsoft.Json.Linq;

namespace nPact.Common.Contracts
{
    public interface IJsonable
    {
         JContainer Render();
    }
}