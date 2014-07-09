using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autotrade.model
{
    class ServerLine
    {
        public string BrokerId { get; set; }

        public string BrokerName { get; set; }

        public string ServerAddr { get; set; }

        public int ServerPort { set; get; }
    }
}
