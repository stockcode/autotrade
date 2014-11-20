using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using autotrade.model;
using autotrade.util;
using log4net;
using MongoRepository;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;

namespace autotrade.business
{
    public class OrderManager
    {
        public delegate void OrderHandler(object sender, OrderEventArgs e);

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MongoRepository<OrderLog> orderLogRepo = new MongoRepository<OrderLog>();
        private readonly BindingList<OrderRecord> orderRecords = new BindingList<OrderRecord>();
        private readonly BindingList<OrderLog> orderlogs = new BindingList<OrderLog>();
        private readonly BindingList<Order> orders = new BindingList<Order>();
        private readonly ReaderWriterLockSlim sw;
        private readonly ReaderWriterLockSlim swRecord = new ReaderWriterLockSlim();
        private readonly TraderApiWrapper tradeApi;

        private readonly BindingList<TradeRecord> tradeRecords = new BindingList<TradeRecord>();


        public MongoRepository<Order> OrderRepository = new MongoRepository<Order>();

        public OrderManager(TraderApiWrapper traderApi, ReaderWriterLockSlim sw)
        {
            this.sw = sw;
            HasMoney = true;
            tradeApi = traderApi;

            traderApi.OnRspOrderAction += traderApi_OnRspOrderAction;
            tradeApi.OnRtnOrder += tradeApi_OnRtnOrder;
            tradeApi.OnRtnTrade += tradeApi_OnRtnTrade;
            tradeApi.OnErrRtnOrderInsert += tradeApi_OnErrRtnOrderInsert;
            tradeApi.OnErrRtnOrderAction += tradeApi_OnErrRtnOrderAction;
            tradeApi.OnRspOrderInsert += tradeApi_OnRspOrderInsert;
        }

        public AccountManager AccountManager { get; set; }

        public bool HasMoney { get; set; }
        public event OrderHandler OnRspQryOrder;

        private void traderApi_OnRspOrderAction(object sender, OnRspOrderActionArgs e)
        {
            log.Info(e.pRspInfo);
        }

        private void tradeApi_OnRtnTrade(object sender, OnRtnTradeArgs e)
        {
            var order = UpdateTradeID(e.pTrade);


            if (order != null && order.StatusType == EnumOrderStatus.已平仓)
            {
                OnRspQryOrder(this, new OrderEventArgs(() => HandleOrderClose(order)));
                if (!HasMoney) HasMoney = true;
            }

            log.Info(e.pTrade);
        }

        private void HandleOrderClose(Order order)
        {
            var orderLog = new OrderLog();
            ObjectUtils.Copy(order, orderLog);
            orderLog.Id = null;

            sw.EnterWriteLock();

            orders.Remove(order);
            orderlogs.Add(orderLog);
            orderLogRepo.Add(orderLog);
            OrderRepository.Delete(order);

            sw.ExitWriteLock();
        }

        private void tradeApi_OnRtnOrder(object sender, OnRtnOrderArgs e)
        {
            OrderRecord orderRecord = GetOrderRecord(e.pOrder.RequestID);

            swRecord.EnterWriteLock();
            if (orderRecord == null)
            {
                orderRecord = new OrderRecord();
                orderRecords.Add(orderRecord);
            }
            swRecord.ExitWriteLock();

            ObjectUtils.CopyStruct(e.pOrder, orderRecord);

            //OnRspQryOrderRecord(this, new OrderRecordEventArgs(orderRecords));

            UpdateOrderRef(e.pOrder);
        }


        private void tradeApi_OnRspOrderInsert(object sender, OnRspOrderInsertArgs e)
        {
            log.Info(e.pRspInfo);
            log.Info(e.pInputOrder);
        }

        private void tradeApi_OnErrRtnOrderAction(object sender, OnErrRtnOrderActionArgs e)
        {
            log.Info(e.pOrderAction.StatusMsg);
        }

        private void tradeApi_OnErrRtnOrderInsert(object sender, OnErrRtnOrderInsertArgs e)
        {
            log.Info(e.pRspInfo);
            log.Info(e.pInputOrder);

            if (e.pRspInfo.ErrorID == 31) //资金不足
            {
                HasMoney = false;
                OnRspQryOrder(this, new OrderEventArgs(() => DeleteErrOrder(e.pInputOrder)));
                
            }
        }


