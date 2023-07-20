using Mixwel;
using Mixwel.Domain.Models;
using System;
using System.Runtime.Serialization;

namespace Mixwel.Domain.Interfaces;

public interface ISearchService
{
    Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
}