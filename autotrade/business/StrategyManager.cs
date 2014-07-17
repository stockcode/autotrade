using System;
using System.Collections.Generic;
using System.Linq;
using autotrade.Strategies;
using autotrade.model;

namespace autotrade.business
{
    class StrategyManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public List<IStrategy> Strategies = new List<IStrategy>();

        private IndicatorManager indicatorManager = new IndicatorManager();

        private OrderManager orderManager;

        public StrategyManager(OrderManager orderManager)
        {
            Strategies.Add(new AboveMAStrategy(indicatorManager, 20));
            Strategies.Add(new BelowMAStrategy(indicatorManager, 20));

            this.orderManager = orderManager;
        }

        public void PrcessData(MarketData marketData)
        {
            foreach(IStrategy strategy in Strategies) {
                List<Order> orders = strategy.Match(marketData);
                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        log.Info(order);
                        orderManager.OrderInsert(order);
                    }
                }
            }
        }
    }
}
