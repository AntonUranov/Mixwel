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
            services.AddTransient<ICacheService, RedisService>();
            services.AddScoped<ISearchRoutesService, SearchRoutesService>(x =>
                new SearchRoutesService(
                    new ISearchService[]
                    {
                        x.GetRequiredService<ProviderOneSearchService>()!,
                        x.GetRequiredService<ProviderTwoSearchService>()!,
                    },
                    x.GetRequiredService<ICacheService>()!,
                    x.GetRequiredService<ILogger<SearchRoutesService>>()!));

            return services;
        }

        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient<ISearchService, ProviderOneSearchService>();

            services.AddHttpClient<ISearchService, ProviderTwoSearchService>();

            return services;
        }

        public static IServiceCollection ConfigureRedis(this IServiceCollection services,
            Func<string?> configureConnection) 
        {
            var connectionStrig = configureConnection?.Invoke() ?? string.Empty;
            ArgumentException.ThrowIfNullOrEmpty(connectionStrig);

            IConnectionMultiplexer multiplexer = ConnectionMultiplexer
                    .Connect(connectionStrig);
            services.AddSingleton<IConnectionMultiplexer>(cfg => multiplexer);
            services.AddTransient<IDatabase>(cfg => multiplexer.GetDatabase());
            //default server
            services.AddTransient<StackExchange.Redis.IServer>(cfg =>
                multiplexer.GetServers().Single());



            return services;
        }

        public static async Task ConfigureRedis(this WebApplication app) 
        {
            var cache = app.Services.GetService<ICacheService>();
            var initResult =  await cache.Initialize();
            if (initResult.IsFailure) 
            {
                throw new ApplicationException(
                    $"Cache initialization is failed. Exception: {initResult.Error}");
            }
        }

    }
}
