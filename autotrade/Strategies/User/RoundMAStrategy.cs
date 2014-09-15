﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;
using autotrade.Indicators;
using autotrade.model;
using log4net;
using MongoDB.Bson.Serialization.Attributes;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    internal class RoundMAStrategy : UserStrategy
    {
        private static readonly TimeSpan _whenTimeIsOver = new TimeSpan(14, 59, 00);
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private int _count = 20;

        private int _day = 20;
        private double _maPrice;
        private EnumRecordIntervalType _period = EnumRecordIntervalType.Minute1;
        private double _threshold = 0.02;
        private Indicator_MA ma;

        private bool startReverse = false;

        [DisplayName("参数1")]
        [DefaultValue(20)]
        public int Day
        {
            get { return _day; }
            set
            {
                if (_day != value)
                {
                    _day = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [DisplayName("分析周期")]
        [DefaultValue(1)]
        public EnumRecordIntervalType Period
        {
            get { return _period; }
            set
            {
                if (_period != value)
                {
                    _period = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [DisplayName("百分比阀值")]
        [DefaultValue(0.02)]
        public double Threshold
        {
            get { return _threshold; }
            set
            {
                if (Math.Abs(_threshold - value) > TOLERANCE)
                {
                    _threshold = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [DisplayName("挂单数量")]
        [DefaultValue(20)]
        public int Count
        {
            get { return _count; }
            set
            {
                if (_count != value)
                {
                    _count = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [ReadOnly(true)]
        [BsonIgnore]
        [DisplayName("均线值")]
        public double MAPrice
        {
            get { return _maPrice; }
            set
            {
                if (!(Math.Abs(_maPrice - value) > TOLERANCE)) return;

                _maPrice = value;
                NotifyPropertyChanged();
            }
        }

        [ReadOnly(true)]
        [BsonIgnore]
        [DisplayName("均线上限值")]
        public double MAUBPrice
        {
            get { return MAPrice*(1 + Threshold); }
        }

        [ReadOnly(true)]
        [BsonIgnore]
        [DisplayName("均线下限值")]
        public double MALBPrice
        {
            get { return MAPrice*(1 - Threshold); }
        }


        public override List<Order> Match(MarketData marketData)
        {
            base.Match(marketData);

            newOrders.Clear();

            ma = IndicatorManager.GetMA(currMarketData.InstrumentId, Day, Period);

            if (ma == null) return newOrders;

            MAPrice = ma.Average;

            List<Order> orders = GetStrategyOrders(marketData.InstrumentId);

            if (DateTime.Now.Hour == 14 && DateTime.Now.Minute == 59)
            {
                List<Order> list = orders.FindAll(o => o.StatusType == EnumOrderStatus.开仓中);
                if (list.Count > 0) OrderManager.CancelOrder(list);

                list = orders.FindAll(o => o.StatusType == EnumOrderStatus.平仓中);
                if (list.Count > 0) OrderManager.CancelOrder(list);

                return newOrders;
            }

            if (AllowCloseOrder)    CloseOrder(orders);

            if (AllowOpenOrder)     OpenOrder(orders);

            //ChangeCloseOrder(orders.FindAll(o => o.StatusType == EnumOrderStatus.平仓中));

            return newOrders;
        }

        private void ChangeCloseOrder(List<Order> orders)
        {
            List<Order> sellOrders =
                orders.FindAll(o => o.CloseOrder.Direction == TThostFtdcDirectionType.Sell)
                    .OrderBy(o => o.CloseOrder.Price)
                    .ToList();

            List<Order> buyOrders =
                orders.FindAll(o => o.CloseOrder.Direction == TThostFtdcDirectionType.Buy)
                    .OrderByDescending(o => o.CloseOrder.Price)
                    .ToList();

            if (sellOrders.Count > 0)
            {
                double sellPrice = 0d;

                if (currMarketData.LastPrice > MAUBPrice)
                {
                    sellPrice = currMarketData.LastPrice + 2*InstrumentStrategy.PriceTick;
                }
                else
                {
                    sellPrice = Math.Round(MAUBPrice) + InstrumentStrategy.PriceTick;
                }

                var lastOrder = sellOrders[sellOrders.Count - 1];


                if (!sellOrders.Exists(o => Math.Abs(o.CloseOrder.Price - sellPrice) < TOLERANCE))
                {
                    var closeorder = new Order();
                    closeorder.OffsetFlag = lastOrder.CloseOrder.OffsetFlag;
                    closeorder.Direction = lastOrder.CloseOrder.Direction;
                    closeorder.InstrumentId = currMarketData.InstrumentId;
                    closeorder.Price = sellPrice;
                    closeorder.Volume = lastOrder.CloseOrder.Volume;
                    closeorder.StrategyType = GetType().ToString();

                    OrderManager.ChangeCloseOrder(lastOrder, closeorder);
                }


                var cancelOrders = sellOrders.FindAll(o => o.CloseOrder.Price < MAUBPrice);

                if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);
            }

            if (buyOrders.Count > 0)
            {
                double buyPrice = 0d;

                if (currMarketData.LastPrice < MALBPrice)
                {
                    buyPrice = currMarketData.LastPrice - 2*InstrumentStrategy.PriceTick;
                }
                else
                {
                    buyPrice = Math.Round(MALBPrice) - InstrumentStrategy.PriceTick;
                }

                var lastOrder = buyOrders[buyOrders.Count - 1];


                if (!buyOrders.Exists(o => Math.Abs(o.CloseOrder.Price - buyPrice) < TOLERANCE))
                {
                    var closeorder = new Order();
                    closeorder.OffsetFlag = lastOrder.CloseOrder.OffsetFlag;
                    closeorder.Direction = lastOrder.CloseOrder.Direction;
                    closeorder.InstrumentId = currMarketData.InstrumentId;
                    closeorder.Price = buyPrice;
                    closeorder.Volume = InstrumentStrategy.Volume;
                    closeorder.StrategyType = GetType().ToString();

                    OrderManager.ChangeCloseOrder(lastOrder, closeorder);
                }


                var cancelOrders = buyOrders.FindAll(o => o.CloseOrder.Price > MALBPrice);

                if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);
            }
        }

        private void CloseOrder(List<Order> orders)
        {
            var closeOrders = orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓);

            List<Order> sellOrders =
                orders.FindAll(o => o.Direction == TThostFtdcDirectionType.Sell).OrderByDescending(o => o.Price).ToList();

            Order highSellOrder = null;

            if (sellOrders.Count >= 0) highSellOrder = sellOrders[0];

            List<Order> buyOrders =
                orders.FindAll(o => o.Direction == TThostFtdcDirectionType.Buy).OrderBy(o => o.Price).ToList();

            Order lowBuyOrder = null;

            if (buyOrders.Count >= 0) lowBuyOrder = buyOrders[0];

            int index = 1;

            foreach (
                var order in closeOrders.FindAll(o => o.Direction == TThostFtdcDirectionType.Sell).OrderBy(o => o.Price))
            {
                var neworder = new Order();
                neworder.OffsetFlag = order.IsTodayOrder
                    ? TThostFtdcOffsetFlagType.CloseToday
                    : TThostFtdcOffsetFlagType.Close;
                neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                    ? TThostFtdcDirectionType.Sell
                    : TThostFtdcDirectionType.Buy;
                neworder.InstrumentId = currMarketData.InstrumentId;

                if (lowBuyOrder == null)
                {
                    neworder.Price = Math.Round(MALBPrice) - (order.Price - Math.Round(MAUBPrice));
                }
                else
                {
                    neworder.Price = lowBuyOrder.Price - index*InstrumentStrategy.PriceTick;

                }

                neworder.Volume = order.Volume;
                neworder.StrategyType = GetType().ToString();
                //neworder.StrategyLogs.AddRange(dayAverageLogs);

                order.CloseOrder = neworder;

                newOrders.Add(order);
                index++;
            }

            index = 1;
            foreach (
                Order order in
                    orders.FindAll(o => o.Direction == TThostFtdcDirectionType.Buy).OrderByDescending(o => o.Price))
            {
                var neworder = new Order();
                neworder.OffsetFlag = order.IsTodayOrder
                    ? TThostFtdcOffsetFlagType.CloseToday
                    : TThostFtdcOffsetFlagType.Close;
                neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                    ? TThostFtdcDirectionType.Sell
                    : TThostFtdcDirectionType.Buy;
                neworder.InstrumentId = currMarketData.InstrumentId;

                if (highSellOrder == null)
                {
                    neworder.Price = Math.Round(MAUBPrice) + (Math.Round(MALBPrice) - order.Price);
                }
                else
                {
                    neworder.Price = highSellOrder.Price + index*InstrumentStrategy.PriceTick;
                }

                neworder.Volume = order.Volume;
                neworder.StrategyType = GetType().ToString();
                //neworder.StrategyLogs.AddRange(dayAverageLogs);

                order.CloseOrder = neworder;

                newOrders.Add(order);
            }
        }

        private void OpenOrder(List<Order> orders)
        {
            double lastPrice = currMarketData.LastPrice;

            
            List<Order> sellOpenOrders =
                orders.FindAll(o => o.StatusType == EnumOrderStatus.开仓中 && o.Direction == TThostFtdcDirectionType.Sell).OrderBy(o => o.Price).ToList();

            List<Order> buyOpenOrders =
                orders.FindAll(o => o.StatusType == EnumOrderStatus.开仓中 && o.Direction == TThostFtdcDirectionType.Buy).OrderByDescending(o => o.Price).ToList();

            List<Order> sellCloseOrders =
                orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓 && o.CloseOrder.Direction == TThostFtdcDirectionType.Sell).OrderBy(o => o.CloseOrder.Price).ToList();

            List<Order> buyCloseOrders =
                orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓 && o.CloseOrder.Direction == TThostFtdcDirectionType.Buy).OrderByDescending(o => o.CloseOrder.Price).ToList();

            Order lastOrder = sellOpenOrders[sellOpenOrders.Count - 1];

            if (sellCloseOrders.Count > 0) lastOrder = sellCloseOrders[sellCloseOrders.Count - 1];

            if (sellOpenOrders.Count == 0)
            {
                for (int i = 1; i <= Count; i++)
                {
                    var order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = TThostFtdcDirectionType.Sell;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = Math.Round(MAUBPrice) + i*InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }
            }
            else if (sellOpenOrders.Count + sellCloseOrders.Count < Count)
            {

                for (int i = 1; i <= Count - sellOpenOrders.Count; i++)
                {
                    var order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = lastOrder.Direction;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = lastOrder.Price + i*InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }
            }
            else if (sellOpenOrders.Count + sellCloseOrders.Count >= Count)
            {
                if (currMarketData.LastPrice > MAUBPrice)
                {
                    
                    double sellPrice = currMarketData.LastPrice + 2*InstrumentStrategy.PriceTick;

                    if (!sellOpenOrders.Exists(o => Math.Abs(o.Price - sellPrice) < TOLERANCE))
                    {
                        var order = new Order();
                        order.OffsetFlag = lastOrder.OffsetFlag;
                        order.Direction = lastOrder.Direction;
                        order.InstrumentId = lastOrder.InstrumentId;
                        order.Price = sellPrice;
                        order.Volume = lastOrder.Volume;
                        order.StrategyType = GetType().ToString();

                        OrderManager.CancelOrder(lastOrder);

                        newOrders.Add(order);                        
                    }
                }

                List<Order> cancelOrders = sellOpenOrders.FindAll(o => o.Price < MAUBPrice);

                if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);
            }



            lastOrder = buyOpenOrders[buyOpenOrders.Count - 1];

            if (buyCloseOrders.Count > 0) lastOrder = buyCloseOrders[buyCloseOrders.Count - 1];


            if (buyOpenOrders.Count == 0)
            {
                for (int i = 1; i <= Count; i++)
                {
                    var order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = TThostFtdcDirectionType.Buy;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = Math.Round(MALBPrice) - i*InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }
            }
            else if (buyOpenOrders.Count + buyCloseOrders.Count < Count)
            {

                for (int i = 1; i <= Count - buyOpenOrders.Count; i++)
                {
                    var order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = lastOrder.Direction;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = lastOrder.Price - i*InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }
            }
            else if (buyOpenOrders.Count + buyCloseOrders.Count >= Count)
            {
                if (currMarketData.LastPrice < MALBPrice)
                {
                    double buyPrice = currMarketData.LastPrice - 2*InstrumentStrategy.PriceTick;

                    if (!buyOpenOrders.Exists(o => Math.Abs(o.Price - buyPrice) < TOLERANCE))
                    {
                        var order = new Order();
                        order.OffsetFlag = lastOrder.OffsetFlag;
                        order.Direction = lastOrder.Direction;
                        order.InstrumentId = lastOrder.InstrumentId;
                        order.Price = buyPrice;
                        order.Volume = lastOrder.Volume;
                        order.StrategyType = GetType().ToString();

                        OrderManager.CancelOrder(lastOrder);

                        newOrders.Add(order);                        
                    }
                }
                else
                {

                    double buyPrice = Math.Round(MALBPrice) - InstrumentStrategy.PriceTick;

                    if (!buyOpenOrders.Exists(o => Math.Abs(o.Price - buyPrice) < TOLERANCE))
                    {
                        var order = new Order();
                        order.OffsetFlag = lastOrder.OffsetFlag;
                        order.Direction = lastOrder.Direction;
                        order.InstrumentId = lastOrder.InstrumentId;
                        order.Price = buyPrice;
                        order.Volume = lastOrder.Volume;
                        order.StrategyType = GetType().ToString();

                        OrderManager.CancelOrder(lastOrder);

                        newOrders.Add(order);                        
                    }
                }

                List<Order> cancelOrders = buyOpenOrders.FindAll(o => o.Price > MALBPrice);

                if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);
            }
        }
    }
}