using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using autotrade.model;
using log4net;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;

namespace autotrade.business
{
    public class MarketManager
    {
        public delegate void MarketDataHandler(object sender, MarketDataEventArgs e);

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Queue<CThostFtdcDepthMarketDataField> marketQueue = new Queue<CThostFtdcDepthMarketDataField>();

        public Dictionary<String, MarketData> instrumentDictionary = new Dictionary<string, MarketData>();
        public BindingList<MarketData> marketDatas = new BindingList<MarketData>();
        public MdApiWrapper mdApi { get; set; }

        public IndicatorManager indicatorManager { get; set; }

        public StrategyManager strategyManager { get; set; }

        public OrderManager orderManager { get; set; }

        public InstrumentManager InstrumentManager { get; set; }

        public StrategyManager StrategyManager { get; set; }

        public event MarketDataHandler OnRtnMarketData;

        public void Subscribe()
        {
            mdApi.OnRtnDepthMarketData += mdApi_OnRtnDepthMarketData;
            mdApi.OnRspError += mdApi_OnRspError;

            string ppInstrumentId = "";

            foreach (Instrument instrument in InstrumentManager.instruments.Where(instrument => instrument.AutoTrade))
            {
                string instrumentID = instrument.InstrumentID;

                var marketData = new MarketData(instrumentID);
                marketData.Unit = InstrumentManager.GetUnit(instrumentID);

                marketDatas.Add(marketData);
                instrumentDictionary.Add(instrumentID, marketData);
                ppInstrumentId += instrumentID + ",";

                StrategyManager.InitStrategies(instrumentID);
            }

            mdApi.Subscribe(ppInstrumentId, "");

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
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
                        marketData.Unit = orderManager.InstrumentManager.GetUnit(pDepthMarketData.InstrumentID);
                        marketDatas.Add(marketData);
                        instrumentDictionary.Add(pDepthMarketData.InstrumentID, marketData);
                    }

                    indicatorManager.ProcessData(marketData);

                    strategyManager.PrcessData(marketData);

                    orderManager.ProcessData(marketData);

                    //log.Info(marketQueue.Count());
                    //ObjectUtils.CopyStruct(pDepthMarketData, marketData);
                    //OnRtnMarketData(this, new MarketDataEventArgs(marketData));
                    //log.Info(marketData);
                }
            });
        }

        private void mdApi_OnRspError(object sender, OnRspErrorArgs e)
        {
            log.Info(e.pRspInfo);
        }

        private void mdApi_OnRtnDepthMarketData(object sender, OnRtnDepthMarketDataArgs e)
        {
            marketQueue.Enqueue(e.pDepthMarketData);
        }

        public int GetIndex(string instrumentId)
        {
            try
            {
                return marketDatas.IndexOf(marketDatas.First(data => data.InstrumentId == instrumentId));

            }
            catch (Exception)
            {
                
            }

            return -1;
        }
    }

    public class MarketDataEventArgs : EventArgs
    {
        public MarketDataEventArgs(MarketData marketData)
        {
            this.marketData = marketData;
        }

        public MarketData marketData { get; set; }
    }
}