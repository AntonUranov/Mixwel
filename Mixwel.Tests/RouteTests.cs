using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Route = Mixwel.Domain.Models.Route;

namespace Mixwel.Tests
{
    public class RouteTests
    {
        [Fact]
        public void Route_Create_ReturnSuccess() 
        {
            string origin = "Moscow";
            string destination = "Delhi";
            DateTime originDateTime = new DateTime(2010, 1, 1);
            DateTime destinationDateTime = new DateTime(2010, 1, 1, 2, 2, 2);
            decimal price = 10m;
            DateTime timeLimit = new DateTime(2010, 1, 1, 2, 1, 2);

            Route rout = Route.Create(origin, destination, originDateTime,
                destinationDateTime, price, timeLimit);

            Assert.NotNull(rout);
        }

        [Theory]
        [ClassData(typeof(CalculatorTestData))]
        public void Route_Create_ThrowException(string origin, string destination, DateTime originDateTime, DateTime destinationDateTime, decimal price, DateTime timeLimit) 
        {
            Assert.Throws<InvalidOperationException>(() => Route.Create(origin, destination, originDateTime, destinationDateTime, price, timeLimit));
        }
    }

    public class CalculatorTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "","Delhi", new DateTime(2010, 1, 1) , new DateTime(2010, 1, 1, 2, 2, 2), 10m, new DateTime(2010, 1, 1, 2, 1, 2)};
            yield return new object[] { "Moscow","", new DateTime(2010, 1, 1) , new DateTime(2010, 1, 1, 2, 2, 2), 10m, new DateTime(2010, 1, 1, 2, 1, 2)};

        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
