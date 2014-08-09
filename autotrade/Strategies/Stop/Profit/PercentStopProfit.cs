using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.model;
using autotrade.Stop.Profit;

namespace autotrade.Stop.Profit
{
    public class PercentStopProfit : StopProfit
    {
        public override List<Order> Match(MarketData marketData)
        {
            throw new NotImplementedException();
        }
    }
}
