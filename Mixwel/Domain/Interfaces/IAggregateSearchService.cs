using Mixwel.Domain.Models;

namespace Mixwel.Domain.Interfaces
{
    public interface IAggregateSearchService : ISearchService 
    {
        public Task<Models.Route?> GetById(Guid id);
    }
}
