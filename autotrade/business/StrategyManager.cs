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

        private Dictionary<String, List<IStrategy>> dictStrategies = new Dictionary<string, List<IStrategy>>();

        public IndicatorManager indicatorManager { get; set; }

        private OrderManager orderManager;

        private bool isStart = false;

        public StrategyManager(OrderManager orderManager)
        {
            this.orderManager = orderManager;
        }

        public void PrcessData(MarketData marketData)
        {
            if (!isStart) return;

            if (!dictStrategies.ContainsKey(marketData.InstrumentId)) InitStrategies(marketData.InstrumentId);

            foreach (IStrategy strategy in dictStrategies[marketData.InstrumentId])
            {
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

        private void InitStrategies(string instrumentId)
        {
            List<IStrategy> list = new List<IStrategy>();

            //Strategies.Add(new AboveMAStrategy(indicatorManager, 20));
            //Strategies.Add(new BelowMAStrategy(indicatorManager, 20));
            //list.Add(new BollStrategy(indicatorManager, orderManager));
            list.Add(new DayAverageStrategy(orderManager));

            dictStrategies.Add(instrumentId, list);
        }

        public void Start()
        {
            isStart = true;
        }
    }
}
