using Mixwel;
using Mixwel.Domain.Models;
using System;
using System.Runtime.Serialization;

namespace TestTask;

public interface ISearchService
{
    Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
}

public interface ICompositeSearchService : ISearchService { }

public class CompositeSearchService : ICompositeSearchService
{
    private readonly IEnumerable<ISearchService> _services;
    private readonly ILogger<CompositeSearchService> _logger;

    public CompositeSearchService(IEnumerable<ISearchService> services, ILogger<CompositeSearchService> logger)
    {
        if (services?.Any() == false)
            throw new ArgumentException(ValidationMessages.NullOrEmpty(nameof(services)));
        ArgumentNullException.ThrowIfNull(logger);

        _services = services!;
        _logger = logger;
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
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

            return Result.Ok(totalResult!);
        }

        return Result.Fail<SearchResponse>(generalErrorMessage);
    }


    private static string GetGeneralErrorMessege(IEnumerable<Result<SearchResponse>> failedResults) =>
        string.Join(Environment.NewLine, failedResults.Select(x => x.Error));
}