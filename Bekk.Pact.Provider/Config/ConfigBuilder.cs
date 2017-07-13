﻿using System;
using Bekk.Pact.Common.Contracts;

namespace Bekk.Pact.Provider.Config
{
    public class Configuration : IProviderConfiguration, IConsumerConfiguration
    {
        private Configuration()
        {
            comparison = StringComparison.CurrentCultureIgnoreCase;
        }
        public static Configuration With => new Configuration();

        public Configuration BrokerUri(Uri uri)
        {
            brokerUri = uri;
            return this;
        }

        public Configuration BrokerUri(Uri serverUri, string providerName) => BrokerUri(new Uri(serverUri,
            $"/pacts/provider/{providerName}/latest"));

        public Configuration Log(Action<string> log){
            this.log = log;
            return this;
        }

        public Configuration Comparison(StringComparison comparison)
        {
            this.comparison = comparison;
            return this;
        }

        public Configuration MockServiceUri(Uri uri)
        {
            this.mockServiceUri = uri;
            return this;
        }

        private Uri brokerUri;
        private Uri mockServiceUri = new Uri("http://localhost:1234");
        Uri IConfiguration.BrokerUri => brokerUri;

        private Action<string> log;
        Action<string> IConfiguration.Log => log;
        private StringComparison comparison;
        StringComparison IProviderConfiguration.BodyKeyStringComparison => comparison;
        Uri IConsumerConfiguration.MockServiceBaseUri => mockServiceUri;
    }
}