using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autotrade.model.Log
{
    class FallBackStopStrategyLog : OrderStrategyLog
    {
        [DisplayName("最小激活阀值")]
        public double MinProfit { get; set; }

        [DisplayName("回落百分比阀值")]
        public double Percent { get; set; }

        [DisplayName("最大收益")]
        public double MaxProfit { get; set; }

        [DisplayName("当前收益")]
        public double PositionProfit { get; set; }

        [DisplayName("最新价")]
        public double LastPrice { get; set; }

        [DisplayName("更新时间")]
        public string UpdateTime { get; set; }

    }
}
