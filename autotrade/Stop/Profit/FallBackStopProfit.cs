using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using autotrade.model;
using log4net;
using QuantBox.CSharp2CTP;

namespace autotrade.Stop.Profit
{
    class FallBackStopProfit : StopProfit
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private double _percent = 0.4;

        private double maxProfit = 0d;

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

                if ((maxProfit - order.PositionProfit) / maxProfit > Percent)
                {
                    var neworder = new Order();
                    neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
                    neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy ? TThostFtdcDirectionType.Sell : TThostFtdcDirectionType.Buy;
                    neworder.InstrumentId = marketData.InstrumentId;
                    neworder.Price = GetAnyPrice(marketData, neworder.Direction);
                    neworder.Volume = order.Volume;
                    neworder.StrategyType = GetType().ToString();


                    order.CloseOrder = neworder;

                    list.Add(order);

                    log.Info(String.Format("{0}:{1}:{2}:{3}", ToString(), marketData.InstrumentId, marketData.LastPrice,
                        orders.Count()));
                }
            }

            return list;
        }
    }
}
