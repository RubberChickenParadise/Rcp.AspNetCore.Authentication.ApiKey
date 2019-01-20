using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rcp.AspNetCore.Authentication.ApiKey;
using Rcp.AspNetCore.Authentication.ApiKey.Events;

namespace ExampleAndTestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthentication(x =>
                                       {
                                           x.DefaultAuthenticateScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
                                           x.DefaultChallengeScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
                                       })
                    .AddApiKey(options =>
                               {
                                   options.EnableLogging = false;
                                   options.Events = new ApiKeyAuthenticationEvents
                                                    {
                                                        OnValidateCredentials = async context =>
                                                                                {
                                                                                    if (context.ApiKey == "SpecificClaim")
                                                                                    {
                                                                                        context.Principal =
                                                                                            new
                                                                                                ClaimsPrincipal(new
                                                                                                                    ClaimsIdentity(new
                                                                                                                                   []
                                                                                                                                   {
                                                                                                                                       new
                                                                                                                                           Claim("TestClaim",
                                                                                                                                                 "Test"),
                                                                                                                                   },
                                                                                                                                   ApiKeyAuthenticationDefaults
                                                                                                                                       .AuthenticationScheme,
                                                                                                                                   "TestUser",
                                                                                                                                   "user"));

                                                                                        context.Success();
                                                                                    }
                                                                                    else if (context.ApiKey == "GeneralAuth"
                                                                                        )
                                                                                    {
                                                                                        context.Principal =
                                                                                            new
                                                                                                ClaimsPrincipal(new
                                                                                                                    ClaimsIdentity(ApiKeyAuthenticationDefaults
                                                                                                                                       .AuthenticationScheme,
                                                                                                                                   "TestUser",
                                                                                                                                   "user"));

                                                                                        context.Success();
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        context
                                                                                            .Fail(new
                                                                                                      AuthenticationException());
                                                                                    }
                                                                                }
                                                    };
                               });

            services.AddAuthorization(configure =>
                                      {
                                          configure.AddPolicy("TestPolicy",
                                                              policy =>
                                                              {
                                                                  policy.AuthenticationSchemes = new List<string>
                                                                                                 {
                                                                                                     ApiKeyAuthenticationDefaults
                                                                                                         .AuthenticationScheme
                                                                                                 };
                                                                  policy.RequireClaim("TestClaim");
                                                              });
                                      });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
