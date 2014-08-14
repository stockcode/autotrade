using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver.Linq;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;
using Telerik.WinControls.UI;
using autotrade.model;
using autotrade.Repository;
using autotrade.util;
using MongoRepository;

namespace autotrade.business
{
    public class OrderManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TraderApiWrapper tradeApi;

        private BindingList<TradeRecord> tradeRecords = new BindingList<TradeRecord>();
        private BindingList<OrderRecord> orderRecords = new BindingList<OrderRecord>();


        public OrderRepository OrderRepository { get; set; }

        public AccountManager AccountManager { get; set; }

        private BindingList<Order> stubOrders = new BindingList<Order>();


        public delegate void OrderHandler(object sender, OrderEventArgs e);
        public event OrderHandler OnRspQryOrder;

        private bool cancelling = false;

        public OrderManager(TraderApiWrapper traderApi)
        {
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
            Order order = OrderRepository.UpdateTradeID(e.pTrade);

            if (order != null && order.StatusType == EnumOrderStatus.已平仓)
            {
                OnRspQryOrder(this, new OrderEventArgs(new MethodInvoker(() => OrderRepository.Delete(order))));                
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

            Order order = OrderRepository.UpdateOrderRef(e.pOrder);

            log.Info(order);
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
                OnRspQryOrder(this, new OrderEventArgs(new MethodInvoker(() => OrderRepository.Add(order))));


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

                OrderRepository.UpdateTradeID(tradeField);

                OnRspQryOrder(this, new OrderEventArgs(new MethodInvoker(() => OrderRepository.Delete(order)))); 

            }
        }

        private void InsertToCTP(Order order)
        {
            if (order.CloseOrder == null)
            {
                OnRspQryOrder(this, new OrderEventArgs(new MethodInvoker(() => OrderRepository.Add(order))));

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
            tradeApi.CancelOrder(order.OrderRef, order.FrontID, order.SessionID, order.InstrumentId);
            OnRspQryOrder(this, new OrderEventArgs(new MethodInvoker(() => OrderRepository.Delete(order))));                
        }

        public void ProcessData(MarketData marketData)
        {
            
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

            AccountManager.Accounts[0].PositionProfit = OrderRepository.getOrders().Sum(o => o.PositionProfit);
            AccountManager.Accounts[0].CloseProfit = OrderRepository.GetOrderLogs().Sum(o => o.CloseProfit);
            AccountManager.Accounts[0].CurrMargin = OrderRepository.getOrders().Where(o=>o.StatusType == EnumOrderStatus.已开仓).Sum(o => o.UseMargin);
            AccountManager.Accounts[0].FrozenMargin = OrderRepository.getOrders().Where(o => o.StatusType == EnumOrderStatus.开仓中).Sum(o => o.UseMargin);
        }        

        public BindingList<Order> getOrders()
        {
            return cancelling ? stubOrders : OrderRepository.getOrders();
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
            return OrderRepository.GetOrderLogs();
        }

        public void Init()
        {
            OrderRepository.Init(orderRecords);
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
            OrderRepository.ChangeOrderLogs(tradingDay);
        }

        public void CancelOrder(List<Order> orders)
        {
            cancelling = true;

            for (var i = orders.Count - 1; i >= 0; i--)
            {
                CancelOrder(orders[i]);
            }

            cancelling = false;
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
