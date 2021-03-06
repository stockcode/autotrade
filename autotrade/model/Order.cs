﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using autotrade.model.Log;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;
using QuantBox.CSharp2CTP;

namespace autotrade.model
{
    public class Order : Entity, INotifyPropertyChanged
    {
        private static readonly TimeSpan _whenTimeIsOver = new TimeSpan(20, 00, 00);

        /// <summary>
        ///     报单编号
        /// </summary>
        private string _orderSysID;

        private int _remainVolume;
        private EnumOrderStatus _statusType;

        /// <summary>
        ///     成交编号
        /// </summary>
        private string _tradeID;

        private double closeProfit;
        private double positionProfit;

        public Order()
        {
            StrategyLogs = new List<OrderStrategyLog>();
            CloseOrders = new List<Order>();
        }

        [DisplayName("合约")]
        public string InstrumentId { get; set; }

        [Browsable(false)]
        public TThostFtdcOffsetFlagType OffsetFlag { get; set; }

        [DisplayName("买卖")]
        public TThostFtdcDirectionType Direction { get; set; }

        [DisplayName("挂单价")]
        public double Price { get; set; }

        [DisplayName("成交价")]
        public double TradePrice { get; set; }

        [DisplayName("最新价")]
        [BsonIgnore]
        public double LastPrice { get; set; }

        [DisplayName("持仓盈亏")]
        [BsonIgnore]
        public double PositionProfit
        {
            get { return positionProfit; }

            set
            {
                positionProfit = value;
                NotifyPropertyChanged();
            }
        }

        [DisplayName("平仓盈亏")]
        public double CloseProfit
        {
            get { return closeProfit; }

            set
            {
                closeProfit = value;
                NotifyPropertyChanged();
            }
        }

        [DisplayName("持仓手数")]
        public int Volume { get; set; }

        [DisplayName("剩余手数")]
        public int RemainVolume
        {
            get { return _remainVolume; }
            set
            {
                if (_remainVolume != value)
                {
                    _remainVolume = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [DisplayName("占用的保证金")]
        public double UseMargin { get; set; }

        [DisplayName("止盈价格")]
        public double StopProfit { get; set; }

        [DisplayName("状态")]
        public EnumOrderStatus StatusType
        {
            get { return _statusType; }
            set
            {
                if (_statusType != value)
                {
                    _statusType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [Browsable(false)]
        public string OrderRef { get; set; }

        [Browsable(false)]
        public string OrderSysID
        {
            get { return _orderSysID; }
            set
            {
                if (value != null && _orderSysID != value.Trim())
                {
                    _orderSysID = value.Trim();
                    NotifyPropertyChanged();
                }
            }
        }

        [Browsable(false)]
        public int SessionID { get; set; }

        [Browsable(false)]
        public int FrontID { get; set; }

        [Browsable(false)]
        public int Unit { get; set; }


        [Browsable(false)]
        public string TradeID
        {
            get { return _tradeID; }
            set
            {
                if (value != null && _tradeID != value.Trim())
                {
                    _tradeID = value.Trim();
                    NotifyPropertyChanged();
                }
            }
        }

        [Browsable(false)]
        public string TradeDate { get; set; }

        [Browsable(false)]
        public string ActualTradeDate
        {
            get
            {
                if (TradeDate == null) return null;

                return
                    (DateTime.Today.CompareTo(DateTime.ParseExact(TradeDate, "yyyyMMdd", CultureInfo.InvariantCulture)) ==
                     0
                        ? TradeDateTime
                        : DateTime.Today.ToString("yyyyMMdd") + " " + TradeTime);
            }
        }

        [Browsable(false)]
        public string TradeTime { get; set; }

        [BsonIgnore]
        [DisplayName("成交时间")]
        public string TradeDateTime
        {
            get { return TradeDate + " " + TradeTime; }
        }

        [BsonIgnore]
        [DisplayName("持仓时间")]
        public TimeSpan PositionTimeSpan { get; set; }


        [DisplayName("开仓策略")]
        public String StrategyType { get; set; }

        [Browsable(false)]
        public List<Order> CloseOrders { get; set; }

        [Browsable(false)]
        public List<OrderStrategyLog> StrategyLogs { get; set; }

        public string ExchangeID { get; set; }

        [Browsable(false)]
        [BsonIgnore]
        public bool IsTodayOrder
        {
            get
            {
                DateTime today = DateTime.Today;
                DateTime now = DateTime.Now;

                DateTime closeTime = DateTime.ParseExact(today.ToString("yyyyMMdd") + " 20:00:00", "yyyyMMdd HH:mm:ss",
                    CultureInfo.InvariantCulture);

                DateTime trade = DateTime.ParseExact(TradeDateTime, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture);

                if (trade.Date == today)
                {
                    if (trade < closeTime && now < closeTime) return true;

                    if (trade > closeTime && now > closeTime) return true;

                    return false;
                }
                if (today.Subtract(trade).Days == 0)
                {
                    if (trade.TimeOfDay > _whenTimeIsOver && now.TimeOfDay < _whenTimeIsOver) return true;

                    return false;
                }
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public override string ToString()
        {
            return string.Format("Order(合约代码={0},开平标志={1},买卖方向={2},价格={3},手数={4},状态={5},报单引用={6},报单编号={7})"
                , InstrumentId, OffsetFlag, Direction, Price, Volume, StatusType, OrderRef, OrderSysID);
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public List<Order> GetClosingOrders()
        {
            return CloseOrders.FindAll(o => o.StatusType == EnumOrderStatus.开仓中);
        }

        public bool IsClosed()
        {
            return RemainVolume <= 0;
        }
    }
}