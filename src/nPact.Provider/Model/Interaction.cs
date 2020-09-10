using System;
using Newtonsoft.Json;
using nPact.Common.Contracts;

namespace nPact.Provider.Model
{
    public class Interaction : IPactInformation
    {
        public string Description { get; set; }
        [JsonProperty("provider_state")]
        public string ProviderState { get; set; }
        public virtual Request Request { get; set; }
        public virtual Response Response { get; set; }
        public string Consumer { get; set; }
        public DateTime Created { get; set; }
    }
}