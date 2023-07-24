using Microsoft.Net.Http.Headers;
using Mixwel.Domain.Interfaces;
using Mixwel.Domain.Models;
using Mixwel.Providers.ProviderOneSearch;
using System.Net.Http;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Providers.ProviderTwoSearch
{
    public class ProviderTwoSearchService : ISearchService
    {
        private const string SearchPath = "/api/v1/search";
        private const string PingPath = "/api/v1/ping";
        private static Uri BaseAddress = new Uri("http://provider-two/");

        private readonly HttpClient _httpClient;

        public ProviderTwoSearchService(HttpClient httpClient)
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
            ProviderTwoSearchRequest apiRequest = ToApiRequest(request);

            try
            {
                using (HttpResponseMessage response = await _httpClient.PostAsJsonAsync(SearchPath, apiRequest, cancellationToken))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        ProviderTwoSearchResponse? apiResponse = await response.Content
                            .ReadFromJsonAsync<ProviderTwoSearchResponse>(cancellationToken: cancellationToken);

                        IEnumerable<ProviderTwoRoute> apiRoutes = GetFilteredApiRoutes(apiResponse, request.Filters);

                        return Result.Ok(ToRoutes(apiRoutes));
                    }
                    else
                    {
                        return Result.Fail<IEnumerable<Route>>(
                            $"Failed response. Provider:{nameof(ProviderTwoSearchService)}. Http code: {(int)response.StatusCode}. Path:{SearchPath}");
                    }
                }
            }
            catch (Exception e)
            {

                return Result.Fail<IEnumerable<Route>>($"Provider:{nameof(ProviderTwoSearchService)}. Message:{e.Message}");
            }
        }

        private static IEnumerable<ProviderTwoRoute> GetFilteredApiRoutes(ProviderTwoSearchResponse? apiResponse, SearchFilters? filters)
        {
            var routes = apiResponse?.Routes ?? Array.Empty<ProviderTwoRoute>();
            if (filters is null) return routes;

            return routes
                .Where(x => filters.IsAppropriateDestinationTime(x.Arrival.Date))
                .Where(x => filters.IsAffordablePrice(x.Price))
                .Where(x => filters.IsSuitableTimeLimit(x.TimeLimit));
        }

        private ProviderTwoSearchRequest ToApiRequest(SearchRequest request)
        {
            return new ProviderTwoSearchRequest
            {
                Arrival = request.Origin,
                Departure = request.Destination,
                DepartureDate = request.OriginDateTime,
                MinTimeLimit = request.Filters?.MinTimeLimit
            };
        }

        private static IEnumerable<Route> ToRoutes(IEnumerable<ProviderTwoRoute> apiRoutes)
        {
            IEnumerable<Route> routes = apiRoutes
                .Select(x => Route.Create(x.Arrival.Point,
                    x.Departure.Point,
                    x.Arrival.Date,
                    x.Departure.Date,
                    x.Price,
                    x.TimeLimit
                ));

            return routes;
        }
    }
}
