using System.Collections.Generic;
using System.ComponentModel;
using autotrade.business;
using autotrade.model;
using autotrade.Strategies;
using MongoDB.Bson.Serialization.Attributes;
using QuantBox.CSharp2CTP;

namespace autotrade.Stop.Profit
{
    [BsonKnownTypes(typeof(PriceStopProfit), typeof(PercentStopProfit), typeof(FallBackStopProfit))]
    public  abstract class StopProfit : Strategy
    {
    }
}
