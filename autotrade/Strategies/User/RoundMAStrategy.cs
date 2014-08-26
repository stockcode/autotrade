using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.Indicators;
using autotrade.business;
using autotrade.model;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    class RoundMAStrategy : UserStrategy
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly TimeSpan _whenTimeIsOver = new TimeSpan(14, 59, 00);

        private int _day = 20;

        private bool startReverse = false;

        [DisplayName("参数1")]
        [DefaultValue(20)]
        public int Day
        {
            get { return _day; }
            set
            {
                if (this._day != value)
                {
                    this._day = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private EnumRecordIntervalType _period = EnumRecordIntervalType.Minute1;

        [DisplayName("分析周期")]
        [DefaultValue(1)]
        public EnumRecordIntervalType Period
        {
            get { return _period; }
            set
            {
                if (this._period != value)
                {
                    this._period = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _threshold = 0.02;

        [DisplayName("百分比阀值")]
        [DefaultValue(0.02)]
        public double Threshold
        {
            get { return _threshold; }
            set
            {
                if (Math.Abs(this._threshold - value) > TOLERANCE)
                {
                    this._threshold = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int _count = 20;

        [DisplayName("挂单数量")]
        [DefaultValue(20)]
        public int Count
        {
            get { return _count; }
            set
            {
                if (this._count != value)
                {
                    this._count = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _maPrice = 0d;

        [ReadOnly(true)]
        [BsonIgnore]
        [DisplayName("均线值")]
        public double MAPrice
        {
            get { return _maPrice; }
            set
            {
                if (!(Math.Abs(this._maPrice - value) > TOLERANCE)) return;

                this._maPrice = value;
                NotifyPropertyChanged();
            }
        }

        [ReadOnly(true)]
        [BsonIgnore]
        [DisplayName("均线上限值")]
        public double MAUBPrice
        {
            get { return MAPrice * (1 + Threshold); }
        }

        [ReadOnly(true)]
        [BsonIgnore]
        [DisplayName("均线下限值")]
        public double MALBPrice
        {
            get { return MAPrice * (1 - Threshold); }
        }

        private Indicator_MA ma = null;


        public override List<Order> Match(MarketData marketData)
        {
            base.Match(marketData);

            newOrders.Clear();

            ma = IndicatorManager.GetMA(currMarketData.InstrumentId, Day, Period);

            if (ma == null) return newOrders;

            MAPrice = ma.Average;

            var orders = GetStrategyOrders(marketData.InstrumentId);

            if (DateTime.Now.TimeOfDay > _whenTimeIsOver)
            {
                var list = orders.FindAll(o => o.StatusType == EnumOrderStatus.开仓中);
                if (list.Count > 0) OrderManager.CancelOrder(list);

                list = orders.FindAll(o => o.StatusType == EnumOrderStatus.平仓中);
                if (list.Count > 0) OrderManager.CancelOrder(list);

                return newOrders;
            }


            CloseOrder(orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓));

            OpenOrder(orders.FindAll(o => o.StatusType == EnumOrderStatus.开仓中));



            return newOrders;
        }

        private void CloseOrder(List<Order> orders)
        {
            
            double lastPrice = currMarketData.LastPrice;


                foreach (var order in orders.FindAll(o => o.Direction == TThostFtdcDirectionType.Sell).OrderBy(o=>o.Price))
                {
                    var neworder = new Order();
                    neworder.OffsetFlag = order.IsTodayOrder? TThostFtdcOffsetFlagType.CloseToday : TThostFtdcOffsetFlagType.Close;
                    neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                        ? TThostFtdcDirectionType.Sell
                        : TThostFtdcDirectionType.Buy;
                    neworder.InstrumentId = currMarketData.InstrumentId;
                    neworder.Price = Math.Round(MALBPrice) - (order.Price - Math.Round(MAUBPrice));
                    neworder.Volume = order.Volume;
                    neworder.StrategyType = GetType().ToString();
                    //neworder.StrategyLogs.AddRange(dayAverageLogs);

                    order.CloseOrder = neworder;

                    newOrders.Add(order);
                }

                foreach (var order in orders.FindAll(o => o.Direction == TThostFtdcDirectionType.Buy).OrderByDescending(o => o.Price))
                {
                    var neworder = new Order();
                    neworder.OffsetFlag = order.IsTodayOrder ? TThostFtdcOffsetFlagType.CloseToday : TThostFtdcOffsetFlagType.Close;
                    neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                        ? TThostFtdcDirectionType.Sell
                        : TThostFtdcDirectionType.Buy;
                    neworder.InstrumentId = currMarketData.InstrumentId;
                    neworder.Price = Math.Round(MAUBPrice) + (Math.Round(MALBPrice) - order.Price);
                    neworder.Volume = order.Volume;
                    neworder.StrategyType = GetType().ToString();
                    //neworder.StrategyLogs.AddRange(dayAverageLogs);

                    order.CloseOrder = neworder;

                    newOrders.Add(order);
                }

            



//            if (lastPrice > ma.Average*(1 + Threshold)) {
//
//                foreach (var order in orders.FindAll(o => o.Direction == TThostFtdcDirectionType.Buy))
//                {
//                
//                    var neworder = new Order();
//                    neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
//                    neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
//                        ? TThostFtdcDirectionType.Sell
//                        : TThostFtdcDirectionType.Buy;
//                    neworder.InstrumentId = currMarketData.InstrumentId;
//                    neworder.Price = GetAnyPrice(currMarketData, neworder.Direction);
//                    neworder.Volume = order.Volume;
//                    neworder.StrategyType = GetType().ToString();
//                    //neworder.StrategyLogs.AddRange(dayAverageLogs);
//
//                    order.CloseOrder = neworder;
//
//                    newOrders.Add(order);
//                }
//
//            }

//            if (newOrders.Count > 0) startReverse = true;
        }

        private void OpenOrder(List<Order> orders)
        {
            double lastPrice = currMarketData.LastPrice;


            var sellOrders = orders.FindAll(o => o.Direction == TThostFtdcDirectionType.Sell).OrderBy(o=>o.Price).ToList();

            var buyOrders = orders.FindAll(o => o.Direction == TThostFtdcDirectionType.Buy).OrderByDescending(o=>o.Price).ToList();

            if (sellOrders.Count == 0)
            {
                for (int i = 1; i <= Count; i++)
                {
                    Order order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = TThostFtdcDirectionType.Sell;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = Math.Round(MAUBPrice) + i * InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }
            }
            else if (sellOrders.Count < Count)
            {
                var lastOrder = sellOrders[sellOrders.Count - 1];

                for (int i = 1; i <= Count - sellOrders.Count; i++)
                {
                    Order order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = lastOrder.Direction;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = lastOrder.Price + i * InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }
            }
            else if (sellOrders.Count == Count)
            {
                if (currMarketData.LastPrice > MAUBPrice)
                {
                    var lastOrder = sellOrders[sellOrders.Count - 1];

                    var sellPrice = currMarketData.LastPrice + 2 * InstrumentStrategy.PriceTick;

                    if (!sellOrders.Exists(o => Math.Abs(o.Price - sellPrice) < TOLERANCE))
                    {
                        Order order = new Order();
                        order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                        order.Direction = lastOrder.Direction;
                        order.InstrumentId = currMarketData.InstrumentId;
                        order.Price = sellPrice;
                        order.Volume = InstrumentStrategy.Volume;
                        order.StrategyType = GetType().ToString();

                        newOrders.Add(order);

                        OrderManager.CancelOrder(lastOrder);
                    }
                }

                var cancelOrders = sellOrders.FindAll(o => o.Price < MAUBPrice);

                if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);
            }


            if (buyOrders.Count == 0)
            {
                for (int i = 1; i <= Count; i++)
                {
                    Order order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = TThostFtdcDirectionType.Buy;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = Math.Round(MALBPrice) - i * InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }
            }
            else if (buyOrders.Count < Count)
            {
                var lastOrder = buyOrders[buyOrders.Count - 1];

                for (int i = 1; i <= Count - buyOrders.Count; i++)
                {
                    Order order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = lastOrder.Direction;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = lastOrder.Price - i * InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }
            }
            else if (buyOrders.Count == Count)
            {
                if (currMarketData.LastPrice < MALBPrice)
                {
                    var lastOrder = buyOrders[buyOrders.Count - 1];

                    var buyPrice = currMarketData.LastPrice - 2*InstrumentStrategy.PriceTick;

                    if (!buyOrders.Exists(o => Math.Abs(o.Price - buyPrice) < TOLERANCE))
                    {
                        Order order = new Order();
                        order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                        order.Direction = lastOrder.Direction;
                        order.InstrumentId = currMarketData.InstrumentId;
                        order.Price = buyPrice;
                        order.Volume = InstrumentStrategy.Volume;
                        order.StrategyType = GetType().ToString();

                        newOrders.Add(order);

                        OrderManager.CancelOrder(lastOrder);
                    }
                }
                else
                {
                    var lastOrder = buyOrders[buyOrders.Count - 1];

                    var buyPrice = Math.Round(MALBPrice) - InstrumentStrategy.PriceTick;

                    if (!buyOrders.Exists(o => Math.Abs(o.Price - buyPrice) < TOLERANCE))
                    {
                        Order order = new Order();
                        order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                        order.Direction = lastOrder.Direction;
                        order.InstrumentId = currMarketData.InstrumentId;
                        order.Price = buyPrice;
                        order.Volume = InstrumentStrategy.Volume;
                        order.StrategyType = GetType().ToString();

                        newOrders.Add(order);

                        OrderManager.CancelOrder(lastOrder);
                    }
                }

                var cancelOrders = buyOrders.FindAll(o=>o.Price > MALBPrice);

                if (cancelOrders.Count > 0) OrderManager.CancelOrder(cancelOrders);
            }

            
            

         
        }
    }
}