        public void QryInvestorPosition()
        {
            tradeApi.ReqQryInvestorPosition("");
        }

        public void QryInvestorPositionDetail()
        {
            tradeApi.ReqQryInvestorPositionDetail("");
        }


        private OrderRecord GetOrderRecord(int requestID)
        {
            foreach (OrderRecord orderRecord in orderRecords)
            {
                if (orderRecord.RequestID == requestID) return orderRecord;
            }

            return null;
        }


        public void OrderInsert(Order order)
        {
            //InsertToMock(order);

            InsertToCTP(order);
        }

        private void InsertToMock(Order order)
        {
            if (order.CloseOrders.Count == 0)
            {
                OnRspQryOrder(this, new OrderEventArgs(() => Add(order)));


                order.RemainVolume = order.Volume;

                order.OrderRef = tradeApi.MaxOrderRef++.ToString();
                
                order.FrontID = tradeApi.FrontID;

                order.SessionID = tradeApi.SessionID;

                order.TradePrice = order.Price;

                order.TradeDate = tradeApi.TradingDay;

                order.TradeTime = DateTime.Now.ToString("HH:mm:ss");

                order.StatusType = EnumOrderStatus.已开仓;
            }
            else
            {

                foreach (var closeOrder in order.GetClosingOrders())
                {
                    closeOrder.OrderRef = tradeApi.MaxOrderRef++.ToString();                    
                    
                    closeOrder.FrontID = tradeApi.FrontID;

                    closeOrder.SessionID = tradeApi.SessionID;

                    closeOrder.StatusType = EnumOrderStatus.平仓中;
                    closeOrder.OrderSysID = tradeApi.MaxOrderRef++.ToString();


                    var tradeField = new CThostFtdcTradeField();

                    tradeField.TradeID = tradeApi.MaxOrderRef++.ToString();

                    tradeField.OrderSysID = closeOrder.OrderSysID;

                    tradeField.Price = closeOrder.Price;

                    tradeField.TradeDate = tradeApi.TradingDay;

                    tradeField.TradeTime = DateTime.Now.ToString("HH:mm:ss");

                    var o = UpdateTradeID(tradeField);

                    if (o == null || o.StatusType != EnumOrderStatus.已平仓) continue;

                    OnRspQryOrder(this, new OrderEventArgs(() => HandleOrderClose(o)));
                    if (!HasMoney) HasMoney = true;
                }
            }
        }

        private Order Add(Order order)
        {
            sw.EnterWriteLock();

            order.Id = null;
            orders.Add(order);

            sw.ExitWriteLock();

            return OrderRepository.Add(order);
        }

        private void InsertToCTP(Order order)
        {
            if (order.CloseOrders.Count == 0)
            {
                OnRspQryOrder(this, new OrderEventArgs(() => Add(order)));

                int orderRef = tradeApi.OrderInsert(order.InstrumentId, order.OffsetFlag, order.Direction, order.Price,
                    order.Volume);

                order.RemainVolume = order.Volume;

                order.OrderRef = orderRef.ToString();

                order.FrontID = tradeApi.FrontID;

                order.SessionID = tradeApi.SessionID;

                order.StatusType = EnumOrderStatus.开仓中;
            }
            else
            {
                foreach (var closeOrder in order.GetClosingOrders())
                {


                    var orderRef = tradeApi.OrderInsert(closeOrder.InstrumentId, closeOrder.OffsetFlag,
                        closeOrder.Direction,
                        closeOrder.Price,
                        closeOrder.Volume);

                    closeOrder.OrderRef = orderRef.ToString();

                    closeOrder.FrontID = tradeApi.FrontID;

                    closeOrder.SessionID = tradeApi.SessionID;

                    closeOrder.StatusType = EnumOrderStatus.平仓中;
                }

                //order.StatusType = EnumOrderStatus.平仓中;
            }
        }

