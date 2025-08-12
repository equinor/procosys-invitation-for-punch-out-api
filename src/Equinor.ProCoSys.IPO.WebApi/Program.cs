using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Azure.Identity;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Swagger;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Query;
using Equinor.ProCoSys.IPO.WebApi.DIModules;
using Equinor.ProCoSys.IPO.WebApi.Extensions;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using Equinor.ProCoSys.IPO.WebApi.Seeding;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Equinor.ProCoSys.PcsServiceBus;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Fusion.Integration;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

builder.ConfigureAzureAppConfig();

builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = null;
});

builder.ConfigureDatabase();

if (environment.IsDevelopment())
{
    DebugOptions.DebugEntityFrameworkInDevelopment = configuration.GetValue<bool>("DebugEntityFrameworkInDevelopment");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        configuration.Bind("API", options);
    });

builder.ConfigureHttp();

if (configuration.GetValue<bool>("UseAzureAppConfiguration"))
{
    builder.Services.AddAzureAppConfiguration();
}

builder.Services.AddFluentValidationAutoValidation(fv =>
{
    fv.DisableDataAnnotationsValidation = true;
});

builder.Services.AddValidatorsFromAssemblies(new List<Assembly>
{
    typeof(IQueryMarker).GetTypeInfo().Assembly,
    typeof(ICommandMarker).GetTypeInfo().Assembly,
    typeof(Program).Assembly
});

builder.ConfigureSwagger();

builder.Services.AddFluentValidationRulesToSwagger();

builder.Services.AddPcsAuthIntegration();

builder.AddFusionIntegration();

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
});

builder.Services.AddMediatrModules();
builder.Services.AddApplicationModules(configuration);

var serviceBusEnabled = configuration.GetValue<bool>("ServiceBus:Enable") &&
    (!environment.IsDevelopment() || configuration.GetValue<bool>("ServiceBus:EnableInDevelopment"));
if (serviceBusEnabled)
{
    // Env variable used in kubernetes. Configuration is added for easier use locally
    // Url will be validated during startup of service bus integration and give a
    // Uri exception if invalid.
    var leaderElectorUrl = Environment.GetEnvironmentVariable("LEADERELECTOR_SERVICE") ?? (configuration["ServiceBus:LeaderElectorUrl"]);

    builder.Services.AddPcsServiceBusIntegration(options => options
        .UseBusConnection(configuration.GetConnectionString("ServiceBus"))
        .WithLeaderElector(leaderElectorUrl)
        .WithRenewLeaseInterval(int.Parse(configuration["ServiceBus:LeaderElectorRenewLeaseInterval"]))
        .WithSubscription(PcsTopicConstants.Ipo, "ipo_ipo")
        .WithSubscription(PcsTopicConstants.Project, "ipo_project")
        .WithSubscription(PcsTopicConstants.CommPkg, "ipo_commpkg")
        .WithSubscription(PcsTopicConstants.McPkg, "ipo_mcpkg")
        .WithSubscription(PcsTopicConstants.Library, "ipo_library")
        .WithSubscription(PcsTopicConstants.Certificate, "ipo_certificate")
        //THIS METHOD SHOULD BE FALSE IN NORMAL OPERATION.
        //ONLY SET TO TRUE WHEN A LARGE NUMBER OF MESSAGES HAVE FAILED AND ARE COPIED TO DEAD LETTER.
        //WHEN SET TO TRUE, MESSAGES ARE READ FROM DEAD LETTER QUEUE INSTEAD OF NORMAL QUEUE
        .WithReadFromDeadLetterQueue(configuration.GetValue("ServiceBus:ReadFromDeadLetterQueue", defaultValue: false)));

    var topics = configuration["ServiceBus:TopicNames"];
    builder.Services.AddTopicClients(configuration.GetConnectionString("ServiceBus"), topics);
}
else
{
    builder.Services.AddSingleton<IPcsBusSender>(new DisabledServiceBusSender());
}

builder.Services.AddHostedService<VerifyApplicationExistsAsPerson>();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (configuration.GetValue<bool>("UseAzureAppConfiguration"))
{
    app.UseAzureAppConfiguration();
}

if (environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseGlobalExceptionHandling();

app.UseCors(ConfigureHttpExtension.AllowAllOriginsCorsPolicy);

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProCoSys IPO API V1");
    c.DocExpansion(DocExpansion.List);
    c.DisplayRequestDuration();

    c.OAuthClientId(configuration["Swagger:ClientId"]);
    c.OAuthAppName("ProCoSys IPO API V1");
    c.OAuthScopeSeparator(" ");
    c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "resource", configuration["API:Audience"] } });
});

app.UseHttpsRedirection();

app.UseRouting();

// Order of adding middlewares is crucial. Some depend that other has been run in advance
app.UseCurrentPlant();
app.UseCurrentBearerToken();
app.UseAuthentication();
app.UseCurrentUser();
app.UsePersonValidator();
app.UsePlantValidator();
app.UseVerifyOidInDb();
app.UseAuthorization();

app.UseResponseCompression();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Run the application
app.Run();

public abstract partial class Program;
