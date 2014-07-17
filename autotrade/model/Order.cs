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

        public string OrderRef { get; set; }

        /// <summary>
        /// 报单编号
        /// </summary>        
        public string OrderSysID { get; set; }


        
        public override string ToString()
        {
            return string.Format("Order(合约代码={0},开平标志={1},买卖方向={2},价格={3},手数={4},状态={5},报单引用={6},报单编号={7})"
                ,InstrumentId, OffsetFlag, Direction, Price, Volume, StatusType, OrderRef, OrderSysID);
        }
    }
}
