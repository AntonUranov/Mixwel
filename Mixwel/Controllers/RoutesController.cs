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
        private readonly ISearchService _searchService;
        public RoutesController(ICompositeSearchService searchService)
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
        public IActionResult GetById(Guid id, CancellationToken cancellationToken)
        {
            return Ok();
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
