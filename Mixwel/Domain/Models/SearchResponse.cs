using System.Collections.Immutable;

namespace Mixwel.Domain.Models
{
    public class SearchResponse
    {
        private SearchResponse() { }

        private static readonly SearchResponse Empty =
            new SearchResponse() { RoutesWithId = ImmutableDictionary<Guid, Route>.Empty };

        // Mandatory
        // Routes
        public IImmutableDictionary<Guid, Route> RoutesWithId { get; init; }

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


        public static SearchResponse Create(IImmutableDictionary<Guid, Route> routes)
        {
            if(routes?.Any() != true)
                return Empty;

            return new SearchResponse
            {
                RoutesWithId = routes,
                MinPrice = routes.Min(x=>x.Value.Price),
                MaxPrice = routes.Max(x => x.Value.Price),
                MaxMinutesRoute =
                    routes.Max(x => (int)(x.Value.DestinationDateTime - x.Value.OriginDateTime).TotalMinutes),
                MinMinutesRoute =
                    routes.Min(x => (int)(x.Value.DestinationDateTime - x.Value.OriginDateTime).TotalMinutes)
            };
        }
    }
}
