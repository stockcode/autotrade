using autotrade.model;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;

namespace autotrade
{
    public class BarRecord : Entity
    {
        public string InstrumentID { get; set; }

        public string Date { get; set; }

        public string ActualDate { get; set; }

        public string Time { get; set; }

        public double Open { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Close { get; set; }        

        public double Amount { get; set; }

        public double Volume { get; set; }

        public EnumRecordIntervalType IntervalType { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "InstrumentID: {0}, Date: {1}, ActualDate: {2}, Time: {3}, Open: {4}, High: {5}, Low: {6}, Close: {7}, Amount: {8}, Volume: {9}",
                    InstrumentID, Date, ActualDate, Time, Open, High, Low, Close, Amount, Volume);
        }
    }
}