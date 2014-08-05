﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Autofac;
using autotrade.model;
using autotrade.Strategies;
using log4net;
using MongoRepository;
using IContainer = Autofac.IContainer;

namespace autotrade.business
{
    public class StrategyManager
    {
        private readonly Dictionary<String, InstrumentStrategy> dictStrategies =
            new Dictionary<string, InstrumentStrategy>();

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private BindingList<InstrumentStrategy> instrumentStrategies = new BindingList<InstrumentStrategy>(); 

        private readonly MongoRepository<InstrumentStrategy> strategyRepo = new MongoRepository<InstrumentStrategy>();
        private bool isStart;

        public IndicatorManager IndicatorManager { get; set; }

        public OrderManager OrderManager { get; set; }

        public IContainer Container { get; set; }

        public StrategyManager()
        {
            instrumentStrategies.ListChanged += Strategies_ListChanged;
        }

        public void PrcessData(MarketData marketData)
        {
            if (!isStart) return;

            InstrumentStrategy instrumentStrategy = dictStrategies[marketData.InstrumentId];

            if (!instrumentStrategy.StartTrade) return;

            foreach (Strategy strategy in dictStrategies[marketData.InstrumentId].Strategies)
            {
                List<Order> orders = strategy.Match(marketData, instrumentStrategy);
                if (orders != null)
                {
                    foreach (Order order in orders)
                    {
                        order.Unit = instrumentStrategy.VolumeMultiple;

                        log.Info(order);
                        OrderManager.OrderInsert(order);
                    }
                }
            }
        }

        public void InitStrategies(Instrument instrument)
        {
            InstrumentStrategy instrumentStrategy;

            instrumentStrategy = strategyRepo.FirstOrDefault(strat => strat.InstrumentID == instrument.InstrumentID);

            if (instrumentStrategy == null)
            {
                instrumentStrategy = new InstrumentStrategy();

                instrumentStrategy.InstrumentID = instrument.InstrumentID;
                instrumentStrategy.VolumeMultiple = instrument.VolumeMultiple;

                instrumentStrategy.Strategies.Add(Container.Resolve<BollStrategy>());

                instrumentStrategy.Strategies.Add(Container.Resolve<DayAverageStrategy>());

                //Strategies.Add(new AboveMAStrategy(indicatorManager, 20));
                //Strategies.Add(new BelowMAStrategy(indicatorManager, 20));

                strategyRepo.Add(instrumentStrategy);
            }
            else
            {
                
            }

            instrumentStrategy.BindEvent(Container);

            instrumentStrategies.Add(instrumentStrategy);

            dictStrategies.Add(instrument.InstrumentID, instrumentStrategy);
        }

        void Strategies_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {            
            switch (e.ListChangedType)
            {
                    case ListChangedType.ItemChanged:
                    strategyRepo.Update(instrumentStrategies[e.NewIndex]);
                    break;
            }
        }

        public void Start()
        {
            isStart = true;
        }

        public InstrumentStrategy GetInstrumentStrategy(string instrumentId)
        {
            return dictStrategies.ContainsKey(instrumentId) ? dictStrategies[instrumentId] : null;
        }

        public void RemoveStrategies(string instrumentId)
        {
            var instrumentStrategy = GetInstrumentStrategy(instrumentId);

            if (instrumentStrategy == null) return;

            instrumentStrategies.Remove(instrumentStrategy);
            dictStrategies.Remove(instrumentId);
            strategyRepo.Delete(instrumentStrategy);
        }
    }
}