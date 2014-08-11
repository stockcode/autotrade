using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autotrade.model.Log
{
    class PriceStopLossStrategyLog : OrderStrategyLog
    {
        [DisplayName("亏损阀值")]
        public double Price { get; set; }

        [DisplayName("当前收益")]
        public double PositionProfit { get; set; }

        [DisplayName("最新价")]
        public double LastPrice { get; set; }

        [DisplayName("更新时间")]
        public string UpdateTime { get; set; }

    }
}
