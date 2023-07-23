using Microsoft.AspNetCore.Hosting.Server;
using Mixwel.Domain.Interfaces;
using Mixwel.Domain.Models;
using Mixwel.Infrastructure;
using StackExchange.Redis;
using System.Collections.Immutable;
using System.Text.Json;
using IServer = StackExchange.Redis.IServer;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Domain
{
    public class SearchRoutesService : ISearchRoutesService
    {
        private readonly IEnumerable<ISearchService> _services;
        private readonly ICacheService _cache;
        private readonly ILogger<SearchRoutesService> _logger;

        public SearchRoutesService(IEnumerable<ISearchService> services, ICacheService cache, ILogger<SearchRoutesService> logger)
        {
            if (services?.Any() != true)
                throw new ArgumentException(ValidationMessages.NullOrEmpty(nameof(services)));
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(logger);

            _services = services!;
            _logger = logger;
            _cache = cache;
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            IEnumerable<Task<bool>> tasks = _services.Select(x => x.IsAvailableAsync(cancellationToken));
            var results = await Task.WhenAll(tasks);

            return results.All(x => x);
        }

        //private async Task<Result<SearchResponse>> SearchByCache(SearchRequest request, CancellationToken cancellationToken) 
        //{
            

        //    await foreach(var key in _server.KeysAsync(pattern: $"{_prefix}*"))
        //    {
        //        if()
        //    }

        //    return Result.Fail<SearchResponse>("test");
        //}

        //private async IEnumerable<Route> GetRoutes() 
        //{
        //    await foreach (var key in _server.KeysAsync(pattern: $"{_prefix}*"))
        //    {
        //        var json = await _cache.StringGetAsync(key);
        //        var value = JsonSerializer.Deserialize<Route>((string)json);
        //        if 

        //    }
        //}

        public async Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            //if (request.Filters?.OnlyCached == true) 
            //{
            //    return await SearchByCache(request, cancellationToken);
            //}

            var tasks = _services
                .Select(x => x.SearchAsync(request, cancellationToken));
            Result<IEnumerable<Route>>[] results = await Task.WhenAll(tasks);

            
            var failedResults = results.Where(x => x.IsFailure);
            if (failedResults.Any())
            {
                string errorMessage = string
                    .Join(Environment.NewLine, failedResults
                    .Select(x => x.Error)); 

                _logger.LogError(errorMessage);
                if (failedResults.All(x => x.IsFailure)) 
                {
                    return Result.Fail<SearchResponse>(
                        errorMessage);
                }
                    
            }

            var successResults = results.Where(x => x.IsSuccess)
                .Select(x => x.Value);
            IEnumerable<Route> total = Enumerable.Empty<Route>();
            foreach (var item in successResults)
            {
                if (item is null)
                    total = item!;
                else
                    total = total.Union(item);
            }
            var builder = ImmutableDictionary.CreateBuilder<Guid, Route>();
            foreach (var item in total)
            {
                Guid routeGuid =  await _cache.UpdateCacheAndGetRouteId(item);
                builder.Add(routeGuid, item);
            }

            SearchResponse response = SearchResponse.Create(builder.ToImmutable());
            return Result.Ok(response);            
        }

        public Task<Route?> GetById(Guid id) => _cache.GetById(id);
    }
}
