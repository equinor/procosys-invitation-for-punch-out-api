using System.Collections.Generic;
using System.Reflection;
using Azure.Identity;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Query;
using Equinor.ProCoSys.IPO.WebApi.DIModules;
using Equinor.ProCoSys.IPO.WebApi.Extensions;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

// TODO replace debug credential
var credential = new WorkloadIdentityCredential(new WorkloadIdentityCredentialOptions { ClientId = "98cb737b-5c00-4ae6-962b-29c562b7ea21" });
// var credential = new DefaultAzureCredential();
builder.ConfigureAzureAppConfig(credential);

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

// TODO replace debug
if (true)
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

builder.ConfigureFusionIntegration();

builder.Services.ConfigureTelemetry(configuration, credential);

builder.Services.AddMediatrModules();
builder.Services.AddApplicationModules(configuration);

builder.ConfigureServiceBus();

builder.Services.AddHostedService<VerifyApplicationExistsAsPerson>();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
// TODO replace debug
if (true)
{
    app.UseAzureAppConfiguration();
}

if (environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseGlobalExceptionHandling();

app.UseCors(ConfigureHttpExtension.AllowAllOriginsCorsPolicy);

app.ConfigureSwagger(configuration);

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

app.MapControllers();

// Run the application
app.Run();

public partial class Program;
