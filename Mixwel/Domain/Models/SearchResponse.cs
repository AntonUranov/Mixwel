namespace Mixwel.Domain.Models
{
    public record SearchResponse
    {
        private SearchResponse() { }

        // Mandatory
        // Routes
        public IEnumerable<Route> Routes { get; init; }

        // Mandatory
        // The cheapest route
        public decimal MinPrice { get; init; }

        // Mandatory
        // Most expensive route
        public decimal MaxPrice { get; init; }

        // Mandatory
        // The fastest route
        public int MinMinutesRoute { get; init; }

        // Mandatory
        // The longest route
        public int MaxMinutesRoute { get; init; }

        public static SearchResponse Create(IEnumerable<Route> routes)
        {
            var items = routes ?? Enumerable.Empty<Route>();

            return new SearchResponse
            {
                Routes = items,
                MinPrice = items.Min(x=>x.Price),
                MaxPrice = items.Max(x => x.Price),
                MaxMinutesRoute = items.Max(x => (int)(x.DestinationDateTime - x.OriginDateTime).TotalMinutes),
                MinMinutesRoute = items.Min(x => (int)(x.DestinationDateTime - x.OriginDateTime).TotalMinutes)
            };
        }

        public SearchResponse Join(SearchResponse? other) 
        {
            if (other is null) return this;

            IEnumerable<Route> totalRoutes = Routes.Concat(other.Routes);
            decimal totalMinPrice = Math.Min(MinPrice, other.MinPrice);
            decimal totalMaxPrice = Math.Max(MaxPrice, other.MaxPrice);
            int totalMaxMinutesRoute = Math.Max(MaxMinutesRoute, other.MaxMinutesRoute);
            int totalMinMinutesRoute = Math.Min(MinMinutesRoute, other.MinMinutesRoute);

            return new SearchResponse
            {
                Routes = totalRoutes,
                MinPrice = totalMinPrice,
                MaxPrice = totalMaxPrice,
                MaxMinutesRoute = totalMaxMinutesRoute,
                MinMinutesRoute = totalMinMinutesRoute
            };
        }
    }
}
