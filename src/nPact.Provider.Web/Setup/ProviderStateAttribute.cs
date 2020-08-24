using System;

namespace nPact.Provider.Web.Setup
{
    /// <summary>
    /// Use this attribute to decorate methods for setting up
    /// for each provider state in classes inheriting from 
    /// <seealso cref="ProviderStateSetupBase" />.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ProviderStateAttribute: Attribute
    {
        public ProviderStateAttribute(string state)
        {
            State = state;
        }
        public string State { get; private set; }
    }
}