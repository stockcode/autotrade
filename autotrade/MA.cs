using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace autotrade
{
    internal class MA
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int days;
        private String instrumentId;
        private MongoCollection<D1BarRecord> collection;

        public MA(string instrumentId, int days)
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
            collection = database.GetCollection<D1BarRecord>("d1BarRecord");

        }

        public double GetMA()
        {
            IQueryable<D1BarRecord> query =
                (from e in collection.AsQueryable()
                 orderby e.date descending
                 select e).Take(days + 1).Skip(1);

            double avg = 0;

            foreach (D1BarRecord record in query)
            {                
                avg += record.close;
            }

            avg = avg / days;

            avg = double.Parse(avg.ToString("F1"));

            return avg;
        }
    }
}