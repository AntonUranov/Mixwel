using Microsoft.AspNetCore.Mvc;
using Mixwel.Domain.Interfaces;
using Mixwel.Domain.Models;
using Mixwel.Models;

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

            return Ok(result.Value);
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
    }
}
