using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using autotrade.Properties;
using autotrade.util;
using MongoDB.Driver;
using MongoRepository;
using autotrade.Indicators;
using autotrade.model;

namespace autotrade.business
{
    public class IndicatorManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<String, double> maDictionary = new Dictionary<String, double>();
        private Dictionary<String, BarRecord> recordDictionary = new Dictionary<string, BarRecord>();
        private Dictionary<String, MarketData> preMarketDataDictionary = new Dictionary<string, MarketData>();
        private List<BarRecord> oneMinuteRecord = new List<BarRecord>();
        private MongoDatabase database;
        private string today = DateTime.Today.ToString("yyyyMMdd");

        private readonly BlockingQueue<BarRecord> barRecordQueue = new BlockingQueue<BarRecord>(); 


        public IndicatorManager()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MongoServerFutureSettings"].ConnectionString;
            var client = new MongoClient(connectionString);
            MongoServer server = client.GetServer();
            database = server.GetDatabase("futuredata");

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var barRecord = barRecordQueue.Dequeue();
                    MongoCollection collection = database.GetCollection<BarRecord>(barRecord.InstrumentID);
                    collection.Insert(barRecord);

                }
            });
        }

        public void ProcessData(MarketData marketData)
        {
            string instrumentId = marketData.InstrumentId;
            DateTime updateTime = DateTime.Parse(marketData.UpdateTime);


            if (updateTime.Second == 0)
            {


                if (!recordDictionary.ContainsKey(instrumentId))
                {
                    BarRecord barRecord = new BarRecord();
                    barRecord.InstrumentID = instrumentId;
                    barRecord.Open = marketData.LastPrice;
                    barRecord.Low = marketData.LastPrice;
                    barRecord.High = marketData.LastPrice;
                    barRecord.Volume = marketData.Volume;
                    barRecord.Amount = marketData.Turnover;
                    recordDictionary.Add(instrumentId, barRecord);
                }
                else
                {
                    MarketData preMarketData = preMarketDataDictionary[instrumentId];

                    DateTime preUpdateTime = DateTime.Parse(preMarketData.UpdateTime);

                    //log.Info(marketData);
                    //log.Info(preMarketData);

                    if (preUpdateTime.Second == 59)
                    {
                        BarRecord barRecord = recordDictionary[instrumentId];

                        barRecord.Close = preMarketData.LastPrice;                        
                        barRecord.Volume = preMarketData.Volume - barRecord.Volume;
                        barRecord.Amount = preMarketData.Turnover = barRecord.Amount;
                        barRecord.ActualDate = today;
                        barRecord.Date = marketData.TradingDay;
                        barRecord.Time = marketData.UpdateTime;

                        InsertToList(barRecord);

                        //log.Info(barRecord);

                        barRecordQueue.Enqueue(barRecord);

                        barRecord = new BarRecord();
                        barRecord.InstrumentID = instrumentId;
                        barRecord.Open = marketData.LastPrice;
                        barRecord.Low = marketData.LastPrice;
                        barRecord.High = marketData.LastPrice;
                        barRecord.Volume = marketData.Volume;
                        barRecord.Amount = marketData.Turnover;
                        recordDictionary[instrumentId] = barRecord;
                    }
                }
            }

            if (recordDictionary.ContainsKey(instrumentId))
            {
                BarRecord currentRecord = recordDictionary[instrumentId];

                if (currentRecord.High < marketData.LastPrice)
                    currentRecord.High = marketData.LastPrice;

                if (currentRecord.Low > marketData.LastPrice)
                    currentRecord.Low = marketData.LastPrice;
            }

            if (preMarketDataDictionary.ContainsKey(instrumentId))
            {

                preMarketDataDictionary[instrumentId] = marketData.Copy();
            }
            else
            {
                MarketData preMarketData = new MarketData();
                preMarketData = marketData.Copy();
                preMarketDataDictionary.Add(instrumentId, preMarketData);
            }
        }

        private void InsertToList(BarRecord barRecord)
        {
            if (oneMinuteRecord.Count == 20) oneMinuteRecord.RemoveAt(0);

            oneMinuteRecord.Add(barRecord);

            foreach (var record in oneMinuteRecord)
            {
                log.Info(record);
            }
        }

        public MarketData GetPreMarketData(string instrumentId)
        {
            return preMarketDataDictionary.ContainsKey(instrumentId) ? preMarketDataDictionary[instrumentId] : null;
        }

        public double GetMAPrice(String instrumentId, int days) 
        {
            if (!maDictionary.ContainsKey(instrumentId))
            {
                Indicator_MA ma = new Indicator_MA(instrumentId, days);
                maDictionary.Add(instrumentId, ma.GetMA());
            }

            return maDictionary[instrumentId];
        }

        public Indicator_BOLL GetBoll(int day)
        {
            if (day > oneMinuteRecord.Count) return null;

            List<double> closePrices = oneMinuteRecord.Select(barRecord => barRecord.Close).ToList();

            return new Indicator_BOLL(closePrices);
        }
    }
}