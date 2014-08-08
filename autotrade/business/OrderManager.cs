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

        private List<PositionDetail> positionDetails = new List<PositionDetail>();
        private BindingList<TradeRecord> tradeRecords = new BindingList<TradeRecord>();
        private List<PositionRecord> positionRecords = new List<PositionRecord>();
        private BindingList<OrderRecord> orderRecords = new BindingList<OrderRecord>();


        public OrderRepository OrderRepository { get; set; }

        public AccountManager AccountManager { get; set; }


        public delegate void TradeRecordHandler(object sender, TradeRecordEventArgs e);

        public event TradeRecordHandler OnRtnTradeRecord;

        public delegate void PositionDetailHandler(object sender, PositionDetailEventArgs e);
        public event PositionDetailHandler OnRspQryPositionDetail;

        public delegate void PositionRecordHandler(object sender, PositionRecordEventArgs e);
        public event PositionRecordHandler OnRspQryPositionRecord;

        public delegate void OrderRecordHandler(object sender, OrderRecordEventArgs e);
        public event OrderRecordHandler OnRspQryOrderRecord;

        public delegate void OrderHandler(object sender, OrderEventArgs e);
        public event OrderHandler OnRspQryOrder;

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
            log.Info(e.pInputOrderAction);
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

        void tradeApi_OnRspQryInvestorPositionDetail(object sender, OnRspQryInvestorPositionDetailArgs e)
        {
            PositionDetail positionDetail = new PositionDetail();

            ObjectUtils.CopyStruct(e.pInvestorPositionDetail, positionDetail);

            positionDetails.Add(positionDetail);



            if (e.bIsLast)
            {
                OnRspQryPositionDetail(this, new PositionDetailEventArgs(positionDetails));
            }
        }

        void tradeApi_OnRspQryInvestorPosition(object sender, OnRspQryInvestorPositionArgs e)
        {
            PositionRecord positionRecord = new PositionRecord();

            ObjectUtils.CopyStruct(e.pInvestorPosition, positionRecord);

            positionRecords.Add(positionRecord);

            if (e.bIsLast) OnRspQryPositionRecord(this, new PositionRecordEventArgs(positionRecords));
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


        public int OrderInsert(Order order)
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

                int orderRef = tradeApi.OrderInsert(closeOrder.InstrumentId, closeOrder.OffsetFlag, closeOrder.Direction, closeOrder.Price,
                        closeOrder.Volume);

                order.CloseOrder.OrderRef = orderRef.ToString();

                order.CloseOrder.FrontID = tradeApi.FrontID;

                order.CloseOrder.SessionID = tradeApi.SessionID;
                
                order.CloseOrder.StatusType = EnumOrderStatus.平仓中;

                order.StatusType = EnumOrderStatus.平仓中;
                
            }


            return 0;
        }

        public void CancelOrder(Order order)
        {
            tradeApi.CancelOrder(order.OrderRef, order.FrontID, order.SessionID, order.InstrumentId);
            OrderRepository.Delete(order);                
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
            AccountManager.Accounts[0].CloseProfit = OrderRepository.getOrders().Sum(o => o.CloseProfit);
        }        

        public BindingList<Order> getOrders()
        {
            return OrderRepository.getOrders();
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

        public BindingList<OrderLog> GetOrderLogs(String tradingDay)
        {
            return OrderRepository.GetOrderLogs(tradingDay);
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
    }

    public class TradeRecordEventArgs : EventArgs
    {
        public BindingList<TradeRecord> tradeRecords { get; set; }
        public TradeRecordEventArgs(BindingList<TradeRecord> tradeRecords)
            : base()
        {
            this.tradeRecords = tradeRecords;
        }
    }

    public class PositionDetailEventArgs : EventArgs
    {
        public List<PositionDetail> PositionDetails { get; set; }
        public PositionDetailEventArgs(List<PositionDetail> positionDetails)            
        {
            this.PositionDetails = positionDetails;
        }
    }

    public class PositionRecordEventArgs : EventArgs
    {
        public List<PositionRecord> PositionRecords { get; set; }
        public PositionRecordEventArgs(List<PositionRecord> positionRecords)
            : base()
        {
            this.PositionRecords = positionRecords;
        }
    }

    public class OrderRecordEventArgs : EventArgs
    {
        public BindingList<OrderRecord> OrderRecords { get; set; }
        public OrderRecordEventArgs(BindingList<OrderRecord> orderRecords)
            : base()
        {
            this.OrderRecords = orderRecords;
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
