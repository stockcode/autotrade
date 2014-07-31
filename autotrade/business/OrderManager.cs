using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;
using Telerik.WinControls.UI;
using autotrade.model;
using autotrade.Repository;
using autotrade.util;
using MongoRepository;

namespace autotrade.business
{
    class OrderManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TraderApiWrapper tradeApi;
        private List<PositionDetail> positionDetails = new List<PositionDetail>();
        private BindingList<TradeRecord> tradeRecords = new BindingList<TradeRecord>();
        private List<PositionRecord> positionRecords = new List<PositionRecord>();
        private BindingList<OrderRecord> orderRecords = new BindingList<OrderRecord>();


        private OrderRepository _orderRepository = new OrderRepository();

        public InstrumentManager InstrumentManager { get; set; }

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

        public OrderManager(TraderApiWrapper tradeApi)
        {
            this.tradeApi = tradeApi;
            this.tradeApi.OnRspQryOrder += tradeApi_OnRspQryOrder;
            this.tradeApi.OnErrRtnOrderInsert += tradeApi_OnErrRtnOrderInsert;
            this.tradeApi.OnErrRtnOrderAction += tradeApi_OnErrRtnOrderAction;
            this.tradeApi.OnRspQryTrade += tradeApi_OnRspQryTrade;
            this.tradeApi.OnRspOrderInsert += tradeApi_OnRspOrderInsert;
            this.tradeApi.OnRspQryInvestorPosition += tradeApi_OnRspQryInvestorPosition;
            this.tradeApi.OnRspQryInvestorPositionDetail += tradeApi_OnRspQryInvestorPositionDetail;


            this.tradeApi.OnRtnOrder += tradeApi_OnRtnOrder;
            this.tradeApi.OnRtnTrade += tradeApi_OnRtnTrade;            
        }

        void tradeApi_OnRtnTrade(object sender, OnRtnTradeArgs e)
        {
            TradeRecord tradeRecord = new TradeRecord();

            ObjectUtils.Copy(e.pTrade, tradeRecord);

            //OnRtnTreadeRecord(this, new TradeRecordEventArgs(tradeRecord));


            Order order = _orderRepository.UpdateTradeID(e.pTrade);

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

            ObjectUtils.Copy(e.pOrder, orderRecord);

            //OnRspQryOrderRecord(this, new OrderRecordEventArgs(orderRecords));

            Order order = _orderRepository.UpdateOrderRef(e.pOrder);

            log.Info(order);
        }

        void tradeApi_OnRspQryInvestorPositionDetail(object sender, OnRspQryInvestorPositionDetailArgs e)
        {
            PositionDetail positionDetail = new PositionDetail();

            ObjectUtils.Copy(e.pInvestorPositionDetail, positionDetail);

            positionDetails.Add(positionDetail);



            if (e.bIsLast)
            {
                OnRspQryPositionDetail(this, new PositionDetailEventArgs(positionDetails));
            }
        }

        void tradeApi_OnRspQryInvestorPosition(object sender, OnRspQryInvestorPositionArgs e)
        {
            PositionRecord positionRecord = new PositionRecord();

            ObjectUtils.Copy(e.pInvestorPosition, positionRecord);

            positionRecords.Add(positionRecord);

            if (e.bIsLast) OnRspQryPositionRecord(this, new PositionRecordEventArgs(positionRecords));
        }

        void tradeApi_OnRspOrderInsert(object sender, OnRspOrderInsertArgs e)
        {
            log.Info(e.pRspInfo);
            log.Info(e.pInputOrder);
        }

