using System;
using System.Collections.Generic;
using System.Reflection;
using autotrade.model;
using autotrade.util;
using CTPTradeApi;
using log4net;

namespace autotrade.business
{
    internal class OrderManager
    {
        public delegate void OrderRecordHandler(object sender, OrderRecordEventArgs e);

        public delegate void PositionDetailHandler(object sender, PositionDetailEventArgs e);

        public delegate void PositionRecordHandler(object sender, PositionRecordEventArgs e);

        public delegate void TradeRecordHandler(object sender, TradeRecordEventArgs e);

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<OrderRecord> orderRecords = new List<OrderRecord>();
        private readonly List<PositionDetail> positionDetails = new List<PositionDetail>();
        private readonly List<PositionRecord> positionRecords = new List<PositionRecord>();
        private readonly TradeApi tradeApi;
        private readonly List<TradeRecord> tradeRecords = new List<TradeRecord>();

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

        public event TradeRecordHandler OnRtnTradeRecord;

        public event PositionDetailHandler OnRspQryPositionDetail;

        public event PositionRecordHandler OnRspQryPositionRecord;

        public event OrderRecordHandler OnRspQryOrderRecord;

        private void tradeApi_OnRspQryInvestorPosition(ref CThostFtdcInvestorPositionField pInvestorPosition,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            var positionRecord = new PositionRecord();

            ObjectUtils.Copy(pInvestorPosition, positionRecord);

            positionRecords.Add(positionRecord);

            if (bIsLast)
            {
                OnRspQryPositionRecord(this, new PositionRecordEventArgs(positionRecords));
            }
        }

        private void tradeApi_OnRspQryInvestorPositionDetail(
            ref CThostFtdcInvestorPositionDetailField pInvestorPositionDetail, ref CThostFtdcRspInfoField pRspInfo,
            int nRequestID, bool bIsLast)
        {
            var positionDetail = new PositionDetail();

            ObjectUtils.Copy(pInvestorPositionDetail, positionDetail);

            positionDetails.Add(positionDetail);

            if (bIsLast)
            {
                OnRspQryPositionDetail(this, new PositionDetailEventArgs(positionDetails));
            }
        }

        public void QryOrder()
        {
            tradeApi.QryOrder();
        }

        public void QryTrade()
        {
            tradeApi.QryTrade();
        }

        private void tradeApi_OnRspQryTrade(ref CThostFtdcTradeField pTrade, ref CThostFtdcRspInfoField pRspInfo,
            int nRequestID, bool bIsLast)
        {
            var tradeRecord = new TradeRecord();

            ObjectUtils.Copy(pTrade, tradeRecord);

            tradeRecords.Add(tradeRecord);

            if (bIsLast)
            {
                OnRtnTradeRecord(this, new TradeRecordEventArgs(tradeRecords));
            }

            //log.Info(pRspInfo);
            //log.Info(pTrade);
        }

        public void QryInvestorPosition()
        {
            tradeApi.QryInvestorPosition();
        }

        public void QryInvestorPositionDetail()
        {
            tradeApi.QryInvestorPositionDetail();
        }


        private void tradeApi_OnRtnTrade(ref CThostFtdcTradeField pTrade)
        {
            var tradeRecord = new TradeRecord();

            ObjectUtils.Copy(pTrade, tradeRecord);

            //OnRtnTreadeRecord(this, new TradeRecordEventArgs(tradeRecord));

            log.Info(pTrade);
        }

        private void tradeApi_OnRtnOrder(ref CThostFtdcOrderField pOrder)
        {
            log.Info(pOrder);
        }

        private void tradeApi_OnRspOrderInsert(ref CThostFtdcInputOrderField pInputOrder,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pInputOrder);
        }


        private void tradeApi_OnErrRtnOrderAction(ref CThostFtdcOrderActionField pOrderAction,
            ref CThostFtdcRspInfoField pRspInfo)
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

            //tradeApi.QryTrade();

            return 0;
        }

        private void tradeApi_OnOnErrRtnOrderInsert(ref CThostFtdcInputOrderField pInputOrder,
            ref CThostFtdcRspInfoField pRspInfo)
        {
            log.Info(pRspInfo);
            log.Info(pInputOrder);
        }

        private void tradeApi_OnRspQryOrder(ref CThostFtdcOrderField porder, ref CThostFtdcRspInfoField prspinfo,
            int nrequestid, bool bislast)
        {
            var orderRecord = new OrderRecord();

            ObjectUtils.Copy(porder, orderRecord);

            orderRecords.Add(orderRecord);

            if (bislast)
            {
                OnRspQryOrderRecord(this, new OrderRecordEventArgs(orderRecords));
            }
        }
    }

    internal class TradeRecordEventArgs : EventArgs
    {
        public TradeRecordEventArgs(List<TradeRecord> tradeRecords)
        {
            this.tradeRecords = tradeRecords;
        }

        public List<TradeRecord> tradeRecords { get; set; }
    }

    internal class PositionDetailEventArgs : EventArgs
    {
        public PositionDetailEventArgs(List<PositionDetail> positionDetails)
        {
            PositionDetails = positionDetails;
        }

        public List<PositionDetail> PositionDetails { get; set; }
    }

    internal class PositionRecordEventArgs : EventArgs
    {
        public PositionRecordEventArgs(List<PositionRecord> positionRecords)
        {
            PositionRecords = positionRecords;
        }

        public List<PositionRecord> PositionRecords { get; set; }
    }

    internal class OrderRecordEventArgs : EventArgs
    {
        public OrderRecordEventArgs(List<OrderRecord> orderRecords)
        {
            OrderRecords = orderRecords;
        }

        public List<OrderRecord> OrderRecords { get; set; }
    }
}