        public void CancelOrder(Order order)
        {
            if (order == null) return;

            sw.EnterWriteLock();

            if (order.CloseOrders.Count == 0)
            {
                tradeApi.CancelOrder(order.OrderRef, order.FrontID, order.SessionID, order.InstrumentId);

                OnRspQryOrder(this, new OrderEventArgs(() => orders.Remove(order)));

                OrderRepository.Delete(order);
            }
            else
            {
                foreach (var item in order.GetClosingOrders())
                {
                    tradeApi.CancelOrder(item.OrderRef, item.FrontID, item.SessionID, item.InstrumentId);
                    item.CloseOrders.Remove(item);
                }

                order.StatusType = EnumOrderStatus.已开仓;
                OrderRepository.Update(order);
            }

            sw.ExitWriteLock();
        }

        public void ProcessData(MarketData marketData)
        {
            sw.EnterReadLock();
            foreach (
                Order order in
                    getOrders()
                        .Where(o => o.InstrumentId == marketData.InstrumentId && o.StatusType != EnumOrderStatus.开仓中))
            {
                double profit = (order.Direction == TThostFtdcDirectionType.Buy)
                    ? marketData.LastPrice - order.TradePrice
                    : order.TradePrice - marketData.LastPrice;

                order.LastPrice = marketData.LastPrice;
                order.PositionProfit = profit*order.Unit*order.Volume;
                order.PositionTimeSpan =
                    DateTime.Now.Subtract(DateTime.ParseExact(order.ActualTradeDate, "yyyyMMdd HH:mm:ss",
                        CultureInfo.InvariantCulture));
            }

            AccountManager.Accounts[0].PositionProfit = orders.Sum(o => o.PositionProfit);
            AccountManager.Accounts[0].CloseProfit = orderlogs.Sum(o => o.CloseProfit);
            AccountManager.Accounts[0].CurrMargin =
                orders.Where(o => o.StatusType == EnumOrderStatus.已开仓).Sum(o => o.UseMargin);
            AccountManager.Accounts[0].FrozenMargin =
                orders.Where(o => o.StatusType == EnumOrderStatus.开仓中).Sum(o => o.UseMargin);

            sw.ExitReadLock();
        }

        public BindingList<Order> getOrders()
        {
            return orders;
        }

        public void AddOrderRecord(OrderRecord orderRecord)
        {
            swRecord.EnterWriteLock();
            orderRecords.Add(orderRecord);
            swRecord.ExitWriteLock();
        }

        public BindingList<OrderRecord> GetOrderRecords()
        {
            return orderRecords;
        }

        public void AddTradeRecord(TradeRecord tradeRecord)
        {
            tradeRecords.Add(tradeRecord);
        }

        public BindingList<TradeRecord> GetTradeRecords()
        {
            return tradeRecords;
        }

        public BindingList<OrderLog> GetOrderLogs()
        {
            return orderlogs;
        }

        public void Init()
        {
            orders.RaiseListChangedEvents = false;

            sw.EnterWriteLock();

            swRecord.EnterReadLock();
            foreach (Order order in OrderRepository)
            {
                if (order.StatusType == EnumOrderStatus.已平仓) continue;

                if (order.TradeID != null || orderRecords.Any(record => record.OrderSysID.Trim() == order.OrderSysID))
                {
                    orders.Add(order);
                }
            }

            swRecord.ExitReadLock();
            sw.ExitWriteLock();

            orders.RaiseListChangedEvents = true;
        }

