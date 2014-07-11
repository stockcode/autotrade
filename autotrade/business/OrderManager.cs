using System;
using System.Collections.Generic;
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

        public delegate void TradeRecordHandler(object sender, TradeRecordEventArgs e);
        public event TradeRecordHandler OnRtnTreadeRecord;

        public delegate void PositionDetailHandler(object sender, PositionDetailEventArgs e);
        public event PositionDetailHandler OnRspQryPositionDetail;

        public OrderManager(TradeApi tradeApi)
        {
            this.tradeApi = tradeApi;
            this.tradeApi.OnRspQryOrder += tradeApi_OnRspQryOrder;
            this.tradeApi.OnErrRtnOrderInsert += tradeApi_OnOnErrRtnOrderInsert;
            this.tradeApi.OnErrRtnOrderAction += tradeApi_OnErrRtnOrderAction;
            this.tradeApi.OnRspQryTrade += tradeApi_OnRspQryTrade;
            this.tradeApi.OnRspOrderInsert += tradeApi_OnRspOrderInsert;
            this.tradeApi.OnRspQryInvestorPositionDetail += tradeApi_OnRspQryInvestorPositionDetail;

            this.tradeApi.OnRtnOrder += tradeApi_OnRtnOrder;
            this.tradeApi.OnRtnTrade += tradeApi_OnRtnTrade;            
        }

        void tradeApi_OnRspQryInvestorPositionDetail(ref CThostFtdcInvestorPositionDetailField pInvestorPositionDetail, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            PositionDetail positionDetail = new PositionDetail();

            ObjectUtils.Copy(pInvestorPositionDetail, positionDetail);

            OnRspQryPositionDetail(this, new PositionDetailEventArgs(positionDetail));
        }

        public int QryTrade()
        {
            return this.tradeApi.QryTrade();
        }

        public int QryInvestorPositionDetail()
        {
            return this.tradeApi.QryInvestorPositionDetail();
        }

        void tradeApi_OnRtnTrade(ref CThostFtdcTradeField pTrade)
        {
            TradeRecord tradeRecord = new TradeRecord();

            ObjectUtils.Copy(pTrade, tradeRecord);

            OnRtnTreadeRecord(this, new TradeRecordEventArgs(tradeRecord));

            log.Info(pTrade);
        }

        void tradeApi_OnRtnOrder(ref CThostFtdcOrderField pOrder)
        {
            log.Info(pOrder);
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

            OnRtnTreadeRecord(this, new TradeRecordEventArgs(tradeRecord));

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

            //tradeApi.QryTrade();

            return 0;
        }

        private void tradeApi_OnOnErrRtnOrderInsert(ref CThostFtdcInputOrderField pInputOrder, ref CThostFtdcRspInfoField pRspInfo)
        {
            log.Info(pRspInfo);
            log.Info(pInputOrder);
        }

        private void tradeApi_OnRspQryOrder(ref CThostFtdcOrderField porder, ref CThostFtdcRspInfoField prspinfo, int nrequestid, bool bislast)
        {
            log.Info(prspinfo);
            log.Info(porder);
        }

        
    }

    internal class TradeRecordEventArgs : EventArgs
    {
        public TradeRecord treadeRecord { get; set; }
        public TradeRecordEventArgs(TradeRecord treadeRecord)
            : base()
        {
            this.treadeRecord = treadeRecord;
        }
    }

    internal class PositionDetailEventArgs : EventArgs
    {
        public PositionDetail positionDetail { get; set; }
        public PositionDetailEventArgs(PositionDetail positionDetail)
            : base()
        {
            this.positionDetail = positionDetail;
        }
    }
    
}
