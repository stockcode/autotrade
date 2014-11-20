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
    class PriceStopProfit : StopProfit
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private double _price = 2, tmpPrice = 0d;

        [DisplayName("止盈收益阀值")]
        public double Price
        {
            get { return _price; }
            set
            {
                if (Math.Abs(this._price - value) > TOLERANCE)
                {
                    this._price = value;
                    NotifyPropertyChanged();
                }
            }
        }        


        public override List<Order> Match(MarketData marketData)
        {
             var instrumentId = marketData.InstrumentId;

             List<Order> orders = GetOrders(instrumentId);


            var list = new List<Order>();

            foreach (var order in orders)
            {
                if (order.Volume == order.RemainVolume) tmpPrice = Price;

                if (order.RemainVolume == order.Volume/4) break;

                if (order.StatusType == EnumOrderStatus.已开仓 && order.PositionProfit/(InstrumentStrategy.VolumeMultiple*order.Volume) >= tmpPrice)
                {
                    var neworder = new Order();
                    neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
                    neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy ? TThostFtdcDirectionType.Sell : TThostFtdcDirectionType.Buy;
                    neworder.InstrumentId = marketData.InstrumentId;
                    neworder.Price = GetAnyPrice(marketData, neworder.Direction);
                    neworder.Volume = order.Volume / 4;
                    neworder.StrategyType = GetType().ToString();


                    order.CloseOrders.Add(neworder);

                    list.Add(order);

                    log.Info(String.Format("{0}:{1}:{2}:{3}", ToString(), marketData.InstrumentId, marketData.LastPrice,
                        orders.Count()));

                    tmpPrice += 1;
                }
            }

            return list;
        }
    }
}
