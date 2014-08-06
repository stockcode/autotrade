using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.Stop.Loss;
using autotrade.Strategies;

namespace autotrade.converter
{
    public class StopLossUIEditor : CollectionEditor
    {
        private Type[] types;

        public StopLossUIEditor(Type type)
            : base(type)
        {
            types = new Type[] {typeof (PriceStopLoss), typeof (PercentStopLoss)};
        }

        protected override Type[] CreateNewItemTypes()
        {
            return types; 
        }
    }
}
