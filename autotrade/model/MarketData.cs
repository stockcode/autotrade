using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using QuantBox.CSharp2CTP;

namespace autotrade.model
{
    public class MarketData : INotifyPropertyChanged
    {
        private string _updateTime;
        private double averagePrice;

        public MarketData(CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            CopyFrom(pDepthMarketData);
        }

        public MarketData()
        {
        }

        public MarketData(string instrumentId)
        {
            InstrumentId = instrumentId;
        }


        [DisplayName("合约")]
        public string InstrumentId { get; set; }

        public string Code
        {
            get { return Char.IsNumber(InstrumentId[1]) ? InstrumentId.Substring(0, 1) : InstrumentId.Substring(0, 2); }
        }

        [DisplayName("最新价")]
        public double LastPrice { get; set; }

        [DisplayName("当日均价")]
        public double AveragePrice
        {
            get { return averagePrice; }
            set
            {
                averagePrice = ExchangeID == "CZCE"
                    ? value
                    : Math.Round(value/Unit, 2, MidpointRounding.AwayFromZero);
            }
        }

        [DisplayName("更新时间")]
        public string UpdateTime
        {
            get { return _updateTime; }
            set
            {
                if (_updateTime != value)
                {
                    _updateTime = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [DisplayName("买价")]
        public double BidPrice1 { get; set; }

        [DisplayName("买量")]
        public int BidVolume1 { get; set; }

        [DisplayName("卖价")]
        public double AskPrice1 { get; set; }

        [DisplayName("卖量")]
        public int AskVolume1 { get; set; }

        [DisplayName("成交量")]
        public int Volume { get; set; }

        [DisplayName("持仓量")]
        public double OpenInterest { get; set; }

        [DisplayName("今开盘")]
        public double OpenPrice { get; set; }

        [DisplayName("最高价")]
        public double HighestPrice { get; set; }

        [DisplayName("最低价")]
        public double LowestPrice { get; set; }

        [DisplayName("涨停价")]
        public double UpperLimitPrice { get; set; }

        [DisplayName("跌停价")]
        public double LowerLimitPrice { get; set; }

        [DisplayName("昨结算")]
        public double PreSettlementPrice { get; set; }

        [DisplayName("昨收盘")]
        public double PreClosePrice { get; set; }


        [DisplayName("成交额")]
        public double Turnover { get; set; }

        [Browsable(false)]
        public int Unit { get; set; }


        /// <summary>
        ///     昨持仓量
        /// </summary>
        [Browsable(false)]
        public double PreOpenInterest { get; set; }


        /// <summary>
        ///     今收盘
        /// </summary>
        [Browsable(false)]
        public double ClosePrice { get; set; }

        /// <summary>
        ///     本次结算价
        /// </summary>
        [Browsable(false)]
        public double SettlementPrice { get; set; }


        /// <summary>
        ///     最后修改毫秒
        /// </summary>
        [Browsable(false)]
        public int UpdateMillisec { get; set; }


        /// <summary>
        ///     交易日
        /// </summary>
        [Browsable(false)]
        public string TradingDay { get; set; }

        /// <summary>
        ///     交易所代码
        /// </summary>
        [Browsable(false)]
        public string ExchangeID { get; set; }

        [Browsable(false)]
        public string UpdateTimeSec
        {
            get { return UpdateTime + ":" + UpdateMillisec; }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void CopyFrom(CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            InstrumentId = pDepthMarketData.InstrumentID;
            TradingDay = pDepthMarketData.TradingDay;
            LastPrice = pDepthMarketData.LastPrice;
            BidPrice1 = pDepthMarketData.BidPrice1;
            BidVolume1 = pDepthMarketData.BidVolume1;
            AskPrice1 = pDepthMarketData.AskPrice1;
            AskVolume1 = pDepthMarketData.AskVolume1;
            PreSettlementPrice = pDepthMarketData.PreSettlementPrice;
            PreClosePrice = pDepthMarketData.PreClosePrice;
            PreOpenInterest = pDepthMarketData.PreOpenInterest;
            OpenPrice = pDepthMarketData.OpenPrice;
            HighestPrice = pDepthMarketData.HighestPrice;
            LowestPrice = pDepthMarketData.LowestPrice;
            Volume = pDepthMarketData.Volume;
            Turnover = pDepthMarketData.Turnover;
            OpenInterest = pDepthMarketData.OpenInterest;
            ClosePrice = pDepthMarketData.ClosePrice;
            SettlementPrice = pDepthMarketData.SettlementPrice;
            UpperLimitPrice = pDepthMarketData.UpperLimitPrice;
            LowerLimitPrice = pDepthMarketData.LowerLimitPrice;
            UpdateMillisec = pDepthMarketData.UpdateMillisec;
            AveragePrice = pDepthMarketData.AveragePrice;
            //ExchangeID = pDepthMarketData.ExchangeID;
            UpdateTime = pDepthMarketData.UpdateTime;
        }

        public MarketData Copy()
        {
            return (MarketData) MemberwiseClone();
        }

        public override string ToString()
        {
            return String.Format("MarketData(交易日={0},合约代码={1},最新价={2},上次结算价={3},昨收盘={4},昨持仓量={5},今开盘={6}" +
                                 ",最高价={7},最低价={8},数量={9},成交金额={10},持仓量={11},今收盘={12},本次结算价={13},涨停板价={14},跌停板价={15}" +
                                 ",最后修改时间={16},最后修改毫秒={17},申买价一={18},申买量一={19},申卖价一={20},申卖量一={21},当日均价={22})"
                , TradingDay, InstrumentId, LastPrice, PreSettlementPrice, PreClosePrice, PreOpenInterest, OpenPrice,
                HighestPrice, LowestPrice, Volume, Turnover, OpenInterest, ClosePrice, SettlementPrice
                , UpperLimitPrice, LowerLimitPrice, UpdateTime, UpdateMillisec, BidPrice1, BidVolume1, AskPrice1,
                AskVolume1, AveragePrice);
        }

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

        public string SimpleFormat()
        {
            return String.Format("{0},{1},{2},{3},{4}", InstrumentId, LastPrice, AveragePrice, UpdateTime, UpdateMillisec);
        }
    }
}