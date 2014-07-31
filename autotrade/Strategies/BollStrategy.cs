using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using autotrade.business;
using autotrade.model;
using log4net;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    internal class BollStrategy : IStrategy
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private List<Order> orders;

        private int days, tick;
        private IndicatorManager indicatorManager;
        private OrderManager orderManager;

        private double maPrice = 0d;


        public BollStrategy(IndicatorManager indicatorManager, OrderManager orderManager)
        {
            this.indicatorManager = indicatorManager;
            this.orderManager = orderManager;
        }

        public List<Order> Match(MarketData marketData)
        {
            List<Order> orders =
                orderManager.getOrders().Where(o => o.InstrumentId == marketData.InstrumentId && o.StrategyType == GetType().ToString()).ToList();

            tick++;

            List<Order> list = new List<Order>();

            foreach (Order order in orders)
            {
                if (order.StatusType == EnumOrderStatus.已开仓 && tick >= 10 && (tick % 10) == 0)
                {
                    var neworder = new Order();
                    neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
                    neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy ? TThostFtdcDirectionType.Sell : TThostFtdcDirectionType.Buy;
                    neworder.InstrumentId = marketData.InstrumentId;
                    neworder.Price = marketData.LastPrice;
                    neworder.Volume = order.Volume;
                    neworder.StrategyType = GetType().ToString();
                    

                    order.CloseOrder = neworder;

                    list.Add(order);

                    log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), marketData.InstrumentId, marketData.LastPrice, maPrice,
                        orders.Count()));
                }
            }

            if (orders.Count(o => o.StatusType != EnumOrderStatus.已平仓) == 0)
            {
                var neworder = new Order();
                neworder.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                neworder.Direction = TThostFtdcDirectionType.Buy;
                neworder.InstrumentId = marketData.InstrumentId;
                neworder.Price = marketData.LastPrice;
                neworder.Volume = 1;
                neworder.StrategyType = GetType().ToString();

                list.Add(neworder);
            }

            return list;
        }
    }
}