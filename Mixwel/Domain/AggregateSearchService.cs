using Microsoft.AspNetCore.Hosting.Server;
using Mixwel.Domain.Interfaces;
using Mixwel.Domain.Models;
using StackExchange.Redis;
using System.Text.Json;
using IServer = StackExchange.Redis.IServer;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Domain
{
    public class AggregateSearchService : IAggregateSearchService
    {
        private const string _prefix = "route:";

        private readonly IEnumerable<ISearchService> _services;
        private readonly IDatabase _cache;
        private readonly IServer _server;
        private readonly ILogger<AggregateSearchService> _logger;

        public AggregateSearchService(IEnumerable<ISearchService> services, IDatabase cache, IServer server,ILogger<AggregateSearchService> logger)
        {
            if (services?.Any() != true)
                throw new ArgumentException(ValidationMessages.NullOrEmpty(nameof(services)));
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(logger);

            _services = services!;
            _logger = logger;
            _cache = cache;
            _server = server;
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            IEnumerable<Task<bool>> tasks = _services.Select(x => x.IsAvailableAsync(cancellationToken));
            var results = await Task.WhenAll(tasks);

            return results.All(x => x);
        }

        private async Task<Result<SearchResponse>> SearchByCache(SearchRequest request, CancellationToken cancellationToken) 
        {
            

            await foreach(var key in _server.KeysAsync(pattern: $"{_prefix}*"))
            {
                if()
            }

            return Result.Fail<SearchResponse>("test");
        }

        private async IEnumerable<Route> GetRoutes() 
        {
            await foreach (var key in _server.KeysAsync(pattern: $"{_prefix}*"))
            {
                var json = await _cache.StringGetAsync(key);
                var value = JsonSerializer.Deserialize<Route>((string)json);
                if 

            }
        }

        public async Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            if (request.Filters?.OnlyCached == true) 
            {
                return await SearchByCache(request, cancellationToken);
            }

            IEnumerable<Task<Result<SearchResponse>>> tasks =
                _services.Select(x => x.SearchAsync(request, cancellationToken));
            Result<SearchResponse>[] results = await Task.WhenAll(tasks);

            string generalErrorMessage = string.Empty;
            var failedResults = results.Where(x => x.IsFailure);
            if (failedResults.Any())
            {
                generalErrorMessage = GetGeneralErrorMessege(failedResults);
                _logger.LogError(generalErrorMessage);
            }


            var successResults = results.Where(x => x.IsSuccess).Select(x => x.Value);
            if (successResults.Any())
            {
                SearchResponse? totalResult = null;
                foreach (var item in successResults)
                {
                    if (totalResult == null)
                        totalResult = item;
                    else
                        totalResult = totalResult.Join(item);
                }

                foreach (var route in totalResult.Routes) 
                {
                    await _cache.StringSetAsync(GetKey(route.Id), 
                        JsonSerializer.Serialize(route), TimeSpan.FromMinutes(3));
                }

                return Result.Ok(totalResult!);
            }

            return Result.Fail<SearchResponse>(generalErrorMessage);
        }

        public async Task<Route?> GetById(Guid id)
        {
            Route? result = default; 
            RedisValue item = await _cache.StringGetAsync(GetKey(id));
            if (item.HasValue) 
            {
                result = JsonSerializer.Deserialize<Route?>((string)item!);
            }

            return result;
        }

        private static string GetGeneralErrorMessege(IEnumerable<Result<SearchResponse>> failedResults) =>
            string.Join(Environment.NewLine, failedResults.Select(x => x.Error));

        private static string GetKey(Guid id) => $"{_prefix}{id}";
    }
}
