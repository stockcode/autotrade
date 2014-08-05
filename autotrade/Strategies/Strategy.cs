using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.business;
using autotrade.model;
using MongoDB.Bson.Serialization.Attributes;

namespace autotrade.Strategies
{
    [BsonKnownTypes(typeof(BollStrategy), typeof(DayAverageStrategy))]
    public  abstract class Strategy
    {
        [BsonIgnore]
        [Browsable(false)]
        public IndicatorManager IndicatorManager { get; set; }

        [BsonIgnore]
        [Browsable(false)]
        public OrderManager OrderManager { get; set; }

        public abstract List<Order> Match(MarketData marketData, InstrumentStrategy instrumentStrategy);
    }
}
