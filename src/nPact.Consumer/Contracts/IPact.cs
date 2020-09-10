using System;
using System.Threading.Tasks;

namespace nPact.Consumer.Contracts
{
    public interface IPact : IDisposable
    {
        /// <summary>
        /// Call this method to verify that the request was replied to once.
        /// This also happens in the dispose method.
        /// </summary>
        void Verify();
        /// <summary>
        /// Call this method to verify and store the interaction in the repo.
        /// </summary>
        /// <remarks>It is preferred to use the context for this.</remarks>
        Task VerifyAndSave();
    }
}