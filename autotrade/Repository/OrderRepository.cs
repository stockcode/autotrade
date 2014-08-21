using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
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
        private ReaderWriterLockSlim sw;

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BindingList<Order> orders = new BindingList<Order>();

        private readonly BindingList<OrderLog> orderlogs = new BindingList<OrderLog>();

        private MongoRepository<OrderLog> orderLogRepo = new MongoRepository<OrderLog>(); 

        public OrderRepository(ReaderWriterLockSlim sw)
        {
            this.sw = sw;
        }


        public override Order Add(Order entity)
        {
            sw.EnterWriteLock();
            
            entity.Id = null;            
            orders.Add(entity);

            sw.ExitWriteLock();
            

            return base.Add(entity);
        }

        public void UpdateOrderRef(CThostFtdcOrderField pOrder)
        {
            sw.EnterWriteLock();

            var order = orders.FirstOrDefault(
                o => o.OrderRef == pOrder.OrderRef && o.FrontID == pOrder.FrontID && o.SessionID == pOrder.SessionID);

            if (order != null)
            {
                order.OrderSysID = pOrder.OrderSysID;
                order.ExchangeID = pOrder.ExchangeID;
                Update(order);
            }

            order = orders.FirstOrDefault(
                o =>
                    o.CloseOrder != null && o.CloseOrder.OrderRef == pOrder.OrderRef &&
                    o.CloseOrder.FrontID == pOrder.FrontID && o.CloseOrder.SessionID == pOrder.SessionID);

            if (order != null)
            {

                order.CloseOrder.OrderSysID = pOrder.OrderSysID;
                Update(order);
            }

            sw.ExitWriteLock();
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

                    double profit = (order.Direction == TThostFtdcDirectionType.Buy)
                    ? order.CloseOrder.TradePrice - order.TradePrice
                    : order.TradePrice - order.CloseOrder.TradePrice;
                    order.CloseProfit = profit * order.Volume * order.Unit;
                    order.PositionProfit = 0;
                    Update(order);

                    return order;
                }
            }

            return null;
        }

        public override void Delete(Order entity)
        {
            sw.EnterWriteLock();

            OrderLog orderLog = new OrderLog();            
            ObjectUtils.Copy(entity, orderLog);
            orderLog.Id = null;
            
            orders.Remove(entity);
            orderlogs.Add(orderLog);
            orderLogRepo.Add(orderLog);
            sw.ExitWriteLock();

            base.Delete(entity);
        }

        public void Init(BindingList<OrderRecord> orderRecords)
        {
            List<Order> orderdb = this.ToList();

            orders.RaiseListChangedEvents = false;

            sw.EnterWriteLock();
            foreach (var order in orderdb)
            {
                if (order.StatusType == EnumOrderStatus.已平仓) continue;

                if (order.TradeID != null || orderRecords.Any(record => record.OrderSysID.Trim() == order.OrderSysID))
                {
                    orders.Add(order);
                }
            }
            sw.ExitWriteLock();

            orders.RaiseListChangedEvents = true;
        }

        public BindingList<Order> getOrders()
        {
            return orders;
        }

        public BindingList<OrderLog> GetOrderLogs()
        {
            return orderlogs;
        }

        public void ChangeOrderLogs(string tradingDay)
        {
            orderlogs.RaiseListChangedEvents = false;

            orderlogs.Clear();
            foreach (OrderLog orderLog in orderLogRepo.Where(ol => ol.TradeDate == tradingDay).ToList())
            {
                orderlogs.Add(orderLog);
            }
            orderlogs.RaiseListChangedEvents = true;

            orderlogs.ResetBindings();
        }
    }
}