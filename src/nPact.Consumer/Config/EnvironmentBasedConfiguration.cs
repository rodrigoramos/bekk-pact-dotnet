using System;
using nPact.Common.Contracts;

namespace nPact.Consumer.Config
{
    public class EnvironmentBasedConfiguration : nPact.Common.Config.EnvironmentBasedConfigurationBase, IConsumerConfiguration
    {
        public Uri MockServiceBaseUri => GetUriValue("Consumer", nameof(MockServiceBaseUri));
    }
}