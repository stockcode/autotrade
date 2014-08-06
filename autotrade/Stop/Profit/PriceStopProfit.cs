using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using autotrade.model;
using log4net;
using QuantBox.CSharp2CTP;

namespace autotrade.Stop.Profit
{
    class PriceStopProfit : StopProfit
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public double Price { get; set; }


        public PriceStopProfit()
        {
            Price = 5000;
        }

        public override List<Order> Match(MarketData marketData, InstrumentStrategy instrumentStrategy)
        {
             var instrumentId = marketData.InstrumentId;

            List<Order> orders =
                OrderManager.getOrders()
                    .Where(o => o.InstrumentId == marketData.InstrumentId)
                    .ToList();


            var list = new List<Order>();

            foreach (var order in orders)
            {
                if (order.StatusType == EnumOrderStatus.已开仓 && Price - order.PositionProfit <= 0)
                {
                    var neworder = new Order();
                    neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
                    neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy ? TThostFtdcDirectionType.Sell : TThostFtdcDirectionType.Buy;
                    neworder.InstrumentId = marketData.InstrumentId;
                    neworder.Price = GetAnyPrice(marketData, instrumentStrategy, neworder.Direction);
                    neworder.Volume = order.Volume;
                    neworder.StrategyType = GetType().ToString();


                    order.CloseOrder = neworder;

                    list.Add(order);

                    log.Info(String.Format("{0}:{1}:{2}:{3}", ToString(), marketData.InstrumentId, marketData.LastPrice,
                        orders.Count()));
                }
            }

            return list;
        }
    }
}
