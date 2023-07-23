using Mixwel.Domain.Models;

namespace Mixwel.Domain.Interfaces
{
    public interface ISearchRoutesService 
    {
        public Task<Models.Route?> GetById(Guid id);
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
        Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
    }
}
