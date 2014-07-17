using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.model;
using CTPTradeApi;
using MongoDB.Driver.Linq;
using autotrade.util;

namespace autotrade.business
{
    class OrderManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TradeApi tradeApi;
        private List<PositionDetail> positionDetails = new List<PositionDetail>();
        private List<TradeRecord> tradeRecords = new List<TradeRecord>();
        private List<PositionRecord> positionRecords = new List<PositionRecord>();
        private List<OrderRecord> orderRecords = new List<OrderRecord>();
        private List<Order> orders = new List<Order>();

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

            if (bIsLast) OnRspQryPositionDetail(this, new PositionDetailEventArgs(positionDetails));
                
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

            log.Info(pTrade);
        }

        void tradeApi_OnRtnOrder(ref CThostFtdcOrderField pOrder)
        {
            OrderRecord orderRecord = new OrderRecord();

            ObjectUtils.Copy(pOrder, orderRecord);

            if (orderRecords.Contains(orderRecord))
            {
                orderRecords.Remove(orderRecord);                
            }

            orderRecords.Add(orderRecord);

            OnRspQryOrderRecord(this, new OrderRecordEventArgs(orderRecords));

            Order order = GetOrderByOrderRef(pOrder.OrderRef);

            if (order != null) order.OrderSysID = pOrder.OrderSysID;

            log.Info(order);
        }

        private void tradeApi_OnRspQryOrder(ref CThostFtdcOrderField porder, ref CThostFtdcRspInfoField prspinfo, int nrequestid, bool bislast)
        {
            OrderRecord orderRecord = new OrderRecord();

            ObjectUtils.Copy(porder, orderRecord);

            orderRecords.Add(orderRecord);

            if (bislast) OnRspQryOrderRecord(this, new OrderRecordEventArgs(orderRecords));
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

            if (bIsLast) OnRtnTradeRecord(this, new TradeRecordEventArgs(tradeRecords));

            log.Info(pRspInfo);
            log.Info(pTrade);
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

            orders.Add(order);


            return 0;
        }

        private void tradeApi_OnOnErrRtnOrderInsert(ref CThostFtdcInputOrderField pInputOrder, ref CThostFtdcRspInfoField pRspInfo)
        {
            log.Info(pRspInfo);
            log.Info(pInputOrder);
        }


        private Order GetOrderByOrderRef(String orderRef)
        {
            foreach(var order in orders) 
            {
                if (order.OrderRef == orderRef)
                {
                    return order;   
                }

            }

            return null;
        }
        
    }

    internal class TradeRecordEventArgs : EventArgs
    {
        public List<TradeRecord> tradeRecords { get; set; }
        public TradeRecordEventArgs(List<TradeRecord> tradeRecords)
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
        public List<OrderRecord> OrderRecords { get; set; }
        public OrderRecordEventArgs(List<OrderRecord> orderRecords)
            : base()
        {
            this.OrderRecords = orderRecords;
        }
    }


    
}
