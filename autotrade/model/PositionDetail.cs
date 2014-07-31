using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantBox.CSharp2CTP;

namespace autotrade.model
{
    class PositionDetail
    {
        /// <summary>
        /// 成交编号
        /// </summary>        
        public string TradeID { get; set; }

        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID { get; set; }

        /// <summary>
        /// 买卖
        /// </summary>
        public TThostFtdcDirectionType Direction { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Volume { get; set; }
        
        /// <summary>
        /// 开仓价
        /// </summary>
        public double OpenPrice { get; set; }

        /// <summary>
        /// 投机套保标志
        /// </summary>
        public TThostFtdcHedgeFlagType HedgeFlag { get; set; }
        
        /// <summary>
        /// 开仓日期
        /// </summary>
        public string OpenDate { get; set; }

        /// <summary>
        /// 投资者保证金
        /// </summary>
        public double Margin { get; set; }


        /// <summary>
        /// 成交类型
        /// </summary>
        public TThostFtdcTradeTypeType TradeType { get; set; }

        /// <summary>
        /// 逐笔对冲持仓盈亏
        /// </summary>
        public double PositionProfitByTrade { get; set; }

        /// <summary>
        /// 交易所代码
        /// </summary>
        public string ExchangeID { get; set; }
    }
}
