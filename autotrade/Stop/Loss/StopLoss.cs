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
    public  abstract class StopLoss
    {
        [BsonIgnore]
        [Browsable(false)]
        public IndicatorManager IndicatorManager { get; set; }

        [BsonIgnore]
        [Browsable(false)]
        public OrderManager OrderManager { get; set; }

        public abstract List<Order> Match(MarketData marketData, InstrumentStrategy instrumentStrategy);

        protected double GetAnyPrice(MarketData marketData, InstrumentStrategy instrumentStrategy, TThostFtdcDirectionType direction)
        {
            if (direction == TThostFtdcDirectionType.Buy)
            {
                return marketData.LastPrice + instrumentStrategy.PriceTick * 3;
            }
            else
            {
                return marketData.LastPrice - instrumentStrategy.PriceTick * 3;
            }
        }
    }
}
