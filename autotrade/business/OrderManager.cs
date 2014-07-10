using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.model;
using CTPTradeApi;

namespace autotrade.business
{
    class OrderManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TradeApi tradeApi;

        public OrderManager(TradeApi tradeApi)
        {
            this.tradeApi = tradeApi;
            this.tradeApi.OnRspQryOrder += tradeApi_OnRspQryOrder;
            this.tradeApi.OnErrRtnOrderInsert += tradeApi_OnOnErrRtnOrderInsert;
        }

        public int OrderInsert(Order order)
        {
            tradeApi.OrderInsert(order.InstrumentId, order.OffsetFlag, order.Direction, order.Price, order.Volume);

            order.StatusType = EnumOrderStatus.开仓中;

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
    
}
