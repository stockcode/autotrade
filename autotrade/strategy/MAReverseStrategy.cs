using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTPMdApi;

namespace autotrade.strategy
{
    class MAReverseStrategy
    {
        private int days;
        private String instrumentId;
        private MA ma;
        private double maPrice = 0;
        public MAReverseStrategy(int days, string instrumentId)
        {
            this.days = days;
            this.instrumentId = instrumentId;

            ma = new MA(instrumentId, days);
            maPrice = ma.GetMA();
        }

        public bool Match(CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            double lastPrice = pDepthMarketData.LastPrice;
            if (lastPrice > maPrice*1.02) return true;
            else if (lastPrice < maPrice*0.98) return true;
            else return false;
        }
    }
}
