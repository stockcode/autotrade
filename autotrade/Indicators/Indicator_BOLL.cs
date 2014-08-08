using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using autotrade;

namespace autotrade.Indicators
{
    public class Indicator_BOLL
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string InstrumentID { get; set; }

        public double Boll { get; set; }

        public double UB { get; set; }

        public double LB { get; set; }
        

        private int days;

        public Indicator_BOLL(List<double> prices)
        {
            foreach (var price in prices)
            {
                Boll += price;
            }

            Boll /= prices.Count;

            double sqrt = 0d;

            foreach (var price in prices)
            {
                sqrt += Math.Pow(Math.Abs(price - Boll), 2);
            }

            sqrt /= prices.Count;
            sqrt = Math.Pow(sqrt, 0.5);

            UB = Boll + sqrt*2;
            LB = Boll - sqrt * 2;
        }

        public override string ToString()
        {
            return string.Format("InstrumentID: {0}, Boll: {1}, UB: {2}, LB: {3}", InstrumentID, Boll, UB, LB);
        }
    }
}