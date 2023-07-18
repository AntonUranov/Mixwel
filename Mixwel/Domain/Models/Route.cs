namespace Mixwel.Domain.Models
{
    public class Route
    {
        private Route() { }

        // Mandatory
        // Identifier of the whole route
        public Guid Id { get; init; }

        // Mandatory
        // Start point of route
        public string Origin { get; init; }

        // Mandatory
        // End point of route
        public string Destination { get; init; }

        // Mandatory
        // Start date of route
        public DateTime OriginDateTime { get; init; }

        // Mandatory
        // End date of route
        public DateTime DestinationDateTime { get; init; }

        // Mandatory
        // Price of route
        public decimal Price { get; init; }

        // Mandatory
        // Timelimit. After it expires, route became not actual
        public DateTime TimeLimit { get; init; }

        public static Route Create(Guid id, string origin, string destination,
            DateTime originDateTime,
            DateTime destinationDateTime,
            decimal price,
            DateTime timeLimit)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Identifier shouldn't be empty.");

            ArgumentException.ThrowIfNullOrEmpty(origin);
            ArgumentException.ThrowIfNullOrEmpty(destination);

            if (price < 0)
                throw new ArgumentException("Price should be non-negative value.");

            return new Route
            {
                Id = id,
                Origin = origin,
                Destination = destination,
                OriginDateTime = originDateTime,
                DestinationDateTime = destinationDateTime,
                Price = price,
                TimeLimit = timeLimit
            };
        }
    }
}
