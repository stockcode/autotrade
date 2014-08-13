using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using autotrade;

namespace autotrade.Indicators
{
    public class Indicator_MA
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public double Average { get; set; }

        public Indicator_MA(List<double> prices)
        {
            Average = 0d;

            foreach (double price in prices)
            {
                Average += price;
            }

            Average /= prices.Count;
        }
    }
}