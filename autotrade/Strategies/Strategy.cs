using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.model;
using MongoDB.Bson.Serialization.Attributes;

namespace autotrade.Strategies
{
    [BsonKnownTypes(typeof(BollStrategy), typeof(DayAverageStrategy))]
    public  abstract class Strategy
    {
        public abstract List<Order> Match(MarketData marketData);
    }
}
