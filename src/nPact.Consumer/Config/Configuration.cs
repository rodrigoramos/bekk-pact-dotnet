using System;
using nPact.Common.Config;
using nPact.Common.Contracts;
using Newtonsoft.Json.Linq;

namespace nPact.Consumer.Config
{
    /// <summary>
    /// Use this class to build a configuration object.
    /// Use <see cref="With"/> to create a new instance.
    /// </summary>
    public class Configuration : Configuration<Configuration>, IConsumerConfiguration
    {
        internal Configuration(): this(FromEnvironmentVartiables())
        {
        }
        internal Configuration(IConsumerConfiguration inner) : base(inner)
        {
           this.inner = inner;
           MockServiceBaseUri(new Uri("http://127.0.0.1:1234"));
        }
        /// <summary>
        /// Create a new instance.
        /// </summary>
        public static Configuration With => new Configuration(FromEnvironmentVartiables());

        /// <summary>
        /// Reads environment variables into a configuration object.
        /// </summary>
        public static IConsumerConfiguration FromEnvironmentVartiables() => new EnvironmentBasedConfiguration();
        
        private Uri mockServiceUri;
        private IConsumerConfiguration inner;

        /// <summary>
        /// Sets the value of <see cref="IConsumerConfiguration.MockServiceBaseUri"/>
        /// Must be parseable to an uri (absolute).
        /// </summary>
        public Configuration MockServiceBaseUri(string url) => MockServiceBaseUri(new Uri(url));

        /// <summary>
        /// Sets the value of <see cref="IConsumerConfiguration.MockServiceBaseUri"/>
        /// </summary>
        public Configuration MockServiceBaseUri(Uri uri)
        {
            mockServiceUri = uri;
            return this;
        }

        protected override void ReadFromJson(JToken json)
        {
            var data = json?["Consumer"];
            if(data != null)
            {
                Read(data, nameof(IConsumerConfiguration.MockServiceBaseUri), (Uri uri) => MockServiceBaseUri(uri));
            }
        }

        Uri IConsumerConfiguration.MockServiceBaseUri => inner?.MockServiceBaseUri ?? mockServiceUri;
    }

}