using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using autotrade.Properties;
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
        private MongoDatabase database;

        public IndicatorManager()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString;
            var client = new MongoClient(connectionString);
            MongoServer server = client.GetServer();
            database = server.GetDatabase("future");
            
        }

        public void ProcessData(MarketData marketData)
        {
            string instrumentId = marketData.InstrumentId;
            DateTime updateTime = DateTime.Parse(marketData.UpdateTime);


            if (updateTime.Minute % 5 == 0 && updateTime.Second == 0)
            {


                if (!recordDictionary.ContainsKey(instrumentId))
                {
                    BarRecord barRecord = new BarRecord();
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

                    log.Info(marketData);
                    log.Info(preMarketData);

                    if (preUpdateTime.Second == 59)
                    {
                        BarRecord barRecord = recordDictionary[instrumentId];

                        barRecord.Close = preMarketData.LastPrice;
                        barRecord.Date = marketData.UpdateTime;
                        barRecord.Volume = preMarketData.Volume - barRecord.Volume;
                        barRecord.Amount = preMarketData.Turnover = barRecord.Amount;



                        log.Info(barRecord);

                        barRecord = new BarRecord();
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

        public double GetMAPrice(String instrumentId, int days) 
        {
            if (!maDictionary.ContainsKey(instrumentId))
            {
                Indicator_MA ma = new Indicator_MA(instrumentId, days);
                maDictionary.Add(instrumentId, ma.GetMA());
            }

            return maDictionary[instrumentId];
        }
    }
}