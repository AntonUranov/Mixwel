using StackExchange.Redis;
using System.Globalization;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Infrastructure
{
    public class RouteBuilder
    {
        public string Origin { get; private set; }
        public string Destination { get; private set; }
        public DateTime OriginDateTime { get; private set; }
        public DateTime DestinationDateTime { get; private set; }
        public decimal Price { get; private set; }
        public DateTime TimeLimit { get; private set; }

        public void SetData(string key, RedisResult val)
        {
            switch (key)
            {
                case RoutePropertyNames.Origin: Origin = (string)val!; break;
                case RoutePropertyNames.Destination: Destination = (string)val!; break;
                case RoutePropertyNames.OriginDateTime: OriginDateTime = new DateTime((long)val); break;
                case RoutePropertyNames.DestinationDateTime: DestinationDateTime = new DateTime((long)val); break;
                case RoutePropertyNames.Price: Price = decimal.Parse((string)val!, CultureInfo.InvariantCulture); break;
                case RoutePropertyNames.TimeLimit: TimeLimit = new DateTime((long)val); break;
            }
        }

        public Route Build() => Route.Create(Origin, Destination, OriginDateTime, DestinationDateTime, Price, TimeLimit);
    }
}
