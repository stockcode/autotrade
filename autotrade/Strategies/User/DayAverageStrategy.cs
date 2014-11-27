using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using autotrade.model;
using autotrade.model.Log;
using log4net;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    internal class DayAverageStrategy : UserStrategy
    {
        private readonly List<OrderStrategyLog> dayAverageLogs = new List<OrderStrategyLog>();
        private readonly string hedgeType = "DayAverageHedge";
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<String, MarketData> preMarketDataDictionary = new Dictionary<string, MarketData>();

        private int _maxCloseThreshold;

        private bool _maxLossStop = true;

        private int _maxLossThreshold = 3;
        private int _maxOpenThreshold;
        private bool closeCount;

        private int closeThreshold, lossThreshold;
        private double maPrice = 0d;
        private List<Order> newOrders = new List<Order>();
        private bool openCount;

        private TThostFtdcDirectionType openDirection;
        private int openThreshold;

        private MarketData preMarketData;
        private int tick;


        public DayAverageStrategy()
        {
            MaxOpenThreshold = 3;
            MaxCloseThreshold = 5;
        }

        [DisplayName("开仓阀值")]
        [DefaultValue(3)]
        public int MaxOpenThreshold
        {
            get { return _maxOpenThreshold; }
            set
            {
                if (_maxOpenThreshold != value)
                {
                    _maxOpenThreshold = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [DisplayName("平仓阀值")]
        [DefaultValue(5)]
        public int MaxCloseThreshold
        {
            get { return _maxCloseThreshold; }
            set
            {
                if (_maxCloseThreshold != value)
                {
                    _maxCloseThreshold = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [DisplayName("连亏暂停")]
        [DefaultValue(true)]
        public bool MaxLossStop
        {
            get { return _maxLossStop; }
            set
            {
                if (_maxLossStop == value) return;

                _maxLossStop = value;

                lossThreshold = 0;

                NotifyPropertyChanged();
            }
        }

        [DisplayName("连亏阀值")]
        [DefaultValue(3)]
        public int MaxLossThreshold
        {
            get { return _maxLossThreshold; }
            set
            {
                if (_maxLossThreshold == value) return;

                _maxLossThreshold = value;
                NotifyPropertyChanged();
            }
        }

        public override List<Order> Match(MarketData marketData)
        {
            base.Match(marketData);

            newOrders.Clear();


            log.Info("AVGPrice=" + GetAvgPrice());

            string instrumentId = marketData.InstrumentId;

            preMarketData = IndicatorManager.GetPreMarketData(instrumentId);

            if (preMarketData == null) return newOrders;


            var orders = GetStrategyOrders(instrumentId);


            CloseOrder(orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓));

            if (!orders.Exists(o => o.StatusType != EnumOrderStatus.已平仓))
                OpenOrder();


            if (lossThreshold >= MaxLossThreshold)
            {
                tick++;
                if (tick >= 2400)
                {
                    lossThreshold = 0;
                    tick = 0;
                }
            }

            log.Info("lossThreshold=" + lossThreshold);

            return newOrders;
        }

        private void CheckHeding()
        {
            List<Order> orders = GetStrategyOrders(currMarketData.InstrumentId, hedgeType);

            foreach (
                Order order in
                    orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓)
                        .Where(order => order.PositionProfit < order.StopProfit))
            {
                CloseHedge();
            }
        }

        private void OpenOrder()
        {
            if (MaxLossStop && lossThreshold >= MaxLossThreshold) return;

            TThostFtdcDirectionType result = Cross(preMarketData, currMarketData);

            if (result == TThostFtdcDirectionType.Buy)
            {
                openCount = true;
                openDirection = result;

                openThreshold = 1;

                GetLog(result.ToString(), preMarketData, currMarketData, openThreshold);
            }
            else if (result == TThostFtdcDirectionType.Sell)
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
                    var neworder = new Order
                    {
                        OffsetFlag = TThostFtdcOffsetFlagType.Open,
                        Direction = openDirection,
                        InstrumentId = currMarketData.InstrumentId,
                        LastPrice = currMarketData.LastPrice,
                        Price = GetAnyPrice(GetAvgPrice(), openDirection),
                        StopProfit = -300 * InstrumentStrategy.Volume,
                        Volume = InstrumentStrategy.Volume,
                        StrategyType = GetType().ToString()
                    };
                    neworder.StrategyLogs.AddRange(dayAverageLogs);

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
            foreach (Order order in orders)
            {
//                if (order.PositionProfit < order.StopProfit)
//                {
//                    CloseOrder(order);
//                    continue;
//                }

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
                    if (currMarketData.LastPrice < currMarketData.AveragePrice &&
                        order.Direction == TThostFtdcDirectionType.Buy) closeCount = true;

                    if (currMarketData.LastPrice > currMarketData.AveragePrice &&
                        order.Direction == TThostFtdcDirectionType.Sell) closeCount = true;


                    if (!closeCount) continue;

                    if (closeThreshold >= MaxCloseThreshold)
                    {
                        if (MaxLossStop && order.PositionProfit < TOLERANCE) lossThreshold++;
                        else lossThreshold = 0;

                        CloseOrder(order);

                        log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), currMarketData.InstrumentId,
                            currMarketData.LastPrice, maPrice,
                            orders.Count()));

                        if (lossThreshold < MaxLossThreshold)
                        {
                            //开反向新仓
                            var neworder = new Order();
                            neworder.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                            neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                                ? TThostFtdcDirectionType.Sell
                                : TThostFtdcDirectionType.Buy;

                            neworder.InstrumentId = currMarketData.InstrumentId;
                            neworder.LastPrice = currMarketData.LastPrice;
                            neworder.Price = GetAnyPrice(currMarketData, neworder.Direction);
                            neworder.StopProfit = -300;
                            neworder.Volume = InstrumentStrategy.Volume;
                            neworder.StrategyType = GetType().ToString();
                            neworder.StrategyLogs.AddRange(dayAverageLogs);

                            newOrders.Add(neworder);
                        }


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
                else
                {
                    GetLog(result.ToString(), preMarketData, currMarketData, closeThreshold);
                    closeCount = true;
                }
            }
        }

        private void CloseOrder(Order order)
        {
            var neworder = new Order();
            neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
            neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                ? TThostFtdcDirectionType.Sell
                : TThostFtdcDirectionType.Buy;
            neworder.InstrumentId = currMarketData.InstrumentId;
            neworder.LastPrice = currMarketData.LastPrice;
            neworder.Price = GetAnyPrice(currMarketData, neworder.Direction);
            neworder.Volume = order.RemainVolume;
            neworder.StrategyType = GetType().ToString();
            neworder.StrategyLogs.AddRange(dayAverageLogs);

            order.CloseOrders.Add(neworder);

            newOrders.Add(order);
        }

        private double GetAvgPrice()
        {
            double avg = currMarketData.AveragePrice;
            double f1 = avg%InstrumentStrategy.PriceTick;
            if (f1 < TOLERANCE) return avg;

            avg += InstrumentStrategy.PriceTick - f1;

            return avg;
        }

        private void CloseHedge()
        {
            foreach (Order order in GetStrategyOrders(currMarketData.InstrumentId, hedgeType))
            {
                switch (order.StatusType)
                {
                    case EnumOrderStatus.开仓中:
                        OrderManager.CancelOrder(order);
                        break;
                    case EnumOrderStatus.已开仓:
                        var neworder = new Order();
                        neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
                        neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                            ? TThostFtdcDirectionType.Sell
                            : TThostFtdcDirectionType.Buy;
                        neworder.InstrumentId = currMarketData.InstrumentId;
                        neworder.LastPrice = currMarketData.LastPrice;
                        neworder.Price = GetAnyPrice(currMarketData, neworder.Direction);
                        neworder.Volume = order.RemainVolume;
                        neworder.StrategyType = hedgeType;

                        order.CloseOrders.Add(neworder);

                        OrderManager.OrderInsert(order);
                        break;
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
            }
            if (preMarketData.LastPrice > preMarketData.AveragePrice &&
                marketData.LastPrice < marketData.AveragePrice)
            {
                return TThostFtdcDirectionType.Sell;
            }
            return TThostFtdcDirectionType.Nothing;
        }
    }
}