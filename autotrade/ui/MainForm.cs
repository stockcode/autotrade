using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using autotrade.business;
using autotrade.model;
using autotrade.strategy;
using CTPMdApi;
using CTPTradeApi;
using MongoDB.Driver.Linq;
using CThostFtdcContractBankField = CTPTradeApi.CThostFtdcContractBankField;
using CThostFtdcInputOrderField = CTPTradeApi.CThostFtdcInputOrderField;
using CThostFtdcOrderField = CTPTradeApi.CThostFtdcOrderField;
using CThostFtdcReqQueryAccountField = CTPTradeApi.CThostFtdcReqQueryAccountField;
using CThostFtdcRspInfoField = CTPTradeApi.CThostFtdcRspInfoField;
using CThostFtdcTradingAccountField = CTPTradeApi.CThostFtdcTradingAccountField;
using EnumDirectionType = CTPTradeApi.EnumDirectionType;
using EnumOffsetFlagType = CTPTradeApi.EnumOffsetFlagType;

namespace autotrade
{
    public partial class MainForm : Form
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MdApi mdApi = new MdApi("00000081", "123456", "1061", "tcp://219.143.242.155:21213");
        TradeApi tradeApi = new TradeApi("00000081", "123456", "1061", "tcp://219.143.242.155:21205");
        private MAReverseStrategy maReverseStrategy;
        private OrderManager orderManager;

        readonly string[] ppInstrumentID = { "IF1407" };	// 行情订阅列表
        public MainForm()
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
            tradeApi.OnRspQryContractBank += tradeApi_OnRspQryContractBank;
            tradeApi.OnRspQryInvestorPositionDetail += tradeApi_OnRspQryInvestorPositionDetail;
            tradeApi.OnRspQryInvestorPositionCombineDetail += tradeApi_OnRspQryInvestorPositionCombineDetail;
        }

        void tradeApi_OnRspQryInvestorPositionCombineDetail(ref CTPTradeApi.CThostFtdcInvestorPositionCombineDetailField pInvestorPositionCombineDetail, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pInvestorPositionCombineDetail);
        }

        void tradeApi_OnRspQryInvestorPositionDetail(ref CTPTradeApi.CThostFtdcInvestorPositionDetailField pInvestorPositionDetail, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pInvestorPositionDetail);
        }

        

        private void tradeApi_OnRspQryContractBank(ref CThostFtdcContractBankField pcontractbank, ref CThostFtdcRspInfoField prspinfo, int nrequestid, bool bislast)
        {
            log.Info(prspinfo);
            log.Info(pcontractbank);
        }

        void tradeApi_OnRspQryInvestorPosition(ref CTPTradeApi.CThostFtdcInvestorPositionField pInvestorPosition, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pInvestorPosition);
        }

        void tradeApi_OnRspQryTradingAccount(ref CThostFtdcTradingAccountField pTradingAccount, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
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
            //tradeApi.QryInvestorPosition();
            //tradeApi.QryInvestorPositionDetail();
            tradeApi.QryInvestorPositionCombinaDetail();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Order order = new Order();
            order.InstrumentId = "IF1407";
            order.OffsetFlag = EnumOffsetFlagType.Open;
            order.Direction = EnumDirectionType.Buy;
            order.Price = 2000;
            order.Volume = 1;

            orderManager.OrderInsert(order);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            orderManager = new OrderManager(tradeApi);

            LoginForm loginForm = new LoginForm();
            loginForm.ShowDialog();
        }

        private void c1SplitterPanel1_Click(object sender, EventArgs e)
        {

        }
    }
}
