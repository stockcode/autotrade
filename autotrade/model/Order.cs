using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CTPTradeApi;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;

namespace autotrade.model
{
    class Order : Entity, INotifyPropertyChanged
    {
        public string InstrumentId { get; set; }

        public EnumOffsetFlagType OffsetFlag { get; set; }

        public EnumDirectionType Direction { get; set; }

        public double Price { get; set; }

        public double TradePrice { get; set; }

        public string TradeTime { get; set; }

        public int Volume { get; set; }

        private EnumOrderStatus _statusType;

        public EnumOrderStatus StatusType
        {
            get { return _statusType; }
            set
            {
                if (this._statusType != value)
                {
                    this._statusType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string OrderRef { get; set; }

        /// <summary>
        /// 报单编号
        /// </summary>        
        private string _orderSysID;

        public string OrderSysID
        {
            get { return _orderSysID; }
            set
            {
                if (value != null && this._orderSysID != value.Trim())
                {
                    this._orderSysID = value.Trim();
                    NotifyPropertyChanged();
                }
            }
        }

        public int Unit { get; set; }

        private double profit;

        [BsonIgnore]
        public double Profit
        {
            get { return profit; }

            set
            {
                if (this.profit != value)
                {
                    this.profit = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// 成交编号
        /// </summary>
        private string _tradeID;

        public string TradeID
        {
            get { return _tradeID; }
            set
            {
                if (value != null && this._tradeID != value.Trim())
                {
                    this._tradeID = value.Trim();
                    NotifyPropertyChanged();
                }
            }
        }

        public String StrategyType { get; set; }

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
