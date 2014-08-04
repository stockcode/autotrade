using System.ComponentModel;
using autotrade.converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantBox.CSharp2CTP;

namespace autotrade.model
{
    public class TradeRecord
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
        /// 买卖方向
        /// </summary>
        [TypeConverterAttribute(typeof(DirectionConverter))]
        public TThostFtdcDirectionType Direction { get; set; }

        /// <summary>
        /// 开平标志
        /// </summary>
        public TThostFtdcOffsetFlagType OffsetFlag { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// 成交时间
        /// </summary>
        public string TradeTime { get; set; }

        /// <summary>
        /// 报单编号
        /// </summary>
        public string OrderSysID { get; set; }

        /// <summary>
        /// 成交类型
        /// </summary>
        public TThostFtdcTradeTypeType TradeType { get; set; }

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
