using System.Net;
using System.Threading.Tasks;

namespace nPact.Consumer.Contracts
{
    public interface IResponseBuilder : IMessageBuilder<IResponseBuilder>
    {
       
        /// <summary>
        /// Call this method to create a pact object and start listening for a request.
        /// This method must be awaited.
        /// </summary>
        /// <returns>A pact object. Dispose this object at the end of the test.</returns>
        Task<IPact> InPact();
    }
}