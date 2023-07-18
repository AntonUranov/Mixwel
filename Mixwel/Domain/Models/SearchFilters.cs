namespace Mixwel.Domain.Models
{
    public class SearchFilters
    {
        private SearchFilters() { }

        // Optional
        // End date of route
        public DateTime? DestinationDateTime { get; init; }

        // Optional
        // Maximum price of route
        public decimal? MaxPrice { get; init; }

        // Optional
        // Minimum value of timelimit for route
        public DateTime? MinTimeLimit { get; init; }

        // Optional
        // Forcibly search in cached data
        public bool OnlyCached { get; init; }

        public static SearchFilters Create(DateTime? destinationDateTime,
            decimal? maxPrice,
            DateTime? minTimeLimit,
            bool onlyCached)
        {
            return new SearchFilters
            {
                DestinationDateTime = destinationDateTime,
                MaxPrice = maxPrice,
                MinTimeLimit = minTimeLimit,
                OnlyCached = onlyCached
            };
        }
    }
}
