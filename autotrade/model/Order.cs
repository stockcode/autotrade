using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;
using QuantBox.CSharp2CTP;

namespace autotrade.model
{
    public class Order : Entity, INotifyPropertyChanged
    {
        public Order()
        {            
            DayAverageLogs = new List<DayAverageLog>();
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

        private double positionProfit;

        [DisplayName("持仓盈亏")]
        [BsonIgnore]
        public double PositionProfit
        {
            get { return positionProfit; }

            set
            {
                
                    this.positionProfit = value;
                    NotifyPropertyChanged();
                
            }
        }

        [DisplayName("持仓手数")]
        public int Volume { get; set; }

        [DisplayName("占用的保证金")]
        public double UseMargin { get; set; }

        private EnumOrderStatus _statusType;

        [DisplayName("状态")]
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

        [Browsable(false)]
        public string OrderRef { get; set; }

        /// <summary>
        /// 报单编号
        /// </summary>        
        private string _orderSysID;

        [Browsable(false)]
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

        [Browsable(false)]
        public int SessionID { get; set; }

        [Browsable(false)]
        public int FrontID { get; set; }

        [Browsable(false)]
        public int Unit { get; set; }

        

        private double closeProfit;

        [Browsable(false)]
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
        private string _tradeID;

        [Browsable(false)]
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

        [Browsable(false)]
        public string TradeDate { get; set; }

        [Browsable(false)]
        public string ActualTradeDate {
            get
            {
                return (DateTime.Today.CompareTo(DateTime.ParseExact(TradeDate, "yyyyMMdd", CultureInfo.InvariantCulture)) ==
                       0
                        ? TradeDateTime
                        : DateTime.Today.ToString("yyyyMMdd") + " " + TradeTime);
            }
        }

        [Browsable(false)]
        public string TradeTime { get; set; }

        [BsonIgnore]
        [DisplayName("成交时间")]
        public string TradeDateTime {
            get { return TradeDate + " " + TradeTime; }
        }

        [BsonIgnore]
        [DisplayName("持仓时间")]
        public TimeSpan PositionTimeSpan { get; set; }
        


        [DisplayName("开仓策略")]
        public String StrategyType { get; set; }

        [BsonIgnore]
        [DisplayName("平仓挂单价")]
        public double ClosePrice {
            get {
                return CloseOrder != null ? CloseOrder.Price : 0;
            }
        }

        [BsonIgnore]
        [DisplayName("平仓策略")]
        public String CloseStrategyType
        {
            get { return CloseOrder != null ? CloseOrder.StrategyType : ""; }
        }

        [Browsable(false)]        
        public Order CloseOrder { get; set; }

        [Browsable(false)]
        public List<DayAverageLog> DayAverageLogs { get; set; }

        public string ExchangeID { get; set; }

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
