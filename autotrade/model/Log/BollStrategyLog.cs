using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autotrade.model.Log
{
    class BollStrategyLog : OrderStrategyLog
    {
        [DisplayName("方向")]
        public string Direction { get; set; }

        [DisplayName("最新价")]
        public double LastPrice { get; set; }

        [DisplayName("上限")]
        public double UB { get; set; }

        [DisplayName("下限")]
        public double LB { get; set; }

        [DisplayName("更新时间")]
        public string UpdateTime { get; set; }

    }
}
