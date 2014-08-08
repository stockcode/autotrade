﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using autotrade.business;
using autotrade.Indicators;
using autotrade.model;
using log4net;
using MongoDB.Bson.Serialization.Attributes;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    public class BollStrategy : Strategy
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        

        private List<Order> newOrders = new List<Order>();

        private int days, tick;

        public int Day { get; set; }

        private double maPrice = 0d;

        private MarketData currMarketData;

        public BollStrategy()
        {
            Day = 20;
        }

        public override List<Order> Match(MarketData marketData)
        {
            newOrders.Clear();

            var instrumentId = marketData.InstrumentId;
            
            currMarketData = marketData;

            tick++;

            List<Order> orders = GetStrategyOrders(instrumentId);


            CloseOrder(orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓));

            if (!orders.Exists(o => o.StatusType != EnumOrderStatus.已平仓))
                OpenOrder();


            return newOrders;
        }

        private void OpenOrder()
        {
            Indicator_BOLL boll = IndicatorManager.GetBoll(Day);

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
                StrategyType = GetType().ToString()
            };

            newOrders.Add(neworder);
        }

        private void CloseOrder(List<Order> orders)
        {
            Indicator_BOLL boll = IndicatorManager.GetBoll(Day);

            if (boll == null) return;

            log.Info(boll);

            foreach (var order in orders)
            {
                if (order.Direction == TThostFtdcDirectionType.Buy && currMarketData.LastPrice < boll.UB) continue;

                if (order.Direction == TThostFtdcDirectionType.Sell && currMarketData.LastPrice > boll.LB) continue;

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