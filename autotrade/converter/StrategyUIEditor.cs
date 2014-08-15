using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.Strategies;

namespace autotrade.converter
{
    public class StrategyUIEditor : CollectionEditor
    {
        private Type[] types; 

        public StrategyUIEditor(Type type) : base(type)
        {
            types = new Type[] { typeof(BollStrategy), typeof(DayAverageStrategy), typeof(RoundMAStrategy), typeof(RandomStrategy) };
        }

        protected override Type[] CreateNewItemTypes()
        {
            return types; 
        }
    }
}
