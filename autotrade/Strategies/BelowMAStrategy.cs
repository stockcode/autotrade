using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTPTradeApi;
using autotrade.Indicators;
using autotrade.business;
using autotrade.model;

namespace autotrade.Strategies
{
    class BelowMAStrategy : IStrategy
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int days;
        private IndicatorManager indicatorManager;
        private double maPrice = 0d;

        private List<Order> orders = new List<Order>();


        public BelowMAStrategy(IndicatorManager indicatorManager, int days)
        {
            this.days = days;
            this.indicatorManager = indicatorManager;            

        }
        public List<Order> Match(MarketData marketData)
        {
            if ( orders.Count() > 0) return null;

            maPrice = this.indicatorManager.GetMAPrice(marketData.InstrumentId, days);            
            double lastPrice = marketData.LastPrice;


            if (lastPrice < maPrice * 0.98)
            {
                for (int i = 1; i <= 5; i++)
                {
                    Order order = new Order();
                    order.OffsetFlag = EnumOffsetFlagType.Open;
                    order.Direction = EnumDirectionType.Buy;
                    order.InstrumentId = marketData.InstrumentId;
                    order.Price = lastPrice - i*5;
                    order.Volume = 1;
                    orders.Add(order);
                }

                log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", this.ToString(), marketData.InstrumentId, lastPrice, maPrice, orders.Count()));
            }

            

            return orders;
        }

    }
}
