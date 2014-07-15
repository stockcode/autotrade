using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using autotrade.model;
using CTPMdApi;
using autotrade.util;

namespace autotrade.business
{
    class MarketManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private MdApi mdApi;
        private BlockingQueue<MarketData> marketQueue = new BlockingQueue<MarketData>();
        
        public delegate void MarketDataHandler(object sender, MarketDataEventArgs e);

        public event MarketDataHandler OnRtnMarketData;

        public MarketManager(MdApi mdApi)
        {
            this.mdApi = mdApi;
            this.mdApi.OnRtnDepthMarketData += mdApi_OnRtnDepthMarketData;

            Task.Factory.StartNew(()=>{
                while(true) {
                    MarketData  marketData = marketQueue.Dequeue();

                    OnRtnMarketData(this, new MarketDataEventArgs(marketData));
                    log.Info(marketData);
                }
            });
        }

        void mdApi_OnRtnDepthMarketData(ref CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            MarketData marketData = new MarketData();

            ObjectUtils.Copy(pDepthMarketData, marketData);
            
            marketQueue.Enqueue(marketData);
        }

        public void SubMarketData(params string[] instruments)
        {
            mdApi.SubMarketData(instruments);
        }
    }

    internal class MarketDataEventArgs : EventArgs
    {
        public MarketData marketData { get; set; }
        public MarketDataEventArgs(MarketData marketData)
            : base()
        {
            this.marketData = marketData;
        }
    }
}
