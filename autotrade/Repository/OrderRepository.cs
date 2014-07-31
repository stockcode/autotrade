using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using autotrade.model;
using log4net;
using MongoDB.Driver.Builders;
using MongoRepository;
using QuantBox.CSharp2CTP;

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

        public Order UpdateOrderRef(CThostFtdcOrderField pOrder)
        {
            string orderRef = pOrder.OrderRef.Trim();

            foreach (Order order in orders)
            {
                if (order.OrderRef == orderRef)
                {
                    order.OrderSysID = pOrder.OrderSysID;
                    return order; ;
                }

                if (order.CloseOrder != null && order.CloseOrder.OrderRef == orderRef)
                {
                    order.CloseOrder.OrderSysID = pOrder.OrderSysID;
                    return order.CloseOrder;
                }
            }

            return null;
        }

        public Order UpdateTradeID(CThostFtdcTradeField pTrade)
        {
            string orderSysID = pTrade.OrderSysID.Trim();

            foreach (Order order in orders)
            {
                if (order.OrderSysID == orderSysID && order.TradeID == null)
                {
                    order.TradeID = pTrade.TradeID;
                    order.TradePrice = pTrade.Price;
                    order.TradeTime = pTrade.TradeTime;
                    order.StatusType = EnumOrderStatus.已开仓;

                    return order;
                }

                if (order.CloseOrder != null && order.CloseOrder.OrderSysID == orderSysID && order.CloseOrder.TradeID == null)
                {
                    order.CloseOrder.TradeID = pTrade.TradeID;
                    order.CloseOrder.TradePrice = pTrade.Price;
                    order.CloseOrder.TradeTime = pTrade.TradeTime;
                    order.StatusType = EnumOrderStatus.已平仓;
                    order.CloseProfit = order.PositionProfit;
                    order.PositionProfit = 0;
                    ;

                    return order;
                }
            }

            return null;
        }

        public void Init(BindingList<OrderRecord> orderRecords)
        {
            orders.RaiseListChangedEvents = false;

            foreach (OrderRecord orderRecord in orderRecords)
            {
                string orderSysID = orderRecord.OrderSysID.Trim();
                if (orderSysID == "") continue;

                Order order = this.Collection.FindOne(Query.EQ("OrderSysID", orderSysID));
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