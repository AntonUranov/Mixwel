using Mixwel.Domain.Models;
using Route =  Mixwel.Domain.Models.Route;

namespace Mixwel.Domain.Interfaces
{
    public interface ISearchRoutesService 
    {
        public Task<KeyValuePair<Guid, Route>?> GetById(Guid id);
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
        Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
    }
}
