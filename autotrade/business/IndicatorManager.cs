using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using autotrade.Properties;
using autotrade.util;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoRepository;
using autotrade.Indicators;
using autotrade.model;

namespace autotrade.business
{
    public class IndicatorManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<String, double> maDictionary = new Dictionary<String, double>();        
        private Dictionary<String, MarketData> preMarketDataDictionary = new Dictionary<string, MarketData>();


        private readonly Dictionary<string, Dictionary<EnumRecordIntervalType, BarRecord>> recordMinuteDictionary = new Dictionary<string, Dictionary<EnumRecordIntervalType, BarRecord>>();
        private readonly Dictionary<string, Dictionary<EnumRecordIntervalType, List<BarRecord>>> recordMinutesDictionary = new Dictionary<string, Dictionary<EnumRecordIntervalType, List<BarRecord>>>();

        private MongoDatabase database;
        private string today = DateTime.Today.ToString("yyyyMMdd");

        private readonly BlockingQueue<BarRecord> barRecordQueue = new BlockingQueue<BarRecord>(); 


        public IndicatorManager()
        {            
        }

        public void Init(IEnumerable<MarketData> marketDatas)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoServerFutureSettings"].ConnectionString;
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            database = server.GetDatabase("futuredata");

            foreach (var marketData in marketDatas)
            {
                var dict = new Dictionary<EnumRecordIntervalType, List<BarRecord>>();

                foreach (EnumRecordIntervalType intervalType in Enum.GetValues(typeof(EnumRecordIntervalType)))
                {
                    var collection = database.GetCollection<BarRecord>(marketData.InstrumentId + intervalType);

                    var query =
                    (from e in collection.AsQueryable()
                     orderby e.ActualDate descending
                     select e).Take(20);

                    var barRecords = query.ToList();

                    dict.Add(intervalType, barRecords);
                    
                }

                recordMinutesDictionary.Add(marketData.InstrumentId, dict);
            }

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var barRecord = barRecordQueue.Dequeue();
                    MongoCollection collection = database.GetCollection<BarRecord>(barRecord.InstrumentID + barRecord.IntervalType);
                    collection.Insert(barRecord);

                }
            });
        }

        public void ProcessData(MarketData marketData)
        {
            var updateTime = DateTime.Parse(marketData.UpdateTime);

            

            foreach (EnumRecordIntervalType intervalType in Enum.GetValues(typeof(EnumRecordIntervalType)))
            {
                if (!recordMinuteDictionary.ContainsKey(marketData.InstrumentId)) recordMinuteDictionary.Add(marketData.InstrumentId, new Dictionary<EnumRecordIntervalType, BarRecord>());

                ProcessBarRecord(marketData, updateTime, intervalType);
            }

            RecordPreMarketData(marketData);
        }

        private void ProcessBarRecord(MarketData marketData, DateTime updateTime, EnumRecordIntervalType intervalType)
        {
            var instrumentId = marketData.InstrumentId;

            if (updateTime.Minute % (int)intervalType == 0 && updateTime.Second == 0)
            {
                if (!recordMinuteDictionary[instrumentId].ContainsKey(intervalType))
                {
                    var barRecord = new BarRecord
                    {
                        InstrumentID = instrumentId,
                        Open = marketData.LastPrice,
                        Low = marketData.LastPrice,
                        High = marketData.LastPrice,
                        Volume = marketData.Volume,
                        Amount = marketData.Turnover,
                        IntervalType = intervalType
                    };

                    recordMinuteDictionary[instrumentId].Add(intervalType, barRecord);
                }
                else
                {
                    MarketData preMarketData = preMarketDataDictionary[instrumentId];

                    DateTime preUpdateTime = DateTime.Parse(preMarketData.UpdateTime);


                    if (preUpdateTime.Second == 59)
                    {
                        var barRecord = recordMinuteDictionary[instrumentId][intervalType];

                        barRecord.Close = preMarketData.LastPrice;
                        barRecord.Volume = preMarketData.Volume - barRecord.Volume;
                        barRecord.Amount = preMarketData.Turnover = barRecord.Amount;
                        barRecord.ActualDate = today;
                        barRecord.Date = marketData.TradingDay;
                        barRecord.Time = marketData.UpdateTime;

                        InsertToList(recordMinutesDictionary[instrumentId][intervalType], barRecord);

                        //log.Info(barRecord);

                        barRecordQueue.Enqueue(barRecord);

                        barRecord = new BarRecord
                        {
                            InstrumentID = instrumentId,
                            Open = marketData.LastPrice,
                            Low = marketData.LastPrice,
                            High = marketData.LastPrice,
                            Volume = marketData.Volume,
                            Amount = marketData.Turnover,
                            IntervalType = intervalType
                        };
                        recordMinuteDictionary[instrumentId][intervalType] = barRecord;
                    }
                }
            }

            if (recordMinuteDictionary[instrumentId].ContainsKey(intervalType))
            {
                BarRecord currentRecord = recordMinuteDictionary[instrumentId][intervalType];

                if (currentRecord.High < marketData.LastPrice)
                    currentRecord.High = marketData.LastPrice;

                if (currentRecord.Low > marketData.LastPrice)
                    currentRecord.Low = marketData.LastPrice;
            }
        }

        private void RecordPreMarketData(MarketData marketData)
        {
            if (preMarketDataDictionary.ContainsKey(marketData.InstrumentId))
            {
                preMarketDataDictionary[marketData.InstrumentId] = marketData.Copy();
            }
            else
            {
                var preMarketData = marketData.Copy();
                preMarketDataDictionary.Add(marketData.InstrumentId, preMarketData);
            }
        }

        private void InsertToList(List<BarRecord> barRecords, BarRecord barRecord)
        {
            if (barRecords.Count == 20) barRecords.RemoveAt(0);

            barRecords.Add(barRecord);            
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

        public Indicator_BOLL GetBoll(int day, string instrumentID, EnumRecordIntervalType intervalType)
        {            
            if (day > recordMinutesDictionary[instrumentID][intervalType].Count) return null;

            var closePrices = recordMinutesDictionary[instrumentID][intervalType].Select(barRecord => barRecord.Close).ToList();

            return new Indicator_BOLL(closePrices);
        }
    }
}