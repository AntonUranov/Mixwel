using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Mixwel.Infrastructure.Health
{
    public class RedisHealth : IHealthCheck
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _multiplexer;

        public RedisHealth(IDatabase database, IConnectionMultiplexer multiplexer)
        {
            _database = database;
            _multiplexer = multiplexer;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_multiplexer.IsConnected) 
                {
                    return HealthCheckResult.Unhealthy(GetProblemDescription(" Redis multiplexer is not connected."));
                }

                var indexes = await (_database.ExecuteAsync("FT._LIST"));
                if (((RedisResult[])indexes).Any(x => (string)x == RedisService.RoutesIndexName))
                {
                    return HealthCheckResult.Healthy();
                }

                return HealthCheckResult.Unhealthy(GetProblemDescription($"Index {RedisService.RoutesIndexName} is not found."));
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(GetProblemDescription(e.Message));
            }
        }

        private string GetProblemDescription(string message) 
        {
            return $"HealthCheck:{nameof(RedisHealth)}. Message:{message}";
        }
    }
}
