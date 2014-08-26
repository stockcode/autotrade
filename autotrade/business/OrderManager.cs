using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver.Linq;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;
using Telerik.WinControls.UI;
using autotrade.model;
using autotrade.util;
using MongoRepository;

namespace autotrade.business
{
    public class OrderManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TraderApiWrapper tradeApi;
        private ReaderWriterLockSlim sw;

        private BindingList<TradeRecord> tradeRecords = new BindingList<TradeRecord>();
        private BindingList<OrderRecord> orderRecords = new BindingList<OrderRecord>();

        private readonly BindingList<Order> orders = new BindingList<Order>();

        private readonly BindingList<OrderLog> orderlogs = new BindingList<OrderLog>();

        private MongoRepository<OrderLog> orderLogRepo = new MongoRepository<OrderLog>();


        public MongoRepository<Order> OrderRepository = new MongoRepository<Order>();

        public AccountManager AccountManager { get; set; }

        public delegate void OrderHandler(object sender, OrderEventArgs e);
        public event OrderHandler OnRspQryOrder;

        public OrderManager(TraderApiWrapper traderApi, ReaderWriterLockSlim sw)
        {
            this.sw = sw;
            this.tradeApi = traderApi;

//            this.tradeApi.OnRspQryOrder += tradeApi_OnRspQryOrder;

//            this.tradeApi.OnRspQryTrade += tradeApi_OnRspQryTrade;
//            this.tradeApi.OnRspQryInvestorPosition += tradeApi_OnRspQryInvestorPosition;
//            this.tradeApi.OnRspQryInvestorPositionDetail += tradeApi_OnRspQryInvestorPositionDetail;
//
//
            traderApi.OnRspOrderAction += traderApi_OnRspOrderAction;
            this.tradeApi.OnRtnOrder += tradeApi_OnRtnOrder;
            this.tradeApi.OnRtnTrade += tradeApi_OnRtnTrade;
                        this.tradeApi.OnErrRtnOrderInsert += tradeApi_OnErrRtnOrderInsert;
                        this.tradeApi.OnErrRtnOrderAction += tradeApi_OnErrRtnOrderAction;
                        this.tradeApi.OnRspOrderInsert += tradeApi_OnRspOrderInsert;
        }

        void traderApi_OnRspOrderAction(object sender, OnRspOrderActionArgs e)
        {
            log.Info(e.pRspInfo);
        }

        void tradeApi_OnRtnTrade(object sender, OnRtnTradeArgs e)
        {             
            Order order = UpdateTradeID(e.pTrade);

            if (order != null && order.StatusType == EnumOrderStatus.已平仓)
            {
                var orderLog = new OrderLog();
                ObjectUtils.Copy(order, orderLog);
                orderLog.Id = null;

                orders.Remove(order);
                orderlogs.Add(orderLog);
                orderLogRepo.Add(orderLog);
                OrderRepository.Delete(order);                
            }

            log.Info(e.pTrade);
        }

        void tradeApi_OnRtnOrder(object sender, OnRtnOrderArgs e)
        {
            OrderRecord orderRecord = GetOrderRecord(e.pOrder.RequestID);

            if (orderRecord == null)
            {
                orderRecord = new OrderRecord();
                orderRecords.Add(orderRecord);

            }

            ObjectUtils.CopyStruct(e.pOrder, orderRecord);

            //OnRspQryOrderRecord(this, new OrderRecordEventArgs(orderRecords));

            UpdateOrderRef(e.pOrder);
        }


        void tradeApi_OnRspOrderInsert(object sender, OnRspOrderInsertArgs e)
        {
            log.Info(e.pRspInfo);
            log.Info(e.pInputOrder);
        }        

        void tradeApi_OnErrRtnOrderAction(object sender, OnErrRtnOrderActionArgs e)
        {
            log.Info(e.pOrderAction.StatusMsg);
        }

        void tradeApi_OnErrRtnOrderInsert(object sender, OnErrRtnOrderInsertArgs e)
        {
            log.Info(e.pRspInfo);
            log.Info(e.pInputOrder);
        }


