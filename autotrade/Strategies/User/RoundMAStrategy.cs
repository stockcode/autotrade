using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static readonly TimeSpan _whenTimeIsOver = new TimeSpan(14, 59, 55);
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


            OpenBuyOrder(orders);

            OpenSellOrder(orders);


            return newOrders;
        }


        private void OpenBuyOrder(List<Order> orders)
        {
            var buyPrice = Math.Round(MALBPrice) - InstrumentStrategy.PriceTick;

            if (currMarketData.LastPrice < MALBPrice)
            {
                buyPrice = currMarketData.LastPrice - 2 * InstrumentStrategy.PriceTick;
            }

            List<Order> buyOpenOrders =
                orders.FindAll(o => o.StatusType == EnumOrderStatus.开仓中 && o.Direction == TThostFtdcDirectionType.Buy)
                    .OrderByDescending(o => o.Price)
                    .ToList();


            List<Order> buyUnClosingOrders =
                orders.FindAll(
                    o => o.StatusType == EnumOrderStatus.已开仓 && o.Direction == TThostFtdcDirectionType.Sell)
                    .ToList();

            List<Order> buyClosingOrders =
                orders.FindAll(
                    o => o.StatusType == EnumOrderStatus.平仓中 && o.Direction == TThostFtdcDirectionType.Sell)
                    .ToList();

            Order lastOpenOrder = null, unClosingOrder = null;

            if (buyUnClosingOrders.Count > 0) unClosingOrder = buyUnClosingOrders[0];

            if (buyOpenOrders.Count > 0) lastOpenOrder = buyOpenOrders[buyOpenOrders.Count - 1];

            if (buyOpenOrders.Count == 0 && AllowOpenOrder)
            {
                for (var i = 1; i <= Count; i++)
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

            if (buyOpenOrders.Count + buyClosingOrders.Count < Count)
            {
                for (var i = 0; i < Count - buyOpenOrders.Count - buyClosingOrders.Count; i++)
                {
                    var order = new Order();                    
                    order.Direction = TThostFtdcDirectionType.Buy;
                    order.InstrumentId = currMarketData.InstrumentId;

                    if (lastOpenOrder == null)
                        order.Price = Math.Round(MALBPrice) - i * InstrumentStrategy.PriceTick;
                    else
                        order.Price = lastOpenOrder.Price - i*InstrumentStrategy.PriceTick;
                    
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    if (AllowOpenOrder)
                    {
                        order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                        newOrders.Add(order);
                    }
                    else if (AllowCloseOrder && buyUnClosingOrders.Count > i)
                    {

                        order.OffsetFlag = buyUnClosingOrders[i].IsTodayOrder
                                ? TThostFtdcOffsetFlagType.CloseToday
                                : TThostFtdcOffsetFlagType.Close;


                        buyUnClosingOrders[i].CloseOrder = order;
                        newOrders.Add(buyUnClosingOrders[i]);
                    }
                }
            }
            else if (buyOpenOrders.Count + buyClosingOrders.Count >= Count)
            {

                if (!buyOpenOrders.Exists(o => Math.Abs(o.Price - buyPrice) < TOLERANCE) &&
                    !buyClosingOrders.Exists(o => Math.Abs(o.CloseOrder.Price - buyPrice) < TOLERANCE))
                {
                    var order = new Order();

                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = TThostFtdcDirectionType.Buy;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = buyPrice;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    if (unClosingOrder != null)
                    {
                        if (AllowCloseOrder)
                        {
                            order.OffsetFlag = unClosingOrder.IsTodayOrder
                                ? TThostFtdcOffsetFlagType.CloseToday
                                : TThostFtdcOffsetFlagType.Close;


                            unClosingOrder.CloseOrder = order;
                            newOrders.Add(unClosingOrder);
                        }
                    }
                    else if (AllowOpenOrder)
                    {
                        OrderManager.CancelOrder(lastOpenOrder);

                        newOrders.Add(order);
                    }
                }


                List<Order> cancelOrders = buyOpenOrders.FindAll(o => o.Price > MALBPrice);

                if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);

                cancelOrders = buyClosingOrders.FindAll(o => o.CloseOrder.Price > MALBPrice);

                if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);
            }
        }

        private void OpenSellOrder(List<Order> orders)
        {
            List<Order> sellOpenOrders =
                orders.FindAll(o => o.StatusType == EnumOrderStatus.开仓中 && o.Direction == TThostFtdcDirectionType.Sell)
                    .OrderBy(o => o.Price)
                    .ToList();

            List<Order> sellClosingOrders =
                orders.FindAll(
                    o =>
                        o.StatusType == EnumOrderStatus.平仓中 && o.Direction == TThostFtdcDirectionType.Buy)
                    .ToList();

            List<Order> sellUnClosingOrders =
                orders.FindAll(
                    o =>
                        o.StatusType == EnumOrderStatus.已开仓 && o.Direction == TThostFtdcDirectionType.Buy)
                    .ToList();

            Order lastOpenOrder = null, unClosingOrder = null;

            if (sellOpenOrders.Count > 0)
                lastOpenOrder = sellOpenOrders[sellOpenOrders.Count - 1];

            if (sellUnClosingOrders.Count > 0) unClosingOrder = sellUnClosingOrders[0];

            if (sellOpenOrders.Count == 0 && AllowOpenOrder)
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
            
            if (sellOpenOrders.Count + sellClosingOrders.Count < Count)
            {
                for (int i = 0; i < Count - sellOpenOrders.Count; i++)
                {
                    var order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = TThostFtdcDirectionType.Sell;
                    order.InstrumentId = currMarketData.InstrumentId;
                    
                    if (lastOpenOrder == null)
                        order.Price = Math.Round(MAUBPrice) + i * InstrumentStrategy.PriceTick;
                    else
                        order.Price = lastOpenOrder.Price + i*InstrumentStrategy.PriceTick;

                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    if (AllowOpenOrder)
                    {
                        order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                        newOrders.Add(order);
                    }
                    else if (AllowCloseOrder && sellUnClosingOrders.Count > i)
                    {

                        order.OffsetFlag = sellUnClosingOrders[i].IsTodayOrder
                                ? TThostFtdcOffsetFlagType.CloseToday
                                : TThostFtdcOffsetFlagType.Close;


                        sellUnClosingOrders[i].CloseOrder = order;
                        newOrders.Add(sellUnClosingOrders[i]);
                    }
                }
            }
            else if (sellOpenOrders.Count + sellClosingOrders.Count >= Count)
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


                if (!sellOpenOrders.Exists(o => Math.Abs(o.Price - sellPrice) < TOLERANCE) &&
                    !sellClosingOrders.Exists(o => Math.Abs(o.CloseOrder.Price - sellPrice) < TOLERANCE))
                {
                    var order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = TThostFtdcDirectionType.Sell;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = sellPrice;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();


                    if (unClosingOrder != null)
                    {
                        if (AllowCloseOrder)
                        {
                            order.OffsetFlag = unClosingOrder.IsTodayOrder
                                ? TThostFtdcOffsetFlagType.CloseToday
                                : TThostFtdcOffsetFlagType.Close;


                            unClosingOrder.CloseOrder = order;
                            newOrders.Add(unClosingOrder);
                        }
                    }
                    else if (AllowOpenOrder)
                    {
                        OrderManager.CancelOrder(lastOpenOrder);

                        newOrders.Add(order);
                    }
                }


                List<Order> cancelOrders = sellOpenOrders.FindAll(o => o.Price < MAUBPrice);

                if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);

                cancelOrders = sellClosingOrders.FindAll(o => o.CloseOrder.Price < MAUBPrice);

                if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);
            }
        }
    }
}