using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.Stop.Profit;
using autotrade.Strategies;

namespace autotrade.converter
{
    public class StopProfitUIEditor : CollectionEditor
    {
        private Type[] types;

        public StopProfitUIEditor(Type type)
            : base(type)
        {
            types = new Type[] { typeof(PriceStopProfit), typeof(PercentStopProfit), typeof(FallBackStopProfit) };
        }

        protected override Type[] CreateNewItemTypes()
        {
            return types; 
        }
    }
}
