using Microsoft.AspNetCore.Routing;
using Mixwel.Domain.Models;
using StackExchange.Redis;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Infrastructure
{
    public class RedisService: ICacheService
    {
        private const string keyPrefix = "route:";
        private const string routeIndex = $"{RoutesIndexName} ON Hash PREFIX 1 \"route:\" SCHEMA Origin TEXT Destination TEXT OriginDateTime NUMERIC";
        //todo: read from config options
        private readonly TimeSpan _expirationPeriod = TimeSpan.FromMinutes(10);

        private readonly IDatabase _database;

        public const string RoutesIndexName = "idx:route";

        public RedisService(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);
            _database = database;
        }

        public async Task<Result> Initialize()
        {
            RedisResult flushResult = await _database.ExecuteAsync("FLUSHALL");
            if (!IsOk(flushResult)) 
                Result.Fail<bool>("Failed FLUSHALL command");
            
            string[] args = routeIndex.Replace("\"", "")
                .Split(" ")
                .Select(x => x.Trim())
                .ToArray();

            RedisResult indexCreateResult = await _database.ExecuteAsync("FT.CREATE", args);
            if(!IsOk(indexCreateResult))
                Result.Fail<bool>($"Failed FT.CREATE command. Args:{routeIndex}");

            return Result.Ok();
        }

        public async Task<Guid> UpdateCacheAndGetRouteId(Route route)
        {
            Guid? cachedRouteId = await GetRouteIdFromCache(route);
            if (cachedRouteId.HasValue)
            {
                SetExpiration(GetKey(cachedRouteId.Value));
                return cachedRouteId.Value;
            }

            Guid newRouteId = Guid.NewGuid();
            string key = $"{keyPrefix}{newRouteId}";
            await _database.HashSetAsync(key, new HashEntry[]
            {
                new HashEntry(RoutePropertyNames.Origin, route.Origin),
                new HashEntry(RoutePropertyNames.Destination, route.Destination),
                new HashEntry(RoutePropertyNames.OriginDateTime, route.OriginDateTime.Ticks),
                new HashEntry(RoutePropertyNames.DestinationDateTime, route.DestinationDateTime.Ticks),
                new HashEntry(RoutePropertyNames.Price, route.Price.ToString(CultureInfo.InvariantCulture)),
                new HashEntry(RoutePropertyNames.TimeLimit, route.TimeLimit.Ticks),
            });
            await SetExpiration(key);

            return newRouteId;
        }

        public async Task<KeyValuePair<Guid, Route>?> GetById(Guid id)
        {
            HashEntry[] entries = await _database.HashGetAllAsync(GetKey(id));
            if (!entries.Any()) 
                return default;

            RouteBuilder routeBuilder = new RouteBuilder();
            foreach (var item in entries) 
            {
                routeBuilder.SetData(item.Name, RedisResult.Create(item.Value));
            }

            Route route =  routeBuilder.Build();
            return new KeyValuePair<Guid, Route>(id, route);
        }

        public async Task<IImmutableDictionary<Guid, Route>> GetFromCache(SearchRequest searchRequest) 
        {
            object[] args = new object[]
            {
                RoutesIndexName,
                $"@Origin:{searchRequest.Origin} @Destination:{searchRequest.Destination} @OriginDateTime:[{searchRequest.OriginDateTime.Ticks} inf]"
            };

            RedisResult redisResult = await _database.ExecuteAsync("FT.SEARCH", args);

            SearchFilters filters = searchRequest.Filters!;
            Func<Route, bool> filter = x =>
                filters.IsAppropriateDestinationTime(x.DestinationDateTime)
                && filters.IsSuitableTimeLimit(x.TimeLimit)
                && filters.IsAffordablePrice(x.Price);

            var routes = GetRoutesFromRedisResult(redisResult);

            return routes;
        }

        private async Task<Guid?> GetRouteIdFromCache(Route route)
        {
            object[] args = new object[]
            {
                RoutesIndexName,
                $"@Origin:{route.Origin} @Destination:{route.Destination} @OriginDateTime:[{route.OriginDateTime.Ticks} {route.OriginDateTime.Ticks}]"

            };
            RedisResult redisResult = await _database.ExecuteAsync("FT.SEARCH", args);

            var routes = GetRoutesFromRedisResult(redisResult);
            if (routes.Any()) 
            {
                return routes.FirstOrDefault(x => x.Value == route).Key;
            }

            return default;
        }

        private static IImmutableDictionary<Guid, Route> GetRoutesFromRedisResult(RedisResult redisResult, 
            Func<Route, bool> IsAppropriateRoute = null)
        {
            var dictionaryBuilder = ImmutableDictionary.CreateBuilder<Guid, Route>();
            if (redisResult.IsNull || redisResult.Type != ResultType.MultiBulk)
                return dictionaryBuilder.ToImmutable();

            var ar = (RedisResult[])redisResult!;
            int pairsCount = (int)ar[0];//pairs
            for (int i = 1; i < pairsCount * 2; i += 2)
            {
                string redisKey = (string)ar[i];
                var items = (RedisResult[])ar[i + 1];

                Guid id = Guid.Parse(redisKey.AsSpan(keyPrefix.Length));

                RouteBuilder routeBuilder = new RouteBuilder();
                
                for (int j = 0; j < items.Length; j += 2)
                {
                    var key = items[j];
                    var val = items[j + 1];
                    routeBuilder.SetData((string)key, val);
                }

                var route = routeBuilder.Build();

                if (IsAppropriateRoute?.Invoke(route) == false)
                    continue;

                dictionaryBuilder.Add(id, route);
            }


            return dictionaryBuilder.ToImmutable();
        }

        private async Task SetExpiration(string key)
        {
            await _database.KeyExpireAsync(key, _expirationPeriod);
        }

        private static string GetKey(Guid id) => $"{keyPrefix}{id}";

        private static bool IsOk(RedisResult? redisResult) => (string)redisResult == "OK";

    }
}
