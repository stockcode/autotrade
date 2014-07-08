using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using autotrade.strategy;
using CTPMdApi;
using CTPTradeApi;
using MongoDB.Driver.Linq;
using CThostFtdcReqQueryAccountField = CTPTradeApi.CThostFtdcReqQueryAccountField;
using CThostFtdcRspInfoField = CTPTradeApi.CThostFtdcRspInfoField;
using CThostFtdcTradingAccountField = CTPTradeApi.CThostFtdcTradingAccountField;

namespace autotrade
{
    public partial class Form1 : Form
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MdApi mdApi = new MdApi("50001693", "05221058", "8888", "tcp://210.51.25.90:32111");
        TradeApi tradeApi = new TradeApi("50001693", "05221058", "8888", "tcp://210.51.25.90:41205");
        private MAReverseStrategy maReverseStrategy;

        readonly string[] ppInstrumentID = { "IF1407" };	// 行情订阅列表
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            maReverseStrategy = new MAReverseStrategy(20, "IF1407");

            log.Info("start login");

            mdApi.Connect();

            mdApi.OnFrontConnected += mdApi_OnFrontConnected;
            mdApi.OnRspUserLogin += mdApi_OnRspUserLogin;
            mdApi.OnRspError += mdApi_OnRspError;
            mdApi.OnRspSubMarketData += mdApi_OnRspSubMarketData;
            mdApi.OnRtnDepthMarketData += mdApi_OnRtnDepthMarketData;
            log.Info("login");
        }

        void mdApi_OnRtnDepthMarketData(ref CTPMdApi.CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            string s = string.Format("{0,-6} : UpdateTime = {1}.{2:D3},  LasPrice = {3}", pDepthMarketData.InstrumentID, pDepthMarketData.UpdateTime, pDepthMarketData.UpdateMillisec, pDepthMarketData.LastPrice);
            s += maReverseStrategy.Match(pDepthMarketData);
            log.Info(s);
        }

        void mdApi_OnRspSubMarketData(ref CTPMdApi.CThostFtdcSpecificInstrumentField pSpecificInstrument, ref CTPMdApi.CThostFtdcRspInfoField pRspInfo, int nRequestId, bool bIsLast)
        {
            log.Info(pSpecificInstrument.InstrumentID);
        }

        void mdApi_OnRspUserLogin(ref CTPMdApi.CThostFtdcRspUserLoginField pRspUserLogin, ref CTPMdApi.CThostFtdcRspInfoField pRspInfo, int nRequestId, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pRspUserLogin);
            mdApi.SubMarketData(ppInstrumentID);
        }

        void mdApi_OnRspError(ref CTPMdApi.CThostFtdcRspInfoField pRspInfo, int nRequestId, bool bIsLast)
        {
            log.Info(pRspInfo.ErrorMsg);
        }

        void mdApi_OnFrontConnected()
        {
            log.Info("connected");

            mdApi.UserLogin();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tradeApi.Connect();

            tradeApi.OnFrontConnect += tradeApi_OnFrontConnect;
            tradeApi.OnRspUserLogin += tradeApi_OnRspUserLogin;
            tradeApi.OnRspQryTradingAccount += tradeApi_OnRspQryTradingAccount;
            tradeApi.OnRspQryInvestorPosition += tradeApi_OnRspQryInvestorPosition;

        }

        void tradeApi_OnRspQryInvestorPosition(ref CTPTradeApi.CThostFtdcInvestorPositionField pInvestorPosition, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pInvestorPosition);
        }

        void tradeApi_OnRspQryTradingAccount(ref CThostFtdcTradingAccountField pTradingAccount, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo.ErrorMsg);
            log.Info(pTradingAccount);
        }

        void tradeApi_OnRspUserLogin(ref CTPTradeApi.CThostFtdcRspUserLoginField pRspUserLogin, ref CTPTradeApi.CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pRspUserLogin);


            tradeApi.QryTradingAccount();
        }

        void tradeApi_OnFrontConnect()
        {
            log.Info("connected");

            tradeApi.UserLogin();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tradeApi.QryInvestorPosition();
        }
    }
}
