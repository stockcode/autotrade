using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using autotrade.model;
using autotrade.util;
using log4net;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;
using MongoRepository;

namespace autotrade.business
{
    public class MarketManager
    {
        public delegate void MarketDataHandler(object sender, MarketDataEventArgs e);

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly BlockingQueue<CThostFtdcDepthMarketDataField> marketQueue = new BlockingQueue<CThostFtdcDepthMarketDataField>();

        public Dictionary<String, MarketData> instrumentDictionary = new Dictionary<string, MarketData>();
        public BindingList<MarketData> marketDatas = new BindingList<MarketData>();

        public Dictionary<String, List<String>> RecorDictionary = new Dictionary<string, List<String>>();

        public BindingList<Instrument> instruments = new BindingList<Instrument>();

        private MongoRepository<Instrument> instrumentRepo = new MongoRepository<Instrument>();

        public MdApiWrapper mdApi { get; set; }

        public IndicatorManager indicatorManager { get; set; }

        public StrategyManager strategyManager { get; set; }

        public OrderManager orderManager { get; set; }

        public StrategyManager StrategyManager { get; set; }

        public event MarketDataHandler OnRtnMarketData;

        public MarketManager()
        {
            foreach (var instrument in instrumentRepo.Collection.FindAll())
            {
                instruments.Add(instrument);
            }

            instruments.ListChanged += instruments_ListChanged;
        }

        public void Subscribe()
        {
            mdApi.OnRtnDepthMarketData += mdApi_OnRtnDepthMarketData;
            mdApi.OnRspError += mdApi_OnRspError;

            string ppInstrumentId = "";

            foreach (Instrument instrument in instruments.Where(instrument => instrument.AutoTrade))
            {
                string instrumentID = instrument.InstrumentID;

                var marketData = new MarketData(instrumentID);
                marketData.Unit = instrument.VolumeMultiple;
                marketData.ExchangeID = instrument.ExchangeID;

                marketDatas.Add(marketData);
                instrumentDictionary.Add(instrumentID, marketData);
                ppInstrumentId += instrumentID + ",";

                StrategyManager.InitStrategies(instrument);
            }

            mdApi.Subscribe(ppInstrumentId, "");

            Task.Factory.StartNew(() =>
            {
                while (true)
                {              
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
                        var instrument = GetInstrument(pDepthMarketData.InstrumentID);

                        marketData = new MarketData(pDepthMarketData);
                        marketData.Unit = instrument.VolumeMultiple;
                        marketData.ExchangeID = instrument.ExchangeID;
                        marketDatas.Add(marketData);
                        instrumentDictionary.Add(pDepthMarketData.InstrumentID, marketData);
                    }

                    if (!RecorDictionary.ContainsKey(marketData.InstrumentId))
                    {
                        var datas = new List<String>();
                        RecorDictionary.Add(marketData.InstrumentId, datas);
                    }

                    RecorDictionary[marketData.InstrumentId].Add(marketData.SimpleFormat());

                    //Task.Factory.StartNew(() =>
                    //{
                        strategyManager.PrcessData(marketData);

                        orderManager.ProcessData(marketData);

                        indicatorManager.ProcessData(marketData);
                   // });

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

        public void Subscribe(string instrumentID)
        {
            var marketData = new MarketData(instrumentID);
            marketData.Unit = GetInstrument(instrumentID).VolumeMultiple;


            StrategyManager.InitStrategies(GetInstrument(instrumentID));


            marketDatas.Add(marketData);
            instrumentDictionary.Add(instrumentID, marketData);

            

            mdApi.Subscribe(instrumentID, "");
        }

        public void UnSubscribe(string instrumentID)
        {
            mdApi.Unsubscribe(instrumentID, "");

            marketDatas.Remove(marketDatas.First(md => md.InstrumentId == instrumentID));

            instrumentDictionary.Remove(instrumentID);

            StrategyManager.RemoveStrategies(instrumentID);
            
        }

        private void mdApi_OnRtnDepthMarketData(object sender, OnRtnDepthMarketDataArgs e)
        {
            marketQueue.Enqueue(e.pDepthMarketData);
        }

        public void AddSimData(CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            marketQueue.Enqueue(pDepthMarketData);
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

        public Instrument GetInstrument(string instrumentId)
        {
            return instruments.First(inst => inst.InstrumentID == instrumentId);
        }

        void instruments_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    instrumentRepo.Add(instruments[e.NewIndex]);
                    break;
                case ListChangedType.ItemChanged:
                    var instrument = instruments[e.NewIndex];
                    instrumentRepo.Update(instrument);
                    if (instrument.AutoTrade) Subscribe(instrument.InstrumentID);
                    else UnSubscribe(instrument.InstrumentID);

                    break;
                case ListChangedType.ItemDeleted:
                    instrumentRepo.Delete(instruments[e.NewIndex]);
                    break;
            }
        }

        public void Export(string tickDir)
        {
            
            foreach (var record in RecorDictionary)
            {
                var path = tickDir + record.Key;

                Directory.CreateDirectory(path);
                
                File.WriteAllLines(path + "\\" + DateTime.Today.ToString("yyyyMMdd") + ".csv", record.Value);
            }

            RecorDictionary.Clear();
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