using Microsoft.AspNetCore.Mvc;
using Mixwel.Domain.Interfaces;
using Mixwel.Domain.Models;
using Mixwel.Models;
using System.Net;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly ISearchRoutesService _searchService;
        public RoutesController(ISearchRoutesService searchService)
        {
            _searchService = searchService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SearchResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> Search([FromBody] SearchRequestModel request,
            CancellationToken cancellationToken)
        {
            if (request is null)
                return BadRequest();

            Result<SearchRequest> mappedRequestResult = MapRequest(request);
            if (mappedRequestResult.IsFailure)
                return BadRequest();

            var result = await _searchService.
                SearchAsync(mappedRequestResult.Value!, cancellationToken);
            
            if (result.IsFailure)
                return Problem();

            return Ok(ToSearchResponseModel(result.Value!));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RouteModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetById(Guid id)
        {
            var result = await _searchService.GetById(id);
            if (!result.HasValue)
                return NotFound();
            var routeById = result.Value;
            return Ok(ToRouteModel(routeById.Value, routeById.Key));
        }

        private static Result<SearchRequest> MapRequest(SearchRequestModel request)
        {
            SearchFilters? filters = MapFilters(request.Filters);

            return SearchRequest
                .Create(request.Origin, request.Destination, request.OriginDateTime, filters);
        }

        private static SearchFilters? MapFilters(SearchFiltersModel? filters)
        {
            if (filters is null) return null;

            return SearchFilters.Create(filters.DestinationDateTime, filters.MaxPrice,
                filters.MinTimeLimit, filters.OnlyCached == true);

        }

        private static SearchResponseModel ToSearchResponseModel(SearchResponse response) 
        {
            return new SearchResponseModel
            {
                MaxMinutesRoute = response.MaxMinutesRoute,
                MinMinutesRoute = response.MinMinutesRoute,
                MaxPrice = response.MaxPrice,
                MinPrice = response.MinPrice,
                Routes = response.RoutesWithId.Select(x => ToRouteModel(x.Value, x.Key))
            };
        }

        private static RouteModel ToRouteModel(Route route, Guid routeId) 
        {
            return new RouteModel
            {
                Id = routeId,
                Origin = route.Origin,
                Destination = route.Destination,
                OriginDateTime = route.OriginDateTime,
                DestinationDateTime = route.DestinationDateTime,
                Price = route.Price,
                TimeLimit = route.TimeLimit
            };
        }
    }
}
