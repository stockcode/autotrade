using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Autofac;
using autotrade.model;
using autotrade.Properties;
using autotrade.Stop.Loss;
using autotrade.Stop.Profit;
using autotrade.Strategies;
using log4net;
using MongoRepository;
using QuantBox.CSharp2CTP;
using IContainer = Autofac.IContainer;

namespace autotrade.business
{
    public class StrategyManager
    {
        private readonly Dictionary<String, InstrumentStrategy> dictStrategies =
            new Dictionary<string, InstrumentStrategy>();

        private readonly BindingList<InstrumentStrategy> instrumentStrategies = new BindingList<InstrumentStrategy>();
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MongoRepository<InstrumentStrategy> strategyRepo = new MongoRepository<InstrumentStrategy>();
        private bool isStart;

        public StrategyManager()
        {
            instrumentStrategies.ListChanged += Strategies_ListChanged;
        }

        public IndicatorManager IndicatorManager { get; set; }

        public OrderManager OrderManager { get; set; }

        public IContainer Container { get; set; }

        public void PrcessData(MarketData marketData)
        {
            if (!isStart) return;

            InstrumentStrategy instrumentStrategy = dictStrategies[marketData.InstrumentId];

            if (!instrumentStrategy.StartTrade) return;

            foreach (UserStrategy strategy in instrumentStrategy.Strategies)
            {
                if (!strategy.AutoTrade) continue;
                
                

                var sc = (StringCollection) InstrumentType.Default[marketData.Code];

                foreach (var time in sc)
                {
                    TimeSpan endSpan = new TimeSpan(Convert.ToInt32(time.Split(':')[0]), Convert.ToInt32(time.Split(':')[1]), 0);                        
                }



                List<Order> orders = strategy.Match(marketData);

                foreach (Order order in orders)
                {
                    order.Unit = instrumentStrategy.VolumeMultiple;

                    if (order.Direction == TThostFtdcDirectionType.Buy)
                        order.UseMargin = order.Price*order.Unit*order.Volume*instrumentStrategy.LongMarginRatio;
                    else
                        order.UseMargin = order.Price*order.Unit*order.Volume*instrumentStrategy.ShortMarginRatio;

                    log.Info(order);
                    OrderManager.OrderInsert(order);
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
                instrumentStrategy.InstrumentName = instrument.InstrumentName;
                instrumentStrategy.VolumeMultiple = instrument.VolumeMultiple;
                instrumentStrategy.PriceTick = instrument.PriceTick;
                instrumentStrategy.LongMarginRatio = instrument.LongMarginRatio;
                instrumentStrategy.ShortMarginRatio = instrument.ShortMarginRatio;

                //instrumentStrategy.Strategies.Add(Container.Resolve<BollStrategy>());

                instrumentStrategy.Strategies.Add(Container.Resolve<RoundMAStrategy>());

//                instrumentStrategy.StopLosses.Add(Container.Resolve<PriceStopLoss>());
//
//                instrumentStrategy.StopProfits.Add(Container.Resolve<PriceStopProfit>());

                //Strategies.Add(new AboveMAStrategy(indicatorManager, 20));
                //Strategies.Add(new BelowMAStrategy(indicatorManager, 20));

                strategyRepo.Add(instrumentStrategy);
            }

            instrumentStrategy.Configure(Container);
            instrumentStrategy.BindEvent();
            

            instrumentStrategies.Add(instrumentStrategy);

            dictStrategies.Add(instrument.InstrumentID, instrumentStrategy);
        }

        private void Strategies_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                    instrumentStrategies[e.NewIndex].Configure(Container);
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
            InstrumentStrategy instrumentStrategy = GetInstrumentStrategy(instrumentId);

            if (instrumentStrategy == null) return;

            instrumentStrategies.Remove(instrumentStrategy);
            dictStrategies.Remove(instrumentId);
            strategyRepo.Delete(instrumentStrategy);
        }
    }
}