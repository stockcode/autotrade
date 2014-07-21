using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.model;
using autotrade.Repository;
using CTPTradeApi;
using autotrade.util;
using MongoRepository;

namespace autotrade.business
{
    class OrderManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TradeApi tradeApi;
        private List<PositionDetail> positionDetails = new List<PositionDetail>();
        private BindingList<TradeRecord> tradeRecords = new BindingList<TradeRecord>();
        private List<PositionRecord> positionRecords = new List<PositionRecord>();
        private BindingList<OrderRecord> orderRecords = new BindingList<OrderRecord>();


        private OrderRepository _orderRepository = new OrderRepository();

        public InstrumentManager InstrumentManager { get; set; }

        public delegate void TradeRecordHandler(object sender, TradeRecordEventArgs e);

        public event TradeRecordHandler OnRtnTradeRecord;

        public delegate void PositionDetailHandler(object sender, PositionDetailEventArgs e);
        public event PositionDetailHandler OnRspQryPositionDetail;

        public delegate void PositionRecordHandler(object sender, PositionRecordEventArgs e);
        public event PositionRecordHandler OnRspQryPositionRecord;

        public delegate void OrderRecordHandler(object sender, OrderRecordEventArgs e);
        public event OrderRecordHandler OnRspQryOrderRecord;

        public OrderManager(TradeApi tradeApi)
        {
            this.tradeApi = tradeApi;
            this.tradeApi.OnRspQryOrder += tradeApi_OnRspQryOrder;
            this.tradeApi.OnErrRtnOrderInsert += tradeApi_OnOnErrRtnOrderInsert;
            this.tradeApi.OnErrRtnOrderAction += tradeApi_OnErrRtnOrderAction;
            this.tradeApi.OnRspQryTrade += tradeApi_OnRspQryTrade;
            this.tradeApi.OnRspOrderInsert += tradeApi_OnRspOrderInsert;
            this.tradeApi.OnRspQryInvestorPosition += tradeApi_OnRspQryInvestorPosition;
            this.tradeApi.OnRspQryInvestorPositionDetail += tradeApi_OnRspQryInvestorPositionDetail;


            this.tradeApi.OnRtnOrder += tradeApi_OnRtnOrder;
            this.tradeApi.OnRtnTrade += tradeApi_OnRtnTrade;            
        }

        void tradeApi_OnRspQryInvestorPosition(ref CThostFtdcInvestorPositionField pInvestorPosition, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            PositionRecord  positionRecord = new PositionRecord();

            ObjectUtils.Copy(pInvestorPosition, positionRecord);

            positionRecords.Add(positionRecord);

            if (bIsLast) OnRspQryPositionRecord(this, new PositionRecordEventArgs(positionRecords));
        }

        void tradeApi_OnRspQryInvestorPositionDetail(ref CThostFtdcInvestorPositionDetailField pInvestorPositionDetail, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            PositionDetail positionDetail = new PositionDetail();

            ObjectUtils.Copy(pInvestorPositionDetail, positionDetail);

            positionDetails.Add(positionDetail);



            if (bIsLast)
            {
                OnRspQryPositionDetail(this, new PositionDetailEventArgs(positionDetails));
            }
                
        }

        public int QryOrder()
        {
            return tradeApi.QryOrder();
        }

        public int QryTrade()
        {
            return this.tradeApi.QryTrade();
        }

        public int QryInvestorPosition()
        {
            return this.tradeApi.QryInvestorPosition();
        }

        public int QryInvestorPositionDetail()
        {
            return this.tradeApi.QryInvestorPositionDetail();
        }

        

        void tradeApi_OnRtnTrade(ref CThostFtdcTradeField pTrade)
        {
            TradeRecord tradeRecord = new TradeRecord();

            ObjectUtils.Copy(pTrade, tradeRecord);

            //OnRtnTreadeRecord(this, new TradeRecordEventArgs(tradeRecord));


            Order order = _orderRepository.UpdateTradeID(pTrade);

            log.Info(pTrade);
        }

        void tradeApi_OnRtnOrder(ref CThostFtdcOrderField pOrder)
        {
            OrderRecord orderRecord = GetOrderRecord(pOrder.RequestID);

            if (orderRecord == null)
            {
                orderRecord = new OrderRecord();
                orderRecords.Add(orderRecord);
                
            }

            ObjectUtils.Copy(pOrder, orderRecord);

            //OnRspQryOrderRecord(this, new OrderRecordEventArgs(orderRecords));

            Order order = _orderRepository.UpdateOrderRef(pOrder);

            log.Info(order);
        }
 
        private OrderRecord GetOrderRecord(int requestID)
        {
            foreach (var orderRecord in orderRecords)
            {
                if (orderRecord.RequestID == requestID) return orderRecord;
            }

            return null;
        }

        private void tradeApi_OnRspQryOrder(ref CThostFtdcOrderField porder, ref CThostFtdcRspInfoField prspinfo, int nrequestid, bool bislast)
        {
            OrderRecord orderRecord = new OrderRecord();

            ObjectUtils.Copy(porder, orderRecord);

            orderRecords.Add(orderRecord);

            if (bislast)
            {
                _orderRepository.Init(orderRecords);

                OnRspQryOrderRecord(this, new OrderRecordEventArgs(orderRecords));                
            }            
        }

        void tradeApi_OnRspOrderInsert(ref CThostFtdcInputOrderField pInputOrder, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pInputOrder);
        }

        void tradeApi_OnRspQryTrade(ref CThostFtdcTradeField pTrade, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            TradeRecord tradeRecord = new TradeRecord();

            ObjectUtils.Copy(pTrade, tradeRecord);
            
            tradeRecords.Add(tradeRecord);

            _orderRepository.UpdateTradeID(pTrade);

            if (bIsLast)
            {
                OnRtnTradeRecord(this, new TradeRecordEventArgs(tradeRecords));
            }

//            log.Info(pRspInfo);
//            log.Info(pTrade);
        }

        void tradeApi_OnErrRtnOrderAction(ref CThostFtdcOrderActionField pOrderAction, ref CThostFtdcRspInfoField pRspInfo)
        {
            throw new NotImplementedException();
        }

        public int OrderInsert(Order order)
        {
            if (order.Price < 0.01)
                tradeApi.OrderInsert(order.InstrumentId, order.OffsetFlag, order.Direction, order.Volume);
            else
                tradeApi.OrderInsert(order.InstrumentId, order.OffsetFlag, order.Direction, order.Price, order.Volume);

            order.StatusType = EnumOrderStatus.开仓中;

            order.OrderRef = tradeApi.MaxOrderRef.ToString();

            order.Unit = InstrumentManager.GetUnit(order.InstrumentId);

            _orderRepository.AddOrder(order);

            

            return 0;
        }

        public void ProcessData(MarketData marketData)
        {
            Order order = _orderRepository.GetByInstrumentID(marketData.InstrumentId);
            if (order != null && order.StatusType == EnumOrderStatus.已开仓)
            {

                double profit = (order.Direction == EnumDirectionType.Buy)
                    ? marketData.LastPrice - order.TradePrice
                    : order.TradePrice - marketData.LastPrice;

                order.Profit = profit * order.Unit;

                order.StrategyType
            }
        }

        private void tradeApi_OnOnErrRtnOrderInsert(ref CThostFtdcInputOrderField pInputOrder, ref CThostFtdcRspInfoField pRspInfo)
        {
            log.Info(pRspInfo);
            log.Info(pInputOrder);
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


    
}
