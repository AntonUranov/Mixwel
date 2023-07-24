using Microsoft.Extensions.Diagnostics.HealthChecks;
using Mixwel.Domain.Interfaces;

namespace Mixwel.Infrastructure.Health
{
    public class ProvidersHealth : IHealthCheck
    {
        private readonly ISearchRoutesService _searchRoutesService;

        public ProvidersHealth(ISearchRoutesService searchRoutesService)
        {
            _searchRoutesService = searchRoutesService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (await _searchRoutesService.IsAvailableAsync(cancellationToken)) 
            {
                return HealthCheckResult.Healthy();
            }

            return HealthCheckResult.Unhealthy();
        }
    }
}
