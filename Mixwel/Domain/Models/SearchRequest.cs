namespace Mixwel.Domain.Models
{
    public class SearchRequest
    {
        private SearchRequest() { }

        // Mandatory
        // Start point of route, e.g. Moscow 
        public string Origin { get; init; }

        // Mandatory
        // End point of route, e.g. Sochi
        public string Destination { get; init; }

        // Mandatory
        // Start date of route
        public DateTime OriginDateTime { get; init; }

        // Optional
        public SearchFilters? Filters { get; init; }


        public static Result<SearchRequest> Create(string origin, string destination,
            DateTime originDatetime, SearchFilters? searchFilters)
        {
            //invariant checks
            if (string.IsNullOrEmpty(origin))
                return Result.Fail<SearchRequest>(ValidationMessages.NullOrEmpty(nameof(origin)));
            if (string.IsNullOrEmpty(destination))
                return Result.Fail<SearchRequest>(ValidationMessages.NullOrEmpty(nameof(destination)));

            return Result.Ok(new SearchRequest()
            {
                Origin = origin,
                Destination = destination,
                OriginDateTime = originDatetime,
                Filters = searchFilters
            });
        }
    }
}
