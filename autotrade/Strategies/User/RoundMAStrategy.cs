using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.Indicators;
using autotrade.business;
using autotrade.model;
using MongoDB.Driver.Linq;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    class RoundMAStrategy : UserStrategy
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


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


        public override List<Order> Match(MarketData marketData)
        {
            base.Match(marketData);

            newOrders.Clear();

            List<Order> orders = GetStrategyOrders(marketData.InstrumentId);


            CloseOrder(orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓));

            OpenOrder(orders.FindAll(o => o.StatusType == EnumOrderStatus.开仓中));



            return newOrders;
        }

        private void CloseOrder(List<Order> orders)
        {
            Indicator_MA ma = IndicatorManager.GetMA(currMarketData.InstrumentId, Day, Period);

            if (ma == null) return;

            double lastPrice = currMarketData.LastPrice;

            if (lastPrice < ma.Average*(1 - Threshold))
            {

                foreach (var order in orders.FindAll(o => o.Direction == TThostFtdcDirectionType.Sell))
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
                    //neworder.StrategyLogs.AddRange(dayAverageLogs);

                    order.CloseOrder = neworder;

                    newOrders.Add(order);
                }

            }



            if (lastPrice > ma.Average*(1 + Threshold)) {

                foreach (var order in orders.FindAll(o => o.Direction == TThostFtdcDirectionType.Buy))
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
                    //neworder.StrategyLogs.AddRange(dayAverageLogs);

                    order.CloseOrder = neworder;

                    newOrders.Add(order);
                }

            }

            if (newOrders.Count > 0) startReverse = true;
        }

        private void OpenOrder(List<Order> orders)
        {
            if (startReverse && orders.Count > 0)
            {
                OrderManager.CancelOrder(orders);

                return;
            }

            startReverse = false;

            if (orders.Count >= Count) return;

            Indicator_MA ma = IndicatorManager.GetMA(currMarketData.InstrumentId, Day, Period);

            if (ma == null) return;

            double lastPrice = currMarketData.LastPrice;

            if (orders.Count > 0)
            {
                var lastOrder = orders[orders.Count - 1];

                for (int i = 1; i <= Count - orders.Count; i++)
                {
                    Order order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = lastOrder.Direction;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = lastOrder.Direction == TThostFtdcDirectionType.Buy ? lastOrder.Price - i * InstrumentStrategy.PriceTick : lastOrder.Price + i * InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }

                return;
            }
            if (lastPrice < ma.Average * (1 - Threshold))
            {
                for (int i = 1; i <= Count; i++)
                {
                    Order order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = TThostFtdcDirectionType.Buy;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = lastPrice - i * InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }
            }

            if (lastPrice > ma.Average*(1 + Threshold))
            {
                for (int i = 1; i <= Count; i++)
                {
                    Order order = new Order();
                    order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                    order.Direction = TThostFtdcDirectionType.Sell;
                    order.InstrumentId = currMarketData.InstrumentId;
                    order.Price = lastPrice + i * InstrumentStrategy.PriceTick;
                    order.Volume = InstrumentStrategy.Volume;
                    order.StrategyType = GetType().ToString();

                    newOrders.Add(order);
                }
            }
        }
    }
}
