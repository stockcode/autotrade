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

        private readonly Dictionary<String, BarRecord> record1MinuteDictionary = new Dictionary<string, BarRecord>();
        private readonly Dictionary<String,List<BarRecord>> record1MinutesDictionary = new Dictionary<string, List<BarRecord>>();

        private readonly Dictionary<String, BarRecord> record5MinuteDictionary = new Dictionary<string, BarRecord>();
        private readonly Dictionary<String, List<BarRecord>> record5MinutesDictionary = new Dictionary<string, List<BarRecord>>();


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
                var collection = database.GetCollection<BarRecord>(marketData.InstrumentId+"Minute1");

                var query =
                (from e in collection.AsQueryable()
                 orderby e.ActualDate descending
                 select e).Take(20);

                var barRecords = query.ToList();

                record1MinutesDictionary.Add(marketData.InstrumentId, barRecords);

                collection = database.GetCollection<BarRecord>(marketData.InstrumentId + "Minute5");

                query =
                (from e in collection.AsQueryable()
                 orderby e.ActualDate descending
                 select e).Take(20);

                barRecords = query.ToList();

                record5MinutesDictionary.Add(marketData.InstrumentId, barRecords);
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

            Record1Minute(marketData, updateTime);

            Record5Minute(marketData, updateTime);

            RecordPreMarketData(marketData);
        }

        private void Record5Minute(MarketData marketData, DateTime updateTime)
        {
            var instrumentId = marketData.InstrumentId;

            if (updateTime.Minute % 5 == 0 && updateTime.Second == 0)
            {


                if (!record5MinuteDictionary.ContainsKey(instrumentId))
                {
                    var barRecord = new BarRecord
                    {
                        InstrumentID = instrumentId,
                        Open = marketData.LastPrice,
                        Low = marketData.LastPrice,
                        High = marketData.LastPrice,
                        Volume = marketData.Volume,
                        Amount = marketData.Turnover,
                        IntervalType = EnumRecordIntervalType.Minute5
                    };

                    record5MinuteDictionary.Add(instrumentId, barRecord);
                }
                else
                {
                    var preMarketData = preMarketDataDictionary[instrumentId];

                    var preUpdateTime = DateTime.Parse(preMarketData.UpdateTime);

                    if (preUpdateTime.Second == 59)
                    {
                        var barRecord = record5MinuteDictionary[instrumentId];

                        barRecord.Close = preMarketData.LastPrice;
                        barRecord.Date = marketData.UpdateTime;
                        barRecord.Volume = preMarketData.Volume - barRecord.Volume;
                        barRecord.Amount = preMarketData.Turnover = barRecord.Amount;


                        InsertToList(record5MinutesDictionary, barRecord);


                        barRecordQueue.Enqueue(barRecord);

                        barRecord = new BarRecord
                        {
                            InstrumentID = instrumentId,
                            Open = marketData.LastPrice,
                            Low = marketData.LastPrice,
                            High = marketData.LastPrice,
                            Volume = marketData.Volume,
                            Amount = marketData.Turnover,
                            IntervalType = EnumRecordIntervalType.Minute5
                        };
                        record5MinuteDictionary[instrumentId] = barRecord;
                    }
                }
            }

            if (!record5MinuteDictionary.ContainsKey(instrumentId)) return;

            var currentRecord = record5MinuteDictionary[instrumentId];

            if (currentRecord.High < marketData.LastPrice)
                currentRecord.High = marketData.LastPrice;

            if (currentRecord.Low > marketData.LastPrice)
                currentRecord.Low = marketData.LastPrice;
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

        private void Record1Minute(MarketData marketData, DateTime updateTime)
        {
            var instrumentId = marketData.InstrumentId;

            if (updateTime.Second == 0)
            {
                if (!record1MinuteDictionary.ContainsKey(instrumentId))
                {
                    var barRecord = new BarRecord
                    {
                        InstrumentID = instrumentId,
                        Open = marketData.LastPrice,
                        Low = marketData.LastPrice,
                        High = marketData.LastPrice,
                        Volume = marketData.Volume,
                        Amount = marketData.Turnover,
                        IntervalType = EnumRecordIntervalType.Minute1
                    };

                    record1MinuteDictionary.Add(instrumentId, barRecord);
                }
                else
                {
                    MarketData preMarketData = preMarketDataDictionary[instrumentId];

                    DateTime preUpdateTime = DateTime.Parse(preMarketData.UpdateTime);


                    if (preUpdateTime.Second == 59)
                    {
                        var barRecord = record1MinuteDictionary[instrumentId];

                        barRecord.Close = preMarketData.LastPrice;
                        barRecord.Volume = preMarketData.Volume - barRecord.Volume;
                        barRecord.Amount = preMarketData.Turnover = barRecord.Amount;
                        barRecord.ActualDate = today;
                        barRecord.Date = marketData.TradingDay;
                        barRecord.Time = marketData.UpdateTime;

                        InsertToList(record1MinutesDictionary, barRecord);

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
                            IntervalType = EnumRecordIntervalType.Minute1
                        };
                        record1MinuteDictionary[instrumentId] = barRecord;
                    }
                }
            }

            if (record1MinuteDictionary.ContainsKey(instrumentId))
            {
                BarRecord currentRecord = record1MinuteDictionary[instrumentId];

                if (currentRecord.High < marketData.LastPrice)
                    currentRecord.High = marketData.LastPrice;

                if (currentRecord.Low > marketData.LastPrice)
                    currentRecord.Low = marketData.LastPrice;
            }
        }

        private void InsertToList(Dictionary<string, List<BarRecord>> dictionary, BarRecord barRecord)
        {
            List<BarRecord> oneMinuteRecord = dictionary[barRecord.InstrumentID];

            if (oneMinuteRecord.Count == 20) oneMinuteRecord.RemoveAt(0);

            oneMinuteRecord.Add(barRecord);            
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

        public Indicator_BOLL GetBoll(int day, string instrumentID)
        {
            if (day > record1MinutesDictionary[instrumentID].Count) return null;

            List<double> closePrices = record1MinutesDictionary[instrumentID].Select(barRecord => barRecord.Close).ToList();

            return new Indicator_BOLL(closePrices);
        }
    }
}