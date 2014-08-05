using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;
using QuantBox.CSharp2CTP;

namespace autotrade.model
{
    public class OrderLog : Entity , INotifyPropertyChanged
    {
        public string InstrumentId { get; set; }

        public TThostFtdcOffsetFlagType OffsetFlag { get; set; }

        public TThostFtdcDirectionType Direction { get; set; }

        public double Price { get; set; }

        public double TradePrice { get; set; }

        public string TradeDate { get; set; }

        public string TradeTime { get; set; }

        public int Volume { get; set; }

        private EnumOrderStatus _statusType;

        public EnumOrderStatus StatusType { get; set; }

        public string OrderRef { get; set; }

        /// <summary>
        /// 报单编号
        /// </summary>        
        private string _orderSysID;

        public string OrderSysID { get; set; }

        public int Unit { get; set; }

        private double closeProfit;

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
        
        
        /// <summary>
        /// 成交编号
        /// </summary>
        public string TradeID { get; set; }
        

        public String StrategyType { get; set; }

        public Order CloseOrder { get; set; }

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
