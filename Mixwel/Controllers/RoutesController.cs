﻿using Microsoft.AspNetCore.Mvc;
using Mixwel.Domain.Interfaces;
using Mixwel.Domain.Models;
using Mixwel.Models;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly ISearchRoutesService _searchService;
        public RoutesController(ISearchRoutesService searchService)
        {
            _searchService = searchService;
        }

        [HttpPost]
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
        public async Task<ActionResult> GetById(Guid id)
        {
            var result = await _searchService.GetById(id);
            if (result is null)
                return NotFound();

            return Ok(result);
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
