using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace autotrade.model
{
    public class Account
    {
        /// <summary>
        /// 投资者帐号
        /// </summary>
        public string AccountID { get; set; }

        /// <summary>
        /// 平仓盈亏
        /// </summary>
        public double CloseProfit { get; set; }

        /// <summary>
        /// 持仓盈亏
        /// </summary>
        public double PositionProfit { get; set; }

        /// <summary>
        /// 可用资金
        /// </summary>
        public double Available { get; set; }

        /// <summary>
        /// 冻结的保证金
        /// </summary>
        public double FrozenMargin { get; set; }

        /// <summary>
        /// 当前保证金总额
        /// </summary>
        public double CurrMargin { get; set; }

        public override string ToString()
        {
            return String.Format("投资者帐号={0}, 平仓盈亏={1}, 持仓盈亏={2}, 可用资金={3}, 冻结的保证金={4}", AccountID, CloseProfit,
                PositionProfit, Available, FrozenMargin);
        }
    }
}
