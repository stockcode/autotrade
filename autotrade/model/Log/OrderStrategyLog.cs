using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace autotrade.model.Log
{
    [BsonKnownTypes(typeof(BollStrategyLog), typeof(DayAverageLog), typeof(FallBackStopStrategyLog), typeof(PriceStopLossStrategyLog))]
    public class OrderStrategyLog
    {
    }
}
