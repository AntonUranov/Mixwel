using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Mixwel.Domain;
using Mixwel.Domain.Interfaces;
using Mixwel.Providers.ProviderOneSearch;
using Mixwel.Providers.ProviderTwoSearch;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using StackExchange.Redis;
using System.Net;
using System.Net.Http;

namespace Mixwel
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection RegisterDependencies(this IServiceCollection services)
        {
            services.AddScoped<ProviderOneSearchService>();
            services.AddScoped<ProviderTwoSearchService>();
            services.AddScoped<IAggregateSearchService, AggregateSearchService>(x =>
                new AggregateSearchService(
                    new ISearchService[]
                    {
                        x.GetService<ProviderOneSearchService>()!,
                        x.GetService<ProviderTwoSearchService>()!,
                    },
                    x.GetService<IDatabase>()!,
                    x.GetService<StackExchange.Redis.IServer>()!,
                    x.GetService<ILogger<AggregateSearchService>>()!));

            return services;
        }

        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient<ISearchService, ProviderOneSearchService>();

            services.AddHttpClient<ISearchService, ProviderTwoSearchService>();

            return services;
        }

        public static IServiceCollection RegisterRedis(this IServiceCollection services, Func<string?> configureConnection) 
        {
            IConnectionMultiplexer multiplexer = ConnectionMultiplexer
                    .Connect(configureConnection?.Invoke() ?? string.Empty);

            services.AddScoped<IDatabase>(cfg =>
            {
                return multiplexer.GetDatabase();
            });

            services.AddScoped<StackExchange.Redis.IServer>(cfg => 
            {
                //get default server
                StackExchange.Redis.IServer? defaultServer = multiplexer.GetServers()
                        .FirstOrDefault();

                return defaultServer!;
            });

            return services;
        }

    }
}
