using nPact.Common.Contracts;
using Newtonsoft.Json.Linq;

namespace nPact.Provider.Config
{
    /// <summary>
    /// Use this class to build a configuration object.
    /// </summary>
    public class Configuration : nPact.Common.Config.Configuration<Configuration>, IProviderConfiguration
    {
        private Configuration(IProviderConfiguration inner) : base(inner)
        {
        }
        public static Configuration With => new Configuration(FromEnvironmentVartiables());
        public static IProviderConfiguration FromEnvironmentVartiables() => new EnvironmentBasedConfiguration();
        protected override void ReadFromJson(JToken json)
        {
            var data = json?["Provider"];
            if(data != null)
            {
                //Read data
            }
        }       
    }
}
