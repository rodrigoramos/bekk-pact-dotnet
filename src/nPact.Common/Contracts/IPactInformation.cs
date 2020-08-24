namespace nPact.Common.Contracts
{
    public interface IPactInformation
    {
        /// <summary>
        /// The consumer defining the pact.
        /// </summary>
        string Consumer { get; }
        /// <summary>
        /// The description of the pact
        /// </summary>
        string Description { get; }
        /// <summary>
        /// The provider satte property of the pact
        /// </summary>
        string ProviderState { get; }
    }
}