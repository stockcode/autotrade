using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using autotrade.business;
using autotrade.model;
using log4net;
using MongoDB.Bson.Serialization.Attributes;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    public class RandomStrategy : UserStrategy
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        

        private List<Order> newOrders = new List<Order>();

        private int days, tick;

        public int Day { get; set; }

        private double maPrice = 0d;

        private MarketData currMarketData;

        public RandomStrategy()
        {
            Day = 10;
        }

        public override List<Order> Match(MarketData marketData)
        {
            base.Match(marketData);

            newOrders.Clear();

            var instrumentId = marketData.InstrumentId;
            
            currMarketData = marketData;

            tick++;

            if ((10+tick)/10 > 0) return newOrders;


            List<Order> orders = GetStrategyOrders(instrumentId);


            //CloseOrder(orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓));

            if (!orders.Exists(o => o.StatusType != EnumOrderStatus.已平仓))
                OpenOrder();


            return newOrders;
        }

        private void OpenOrder()
        {
            var neworder = new Order();
            neworder.OffsetFlag = TThostFtdcOffsetFlagType.Open;
            neworder.Direction = TThostFtdcDirectionType.Buy;
            neworder.InstrumentId = currMarketData.InstrumentId;
            neworder.Price = GetAnyPrice(currMarketData, neworder.Direction);            
            neworder.Volume = InstrumentStrategy.Volume;
            neworder.StrategyType = GetType().ToString();

            newOrders.Add(neworder);
        }

        private void CloseOrder(List<Order> orders)
        {
            foreach (var order in orders)
            {
                var neworder = new Order();
                neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
                neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                    ? TThostFtdcDirectionType.Sell
                    : TThostFtdcDirectionType.Buy;
                neworder.InstrumentId = currMarketData.InstrumentId;
                neworder.Price = GetAnyPrice(currMarketData, neworder.Direction);
                neworder.Volume = order.Volume;
                neworder.StrategyType = GetType().ToString();


                order.CloseOrder = neworder;

                newOrders.Add(order);

                log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), currMarketData.InstrumentId, currMarketData.LastPrice,
                    maPrice,
                    orders.Count()));
            }
        }
    }
}