using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using autotrade.business;
using autotrade.model;
using autotrade.model.Log;
using log4net;
using MongoDB.Bson.Serialization.Attributes;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    internal class DayAverageStrategy : UserStrategy
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int _maxOpenThreshold;

        [DisplayName("开仓阀值")]
        [DefaultValue(3)]
        public int MaxOpenThreshold
        {
            get { return _maxOpenThreshold; }
            set
            {
                if (this._maxOpenThreshold != value)
                {
                    this._maxOpenThreshold = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private int _maxCloseThreshold;

        [DisplayName("平仓阀值")]
        [DefaultValue(5)]
        public int MaxCloseThreshold
        {
            get { return _maxCloseThreshold; }
            set
            {
                if (this._maxCloseThreshold != value)
                {
                    this._maxCloseThreshold = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private readonly Dictionary<String, MarketData> preMarketDataDictionary = new Dictionary<string, MarketData>();
        

        private int days;

        private double maPrice = 0d;
        private int  openThreshold = 0, closeThreshold = 0;
        private bool openCount, closeCount = false;

        private TThostFtdcDirectionType openDirection;

        private List<OrderStrategyLog> dayAverageLogs = new List<OrderStrategyLog>(); 

        private List<Order> newOrders = new List<Order>();

        private MarketData preMarketData;
        

        public DayAverageStrategy()
        {            
            MaxOpenThreshold = 3;
            MaxCloseThreshold = 5;
        }

        public override List<Order> Match(MarketData marketData)
        {
            base.Match(marketData);

            newOrders.Clear();

            var instrumentId = marketData.InstrumentId;

            preMarketData = IndicatorManager.GetPreMarketData(instrumentId);

            if (preMarketData == null) return newOrders;

            
            List<Order> orders = GetStrategyOrders(instrumentId);


            CloseOrder(orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓));

            if (!orders.Exists(o => o.StatusType != EnumOrderStatus.已平仓))
                OpenOrder();

            
            
            return newOrders;
        }

        private void OpenOrder()
        {
                var result = Cross(preMarketData, currMarketData);

                if (result == TThostFtdcDirectionType.Buy)
                {                    
                    openCount = true;
                    openDirection = result;

                    openThreshold = 1;

                    GetLog(result.ToString(), preMarketData, currMarketData, openThreshold);

                } else if (result == TThostFtdcDirectionType.Sell)
                {
                    openDirection = result;
                    openCount = true;

                    openThreshold = 1;

                    GetLog(result.ToString(), preMarketData, currMarketData, openThreshold);
                }

                if (result == TThostFtdcDirectionType.Nothing && openCount)
                {
                    if (openThreshold >= MaxOpenThreshold)
                    {

                        var neworder = new Order();
                        neworder.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                        neworder.Direction = openDirection;
                        neworder.InstrumentId = currMarketData.InstrumentId;
                        neworder.LastPrice = currMarketData.LastPrice;
                        neworder.Price = 0;
                        neworder.Volume = InstrumentStrategy.Volume;
                        neworder.StrategyType = GetType().ToString();
                        neworder.StrategyLogs.AddRange(dayAverageLogs);

                        newOrders.Add(neworder);

//                        neworder = new Order();
//                        neworder.OffsetFlag = TThostFtdcOffsetFlagType.Open;
//                        neworder.Direction = openDirection == TThostFtdcDirectionType.Buy ? TThostFtdcDirectionType.Sell : TThostFtdcDirectionType.Buy;
//                        neworder.InstrumentId = currMarketData.InstrumentId;
//                        neworder.LastPrice = currMarketData.LastPrice;
//                        neworder.Price = openDirection == TThostFtdcDirectionType.Buy? currMarketData.AveragePrice + 2 : currMarketData.AveragePrice - 2;
//                        neworder.Volume = InstrumentStrategy.Volume;
//                        neworder.StrategyType = "User";
                        //neworder.StrategyLogs.AddRange(dayAverageLogs);

                        newOrders.Add(neworder);

                        log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), currMarketData.InstrumentId,
                            currMarketData.LastPrice, currMarketData.AveragePrice,
                            result));

                        openThreshold = 0;
                        openCount = false;
                        dayAverageLogs.Clear();
                    }
                    else
                    {
                        openThreshold++;

                        GetLog(result.ToString(), preMarketData, currMarketData, openThreshold);
                    }
                }
            
        }

        private void CloseOrder(List<Order> orders)
        {


            foreach (var order in orders)
            {
                    var result = Cross(preMarketData, currMarketData);

                    if (result == order.Direction)
                    {
                        closeCount = false;
                        closeThreshold = 0;
                        dayAverageLogs.Clear();

                        continue;
                    }

                    if (result == TThostFtdcDirectionType.Nothing)
                    {
                        if (closeCount)
                        {
                            if (closeThreshold >= MaxCloseThreshold)
                            {
                                var neworder = new Order();
                                neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
                                neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                                    ? TThostFtdcDirectionType.Sell
                                    : TThostFtdcDirectionType.Buy;
                                neworder.InstrumentId = currMarketData.InstrumentId;
                                neworder.LastPrice = currMarketData.LastPrice;
                                neworder.Price = 0;
                                neworder.Volume = order.Volume;
                                neworder.StrategyType = GetType().ToString();
                                neworder.StrategyLogs.AddRange(dayAverageLogs);

                                order.CloseOrder = neworder;

                                newOrders.Add(order);

                                log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), currMarketData.InstrumentId,
                                    currMarketData.LastPrice, maPrice,
                                    orders.Count()));

                                //开反向新仓
                                neworder = new Order();
                                neworder.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                                neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                                    ? TThostFtdcDirectionType.Sell
                                    : TThostFtdcDirectionType.Buy;

                                neworder.InstrumentId = currMarketData.InstrumentId;
                                neworder.LastPrice = currMarketData.LastPrice;
                                neworder.Price = 0;
                                neworder.Volume = InstrumentStrategy.Volume;
                                neworder.StrategyType = GetType().ToString();
                                neworder.StrategyLogs.AddRange(dayAverageLogs);

                                newOrders.Add(neworder);

                                closeCount = false;
                                closeThreshold = 0;
                                dayAverageLogs.Clear();
                            }
                            else
                            {
                                closeThreshold++;
                                GetLog(result.ToString(), preMarketData, currMarketData, closeThreshold);
                            }
                        }
                    }
                    else
                    {
                        GetLog(result.ToString(), preMarketData, currMarketData, closeThreshold);
                        closeCount = true;
                    }


            }
        }

        private void GetLog(String direction, MarketData preMarketData, MarketData marketData, int threshold)
        {
            var dayAverageLog = new DayAverageLog
            {
                Direction = direction,
                Threshold = threshold,
                PreLastPrice = preMarketData.LastPrice,
                PreAveragePrice = preMarketData.AveragePrice,
                PreUpdateTime = preMarketData.UpdateTimeSec,
                LastPrice = marketData.LastPrice,
                AveragePrice = marketData.AveragePrice,
                UpdateTime = marketData.UpdateTimeSec
            };
            dayAverageLogs.Add(dayAverageLog);   
         
            log.Info(dayAverageLog.Direction + ":" + dayAverageLog.Threshold);
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