using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Common.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Equinor.ProCoSys.IPO.WebApi.Extensions;

public static class ConfigureSwaggerExtension
{
    public static void ConfigureSwagger(this WebApplicationBuilder builder)
    {
        var scopes = builder.GetSwaggerScopes();
        
        builder.Services.AddSwaggerGen(c =>
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
                        AuthorizationUrl = new Uri(builder.Configuration["Swagger:AuthorizationUrl"]),
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
        });
        
        builder.Services.ConfigureSwaggerGen(options =>
        {
            options.CustomSchemaIds(x => x.FullName);
        });
    }
    
    public static void ConfigureSwagger(this IApplicationBuilder app, IConfiguration configuration)
    {
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
    }

    private static IDictionary<string, string> GetSwaggerScopes(this WebApplicationBuilder builder)
    {
        var scopes = builder.Configuration.GetSection("Swagger:Scopes").Get<Dictionary<string, string>>();
        if (scopes != null)
        {
            return scopes;
        }
        
        return new Dictionary<string, string>();
    }
}
