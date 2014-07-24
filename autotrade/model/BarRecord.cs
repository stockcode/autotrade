using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoRepository;

namespace autotrade
{
    class BarRecord : Entity
    {
        public string Date
        {
            get;
            set;
        }

        public double Open
        {
            get;
            set;
        }

        public double High
        {
            get;
            set;
        }

        public double Low
        {
            get;
            set;
        }

        public double Close
        {
            get;
            set;
        }

        public double Amount
        {
            get;
            set;
        }

        public double Volume
        {
            get;
            set;
        }       

        public override string ToString()
        {
            return string.Format("Date: {0} Open: {1} High: {2} Low: {3} Close: {4}", Date, Open, High, Low, Close);
        }
    }
}
