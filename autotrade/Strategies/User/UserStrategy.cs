using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using autotrade.business;
using autotrade.converter;
using autotrade.model;
using autotrade.Stop.Loss;
using autotrade.Stop.Profit;
using MongoDB.Bson.Serialization.Attributes;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    [BsonKnownTypes(typeof(BollStrategy), typeof(DayAverageStrategy), typeof(RoundMAStrategy), typeof(RandomStrategy))]
    public abstract class UserStrategy : Strategy
    {

        [DisplayName("止损列表")]
        [Editor(typeof(StopLossUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public BindingList<StopLoss> StopLosses { get; set; }

        [DisplayName("止盈列表")]
        [Editor(typeof(StopProfitUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public BindingList<StopProfit> StopProfits { get; set; }

        private bool _allowStopLoss = true;

        protected List<Order> newOrders = new List<Order>();
        protected MarketData currMarketData;

        [DisplayName("是否止损")]
        public bool AllowStopLoss
        {
            get { return _allowStopLoss; }
            set
            {
                if (this._allowStopLoss != value)
                {
                    this._allowStopLoss = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _allowStopProfit = true;

        [DisplayName("是否止盈")]
        public bool AllowStopProfit
        {
            get { return _allowStopProfit; }
            set
            {
                if (this._allowStopProfit != value)
                {
                    this._allowStopProfit = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _autoTrade = true;

        [DisplayName("是否交易")]
        public bool AutoTrade
        {
            get { return _autoTrade; }
            set
            {
                if (this._autoTrade != value)
                {
                    this._autoTrade = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _allowOpenOrder = true;

        [DisplayName("是否开仓")]
        public bool AllowOpenOrder
        {
            get { return _allowOpenOrder; }
            set
            {
                if (this._allowOpenOrder != value)
                {
                    this._allowOpenOrder = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _allowCloseOrder = true;

        [DisplayName("是否平仓")]
        public bool AllowCloseOrder
        {
            get { return _allowCloseOrder; }
            set
            {
                if (this._allowCloseOrder != value)
                {
                    this._allowCloseOrder = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected UserStrategy()
        {
            StopLosses = new BindingList<StopLoss>();
            StopProfits = new BindingList<StopProfit>();
        }

        public override List<Order> Match(MarketData marketData)
        {
            currMarketData = marketData;

            if (AllowStopLoss)
            {
                foreach (Order order in StopLosses.Select(stopLoss => stopLoss.Match(marketData)).Where(orders => orders != null).SelectMany(orders => orders))
                {
                    OrderManager.OrderInsert(order);
                }
            }

            if (AllowStopProfit)
            {
                foreach (
                    Order order in
                        StopProfits.Select(stopProfit => stopProfit.Match(marketData))
                            .Where(orders => orders != null)
                            .SelectMany(orders => orders))
                {
                    OrderManager.OrderInsert(order);
                }
            }

            return new List<Order>();
        }
    }
}
