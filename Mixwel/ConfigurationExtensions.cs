using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Mixwel.Domain;
using Mixwel.Domain.Interfaces;
using Mixwel.Infrastructure;
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
            services.AddScoped<ICacheService, RedisService>();
            services.AddScoped<ISearchRoutesService, SearchRoutesService>(x =>
                new SearchRoutesService(
                    new ISearchService[]
                    {
                        x.GetService<ProviderOneSearchService>()!,
                        x.GetService<ProviderTwoSearchService>()!,
                    },
                    x.GetService<ICacheService>()!,
                    x.GetService<ILogger<SearchRoutesService>>()!));

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
            var connectionStrig = configureConnection?.Invoke() ?? string.Empty;
            ArgumentException.ThrowIfNullOrEmpty(connectionStrig);
            IConnectionMultiplexer multiplexer = ConnectionMultiplexer
                    .Connect(connectionStrig);
            services.AddScoped<IDatabase>(cfg => multiplexer.GetDatabase());
            //default server
            services.AddScoped<StackExchange.Redis.IServer>(cfg =>
                multiplexer.GetServers().Single());

            return services;
        }

    }
}
