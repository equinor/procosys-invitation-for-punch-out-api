﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Query;
using Equinor.ProCoSys.IPO.WebApi.DIModules;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Equinor.ProCoSys.IPO.WebApi.Seeding;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Equinor.ProCoSys.PcsServiceBus;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Equinor.ProCoSys.IPO.WebApi
{
    public class Startup
    {
        private const string AllowAllOriginsCorsPolicy = "AllowAllOrigins";
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            _environment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_environment.IsDevelopment() || _environment.IsTest())
            {
                if (Configuration.GetValue<bool>("MigrateDatabase"))
                {
                    services.AddHostedService<DatabaseMigrator>();
                }
            }
            if (_environment.IsDevelopment())
            {
                if (Configuration.GetValue<bool>("SeedDummyData"))
                {
                    services.AddHostedService<Seeder>();
                }
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        Configuration.Bind("API", options);
                    });

            services.AddCors(options =>
            {
                options.AddPolicy(AllowAllOriginsCorsPolicy,
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            }).AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddControllers()
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblies
                    (
                        new List<Assembly>
                        {
                            typeof(IQueryMarker).GetTypeInfo().Assembly,
                            typeof(ICommandMarker).GetTypeInfo().Assembly,
                            typeof(Startup).Assembly,
                        }
                    );
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                });

            var scopes = Configuration.GetSection("Swagger:Scopes")?.Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProCoSys IPO API", Version = "v1" });

                //Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(Configuration["Swagger:AuthorizationUrl"]),
                            Scopes = scopes
                        }
                    }
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        scopes.Keys.ToArray()
                    }
                });

                c.OperationFilter<AddRoleDocumentation>();

                c.AddFluentValidationRules();
            });

            services.ConfigureSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName);
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            services.AddFusionIntegration(options =>
            {
                options.UseServiceInformation("PCS IPO", _environment.EnvironmentName); // Environment identifier
                options.UseDefaultEndpointResolver(Configuration["Meetings:Environment"]);                               // Fusion environment "fprd" = prod, "fqa" = qa, "ci" = dev/test etc
                options.UseDefaultTokenProvider(opts =>
                {
                    opts.ClientId = Configuration["Meetings:ClientId"];                  // Application client ID
                    opts.ClientSecret = Configuration["Meetings:ClientSecret"];          // Application client secret
                });
                options.AddMeetings(s => s.SetHttpClientTimeout(
                    TimeSpan.FromSeconds(Configuration.GetValue<double>("FusionRequestTimeout")),
                    TimeSpan.FromSeconds(Configuration.GetValue<double>("FusionTotalTimeout"))));
                options.DisableClaimsTransformation();                                  // Disable this - Fusion adds relevant claims
            });

            services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:InstrumentationKey"]);
            services.AddMediatrModules();
            services.AddApplicationModules(Configuration);
            if (Configuration.GetValue<bool>("ServiceBus:Enable"))
            {
                // Env variable used in kubernetes. Configuration is added for easier use locally
                // Url will be validated during startup of service bus integration and give a
                // Uri exception if invalid.
                var leaderElectorUrl = "http://" + (Environment.GetEnvironmentVariable("LEADERELECTOR_SERVICE") ?? Configuration["ServiceBus:LeaderElectorUrl"]) + ":3003";

                services.AddPcsServiceBusIntegration(options => options
                    .UseBusConnection(Configuration.GetConnectionString("ServiceBus"))
                    .WithLeaderElector(leaderElectorUrl)
                    .WithRenewLeaseInterval(int.Parse(Configuration["ServiceBus:LeaderElectorRenewLeaseInterval"]))
                    .WithSubscription(PcsTopic.Ipo, "ipo_ipo")
                    .WithSubscription(PcsTopic.Project, "ipo_project")
                    .WithSubscription(PcsTopic.CommPkg, "ipo_commpkg")
                    .WithSubscription(PcsTopic.McPkg, "ipo_mcpkg"));
                
                services.AddTopicClients(
                    Configuration.GetConnectionString("ServiceBus"),
                    Configuration["ServiceBus:TopicNames"]);
            }
            else
            {
                services.AddSingleton<IPcsBusSender>(new DisabledServiceBusSender());
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseGlobalExceptionHandling();

            app.UseCors(AllowAllOriginsCorsPolicy);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProCoSys IPO API V1");
                c.DocExpansion(DocExpansion.List);
                c.DisplayRequestDuration();

                c.OAuthClientId(Configuration["Swagger:ClientId"]);
                c.OAuthAppName("ProCoSys IPO API V1");
                c.OAuthScopeSeparator(" ");
                c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "resource", Configuration["API:Audience"] } });
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCurrentPlant();
            app.UseCurrentBearerToken();
            app.UseAuthentication();
            app.UseCurrentUser();
            app.UsePlantValidator();
            app.UseVerifyOidInDb();
            app.UseAuthorization();

            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
