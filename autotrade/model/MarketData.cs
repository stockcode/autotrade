using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autotrade.model
{
    class MarketData
    {
        /// <summary>
        /// 交易日
        /// </summary>
        public string TradingDay { get; set; }
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID { get; set; }
        /// <summary>
        /// 最新价
        /// </summary>
        public double LastPrice { get; set; }
        /// <summary>
        /// 上次结算价
        /// </summary>
        public double PreSettlementPrice { get; set; }
        /// <summary>
        /// 昨收盘
        /// </summary>
        public double PreClosePrice { get; set; }
        /// <summary>
        /// 昨持仓量
        /// </summary>
        public double PreOpenInterest { get; set; }
        /// <summary>
        /// 今开盘
        /// </summary>
        public double OpenPrice { get; set; }
        /// <summary>
        /// 最高价
        /// </summary>
        public double HighestPrice { get; set; }
        /// <summary>
        /// 最低价
        /// </summary>
        public double LowestPrice { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Volume { get; set; }
        /// <summary>
        /// 成交金额
        /// </summary>
        public double Turnover { get; set; }
        /// <summary>
        /// 持仓量
        /// </summary>
        public double OpenInterest { get; set; }
        /// <summary>
        /// 今收盘
        /// </summary>
        public double ClosePrice { get; set; }
        /// <summary>
        /// 本次结算价
        /// </summary>
        public double SettlementPrice { get; set; }
        /// <summary>
        /// 涨停板价
        /// </summary>
        public double UpperLimitPrice { get; set; }
        /// <summary>
        /// 跌停板价
        /// </summary>
        public double LowerLimitPrice { get; set; }        
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public string UpdateTime { get; set; }
        /// <summary>
        /// 最后修改毫秒
        /// </summary>
        public int UpdateMillisec { get; set; }
        /// <summary>
        /// 申买价一
        /// </summary>
        public double BidPrice1 { get; set; }
        /// <summary>
        /// 申买量一
        /// </summary>
        public int BidVolume1 { get; set; }
        /// <summary>
        /// 申卖价一
        /// </summary>
        public double AskPrice1 { get; set; }
        /// <summary>
        /// 申卖量一
        /// </summary>
        public int AskVolume1 { get; set; }        
        /// <summary>
        /// 当日均价
        /// </summary>
        public double AveragePrice { get; set; }

        /// <summary>
        /// 交易所代码
        /// </summary>
        public string ExchangeID { get; set; }

        public override string ToString()
        {
            return String.Format("MarketData(交易日={0},合约代码={1},最新价={2},上次结算价={3},昨收盘={4},昨持仓量={5},今开盘={6}" +
                                 ",最高价={7},最低价={8},数量={9},成交金额={10},持仓量={11},今收盘={12},本次结算价={13},涨停板价={14},跌停板价={15}" +
                                 ",最后修改时间={16},最后修改毫秒={17},申买价一={18},申买量一={19},申卖价一={20},申卖量一={21},当日均价={22})"
                , TradingDay, InstrumentID, LastPrice, PreSettlementPrice, PreClosePrice, PreOpenInterest, OpenPrice, HighestPrice, LowestPrice, Volume, Turnover, OpenInterest, ClosePrice, SettlementPrice
                , UpperLimitPrice, LowerLimitPrice, UpdateTime, UpdateMillisec, BidPrice1, BidVolume1, AskPrice1, AskVolume1, AveragePrice);
        }
    }
}
