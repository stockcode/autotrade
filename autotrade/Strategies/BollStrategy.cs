using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using autotrade.business;
using autotrade.model;
using CTPTradeApi;
using log4net;

namespace autotrade.Strategies
{
    internal class BollStrategy : IStrategy
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<Order> orders = new List<Order>();

        private int days;
        private IndicatorManager indicatorManager;
        private double maPrice = 0d;

        public BollStrategy(IndicatorManager indicatorManager)
        {
            this.indicatorManager = indicatorManager;
        }

        public List<Order> Match(MarketData marketData)
        {
            if (orders.Count() > 0) return null;

            bool result = false;


            var order = new Order();
            order.OffsetFlag = EnumOffsetFlagType.Open;
            order.Direction = EnumDirectionType.Buy;
            order.InstrumentId = marketData.InstrumentId;
            order.Price = marketData.LastPrice + 1;
            order.Volume = 1;

            order.StrategyType = GetType().ToString();

            orders.Add(order);

            log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), marketData.InstrumentId, marketData.LastPrice, maPrice,
                orders.Count()));

            return orders;
        }
        
    }
}