using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using autotrade.model;
using autotrade.util;
using log4net;
using MongoDB.Driver.Builders;
using MongoRepository;
using QuantBox.CSharp2CTP;

namespace autotrade.Repository
{
    public class OrderRepository : MongoRepository<Order>
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BindingList<Order> orders = new BindingList<Order>();

        private readonly BindingList<OrderLog> orderlogs = new BindingList<OrderLog>();

        private MongoRepository<OrderLog> orderLogRepo = new MongoRepository<OrderLog>(); 

        public OrderRepository()
        {
            
            foreach(OrderLog orderLog in orderLogRepo.Where(log=>log.TradeDate == DateTime.Today.ToString("yyyyMMdd")).ToList())
            {
                orderlogs.Add(orderLog);
            }
        }

        public List<Order> GetByInstrumentIDAndStatusType(string instrumentId, EnumOrderStatus statusType)
        {
            return orders.Where(o => o.InstrumentId == instrumentId && o.StatusType == statusType).ToList();
        }


        public override Order Add(Order entity)
        {
            orders.Add(entity);
            
            return base.Add(entity);
        }

        public Order UpdateOrderRef(CThostFtdcOrderField pOrder)
        {
            string orderRef = pOrder.FrontID.ToString() + pOrder.SessionID.ToString() +  pOrder.OrderRef.Trim();

            foreach (Order order in orders)
            {
                if (order.OrderRef == orderRef)
                {
                    order.OrderSysID = pOrder.OrderSysID;
                    Update(order);
                    return order; ;
                }

                if (order.CloseOrder != null && order.CloseOrder.OrderRef == orderRef)
                {
                    order.CloseOrder.OrderSysID = pOrder.OrderSysID;
                    Update(order);
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
                    order.TradeDate = pTrade.TradeDate;
                    order.TradeTime = pTrade.TradeTime;
                    order.StatusType = EnumOrderStatus.已开仓;
                    Update(order);
                    return order;
                }

                if (order.CloseOrder != null && order.CloseOrder.OrderSysID == orderSysID && order.CloseOrder.TradeID == null)
                {
                    order.CloseOrder.TradeID = pTrade.TradeID;
                    order.CloseOrder.TradePrice = pTrade.Price;
                    order.CloseOrder.TradeDate = pTrade.TradeDate;
                    order.CloseOrder.TradeTime = pTrade.TradeTime;
                    order.StatusType = EnumOrderStatus.已平仓;
                    order.CloseProfit = order.PositionProfit;
                    order.PositionProfit = 0;
                    Update(order);

                    return order;
                }
            }

            return null;
        }

        public override void Delete(Order entity)
        {
            OrderLog orderLog = new OrderLog();

            ObjectUtils.Copy(entity, orderLog);

            orders.Remove(entity);

            orderlogs.Add(orderLog);
            orderLogRepo.Add(orderLog);

            base.Delete(entity);
        }

        public void Init(BindingList<OrderRecord> orderRecords)
        {
            List<Order> orderdb = this.ToList();

            orders.RaiseListChangedEvents = false;

            foreach (var order in orderdb)
            {
                if (order.StatusType == EnumOrderStatus.已平仓) continue;

                if (order.TradeID != null || orderRecords.Any(record => record.OrderSysID.Trim() == order.OrderSysID))
                {
                    orders.Add(order);
                }
            }

            orders.RaiseListChangedEvents = true;
        }

        public BindingList<Order> getOrders()
        {
            return orders;
        }

        public BindingList<OrderLog> getOrderLogs()
        {
            return orderlogs;
        }
    }
}