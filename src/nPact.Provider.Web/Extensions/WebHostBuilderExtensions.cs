using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using nPact.Provider.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nPact.Provider.Web.Config;
using nPact.Provider.Web.Contracts;
using Microsoft.AspNetCore.TestHost;

namespace nPact.Provider.Web.Extensions
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseStartup(this IWebHostBuilder hostBuilder,
            IPact pact, Action<WebHostBuilderContext, IConfigurationBuilder> configureAppConfig = null,
            IEnumerable<Claim> claims = null,
            Action<IServiceCollection> configureServices = null,
            Action<IApplicationBuilder> configure = null,
            Type startupType = null)
        {
            if (pact == null) throw new ArgumentNullException(nameof(pact));
            if (pact.Configuration != null)
            {
                hostBuilder.ConfigureLogging(factory =>
                {
                    var log = new LogWrapper(pact.Configuration);
                    factory.AddProvider(log);
                });
            }

            var startup = new Startup(startupType)
            {
                ConfigureCallback = configure,
                ConfigureServicesCallback = configureServices
            };
            
            foreach (var claim in claims ?? Enumerable.Empty<Claim>())
            {
                startup.AddClaim(claim);
            }

            hostBuilder.ConfigureTestServices(sc => startup.ConfigureServices(sc));
            hostBuilder.Configure(startup.Configure);
            hostBuilder.UseStartup(startupType);
            
            if (configureAppConfig != null)
                hostBuilder.ConfigureAppConfiguration(configureAppConfig);
            
            return hostBuilder;
        }

        public static IWebHostBuilder UseStartup<T>(this IWebHostBuilder hostBuilder,
            IPact pact, Action<WebHostBuilderContext, IConfigurationBuilder> configureAppConfig,
            IEnumerable<Claim> claims = null,
            Action<IServiceCollection> configureServices = null,
            Action<IApplicationBuilder> configure = null) where T : class
        {
            return UseStartup(hostBuilder, pact, configureAppConfig, claims, configureServices, configure, typeof(T));
        }

        public static IWebHostBuilder UseStartup<T>(this IWebHostBuilder hostBuilder,
            IPact pact,
            ProviderStateSetup providerStateSetup) where T : class
        {
            return UseStartup<T>(
                hostBuilder,
                pact, 
                providerStateSetup.ConfigureAppConfig(),
                providerStateSetup.GetClaims(pact.ProviderState),
                providerStateSetup.ConfigureServices(pact.ProviderState),
                providerStateSetup.Configure());
        }
    }
}