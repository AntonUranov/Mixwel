using Mixwel.Domain.Models;
using TestTask;

namespace Mixwel.Providers.ProviderTwoSearch
{
    public class ProviderTwoSearchService : ISearchService
    {
        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            var a = Domain.Models.Route.Create(
                    Guid.NewGuid(),
                    "A",
                    "B",
                    DateTime.UtcNow.AddHours(1),
                    DateTime.UtcNow.AddHours(2),
                    100,
                    DateTime.UtcNow.AddHours(3)
                    );

            return Task.FromResult(Result.Ok(
                    SearchResponse.Create(new Domain.Models.Route[] { a})));

            throw new NotImplementedException();
        }
    }
}
