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

        public Order GetByInstrumentID(string instrumentId)
        {
            foreach (var order in orders)
            {
                if (order.InstrumentId == instrumentId)
                {
                    return order;
                }

            }

            return null;
        }

        public void AddOrder(Order order)
        {
            orders.Add(order);
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
                if (order.OrderSysID == orderSysID && order.TradeID == null)
                {
                    return order;
                }

            }

            return null;
        }

        public Order UpdateOrderRef(CThostFtdcOrderField pOrder)
        {
            Order order = GetOrderByOrderRef(pOrder.OrderRef.Trim());
            if (order != null) order.OrderSysID = pOrder.OrderSysID;

            return order;
        }

        public Order UpdateTradeID(CThostFtdcTradeField pTrade)
        {
            Order order = GetOrderByOrderSysID(pTrade.OrderSysID.Trim());
            if (order != null)
            {
                order.TradeID = pTrade.TradeID;
                order.StatusType = EnumOrderStatus.已开仓;
            }

            return order;
        }

        public void Init(BindingList<OrderRecord> orderRecords)
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
