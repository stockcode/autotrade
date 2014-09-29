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
                    .OrderByDescending(o => o.CloseOrder.Price)
                    .ToList();

            Order lastOpenOrder = null, unClosingOrder = null, lastClosingOrder = null;

            

            if (buyOpenOrders.Count > 0) lastOpenOrder = buyOpenOrders[buyOpenOrders.Count - 1];

            

            for (int i = 0; i < Count; i++)
            {
                double buyPrice = Math.Min(currMarketData.LastPrice, Math.Round(MALBPrice)) -
                                  2*InstrumentStrategy.PriceTick - i*InstrumentStrategy.PriceTick;


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

                    unClosingOrder = null;

                    if (i < buyUnClosingOrders.Count) unClosingOrder = buyUnClosingOrders[i];

                    if (unClosingOrder != null)
                    {
                        if (AllowCloseOrder)
                        {
                            order.OffsetFlag = unClosingOrder.IsTodayOrder
                                ? TThostFtdcOffsetFlagType.CloseToday
                                : TThostFtdcOffsetFlagType.Close;


                            unClosingOrder.CloseOrder = order;
                            newOrders.Add(unClosingOrder);
                            continue;
                        }
                    }

                    lastClosingOrder = null;

                    if (buyClosingOrders.Count > i) lastClosingOrder = buyClosingOrders[buyClosingOrders.Count - i - 1];

                    if (lastClosingOrder != null)
                    {
                        if (AllowCloseOrder)
                        {
                            order.OffsetFlag = lastClosingOrder.CloseOrder.OffsetFlag;

                            OrderManager.ChangeCloseOrder(lastClosingOrder, order);

                            continue;
                        }
                    }

                    if (AllowOpenOrder)
                    {
                        newOrders.Add(order);
                    }
                }
                else
                {
                    break;
                }
            }

            List<Order> cancelOrders = buyOpenOrders.FindAll(o => o.Price > Math.Round(MALBPrice));

            if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);

            cancelOrders = buyClosingOrders.FindAll(o => o.CloseOrder.Price > Math.Round(MALBPrice));

            if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);
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
                    .OrderBy(o => o.CloseOrder.Price)
                    .ToList();

            List<Order> sellUnClosingOrders =
                orders.FindAll(
                    o =>
                        o.StatusType == EnumOrderStatus.已开仓 && o.Direction == TThostFtdcDirectionType.Buy)
                    .ToList();

            Order lastOpenOrder = null, unClosingOrder = null, lastClosingOrder = null;

            if (sellOpenOrders.Count > 0)
                lastOpenOrder = sellOpenOrders[sellOpenOrders.Count - 1];

            

            


            for (int i = 0; i < Count; i++)
            {
                double sellPrice = Math.Max(currMarketData.LastPrice, Math.Round(MAUBPrice)) +
                                   2*InstrumentStrategy.PriceTick + i*InstrumentStrategy.PriceTick;

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

                    unClosingOrder = null;
                    if (i < sellUnClosingOrders.Count) unClosingOrder = sellUnClosingOrders[i];

                    if (unClosingOrder != null)
                    {
                        if (AllowCloseOrder)
                        {
                            order.OffsetFlag = unClosingOrder.IsTodayOrder
                                ? TThostFtdcOffsetFlagType.CloseToday
                                : TThostFtdcOffsetFlagType.Close;


                            unClosingOrder.CloseOrder = order;
                            newOrders.Add(unClosingOrder);
                            continue;
                        }
                    }

                    lastClosingOrder = null;

                    if (sellClosingOrders.Count > i)
                        lastClosingOrder = sellClosingOrders[sellClosingOrders.Count - i - 1];

                    if (lastClosingOrder != null)
                    {
                        if (AllowCloseOrder)
                        {
                            order.OffsetFlag = lastClosingOrder.CloseOrder.OffsetFlag;

                            OrderManager.ChangeCloseOrder(lastClosingOrder, order);
                            continue;
                        }
                    }

                    if (AllowOpenOrder)
                    {
                        //OrderManager.CancelOrder(lastOpenOrder);

                        newOrders.Add(order);
                    }
                }
                else
                {
                    break;
                }
            }

            List<Order> cancelOrders = sellOpenOrders.FindAll(o => o.Price < Math.Round(MAUBPrice));

            if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);

            cancelOrders = sellClosingOrders.FindAll(o => o.CloseOrder.Price < Math.Round(MAUBPrice));

            if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);
        }
    }
}