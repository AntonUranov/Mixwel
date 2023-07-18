using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Mixwel.Providers.ProviderOneSearch;
using Mixwel.Providers.ProviderTwoSearch;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http;
using TestTask;

namespace Mixwel
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection RegisterDependencies(this IServiceCollection services)
        {
            services.AddScoped<Providers.ProviderOneSearch.ProviderOneSearchService>();
            services.AddScoped<Providers.ProviderTwoSearch.ProviderTwoSearchService>();
            services.AddScoped<ICompositeSearchService, CompositeSearchService>(x => new  CompositeSearchService(
                new ISearchService[] 
                {
                    x.GetService<Providers.ProviderOneSearch.ProviderOneSearchService>()!,
                    x.GetService<Providers.ProviderTwoSearch.ProviderTwoSearchService>()!,
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
