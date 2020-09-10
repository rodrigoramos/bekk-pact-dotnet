using System;
using System.Runtime.CompilerServices;
using nPact.Common.Contracts;
using nPact.Consumer.Config;
using nPact.Consumer.Contracts;
using nPact.Consumer.Server;

namespace nPact.Consumer.Builders
{
    public class PactBuilder : IPactBuilder, IConsumerBuilder
    {
        private readonly string _description;
        private string _consumer;
        private string _provider;
        private Version _version;
        private IConsumerConfiguration _configuration;

        private PactBuilder(string description, IConsumerConfiguration config)
        {
            if(string.IsNullOrWhiteSpace(description))throw new ArgumentException("Please provide a description", nameof(description));
            this._description = description;
            _configuration = MergedConfiguration.MergeConfigs(Context.Configuration, config);
        }

        /// <summary>
        /// Call this method to create a pact interaction instance.
        /// </summary>
        /// <remarks>
        /// This should be complemented with an instance of <seealso cref="Context"/>, shared across all tests/pacts.
        /// <list type="bullet">
        /// <listheader><description>Please provide the builder with:</description></listheader>
        /// <item>Consumer: Either with <see cref="IPactBuilder.ForConsumer"/>, <see cref="IConsumerBuilder.And"/> or <seealso cref="Context.ForConsumer"/>.</item>
        /// <item>Provider: Either with <see cref="IPactBuilder.ForProvider"/> or <see cref="IPactBuilder.Between"/>.</item>
        /// <item>Version (optional): Either with <see cref="IPactBuilder.WithVersion"/>, <see cref="IPactBuilder.With(Version)"/> or in <seealso cref="Context"/>.</item>
        /// <item>Configuration: Either with <see cref="IPactBuilder.With(IConsumerConfiguration)"/> or in <seealso cref="Context"/>.</item> 
        /// </list>
        /// Provide provider state by calling <see cref="IPactBuilder.WithProviderState"/> or <see cref="IPactBuilder.Given"/>.
        /// </remarks>
        /// <param name="description">
        /// The description of the interaction. 
        /// If <c>null</c>, the value of <paramref name="methodName"/> (the name of the calling method) is used.
        /// </param>
        /// <param name="methodName">This value is set automatically by compiler services.</param>
        /// <returns>A pact builder instance.</returns>
        public static IPactBuilder Build(string description = null, [CallerMemberName]string methodName = "") => new PactBuilder(description ?? methodName, null);
        IPactBuilder IPactBuilder.With(IConsumerConfiguration config)
        {
            _configuration = MergedConfiguration.MergeConfigs(_configuration, config);
            return this;
        }
        IPactBuilder IPactBuilder.With(Version version)
        {
            this._version = version;
            return this;
        }
        IPactBuilder IPactBuilder.WithVersion(string version) => ((IPactBuilder)this).With(Version.Parse(version));
        IPactBuilder IPactBuilder.ForConsumer(string name)
        {
            _consumer = name;
            return this;
        }
        IPactBuilder IPactBuilder.ForProvider(string name)
        {
            _provider = name;
            return this;
        }
        IConsumerBuilder IPactBuilder.Between(string provider)
        {
            this._provider = provider;
            return this;
        }
        IRequestPathBuilder IPactBuilder.Given(string state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return new InteractionBuilder(state, _consumer ?? Context.ConsumerName, _provider, _description, _version ?? Context.Version, _configuration ?? new Configuration());
        }
        IRequestPathBuilder IPactBuilder.WithProviderState(string state) => ((IPactBuilder)this).Given(state);
        IPactBuilder IConsumerBuilder.And(string consumer)
        {
            this._consumer = consumer;
            return this;
        }
    }
}
