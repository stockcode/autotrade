using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace autotrade
{
    class D1BarRecord
    {

        public ObjectId Id
        {
            get;
            set;
        }

        public string _class { get; set; }

        public string date
        {
            get;
            set;
        }

        public double open
        {
            get;
            set;
        }

        public double high
        {
            get;
            set;
        }

        public double low
        {
            get;
            set;
        }

        public double close
        {
            get;
            set;
        }

        public double amount
        {
            get;
            set;
        }

        public double volume
        {
            get;
            set;
        }        
        
    }
}
