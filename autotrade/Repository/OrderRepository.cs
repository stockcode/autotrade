using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.model;
using CTPTradeApi;
using MongoRepository;

namespace autotrade.Repository
{
    class OrderRepository : MongoRepository<Order>
    {
        private BindingList<Order> orders = new BindingList<Order>();

        public OrderRepository()
        {
            orders.ListChanged += orders_ListChanged;
        }

        void orders_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    Add(orders[e.NewIndex]);
                    break;
                case ListChangedType.ItemChanged:
                    Update(orders[e.NewIndex]);
                    break;
                case ListChangedType.ItemDeleted:
                    Delete(orders[e.NewIndex]);
                    break;
            }
        }

        public Order GetOrderByOrderRef(String orderRef)
        {
            foreach (var order in orders)
            {
                if (order.OrderRef == orderRef)
                {
                    return order;
                }

            }

            return null;
        }

        public Order GetOrderByOrderSysID(String orderSysID)
        {
            foreach (var order in orders)
            {
                if (order.OrderSysID == orderSysID)
                {
                    return order;
                }

            }

            return null;
        }

        public Order UpdateOrderRef(CThostFtdcOrderField pOrder)
        {
            Order order = GetOrderByOrderRef(pOrder.OrderRef);
            if (order != null) order.OrderRef = pOrder.OrderRef;

            return order;
        }

        public Order UpdateTradeID(CThostFtdcTradeField pTrade)
        {
            Order order = GetOrderByOrderSysID(pTrade.OrderSysID);
            if (order != null)
            {
                order.TradeID = pTrade.TradeID;
                order.StatusType = EnumOrderStatus.已开仓;
            }

            return order;
        }

        public void Init(List<OrderRecord> orderRecords)
        {
            orders.RaiseListChangedEvents = false;

            foreach (OrderRecord orderRecord in orderRecords)
            {
                Order order = this.FirstOrDefault(o => o.OrderSysID == orderRecord.OrderSysID.Trim());
                if (order != null) orders.Add(order);
            }

            orders.RaiseListChangedEvents = true;            
        }

        public BindingList<Order> getOrders()
        {
            return orders;
        } 
    }
}
