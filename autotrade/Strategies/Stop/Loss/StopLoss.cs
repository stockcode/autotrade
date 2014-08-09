using System.Collections.Generic;
using System.ComponentModel;
using autotrade.business;
using autotrade.model;
using autotrade.Strategies;
using MongoDB.Bson.Serialization.Attributes;
using QuantBox.CSharp2CTP;

namespace autotrade.Stop.Loss
{
    [BsonKnownTypes(typeof(PriceStopLoss), typeof(PercentStopLoss))]
    public abstract class StopLoss : Strategy
    {        
    }
}