        void tradeApi_OnRspQryTrade(object sender, OnRspQryTradeArgs e)
        {
            TradeRecord tradeRecord = new TradeRecord();

            ObjectUtils.Copy(e.pTrade, tradeRecord);

            tradeRecords.Add(tradeRecord);

            _orderRepository.UpdateTradeID(e.pTrade);

            if (e.bIsLast)
            {
                OnRtnTradeRecord(this, new TradeRecordEventArgs(tradeRecords));
            }

            //            log.Info(pRspInfo);
            //            log.Info(pTrade);
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

        void tradeApi_OnRspQryOrder(object sender, OnRspQryOrderArgs e)
        {
            OrderRecord orderRecord = new OrderRecord();

            ObjectUtils.Copy(e.pOrder, orderRecord);

            orderRecords.Add(orderRecord);

            if (e.bIsLast)
            {
                _orderRepository.Init(orderRecords);

                OnRspQryOrderRecord(this, new OrderRecordEventArgs(orderRecords));
            } 
        }

//        public int QryOrder()
//        {
//            return tradeApi.QryOrder();
//        }
//
//        public int QryTrade()
//        {
//            return this.tradeApi.QryTrade();
//        }

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

//                if (order.Price < 0.01)
//                    tradeApi.OrderInsert(order.InstrumentId, order.OffsetFlag, order.Direction, order.Volume);
//                else
//                    tradeApi.OrderInsert(order.InstrumentId, order.OffsetFlag, order.Direction, order.Price,
//                        order.Volume);

                order.StatusType = EnumOrderStatus.开仓中;

                //order.OrderRef = tradeApi.MaxOrderRef.ToString();

                order.Unit = InstrumentManager.GetUnit(order.InstrumentId);



                OnRspQryOrder(this, new OrderEventArgs(new MethodInvoker(() => _orderRepository.AddOrder(order))));

            }
            else
            {
                var closeOrder = order.CloseOrder;

//                if (order.Price < 0.01)
//                    tradeApi.OrderInsert(closeOrder.InstrumentId, closeOrder.OffsetFlag, closeOrder.Direction, closeOrder.Volume);
//                else
//                    tradeApi.OrderInsert(closeOrder.InstrumentId, closeOrder.OffsetFlag, closeOrder.Direction, closeOrder.Price,
//                        closeOrder.Volume);
//
//                closeOrder.OrderRef = tradeApi.MaxOrderRef.ToString();

                order.StatusType = EnumOrderStatus.平仓中;
            }


            return 0;
        }

        public void ProcessData(MarketData marketData)
        {
            List<Order> orders = _orderRepository.GetByInstrumentIDAndStatusType(marketData.InstrumentId, EnumOrderStatus.已开仓);
            foreach(var order in orders)
            {

                double profit = (order.Direction == TThostFtdcDirectionType.Buy)
                    ? marketData.LastPrice - order.TradePrice
                    : order.TradePrice - marketData.LastPrice;

                order.PositionProfit = profit * order.Unit;                
            }

            AccountManager.Accounts[0].PositionProfit =_orderRepository.getOrders().Sum(o => o.PositionProfit);
            AccountManager.Accounts[0].CloseProfit = _orderRepository.getOrders().Sum(o => o.CloseProfit);
        }        

        public BindingList<Order> getOrders()
        {
            return _orderRepository.getOrders();
        }

        
    }

    internal class TradeRecordEventArgs : EventArgs
    {
        public BindingList<TradeRecord> tradeRecords { get; set; }
        public TradeRecordEventArgs(BindingList<TradeRecord> tradeRecords)
            : base()
        {
            this.tradeRecords = tradeRecords;
        }
    }

    internal class PositionDetailEventArgs : EventArgs
    {
        public List<PositionDetail> PositionDetails { get; set; }
        public PositionDetailEventArgs(List<PositionDetail> positionDetails)
            : base()
        {
            this.PositionDetails = positionDetails;
        }
    }

    internal class PositionRecordEventArgs : EventArgs
    {
        public List<PositionRecord> PositionRecords { get; set; }
        public PositionRecordEventArgs(List<PositionRecord> positionRecords)
            : base()
        {
            this.PositionRecords = positionRecords;
        }
    }

    internal class OrderRecordEventArgs : EventArgs
    {
        public BindingList<OrderRecord> OrderRecords { get; set; }
        public OrderRecordEventArgs(BindingList<OrderRecord> orderRecords)
            : base()
        {
            this.OrderRecords = orderRecords;
        }
    }

    internal class OrderEventArgs : EventArgs
    {
        public MethodInvoker methodInvoker { get; set; }
        public OrderEventArgs(MethodInvoker methodInvoker)
            : base()
        {
            this.methodInvoker = methodInvoker;
        }
    }


    
}
