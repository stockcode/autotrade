using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using autotrade.model;
using autotrade.Strategies;
using log4net;
using MongoRepository;

namespace autotrade.business
{
    public class StrategyManager
    {
        private readonly Dictionary<String, InstrumentStrategy> dictStrategies =
            new Dictionary<string, InstrumentStrategy>();

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MongoRepository<InstrumentStrategy> strategyRepo = new MongoRepository<InstrumentStrategy>();
        private bool isStart;

        public IndicatorManager IndicatorManager { get; set; }

        public OrderManager OrderManager { get; set; }

        public IContainer Container { get; set; }

        public void PrcessData(MarketData marketData)
        {
            if (!isStart) return;

            if (!dictStrategies.ContainsKey(marketData.InstrumentId)) InitStrategies(marketData.InstrumentId);

            foreach (IStrategy strategy in dictStrategies[marketData.InstrumentId].Strategies)
            {
                List<Order> orders = strategy.Match(marketData);
                if (orders != null)
                {
                    foreach (Order order in orders)
                    {
                        log.Info(order);
                        OrderManager.OrderInsert(order);
                    }
                }
            }
        }

        public void InitStrategies(string instrumentId)
        {
            InstrumentStrategy instrumentStrategy;

            instrumentStrategy = strategyRepo.FirstOrDefault(strat => strat.InstrumentID == instrumentId);

            if (instrumentStrategy == null)
            {
                instrumentStrategy = new InstrumentStrategy();

                instrumentStrategy.InstrumentID = instrumentId;

                instrumentStrategy.Strategies.Add(Container.Resolve<BollStrategy>());

                instrumentStrategy.Strategies.Add(Container.Resolve<DayAverageStrategy>());

                //Strategies.Add(new AboveMAStrategy(indicatorManager, 20));
                //Strategies.Add(new BelowMAStrategy(indicatorManager, 20));

                strategyRepo.Add(instrumentStrategy);
            }

            dictStrategies.Add(instrumentId, instrumentStrategy);
        }

        public void Start()
        {
            isStart = true;
        }

        public InstrumentStrategy GetInstrumentStrategy(string instrumentId)
        {
            return dictStrategies.ContainsKey(instrumentId) ? dictStrategies[instrumentId] : null;
        }
    }
}