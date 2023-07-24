using Mixwel.Domain.Models;
using System.Collections.Immutable;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Infrastructure
{
    public interface ICacheService
    {
        Task<Result> Initialize();
        Task<Route?> GetById(Guid id);
        Task<Guid> UpdateCacheAndGetRouteId(Route route);
        Task<IImmutableDictionary<Guid, Route>> GetFromCache(SearchRequest searchRequest);
    }
}