        public void QryInvestorPosition()
        {
            this.tradeApi.ReqQryInvestorPosition("");
        }

        public void QryInvestorPositionDetail()
        {
            this.tradeApi.ReqQryInvestorPositionDetail("");
        }

 
        private OrderRecord GetOrderRecord(int requestID)
        {
            foreach (var orderRecord in orderRecords)
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
            if (order.CloseOrder == null)
            {
                OnRspQryOrder(this, new OrderEventArgs(new MethodInvoker(() => Add(order))));


                order.OrderRef = tradeApi.MaxOrderRef++.ToString();

                order.FrontID = tradeApi.FrontID;

                order.SessionID = tradeApi.SessionID;

                order.TradePrice = order.LastPrice;

                order.TradeDate = tradeApi.TradingDay;

                order.TradeTime = DateTime.Now.ToString("HH:mm:ss");

                order.StatusType = EnumOrderStatus.已开仓;
            }
            else
            {
                var closeOrder = order.CloseOrder;


                closeOrder.OrderRef = tradeApi.MaxOrderRef++.ToString();

                closeOrder.FrontID = tradeApi.FrontID;

                closeOrder.SessionID = tradeApi.SessionID;

                closeOrder.StatusType = EnumOrderStatus.平仓中;
                closeOrder.OrderSysID = tradeApi.MaxOrderRef++.ToString();

                order.StatusType = EnumOrderStatus.平仓中;

                var tradeField = new CThostFtdcTradeField();

                tradeField.OrderSysID = order.CloseOrder.OrderSysID;
                
                tradeField.Price = order.CloseOrder.LastPrice;

                tradeField.TradeDate = tradeApi.TradingDay;

                tradeField.TradeTime = DateTime.Now.ToString("HH:mm:ss");

                UpdateTradeID(tradeField);

                OnRspQryOrder(this, new OrderEventArgs(new MethodInvoker(() => OrderRepository.Delete(order)))); 

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
            if (order.CloseOrder == null)
            {
                OnRspQryOrder(this, new OrderEventArgs(new MethodInvoker(() => Add(order))));

                int orderRef = tradeApi.OrderInsert(order.InstrumentId, order.OffsetFlag, order.Direction, order.Price,
                        order.Volume);

                order.OrderRef = orderRef.ToString();

                order.FrontID = tradeApi.FrontID;

                order.SessionID = tradeApi.SessionID;

                order.StatusType = EnumOrderStatus.开仓中;
            }
            else
            {
                var closeOrder = order.CloseOrder;

                int orderRef = tradeApi.OrderInsert(closeOrder.InstrumentId, closeOrder.OffsetFlag, closeOrder.Direction,closeOrder.Price,
                        closeOrder.Volume);

                order.CloseOrder.OrderRef = orderRef.ToString();

                order.CloseOrder.FrontID = tradeApi.FrontID;

                order.CloseOrder.SessionID = tradeApi.SessionID;

                order.CloseOrder.StatusType = EnumOrderStatus.平仓中;

                order.StatusType = EnumOrderStatus.平仓中;

            }
        }

        public void CancelOrder(Order order)
        {
            var od = order;
            if (order.CloseOrder != null) od = order.CloseOrder;

            tradeApi.CancelOrder(od.OrderRef, od.FrontID, od.SessionID, od.InstrumentId);

            sw.EnterWriteLock();

            if (order.CloseOrder == null)
            {
                OnRspQryOrder(this, new OrderEventArgs(new MethodInvoker(() => orders.Remove(order))));                 

                OrderRepository.Delete(order);          
            }
            else
            {
                order.CloseOrder = null;
                order.StatusType = EnumOrderStatus.已开仓;
                OrderRepository.Update(order);
            }            

            sw.ExitWriteLock();
        }

        public void ProcessData(MarketData marketData)
        {
            sw.EnterReadLock();
            foreach(var order in getOrders().Where(o=>o.InstrumentId == marketData.InstrumentId && o.StatusType != EnumOrderStatus.开仓中))
            {

                double profit = (order.Direction == TThostFtdcDirectionType.Buy)
                    ? marketData.LastPrice - order.TradePrice
                    : order.TradePrice - marketData.LastPrice;

                order.LastPrice = marketData.LastPrice;
                order.PositionProfit = profit * order.Unit * order.Volume;
                order.PositionTimeSpan =
                    DateTime.Now.Subtract(DateTime.ParseExact(order.ActualTradeDate, "yyyyMMdd HH:mm:ss",
                        CultureInfo.InvariantCulture));
            }

            AccountManager.Accounts[0].PositionProfit = orders.Sum(o => o.PositionProfit);
            AccountManager.Accounts[0].CloseProfit = orderlogs.Sum(o => o.CloseProfit);
            AccountManager.Accounts[0].CurrMargin = orders.Where(o=>o.StatusType == EnumOrderStatus.已开仓).Sum(o => o.UseMargin);
            AccountManager.Accounts[0].FrozenMargin = orders.Where(o => o.StatusType == EnumOrderStatus.开仓中).Sum(o => o.UseMargin);

            sw.ExitReadLock();
        }        

        public BindingList<Order> getOrders()
        {
            return orders;
        }

        public void AddOrderRecord(OrderRecord orderRecord)
        {
            orderRecords.Add(orderRecord);
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
            List<Order> orderdb = OrderRepository.ToList();

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

        public void CloseOrder(Order order)
        {
            var neworder = new Order();
            neworder.OffsetFlag = TThostFtdcOffsetFlagType.CloseToday;
            neworder.Direction = order.Direction == TThostFtdcDirectionType.Buy
                ? TThostFtdcDirectionType.Sell
                : TThostFtdcDirectionType.Buy;
            neworder.InstrumentId = order.InstrumentId;
            
            neworder.Price = 0;
            neworder.Volume = order.Volume;
            neworder.StrategyType = "Close Order By User";

            order.CloseOrder = neworder;

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

            for (var i = orders.Count - 1; i >= 0; i--)
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

            order = orders.FirstOrDefault(
                o =>
                    o.CloseOrder != null && o.CloseOrder.OrderRef == pOrder.OrderRef &&
                    o.CloseOrder.FrontID == pOrder.FrontID && o.CloseOrder.SessionID == pOrder.SessionID);

            if (order != null)
            {

                order.CloseOrder.OrderSysID = pOrder.OrderSysID;
                OrderRepository.Update(order);
            }

            sw.ExitWriteLock();
        }

        public Order UpdateTradeID(CThostFtdcTradeField pTrade)
        {
            string orderSysID = pTrade.OrderSysID.Trim();

            sw.EnterWriteLock();

            var order = orders.FirstOrDefault(
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
                order = orders.FirstOrDefault(
                    o =>
                        o.CloseOrder != null && o.CloseOrder.OrderSysID == orderSysID && o.CloseOrder.TradeID == null);

                if (order != null)
                {
                    order.CloseOrder.TradeID = pTrade.TradeID;
                    order.CloseOrder.TradePrice = pTrade.Price;
                    order.CloseOrder.TradeDate = pTrade.TradeDate;
                    order.CloseOrder.TradeTime = pTrade.TradeTime;
                    order.StatusType = EnumOrderStatus.已平仓;

                    double profit = (order.Direction == TThostFtdcDirectionType.Buy)
                        ? order.CloseOrder.TradePrice - order.TradePrice
                        : order.TradePrice - order.CloseOrder.TradePrice;
                    order.CloseProfit = profit*order.Volume*order.Unit;
                    order.PositionProfit = 0;
                    OrderRepository.Update(order);
                }
            }

            sw.ExitWriteLock();

            return order;
        }
    }

    public class OrderEventArgs : EventArgs
    {
        public MethodInvoker methodInvoker { get; set; }
        public OrderEventArgs(MethodInvoker methodInvoker)
            : base()
        {
            this.methodInvoker = methodInvoker;
        }
    }


    
}
