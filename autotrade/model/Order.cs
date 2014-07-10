using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTPTradeApi;

namespace autotrade.model
{
    class Order
    {
        public string InstrumentId { get; set; }

        public EnumOffsetFlagType OffsetFlag { get; set; }

        public EnumDirectionType Direction { get; set; }

        public double Price { get; set; }

        public int Volume { get; set; }

        public EnumOrderStatus StatusType { get; set; }
    }
}
