using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private Queue<CThostFtdcDepthMarketDataField> marketQueue = new  Queue<CThostFtdcDepthMarketDataField>();
        
        public BindingList<MarketData> marketDatas = new BindingList<MarketData>();
        public Dictionary<String, MarketData> instrumentDictionary = new Dictionary<string, MarketData>();

        public IndicatorManager indicatorManager { get; set; }

        public StrategyManager strategyManager { get; set; }

        public OrderManager orderManager { get; set; }

        public delegate void MarketDataHandler(object sender, MarketDataEventArgs e);

        public event MarketDataHandler OnRtnMarketData;

        public MarketManager(MdApi mdApi)
        {
            this.mdApi = mdApi;
            this.mdApi.OnRtnDepthMarketData += mdApi_OnRtnDepthMarketData;
            this.mdApi.OnRspError += mdApi_OnRspError;
            this.mdApi.OnRspSubMarketData += mdApi_OnRspSubMarketData;

            Task.Factory.StartNew(()=>{
                while(true) {
                    if (marketQueue.Count() == 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    CThostFtdcDepthMarketDataField pDepthMarketData = marketQueue.Dequeue();                    
                    MarketData marketData;

                    if (pDepthMarketData.InstrumentID == null) continue;

                    if (instrumentDictionary.ContainsKey(pDepthMarketData.InstrumentID))
                    {
                        marketData = instrumentDictionary[pDepthMarketData.InstrumentID];
                        marketData.CopyFrom(pDepthMarketData);
                    }
                    else
                    {
                        marketData = new MarketData(pDepthMarketData);
                        marketDatas.Add(marketData);
                        instrumentDictionary.Add(pDepthMarketData.InstrumentID, marketData);
                    }

                    indicatorManager.ProcessData(marketData);

                    strategyManager.PrcessData(marketData);
                    
                    orderManager.ProcessData(marketData);

                    //log.Info(marketQueue.Count());
                    //ObjectUtils.Copy(pDepthMarketData, marketData);
                    //OnRtnMarketData(this, new MarketDataEventArgs(marketData));
                    //log.Info(marketData);
                }
            });
        }

        void mdApi_OnRspSubMarketData(ref CThostFtdcSpecificInstrumentField pSpecificInstrument, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pSpecificInstrument.InstrumentID);
        }

        void mdApi_OnRspError(ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
        }

        void mdApi_OnRtnDepthMarketData(ref CThostFtdcDepthMarketDataField pDepthMarketData)
        {            
            marketQueue.Enqueue(pDepthMarketData);
            //log.Info(pDepthMarketData.InstrumentID);
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
