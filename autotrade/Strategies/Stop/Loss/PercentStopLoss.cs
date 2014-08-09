using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.model;

namespace autotrade.Stop.Loss
{
    public class PercentStopLoss : StopLoss
    {
        public override List<Order> Match(MarketData marketData)
        {
            throw new NotImplementedException();
        }
    }
}
