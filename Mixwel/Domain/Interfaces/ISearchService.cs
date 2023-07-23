using Mixwel.Domain.Models;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Domain.Interfaces;

public interface ISearchService
{
    Task<Result<IEnumerable<Route>>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
}