        public void CloseOrder(Order order)
        {
            var neworder = new Order
            {
                OffsetFlag = order.IsTodayOrder
                    ? TThostFtdcOffsetFlagType.CloseToday
                    : TThostFtdcOffsetFlagType.Close,
                Direction = order.Direction == TThostFtdcDirectionType.Buy
                    ? TThostFtdcDirectionType.Sell
                    : TThostFtdcDirectionType.Buy,
                InstrumentId = order.InstrumentId,
                Price = 0,
                Volume = order.RemainVolume,
                StrategyType = "Close Order By User"
            };

            order.CloseOrders.Add(neworder);

            OrderInsert(order);
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

        public void CancelOrder(List<Order> orders)
        {
            for (int i = orders.Count - 1; i >= 0; i--)
            {
                CancelOrder(orders[i]);
            }
        }

        public void OpenOrder(string instrumentId, TThostFtdcDirectionType direction)
        {
            var order = new Order();
            order.OffsetFlag = TThostFtdcOffsetFlagType.Open;
            order.Direction = direction;
            order.InstrumentId = instrumentId;

            order.Price = 0;
            order.Volume = 1;
            order.StrategyType = "Open Order By User";

            OrderInsert(order);
        }

        public void DeleteErrOrder(CThostFtdcInputOrderField pOrder)
        {
            sw.EnterWriteLock();
            

            Order order = orders.FirstOrDefault(
                o => o.OrderRef == pOrder.OrderRef);

            if (order != null)
            {
                orders.Remove(order);

                OrderRepository.Delete(order);
            }

            sw.ExitWriteLock();
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
                OrderRepository.Update(order);
            }
            else
            {
                foreach (var o in orders)
                {
                    var closeorder = o.CloseOrders.FirstOrDefault(c => c.OrderRef == pOrder.OrderRef &&
                                                      c.FrontID == pOrder.FrontID && c.SessionID == pOrder.SessionID);
                    
                    if (closeorder == null) continue;

                    closeorder.OrderSysID = pOrder.OrderSysID;
                    OrderRepository.Update(o);
                    break;
                }                
            }

            sw.ExitWriteLock();
        }

        public Order UpdateTradeID(CThostFtdcTradeField pTrade)
        {
            string orderSysID = pTrade.OrderSysID.Trim();

            sw.EnterWriteLock();

            Order order = orders.FirstOrDefault(
                o => o.OrderSysID == orderSysID && o.TradeID == null);

            if (order != null)
            {
                order.TradeID = pTrade.TradeID;
                order.TradePrice = pTrade.Price;
                order.TradeDate = pTrade.TradeDate;
                order.TradeTime = pTrade.TradeTime;
                order.StatusType = EnumOrderStatus.已开仓;
                OrderRepository.Update(order);
            }
            else
            {

                foreach (var o in orders)
                {
                    var closeorder = o.CloseOrders.FirstOrDefault(c => c.OrderSysID == orderSysID && c.TradeID == null);

                    if (closeorder == null) continue;

                    closeorder.TradeID = pTrade.TradeID;
                    closeorder.TradePrice = pTrade.Price;
                    closeorder.TradeDate = pTrade.TradeDate;
                    closeorder.TradeTime = pTrade.TradeTime;
                    closeorder.StatusType = EnumOrderStatus.已平仓;

                    o.RemainVolume -= closeorder.Volume;

                    o.StatusType = o.IsClosed() ? EnumOrderStatus.已平仓 : EnumOrderStatus.已开仓;

                    var profit = (o.Direction == TThostFtdcDirectionType.Buy)
                        ? closeorder.TradePrice - o.TradePrice
                        : o.TradePrice - closeorder.TradePrice;

                    o.CloseProfit += profit * o.Volume * o.Unit;
                    
                    o.PositionProfit -= o.CloseProfit;

                    

                    OrderRepository.Update(o);

                    order = o;

                    break;
                }                     
            }

            sw.ExitWriteLock();

            return order;
        }

        public void CancelAllOrder()
        {
            List<Order> list = orders.Where(o => o.StatusType == EnumOrderStatus.开仓中).ToList();
            if (list.Count > 0)
            {
                CancelOrder(list);
                HasMoney = true;
            }

            list = orders.Where(o => o.StatusType == EnumOrderStatus.平仓中).ToList();
            CancelOrder(list);
        }

        public void ChangeCloseOrder(Order lastOrder, Order closeorder)
        {
            CancelOrder(lastOrder);

            lastOrder.CloseOrders[0] = closeorder;

            OrderInsert(lastOrder);
        }

        public void DeleteOrder(Order order)
        {
            OnRspQryOrder(this, new OrderEventArgs(() => HandleOrderClose(order)));
        }
    }

    public class OrderEventArgs : EventArgs
    {
        public OrderEventArgs(MethodInvoker methodInvoker)
        {
            this.methodInvoker = methodInvoker;
        }

        public MethodInvoker methodInvoker { get; set; }
    }
}