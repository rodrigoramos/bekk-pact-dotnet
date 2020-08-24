namespace nPact.Consumer.Contracts
{
    public interface IConsumerBuilder
    {
        /// <summary>
        /// The consumer of the pact. (The client calling a service.)
        /// </summary>
        /// <param name="consumer">The name used to recognize this client.</param>
        IPactBuilder And(string consumer);
    }
}