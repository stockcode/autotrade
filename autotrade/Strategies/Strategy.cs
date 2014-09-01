using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using autotrade.business;
using autotrade.converter;
using autotrade.model;
using autotrade.Stop.Loss;
using autotrade.Stop.Profit;
using MongoDB.Bson.Serialization.Attributes;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    [BsonKnownTypes(typeof(StopLoss), typeof(StopProfit), typeof(UserStrategy))]
    public abstract class Strategy : INotifyPropertyChanged
    {
        protected readonly  double TOLERANCE = 0.000000001d;

        [BsonIgnore]
        [Browsable(false)]
        public IndicatorManager IndicatorManager { get; set; }

        [BsonIgnore]
        [Browsable(false)]
        public OrderManager OrderManager { get; set; }

        [BsonIgnore]
        [Browsable(false)]
        public InstrumentStrategy InstrumentStrategy { get; set; }


        public abstract List<Order> Match(MarketData marketData);

    

        protected double GetAnyPrice(MarketData marketData, TThostFtdcDirectionType direction)
        {
            if (direction == TThostFtdcDirectionType.Buy)
            {
                return marketData.LastPrice + InstrumentStrategy.PriceTick * 3;
            }
            else
            {
                return marketData.LastPrice - InstrumentStrategy.PriceTick * 3;
            }
        }

        protected double GetAnyPrice(double price, TThostFtdcDirectionType direction)
        {
            if (direction == TThostFtdcDirectionType.Buy)
            {
                return price + InstrumentStrategy.PriceTick * 3;
            }
            else
            {
                return price - InstrumentStrategy.PriceTick * 3;
            }
        }

        protected List<Order> GetStrategyOrders(string instrumentId)
        {
            return OrderManager.getOrders()
                    .Where(o => o.InstrumentId == instrumentId && o.StrategyType == GetType().ToString())
                    .ToList();
        }

        protected List<Order> GetStrategyOrders(string instrumentId, string strategyType)
        {
            return OrderManager.getOrders()
                    .Where(o => o.InstrumentId == instrumentId && o.StrategyType == strategyType)
                    .ToList();
        }

        protected List<Order> GetOrders(string instrumentId)
        {
            return OrderManager.getOrders()
                    .Where(o => o.InstrumentId == instrumentId)
                    .ToList();
        }



        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
