using System;
using System.Collections.Generic;
using System.Linq;
using MongoRepository;
using autotrade.Indicators;
using autotrade.model;

namespace autotrade.business
{
    class IndicatorManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<String, double> maDictionary = new Dictionary<String, double>();
        private Dictionary<String, BarRecord> recordDictionary = new Dictionary<string, BarRecord>();

        public IndicatorManager()
        {
            
        }

        public void ProcessData(MarketData marketData)
        {
            string instrumentId = marketData.InstrumentId;
            DateTime updateTime = DateTime.Parse(marketData.UpdateTime);

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

            if (updateTime.Minute % 5 == 0 && updateTime.Second == 0)
            {
                log.Info(marketData);

                BarRecord barRecord = recordDictionary[instrumentId];

                barRecord.Close = marketData.LastPrice;
                barRecord.Date = marketData.UpdateTime;
                barRecord.Volume = marketData.Volume - barRecord.Volume;
                barRecord.Amount = marketData.Turnover = barRecord.Amount;

                
                
                log.Info(barRecord);

                barRecord = new BarRecord();
                barRecord.Open = marketData.LastPrice;
                barRecord.Low = marketData.LastPrice;
                barRecord.High = marketData.LastPrice;
                barRecord.Volume = marketData.Volume;
                barRecord.Amount = marketData.Turnover;
                recordDictionary[instrumentId] = barRecord;
            }

            BarRecord currentRecord = recordDictionary[instrumentId];

            if (currentRecord.High < marketData.LastPrice)
                currentRecord.High = marketData.LastPrice;

            if (currentRecord.Low > marketData.LastPrice)
                currentRecord.Low = marketData.LastPrice;
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