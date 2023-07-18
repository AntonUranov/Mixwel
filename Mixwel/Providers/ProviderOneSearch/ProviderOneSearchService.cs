using Mixwel.Domain.Models;
using System.IO;
using System;
using System.Net.Http;
using TestTask;

namespace Mixwel.Providers.ProviderOneSearch
{
    public class ProviderOneSearchService : ISearchService
    {
        private const string SearchPath = "/api/v1/search";
        private static Uri BaseAddress = new Uri("http://provider-one/");
        
        private readonly HttpClient _httpClient;

        public ProviderOneSearchService(HttpClient httpClient)
        {
            httpClient.BaseAddress = BaseAddress;

            _httpClient = httpClient;
        }

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            ProviderOneSearchRequest apiRequest = ToApiRequest(request);
            try
            {
                using (HttpResponseMessage response = await _httpClient.PostAsJsonAsync(SearchPath, apiRequest, cancellationToken))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        ProviderOneSearchResponse? apiResponse = await response.Content
                            .ReadFromJsonAsync<ProviderOneSearchResponse>(cancellationToken: cancellationToken);

                        return Result.Ok(FromApiResponse(apiResponse));
                    }
                    else
                    {
                        return Result.Fail<SearchResponse>(
                            $"Failed response. Provider:{nameof(ProviderOneSearchService)}. Http code: {response.StatusCode}. Path:{SearchPath}");
                    }
                }
            }
            catch (Exception e)
            {

                return Result.Fail<SearchResponse>($"Provider:{nameof(ProviderOneSearchService)}. Message:{e.Message}");
            }
        }

        private static ProviderOneSearchRequest ToApiRequest(SearchRequest searchRequest) 
        {
            return new ProviderOneSearchRequest
            {
                From = searchRequest.Origin,
                To = searchRequest.Destination,
                DateFrom = searchRequest.OriginDateTime,
                DateTo = searchRequest.Filters?.DestinationDateTime,
                MaxPrice = searchRequest.Filters?.MaxPrice
            };
        }

        private static SearchResponse FromApiResponse(ProviderOneSearchResponse apiResponse) 
        {
            IEnumerable<Domain.Models.Route> routes = (apiResponse.Routes ?? Array.Empty<ProviderOneRoute>())
                .Select(x => Domain.Models.Route.Create(Guid.NewGuid(),
                    x.From,
                    x.To,
                    x.DateFrom,
                    x.DateTo,
                    x.Price,
                    x.TimeLimit
                ));
            return SearchResponse.Create(routes);
        }
    }
}
