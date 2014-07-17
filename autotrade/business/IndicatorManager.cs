using System;
using System.Collections.Generic;
using System.Linq;
using autotrade.Indicators;

namespace autotrade.business
{
    class IndicatorManager
    {
        private Dictionary<String, double> maDictinary = new Dictionary<String, double>();

        public double GetMAPrice(String instrumentId, int days) 
        {
            if (!maDictinary.ContainsKey(instrumentId))
            {
                Indicator_MA ma = new Indicator_MA(instrumentId, days);
                maDictinary.Add(instrumentId, ma.GetMA());
            }

            return maDictinary[instrumentId];
        }
    }
}
