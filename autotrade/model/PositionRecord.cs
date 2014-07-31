using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantBox.CSharp2CTP;

namespace autotrade.model
{
    class PositionRecord
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID { get; set; }

        /// <summary>
        /// 持仓多空方向
        /// </summary>
        public TThostFtdcPosiDirectionType PosiDirection { get; set; }

        /// <summary>
        /// 总持仓
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// 上日持仓
        /// </summary>
        public int YdPosition { get; set; }

        /// <summary>
        /// 今日持仓
        /// </summary>
        public int TodayPosition { get; set; }

        /// <summary>
        /// 持仓成本
        /// </summary>
        public double PositionCost { get; set; }

        /// <summary>
        /// 持仓盈亏
        /// </summary>
        public double PositionProfit;

        /// <summary>
        /// 占用的保证金
        /// </summary>
        public double UseMargin { get; set; }

        /// <summary>
        /// 投机套保标志
        /// </summary>
        public TThostFtdcHedgeFlagType HedgeFlag { get; set; }

        /// <summary>
        /// 交易所代码
        /// </summary>
        public string ExchangeID { get; set; }
    }
}
