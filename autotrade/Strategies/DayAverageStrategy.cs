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
    internal class DayAverageStrategy : IStrategy
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly OrderManager orderManager;

        private readonly Dictionary<String, MarketData> preMarketDataDictionary = new Dictionary<string, MarketData>();
        private double TOLERANCE = 0.01d;

        private int days;

        private double maPrice = 0d;
        private List<Order> orders;
        private int tick;

        public DayAverageStrategy(OrderManager orderManager)
        {
            this.orderManager = orderManager;
        }

        public List<Order> Match(MarketData marketData)
        {
            var instrumentId = marketData.InstrumentId;

            List<Order> orders =
                orderManager.getOrders()
                    .Where(o => o.InstrumentId == marketData.InstrumentId && o.StrategyType == GetType().ToString())
                    .ToList();

            tick++;

            var list = new List<Order>();

            foreach (var order in orders)
            {
                if (order.StatusType == EnumOrderStatus.已开仓)
                {
                    var result = Cross(preMarketDataDictionary[instrumentId], marketData);

                    if (result == order.Direction || result == TThostFtdcDirectionType.Nothing) continue;

                    var neworder = new Order();
                    neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
                    neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                        ? TThostFtdcDirectionType.Sell
                        : TThostFtdcDirectionType.Buy;
                    neworder.InstrumentId = marketData.InstrumentId;
                    neworder.Price = marketData.LastPrice;
                    neworder.Volume = order.Volume;
                    neworder.StrategyType = GetType().ToString();


                    order.CloseOrder = neworder;

                    list.Add(order);

                    log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), marketData.InstrumentId,
                        marketData.LastPrice, maPrice,
                        orders.Count()));
                }
            }

            if (orders.Count(o => o.StatusType != EnumOrderStatus.已平仓) == 0 && preMarketDataDictionary.ContainsKey(instrumentId))
            {
                var result = Cross(preMarketDataDictionary[instrumentId], marketData);

                if (result != TThostFtdcDirectionType.Nothing)
                {
                    var neworder = new Order();
                    neworder.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    neworder.Direction = result;
                    neworder.InstrumentId = marketData.InstrumentId;
                    neworder.Price = marketData.LastPrice;
                    neworder.Volume = 1;
                    neworder.StrategyType = GetType().ToString();

                    list.Add(neworder);

                    log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), marketData.InstrumentId,
                        marketData.LastPrice, marketData.AveragePrice,
                        result));
                }
            }


            if (preMarketDataDictionary.ContainsKey(instrumentId))
            {
                preMarketDataDictionary[instrumentId] = marketData.Copy();
            }
            else
            {
                var preMarketData = new MarketData();
                preMarketData = marketData.Copy();
                preMarketDataDictionary.Add(instrumentId, preMarketData);
            }

            return list;
        }

        private TThostFtdcDirectionType Cross(MarketData preMarketData, MarketData marketData)
        {
            if (preMarketData.LastPrice < preMarketData.AveragePrice &&
                marketData.LastPrice > marketData.AveragePrice)
            {
                return TThostFtdcDirectionType.Buy;
            } else if (preMarketData.LastPrice > preMarketData.AveragePrice &&
                       marketData.LastPrice < marketData.AveragePrice)
            {
                return TThostFtdcDirectionType.Sell;
            } else 
                return TThostFtdcDirectionType.Nothing;
        }
    }
}