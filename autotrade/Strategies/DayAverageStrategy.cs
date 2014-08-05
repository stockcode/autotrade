using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms.VisualStyles;
using autotrade.business;
using autotrade.model;
using log4net;
using MongoDB.Bson.Serialization.Attributes;
using QuantBox.CSharp2CTP;

namespace autotrade.Strategies
{
    internal class DayAverageStrategy : Strategy, INotifyPropertyChanged
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
        [DefaultValue(3)]
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
        private double TOLERANCE = 0.01d;

        private int days;

        private double maPrice = 0d;
        private List<Order> orders;
        private int tick, upThreshold = 0, downThreshold = 0, closeThreshold = 0;
        private bool buyCount = false, sellCount = false, closeCount = false;

        public DayAverageStrategy()
        {
            MaxCloseThreshold = 3;
            MaxOpenThreshold = 3;
        }

        public override List<Order> Match(MarketData marketData, InstrumentStrategy instrumentStrategy)
        {
            var instrumentId = marketData.InstrumentId;

            List<Order> orders =
                OrderManager.getOrders()
                    .Where(o => o.InstrumentId == marketData.InstrumentId && o.StrategyType == GetType().ToString())
                    .ToList();

            tick++;

            var list = new List<Order>();

            foreach (var order in orders)
            {
                if (order.StatusType == EnumOrderStatus.已开仓 && preMarketDataDictionary.ContainsKey(instrumentId))
                {
                    var result = Cross(preMarketDataDictionary[instrumentId], marketData);

                    if (result == order.Direction)
                    {
                        closeCount = false;
                        closeThreshold = 0;

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
                                neworder.InstrumentId = marketData.InstrumentId;
                                neworder.Price = marketData.LastPrice;
                                neworder.Volume = order.Volume;
                                neworder.StrategyType = GetType().ToString();


                                order.CloseOrder = neworder;

                                list.Add(order);

                                log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), marketData.InstrumentId,
                                    marketData.LastPrice, maPrice,
                                    orders.Count()));
                            }
                            else
                            {
                                closeThreshold++;
                            }
                        }
                    }
                    else
                    {
                        closeCount = true;
                    }

                    log.Info(instrumentId + ":closeThreshold=" + closeThreshold);
                }

                
            }

            if (orders.Count(o => o.StatusType != EnumOrderStatus.已平仓) == 0 && preMarketDataDictionary.ContainsKey(instrumentId))
            {
                var result = Cross(preMarketDataDictionary[instrumentId], marketData);

                if (result == TThostFtdcDirectionType.Buy)
                {
                    buyCount = true;
                    sellCount = false;

                    upThreshold++;
                    downThreshold = 0;
                } else if (result == TThostFtdcDirectionType.Sell)
                {
                    sellCount = true;
                    buyCount = false;

                    downThreshold++;
                    upThreshold = 0;

                }

                if (result == TThostFtdcDirectionType.Nothing && buyCount)
                {
                    if (upThreshold >= MaxOpenThreshold)
                    {

                        var neworder = new Order();
                        neworder.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                        neworder.Direction = TThostFtdcDirectionType.Buy;
                        neworder.InstrumentId = marketData.InstrumentId;
                        neworder.Price = marketData.LastPrice;
                        neworder.Volume = instrumentStrategy.Volume;
                        neworder.StrategyType = GetType().ToString();

                        list.Add(neworder);

                        log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), marketData.InstrumentId,
                            marketData.LastPrice, marketData.AveragePrice,
                            result));
                        upThreshold = 0;
                    }
                    else
                    {
                        upThreshold++;
                    }
                }

                if (result == TThostFtdcDirectionType.Nothing && sellCount)
                {
                    if (downThreshold >= MaxOpenThreshold)
                    {

                        var neworder = new Order();
                        neworder.OffsetFlag = TThostFtdcOffsetFlagType.Open;
                        neworder.Direction = TThostFtdcDirectionType.Sell;
                        neworder.InstrumentId = marketData.InstrumentId;
                        neworder.Price = marketData.LastPrice;
                        neworder.Volume = instrumentStrategy.Volume;
                        neworder.StrategyType = GetType().ToString();

                        list.Add(neworder);

                        log.Info(String.Format("{0}:{1}:{2}:{3}:{4}", ToString(), marketData.InstrumentId,
                            marketData.LastPrice, marketData.AveragePrice,
                            result));
                        downThreshold = 0;
                    }
                    else
                    {
                        downThreshold++;
                    }
                }
            }


            if (preMarketDataDictionary.ContainsKey(instrumentId))
            {
                preMarketDataDictionary[instrumentId] = marketData.Copy();
            }
            else
            {
                var preMarketData = new MarketData();
                preMarketData = marketData.Copy();
                preMarketDataDictionary.Add(instrumentId, preMarketData);
            }

            //log.Info(instrumentId + ":buyThreshold=" + upThreshold + ":sellThreshold=" + downThreshold);
            return list;
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

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

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
    }
}