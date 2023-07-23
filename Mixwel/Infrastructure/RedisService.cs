using StackExchange.Redis;
using System.Globalization;
using System.Text.Json;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Infrastructure
{
    public class RedisService: ICacheService
    {
        private const string keyPrefix = "route:";
        private const string hashName = "hash";

        private readonly IDatabase _database;

        public RedisService(IDatabase database)
        {
            _database = database;
        }

        public async Task<Guid> UpdateCacheAndGetRouteId(Route route)
        {
            Guid? cachedRouteId = GetRouteIdFromCache(route);
            if (cachedRouteId.HasValue)
            {
                return cachedRouteId.Value;
            }

            Guid newRouteId = Guid.NewGuid();
            string key = $"{keyPrefix}{newRouteId}";
            await _database.HashSetAsync(key, new HashEntry[]
            {
                new HashEntry(nameof(route.Origin), route.Origin),
                new HashEntry(nameof(route.Destination), route.Destination),
                new HashEntry(nameof(route.OriginDateTime), route.OriginDateTime.Ticks),
                new HashEntry(nameof(route.DestinationDateTime), route.DestinationDateTime.Ticks),
                new HashEntry(nameof(route.Price), route.Price.ToString(CultureInfo.InvariantCulture)),
                new HashEntry(nameof(route.TimeLimit), route.TimeLimit.Ticks),
                new HashEntry(hashName, route.GetHashCode())
            });

            //await _cache.KeyExpireAsync(key, TimeSpan.FromMinutes(10));

            return newRouteId;
        }

        

        public async Task<Route?> GetById(Guid id)
        {
            //todo!!!!!!!!!!!!!!!!!!!
            Route? result = default;
            RedisValue item = await _database.StringGetAsync(GetKey(id));
            if (item.HasValue)
            {
                result = JsonSerializer.Deserialize<Route?>((string)item!);
            }

            return result;
        }

        private Guid? GetRouteIdFromCache(Route route)
        {
            return default;
        }

        private static string GetKey(Guid id) => $"{keyPrefix}{id}";
    }

    public interface ICacheService 
    {
        Task<Route?> GetById(Guid id);
        Task<Guid> UpdateCacheAndGetRouteId(Route route);
    }
}
