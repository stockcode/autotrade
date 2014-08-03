using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.CSharp2CTP
{
    class Broker
    {
        String BrokerID { get; set; }

        public List<String> tradeIPs = new List<string>();

        public List<String> marketIPs = new List<string>(); 
    }
}
