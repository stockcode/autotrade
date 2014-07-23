using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using autotrade.model;
using CTPTradeApi;
using log4net;
using MongoRepository;

namespace autotrade.Repository
{
    internal class OrderRepository : MongoRepository<Order>
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BindingList<Order> orders = new BindingList<Order>();

        public OrderRepository()
        {
            orders.ListChanged += orders_ListChanged;
        }

        public List<Order> GetByInstrumentIDAndStatusType(string instrumentId, EnumOrderStatus statusType)
        {
            return orders.Where(o => o.InstrumentId == instrumentId && o.StatusType == statusType).ToList();
        }

        public void AddOrder(Order order)
        {
            orders.Add(order);
        }

        private void orders_ListChanged(object sender, ListChangedEventArgs e)
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
            foreach (Order order in orders)
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
            foreach (Order order in orders)
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
                order.TradePrice = pTrade.Price;
                order.TradeTime = pTrade.TradeTime;
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