using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using autotrade.model;
using autotrade.model.Log;
using log4net;
using QuantBox.CSharp2CTP;

namespace autotrade.Stop.Profit
{
    class FallBackStopProfit : StopProfit
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private double maxProfit = 0d;

        private double _minProfit = 1000;

        [DisplayName("最小激活阀值")]
        public double MinProfit
        {
            get { return _minProfit; }
            set
            {
                if (Math.Abs(this._minProfit - value) < TOLERANCE) return;

                this._minProfit = value;
                NotifyPropertyChanged();
            }
        }

        private double _percent = 0.2;

        [DisplayName("回落百分比阀值")]
        public double Percent
        {
            get { return _percent; }
            set
            {
                if (Math.Abs(this._percent - value) > TOLERANCE)
                {
                    this._percent = value;
                    NotifyPropertyChanged();
                }
            }
        }        


        public override List<Order> Match(MarketData marketData)
        {
             var instrumentId = marketData.InstrumentId;

             List<Order> orders = GetOrders(instrumentId);


            var list = new List<Order>();

            foreach (var order in orders.FindAll(o => o.StatusType == EnumOrderStatus.已开仓))
            {
                if (order.PositionProfit > maxProfit) maxProfit = order.PositionProfit;

                if (order.PositionProfit < 0 || maxProfit < MinProfit) continue;

                

                if ((maxProfit - order.PositionProfit) / maxProfit > Percent)
                {
                    var neworder = new Order();
                    neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
                    neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy ? TThostFtdcDirectionType.Sell : TThostFtdcDirectionType.Buy;
                    neworder.InstrumentId = marketData.InstrumentId;
                    neworder.LastPrice = marketData.LastPrice;
                    neworder.Price = GetAnyPrice(marketData, neworder.Direction);
                    neworder.Volume = order.Volume;
                    neworder.StrategyType = GetType().ToString();


                    var strategyLog = new FallBackStopStrategyLog
                    {
                        MinProfit = MinProfit,
                        Percent = Percent,
                        MaxProfit = maxProfit,
                        PositionProfit = order.PositionProfit,
                        LastPrice = marketData.LastPrice,
                        UpdateTime = marketData.UpdateTimeSec
                    };

                    neworder.StrategyLogs.Add(strategyLog);

                    order.CloseOrder = neworder;

                    list.Add(order);

                    maxProfit = 0d;

                    log.Info(String.Format("{0}:{1}:{2}:{3}", ToString(), marketData.InstrumentId, marketData.LastPrice,
                        orders.Count()));
                }
            }

            return list;
        }
    }
}
