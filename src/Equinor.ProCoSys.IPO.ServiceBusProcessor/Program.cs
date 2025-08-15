using Equinor.ProCoSys.IPO.ServiceBusProcessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
builder.Services.AddOptions<TopicSubscriptionOptions>().BindConfiguration(nameof(TopicSubscriptionOptions));
builder.Services.AddTopicSubscriptionServices();
builder.Services.AddHostedService<OrdersTopicProcessor>();

builder.Build().Run();
