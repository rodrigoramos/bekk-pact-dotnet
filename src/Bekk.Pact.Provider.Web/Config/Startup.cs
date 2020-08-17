using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Bekk.Pact.Provider.Web.Config
{
    public class Startup
    {
        private readonly Type inner;

        private List<Claim> claims = new List<Claim>();
        // private StartupMethods methods;

        public Startup(Type inner)
        {
            this.inner = inner;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ConfigureServicesCallback?.Invoke(services);
            if (inner != null)
            {
                services.AddMvc().AddApplicationPart(inner.GetTypeInfo().Assembly).AddControllersAsServices();
            }

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            ConfigureClaims(app);
            ConfigureCallback?.Invoke(app);
        }


        public Action<IApplicationBuilder> ConfigureCallback { private get; set; }
        public Action<IServiceCollection> ConfigureServicesCallback { private get; set; }

        public Startup AddClaim(Claim claim)
        {
            claims.Add(claim);
            return this;
        }

        public Startup AddClaim(string type, string value) => AddClaim(new Claim(type, value));


        private void ConfigureClaims(IApplicationBuilder app)
        {
            if (!claims.Any()) return;
            app.Use(async (ctx, next) =>
            {
                ctx.User = new ClaimsPrincipal(new[] {new ClaimsIdentity(claims, "Bearer")});
                await next.Invoke();
            });
        }
    }
}