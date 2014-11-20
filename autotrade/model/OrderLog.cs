using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using autotrade.model.Log;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;
using QuantBox.CSharp2CTP;

namespace autotrade.model
{
    public class OrderLog : Entity , INotifyPropertyChanged
    {
        public OrderLog()
        {
            StrategyLogs = new List<OrderStrategyLog>();
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

        [DisplayName("成交日期")]
        public string TradeDate { get; set; }

        [DisplayName("成交时间")]
        public string TradeTime { get; set; }

        [DisplayName("持仓手数")]
        public int Volume { get; set; }

        [DisplayName("占用的保证金")]
        public double UseMargin { get; set; }

        public EnumOrderStatus StatusType { get; set; }

        [Browsable(false)]
        public string OrderRef { get; set; }


        [Browsable(false)]
        public string OrderSysID { get; set; }

        [Browsable(false)]
        public int Unit { get; set; }

        private double closeProfit;

        [DisplayName("平仓盈亏")]
        public double CloseProfit
        {
            get { return closeProfit; }

            set
            {
                if (this.closeProfit != value)
                {
                    this.closeProfit = value;
                    NotifyPropertyChanged();
                }
            }
        }


        [Browsable(false)]
        public string TradeID { get; set; }

        [DisplayName("策略")]
        public String StrategyType { get; set; }

        [Browsable(false)]
        public List<OrderStrategyLog> StrategyLogs { get; set; }

        [Browsable(false)]
        public List<Order> CloseOrders { get; set; }
        

        public override string ToString()
        {
            return string.Format("Order(合约代码={0},开平标志={1},买卖方向={2},价格={3},手数={4},状态={5},报单引用={6},报单编号={7})"
                ,InstrumentId, OffsetFlag, Direction, Price, Volume, StatusType, OrderRef, OrderSysID);
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
