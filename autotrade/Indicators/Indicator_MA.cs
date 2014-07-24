using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using autotrade;

namespace autotrade.Indicators
{
    internal class Indicator_MA
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int days;
        private String instrumentId;
        private MongoCollection<BarRecord> collection;

        public Indicator_MA(string instrumentId, int days)
        {
            this.instrumentId = instrumentId;
            this.days = days;

            InitDb();
        }

        private void InitDb()
        {
            string connectionString = "mongodb://115.28.160.121";
            var client = new MongoClient(connectionString);
            MongoServer server = client.GetServer();
            MongoDatabase database = server.GetDatabase("future");
            collection = database.GetCollection<BarRecord>(instrumentId.ToUpper());

        }

        public double GetMA()
        {
            IQueryable<BarRecord> query =
                (from e in collection.AsQueryable()
                 orderby e.Date descending
                 select e).Take(days + 1).Skip(1);

            double avg = 0;

            foreach (BarRecord record in query)
            {                
                avg += record.Close;
            }

            avg = avg / days;

            avg = double.Parse(avg.ToString("F1"));

            return avg;
        }
    }
}