using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.model.Log;

namespace autotrade.model
{
    public class DayAverageLog : OrderStrategyLog
    {
        [DisplayName("方向")]
        public string Direction { get; set; }

        [DisplayName("Pre最新价")]
        public double PreLastPrice { get; set; }

        [DisplayName("Pre均价")]
        public double PreAveragePrice { get; set; }

        [DisplayName("Pre更新时间")]
        public string PreUpdateTime { get; set; }

        [DisplayName("最新价")]
        public double LastPrice { get; set; }

        [DisplayName("均价")]
        public double AveragePrice { get; set; }

        [DisplayName("更新时间")]
        public string UpdateTime { get; set; }

        [DisplayName("阀值")]
        public int Threshold { get; set; }

    }
}
