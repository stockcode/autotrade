using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using autotrade.business;
using autotrade.Indicators;
using autotrade.model;
using autotrade.model.Log;
using log4net;
using MongoDB.Bson.Serialization.Attributes;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    public class BollStrategy : UserStrategy
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        


        public int Day { get; set; }

        private double maPrice = 0d;

        private MarketData currMarketData;

        public BollStrategy()
        {
            Day = 20;
        }

        public override List<Order> Match(MarketData marketData)
        {
            base.Match(marketData);

            newOrders.Clear();

            var instrumentId = marketData.InstrumentId;
            
            currMarketData = marketData;

            List<Order> orders = GetStrategyOrders(instrumentId);


            CloseOrder(orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓));

            if (!orders.Exists(o => o.StatusType != EnumOrderStatus.已平仓))
                OpenOrder();


            return newOrders;
        }

        private void OpenOrder()
        {
            Indicator_BOLL boll = IndicatorManager.GetBoll(Day,currMarketData.InstrumentId, EnumRecordIntervalType.Minute1);

            if (boll == null) return;

            log.Info(boll);

            TThostFtdcDirectionType direction;

            if (currMarketData.LastPrice > boll.UB) direction = TThostFtdcDirectionType.Sell;            
            else if (currMarketData.LastPrice < boll.LB) direction = TThostFtdcDirectionType.Buy;
            else return;

            var neworder = new Order
            {
                OffsetFlag = TThostFtdcOffsetFlagType.Open,
                Direction = direction,
                InstrumentId = currMarketData.InstrumentId,
                Price = GetAnyPrice(currMarketData, direction),
                Volume = InstrumentStrategy.Volume,
                StrategyType = GetType().ToString(),                
            };

            var strategyLog = new BollStrategyLog();
            strategyLog.Direction = direction.ToString();
            strategyLog.UB = boll.UB;
            strategyLog.LB = boll.LB;
            strategyLog.LastPrice = currMarketData.LastPrice;
            strategyLog.UpdateTime = currMarketData.UpdateTimeSec;

            neworder.StrategyLogs.Add(strategyLog);

            newOrders.Add(neworder);
        }

        private void CloseOrder(List<Order> orders)
        {
            Indicator_BOLL boll = IndicatorManager.GetBoll(Day, currMarketData.InstrumentId, EnumRecordIntervalType.Minute1);

            if (boll == null) return;

            log.Info(boll);

            foreach (var order in orders)
            {
                if (order.Direction == TThostFtdcDirectionType.Buy && currMarketData.LastPrice < boll.UB) continue;

                if (order.Direction == TThostFtdcDirectionType.Sell && currMarketData.LastPrice > boll.LB) continue;

                var neworder = new Order
                {
                    OffsetFlag = TThostFtdcOffsetFlagType.CloseToday,
                    Direction = order.Direction == TThostFtdcDirectionType.Buy
                        ? TThostFtdcDirectionType.Sell
                        : TThostFtdcDirectionType.Buy,
                    InstrumentId = currMarketData.InstrumentId,
                    Volume = order.Volume,
                    StrategyType = GetType().ToString()
                };
                neworder.Price = GetAnyPrice(currMarketData, neworder.Direction);

                var strategyLog = new BollStrategyLog
                {
                    Direction = neworder.Direction.ToString(),
                    UB = boll.UB,
                    LB = boll.LB,
                    LastPrice = currMarketData.LastPrice,
                    UpdateTime = currMarketData.UpdateTimeSec
                };

                neworder.StrategyLogs.Add(strategyLog);


                order.CloseOrder = neworder;

                newOrders.Add(order);

                log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), currMarketData.InstrumentId, currMarketData.LastPrice,
                    maPrice,
                    orders.Count()));
            }
        }
    }
}