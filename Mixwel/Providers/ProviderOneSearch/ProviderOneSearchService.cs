using Mixwel.Domain.Models;
using System.IO;
using System;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using Mixwel.Domain.Interfaces;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Providers.ProviderOneSearch
{
    public class ProviderOneSearchService : ISearchService
    {
        private const string SearchPath = "/api/v1/search";
        private const string PingPath = "/api/v1/ping";
        //private static Uri BaseAddress = new Uri("http://provider-one/");
        private static Uri BaseAddress = new Uri("http://localhost:5089/");
        
        private readonly HttpClient _httpClient;

        public ProviderOneSearchService(HttpClient httpClient)
        {
            httpClient.BaseAddress = BaseAddress;
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");

            _httpClient = httpClient;
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(PingPath);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Result<IEnumerable<Route>>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
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

                        IEnumerable<ProviderOneRoute> apiRoutes = GetFilteredApiRoutes(apiResponse, request.Filters);
                        return Result.Ok(ToRoutes(apiRoutes));
                    }
                    else
                    {
                        return Result.Fail<IEnumerable<Route>>(
                            $"Failed response. Provider:{nameof(ProviderOneSearchService)}. Http code: {(int)response.StatusCode}. Path:{SearchPath}");
                    }
                }
            }
            catch (Exception e)
            {

                return Result.Fail<IEnumerable<Route>>($"Provider:{nameof(ProviderOneSearchService)}. Message:{e.Message}");
            }
        }

        private static IEnumerable<ProviderOneRoute> GetFilteredApiRoutes(ProviderOneSearchResponse? apiResponse, SearchFilters? filters)
        {
            var routes = apiResponse?.Routes ?? Array.Empty<ProviderOneRoute>();
            if(filters is null) return routes;

            return routes
                .Where(x => filters.IsAppropriateDestinationTime(x.DateTo))
                .Where(x => filters.IsAffordablePrice(x.Price))
                .Where(x => filters.IsSuitableTimeLimit(x.TimeLimit));
        }

        private static ProviderOneSearchRequest ToApiRequest(SearchRequest searchRequest) 
        {
            return new ProviderOneSearchRequest
            {
                From = searchRequest.Origin,
                To = searchRequest.Destination,
                DateFrom = searchRequest.OriginDateTime,
                DateTo = searchRequest.Filters?.DestinationDateTime,
                MaxPrice = searchRequest.Filters?.MaxPrice,
            };
        }

        private static IEnumerable<Route> ToRoutes(IEnumerable<ProviderOneRoute> apiRoutes) 
        {
            var routes = apiRoutes
                .Select(x => Route.Create(x.From,
                    x.To,
                    x.DateFrom,
                    x.DateTo,
                    x.Price,
                    x.TimeLimit
                ));

            return routes;
        }
    }
}
