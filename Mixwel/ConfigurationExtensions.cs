using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Mixwel.Domain;
using Mixwel.Domain.Interfaces;
using Mixwel.Providers.ProviderOneSearch;
using Mixwel.Providers.ProviderTwoSearch;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http;

namespace Mixwel
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection RegisterDependencies(this IServiceCollection services)
        {
            services.AddScoped<ProviderOneSearchService>();
            services.AddScoped<ProviderTwoSearchService>();
            services.AddScoped<ICompositeSearchService, CompositeSearchService>(x => new  CompositeSearchService(
                new ISearchService[] 
                {
                    x.GetService<ProviderOneSearchService>()!,
                    x.GetService<ProviderTwoSearchService>()!,
                },
                x.GetService<ILogger<CompositeSearchService>>()!));

            return services;
        }

        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient<ISearchService, ProviderOneSearchService>();

            services.AddHttpClient<ISearchService, ProviderTwoSearchService>();

            return services;
        }

    }
}
