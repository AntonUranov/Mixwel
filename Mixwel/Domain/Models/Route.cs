namespace Mixwel.Domain.Models
{
    public record Route
    {
        private Route() { }

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

        public static Route Create(string origin, string destination,
            DateTime originDateTime,
            DateTime destinationDateTime,
            decimal price,
            DateTime timeLimit)
        {
            ArgumentException.ThrowIfNullOrEmpty(origin);
            ArgumentException.ThrowIfNullOrEmpty(destination);

            if (price < 0)
                throw new ArgumentException("Price should be non-negative value.");
            if (originDateTime >= destinationDateTime) 
            {
                throw new ArgumentException(
                    $"{nameof(OriginDateTime)} should be less than {nameof(DestinationDateTime)}.");
            }

            return new Route
            {
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
